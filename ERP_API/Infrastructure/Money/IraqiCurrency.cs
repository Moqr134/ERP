namespace ERP_API.Infrastructure.Money;

/// <summary>Iraqi cash change helper for POS sales.</summary>
public static class IraqiCurrency
{
    /// <summary>Change to return to the customer (paid − due), never negative.</summary>
    public static double CalculateChange(double due, double paid)
    {
        due = Math.Max(0, Math.Round(due, MidpointRounding.AwayFromZero));
        paid = Math.Max(0, Math.Round(paid, MidpointRounding.AwayFromZero));
        return Math.Max(0, paid - due);
    }
}
