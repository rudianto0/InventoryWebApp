using System.Text.Encodings.Web;
using Inventory.Contracts;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory.Web.Models;

public static class SortHeaderExtensions
{
    public static IHtmlContent SortHeader(
        this IHtmlHelper html,
        string label,
        string sortKey,
        string? currentSortBy,
        SortDirection currentDir,
        string? cssClass = null)
    {
        var request = html.ViewContext.HttpContext.Request;
        var parts = request.Query
            .Where(kv => !string.Equals(kv.Key, "sortBy", StringComparison.OrdinalIgnoreCase)
                      && !string.Equals(kv.Key, "sortDir", StringComparison.OrdinalIgnoreCase)
                      && !string.Equals(kv.Key, "page", StringComparison.OrdinalIgnoreCase))
            .SelectMany(kv => kv.Value.Select(v => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(v ?? string.Empty)}"))
            .ToList();

        var isCurrent = string.Equals(currentSortBy, sortKey, StringComparison.OrdinalIgnoreCase);
        var nextDir = isCurrent && currentDir == SortDirection.Asc ? "desc" : "asc";
        parts.Add($"sortBy={Uri.EscapeDataString(sortKey)}");
        parts.Add($"sortDir={nextDir}");
        var href = "?" + string.Join("&", parts);

        var indicator = isCurrent ? (currentDir == SortDirection.Asc ? " ▲" : " ▼") : string.Empty;
        var ariaSort = isCurrent
            ? (currentDir == SortDirection.Asc ? "ascending" : "descending")
            : "none";

        var encoder = HtmlEncoder.Default;
        var classes = string.IsNullOrWhiteSpace(cssClass) ? "sortable" : $"sortable {cssClass}";
        var text = encoder.Encode(label) + indicator;

        return new HtmlString(
            $"<th class=\"{classes}\" aria-sort=\"{ariaSort}\">" +
            $"<a class=\"sort-link\" href=\"{encoder.Encode(href)}\">{text}</a></th>");
    }
}
