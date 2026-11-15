using Inventory.Contracts.Enums;

namespace Inventory.Domain.Entities;

public interface IApprovable
{
    ApprovalStatus Status { get; set; }
    int? ApproverId { get; set; }
    DateTime? SubmittedAt { get; set; }
    DateTime? ApprovedAt { get; set; }
    int? ApprovedById { get; set; }
    string? RejectionReason { get; set; }
}
