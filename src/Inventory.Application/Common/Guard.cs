using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.Common;

public static class Guard
{
    public static void Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{field} wajib diisi.");
    }

    public static void Positive(int value, string field)
    {
        if (value <= 0)
            throw new ValidationException($"{field} harus lebih besar dari 0.");
    }

    public static void Positive(decimal value, string field)
    {
        if (value <= 0)
            throw new ValidationException($"{field} harus lebih besar dari 0.");
    }

    public static void NotNegative(int value, string field)
    {
        if (value < 0)
            throw new ValidationException($"{field} tidak boleh negatif.");
    }

    public static void NotNegative(decimal value, string field)
    {
        if (value < 0)
            throw new ValidationException($"{field} tidak boleh negatif.");
    }

    public static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
