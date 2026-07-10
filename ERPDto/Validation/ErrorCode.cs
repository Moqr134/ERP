namespace Validation;

public static class ErrorCode
{
    public static string KeyNotFound => "KeyNotFound";
    public static string Required => "هذا الحقل مطلوب";
    public static string MinimumLengthAllowedIs(int x) => $"اقل عدد احرف مطلوب({x})";
    public static string MaximumLengthAllowedIs(int x) => $"اكبر عدد احرف مطلوب({x})";
}
