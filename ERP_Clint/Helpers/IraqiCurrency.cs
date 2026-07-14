namespace ERP_Clint.Helpers;

/// <summary>
/// Iraqi cash tender / change helpers for POS.
/// Common notes: 250, 500, 1,000, 5,000, 10,000, 25,000, 50,000.
/// </summary>
public static class IraqiCurrency
{
    public const int Unit = 250;

    /// <summary>
    /// Suggested amounts the customer is expected to hand over.
    /// Example: bill 23,500 → 23,500 (exact) and 25,000 (next convenient note).
    /// </summary>
    public static IReadOnlyList<double> SuggestCashPayments(double total, int maxSuggestions = 4)
    {
        total = Math.Max(0, Math.Round(total, MidpointRounding.AwayFromZero));
        if (total <= 0)
            return [];

        var suggestions = new SortedSet<double> { total };

        // Prefer larger Iraqi note boundaries cashiers commonly receive.
        foreach (var step in new[] { 5_000, 10_000, 25_000, 50_000 })
        {
            var roundedUp = Math.Ceiling(total / (double)step) * step;
            if (roundedUp > total)
                suggestions.Add(roundedUp);
        }

        // For smaller bills, also offer the next 1,000 step.
        if (total < 10_000)
        {
            var nextThousand = Math.Ceiling(total / 1_000d) * 1_000;
            if (nextThousand > total)
                suggestions.Add(nextThousand);
        }

        foreach (var note in new[] { 25_000, 50_000 })
        {
            if (note >= total)
                suggestions.Add(note);
        }

        return suggestions.Take(Math.Max(1, maxSuggestions)).ToList();
    }

    /// <summary>Change to return to the customer (paid − due), never negative.</summary>
    public static double CalculateChange(double due, double paid)
    {
        due = Math.Max(0, Math.Round(due, MidpointRounding.AwayFromZero));
        paid = Math.Max(0, Math.Round(paid, MidpointRounding.AwayFromZero));
        return Math.Max(0, paid - due);
    }
}
