using System.Net;
using FluentAssertions;
using Inventory.Application.Services;
using Inventory.Contracts.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests;

public class ThirdPartyApprovalIntegrationTests
{
    private static StockInService Service(IntegrationDatabase fixture) =>
        new(
            fixture.Db,
            fixture.CurrentUser,
            new FakeUserDirectory(),
            new StockLedger(fixture.Db, new FakeDocumentNumbers()),
            new FakeDocumentNumbers());

    private static void ActAs(IntegrationDatabase fixture, int userId, string role)
    {
        fixture.CurrentUser.UserId = userId;
        fixture.CurrentUser.Roles = new[] { role };
    }

    private static async Task<Guid> CreateSubmittedStockInAsync(IntegrationDatabase fixture)
    {
        await fixture.SeedBasicsAsync();
        var order = new ProcurementOrder(
            "PO-EXT-001",
            "SUP",
            "WH1",
            new[] { new ProcurementLine("SKU-1", 7, 45000m) });
        var input = await ProcurementMapper.ToStockInInputAsync(fixture, order);
        ActAs(fixture, 10, "Staff");
        var service = Service(fixture);
        var header = await service.CreateAsync(input);
        await service.SubmitAsync(header.Id);
        return header.Id;
    }

    private static HttpClient CreateHttpClient(ThirdPartyApprovalStub stub) =>
        new() { BaseAddress = new Uri(stub.BaseUrl) };

    [Fact]
    public async Task ThirdPartyApproval_ApproveViaHttp_UpdatesStock()
    {
        await using var fixture = new IntegrationDatabase();
        var id = await CreateSubmittedStockInAsync(fixture);
        await using var stub = await ThirdPartyApprovalStub.StartAsync(async request =>
        {
            ActAs(fixture, 20, "SPV");
            await Service(fixture).ApproveAsync(request.DocumentId, request.Reason);
        });
        using var http = CreateHttpClient(stub);
        var client = new ThirdPartyApprovalClient(http);

        var response = await client.SendAsync(new ThirdPartyApprovalRequest(id, "approve", "OK dari third party"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stub.LastRequest!.DocumentId.Should().Be(id);
        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Approved);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7);
        (await fixture.Db.StockMovements.SingleAsync()).Quantity.Should().Be(7);
    }

    [Fact]
    public async Task ThirdPartyApproval_RejectViaHttp_LeavesStockUnchanged()
    {
        await using var fixture = new IntegrationDatabase();
        var id = await CreateSubmittedStockInAsync(fixture);
        await using var stub = await ThirdPartyApprovalStub.StartAsync(async request =>
        {
            ActAs(fixture, 20, "SPV");
            await Service(fixture).RejectAsync(request.DocumentId, request.Reason ?? "rejected");
        });
        using var http = CreateHttpClient(stub);
        var client = new ThirdPartyApprovalClient(http);

        var response = await client.SendAsync(new ThirdPartyApprovalRequest(id, "reject", "Harga tidak sesuai"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var saved = await fixture.Db.StockInHeaders.SingleAsync();
        saved.Status.Should().Be(ApprovalStatus.Rejected);
        saved.RejectionReason.Should().Be("Harga tidak sesuai");
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
        (await fixture.Db.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ThirdPartyApproval_InvalidApiKey_Returns401AndDoesNotApprove()
    {
        await using var fixture = new IntegrationDatabase();
        var id = await CreateSubmittedStockInAsync(fixture);
        var callbackCalled = false;
        await using var stub = await ThirdPartyApprovalStub.StartAsync(_ =>
        {
            callbackCalled = true;
            return Task.CompletedTask;
        });
        using var http = CreateHttpClient(stub);
        var client = new ThirdPartyApprovalClient(http);

        var response = await client.SendAsync(
            new ThirdPartyApprovalRequest(id, "approve", null),
            apiKey: "wrong-key");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        callbackCalled.Should().BeFalse();
        (await fixture.Db.StockInHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Submitted);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ThirdPartyApproval_ServerError_DoesNotApprove()
    {
        await using var fixture = new IntegrationDatabase();
        var id = await CreateSubmittedStockInAsync(fixture);
        var callbackCalled = false;
        await using var stub = await ThirdPartyApprovalStub.StartAsync(
            _ =>
            {
                callbackCalled = true;
                return Task.CompletedTask;
            },
            failWithStatus: StatusCodes.Status500InternalServerError);
        using var http = CreateHttpClient(stub);
        var client = new ThirdPartyApprovalClient(http);

        var response = await client.SendAsync(new ThirdPartyApprovalRequest(id, "approve", null));

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        callbackCalled.Should().BeFalse();
        (await fixture.Db.StockInHeaders.SingleAsync()).Status.Should().Be(ApprovalStatus.Submitted);
        (await fixture.Db.Stocks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ThirdPartyApproval_DuplicateCallback_DoesNotDoubleApply()
    {
        await using var fixture = new IntegrationDatabase();
        var id = await CreateSubmittedStockInAsync(fixture);
        await using var stub = await ThirdPartyApprovalStub.StartAsync(async request =>
        {
            ActAs(fixture, 20, "SPV");
            await Service(fixture).ApproveAsync(request.DocumentId);
        });
        using var http = CreateHttpClient(stub);
        var client = new ThirdPartyApprovalClient(http);

        var first = await client.SendAsync(new ThirdPartyApprovalRequest(id, "approve", null));
        var second = await client.SendAsync(new ThirdPartyApprovalRequest(id, "approve", null));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await fixture.Db.Stocks.SingleAsync()).Quantity.Should().Be(7);
    }
}
