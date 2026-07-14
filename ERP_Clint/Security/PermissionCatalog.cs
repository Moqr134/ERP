namespace ERP_Clint.Security;

public sealed record PermissionMeta(string Name, string DisplayName);

public sealed record PermissionGroup(string Key, string Title, IReadOnlyList<PermissionMeta> Permissions);
public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionGroup> Groups { get; } =
    [
        new("general", "عام",
        [
            new(PermissionNames.FullAccess, "صلاحية كاملة (كل النظام)")
        ]),

        new("users", "المستخدمون",
        [
            new(PermissionNames.GetUsers, "عرض المستخدمين"),
            new(PermissionNames.GetUserById, "عرض تفاصيل مستخدم"),
            new(PermissionNames.GetUsersInfo, "عرض إحصائيات المستخدمين"),
            new(PermissionNames.CreateUser, "إضافة مستخدم"),
            new(PermissionNames.UpdateUser, "تعديل مستخدم"),
            new(PermissionNames.SetUserActive, "تفعيل / تعطيل مستخدم"),
            new(PermissionNames.AssignUserRoles, "تعيين أدوار المستخدم"),
            new(PermissionNames.ChangePassword, "تغيير كلمة مرور مستخدم"),
            new(PermissionNames.GetUserPermissions, "عرض صلاحيات مستخدم"),
            new(PermissionNames.UpdateUserPermission, "تعديل صلاحيات مستخدم"),
            new(PermissionNames.DeleteUser, "حذف مستخدم"),
        ]),

        new("roles", "الأدوار والصلاحيات",
        [
            new(PermissionNames.GetAllRoles, "عرض الأدوار"),
            new(PermissionNames.GetRoleById, "عرض تفاصيل دور"),
            new(PermissionNames.CreateRole, "إضافة دور"),
            new(PermissionNames.UpdateRole, "تعديل دور"),
            new(PermissionNames.DeleteRole, "حذف دور"),
            new(PermissionNames.GetAllPermissions, "عرض كل الصلاحيات"),
            new(PermissionNames.GetRolePermissions, "عرض صلاحيات دور"),
            new(PermissionNames.CreateRolePermission, "تعديل صلاحيات دور"),
        ]),

        new("categories", "الأصناف",
        [
            new(PermissionNames.GetAllCategories, "عرض الأصناف"),
            new(PermissionNames.GetCategoryById, "عرض تفاصيل صنف"),
            new(PermissionNames.CreateCategory, "إضافة صنف"),
            new(PermissionNames.UpdateCategory, "تعديل صنف"),
            new(PermissionNames.DeleteCategory, "حذف صنف"),
        ]),

        new("products", "المنتجات",
        [
            new(PermissionNames.GetAllProductsAsync, "عرض المنتجات"),
            new(PermissionNames.GetProductByIdAsync, "عرض تفاصيل منتج"),
            new(PermissionNames.CreateProduct, "إضافة منتج"),
            new(PermissionNames.UpdateProduct, "تعديل منتج"),
            new(PermissionNames.DeleteProduct, "حذف منتج"),
            new(PermissionNames.GetProductByBarcode, "بحث بالباركود"),
            new(PermissionNames.GetProductStockLedger, "سجل حركة منتج"),
            new(PermissionNames.GetLowStockProduct, "المنتجات منخفضة المخزون"),
            new(PermissionNames.GetProductsInfo, "إحصائيات المنتجات"),
        ]),

        new("suppliers", "الموردون",
        [
            new(PermissionNames.GetAllSuppliers, "عرض الموردين"),
            new(PermissionNames.GetSupplierById, "عرض تفاصيل مورد"),
            new(PermissionNames.AddSuppliers, "إضافة مورد"),
            new(PermissionNames.EditSuppliers, "تعديل مورد"),
            new(PermissionNames.DeleteSuppliers, "حذف مورد"),
        ]),

        new("stock", "حركات المخزون",
        [
            new(PermissionNames.GetStockTransactions, "عرض حركات المخزون"),
            new(PermissionNames.AddStockTransaction, "إضافة حركة مخزون"),
        ]),
    ];

    private static readonly Dictionary<string, string> DisplayNames =
        Groups.SelectMany(g => g.Permissions)
              .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
              .ToDictionary(g => g.Key, g => g.First().DisplayName, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> GroupTitles =
        Groups.SelectMany(g => g.Permissions.Select(p => (p.Name, g.Title)))
              .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
              .ToDictionary(g => g.Key, g => g.First().Title, StringComparer.OrdinalIgnoreCase);

    public static string GetDisplayName(string? permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            return "—";
        return DisplayNames.TryGetValue(permissionName, out var label) ? label : permissionName;
    }

    public static string GetGroupTitle(string? permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            return "أخرى";
        return GroupTitles.TryGetValue(permissionName, out var title) ? title : "أخرى";
    }

    public static IEnumerable<IGrouping<string, T>> GroupByModule<T>(
        IEnumerable<T> items,
        Func<T, string?> nameSelector)
    {
        var order = Groups.Select((g, i) => (g.Title, i)).ToDictionary(x => x.Title, x => x.i);
        return items
            .GroupBy(item => GetGroupTitle(nameSelector(item)))
            .OrderBy(g => order.TryGetValue(g.Key, out var idx) ? idx : int.MaxValue)
            .ThenBy(g => g.Key);
    }
}
