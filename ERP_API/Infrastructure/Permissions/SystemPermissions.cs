namespace ERP_API.Infrastructure.Permissions;

/// <summary>
/// Catalog of all permissions required by the ERP system.
/// Keep this list in sync with [Authorize(Roles = "...")] attributes on controllers.
/// </summary>
public static class SystemPermissions
{
    public const string FullAccess = "FullAccess";

    // Account
    // FullAccess covers register; no granular account perms beyond that.

    // Users
    public const string GetUsers = "GetUsers";
    public const string GetUserById = "GetUserById";
    public const string GetUsersInfo = "GetUsersInfo";
    public const string CreateUser = "CreateUser";
    public const string UpdateUser = "UpdateUser";
    public const string SetUserActive = "SetUserActive";
    public const string AssignUserRoles = "AssignUserRoles";
    public const string ChangePassword = "ChangePassword";
    public const string GetUserPermissions = "GetUserPermissions";
    public const string UpdateUserPermission = "UpdateUserPermission";
    public const string DeleteUser = "DeleteUser";

    // Roles
    public const string GetAllRoles = "GetAllRoles";
    public const string GetRoleById = "GetRoleById";
    public const string CreateRole = "CreateRole";
    public const string UpdateRole = "UpdateRole";
    public const string DeleteRole = "DeleteRole";
    public const string GetAllPermissions = "GetAllPermissions";
    public const string GetRolePermissions = "GetRolePermissions";
    public const string CreateRolePermission = "CreateRolePermission";

    // Categories
    public const string GetAllCategories = "GetAllCategories";
    public const string GetCategoryById = "GetCategoryById";
    public const string CreateCategory = "CreateCategory";
    public const string UpdateCategory = "UpdateCategory";
    public const string DeleteCategory = "DeleteCategory";

    // Products
    public const string GetAllProductsAsync = "GetAllProductsAsync";
    public const string GetProductByIdAsync = "GetProductByIdAsync";
    public const string CreateProduct = "CreateProduct";
    public const string UpdateProduct = "UpdateProduct";
    public const string DeleteProduct = "DeleteProduct";
    public const string GetProductByBarcode = "GetProductByBarcode";
    public const string GetProductStockLedger = "GetProductStockLedger";
    public const string GetLowStockProduct = "GetLowStockProduct";
    public const string GetProductsInfo = "GetProductsInfo";

    // Suppliers
    public const string GetAllSuppliers = "GetAllSuppliers";
    public const string GetSupplierById = "GetSupplierById";
    public const string AddSuppliers = "AddSuppliers";
    public const string EditSuppliers = "EditSuppliers";
    public const string DeleteSuppliers = "DeleteSuppliers";

    // Warehouses
    public const string GetAllWarehouses = "GetAllWarehouses";
    public const string GetWarehouseById = "GetWarehouseById";
    public const string AddWarehouse = "AddWarehouse";
    public const string EditWarehouse = "EditWarehouse";
    public const string DeleteWarehouse = "DeleteWarehouse";

    // Stock
    public const string AddStockTransaction = "AddStockTransaction";
    public const string GetStockTransactions = "GetStockTransactions";
    public const string CreateStockTransfer = "CreateStockTransfer";
    public const string GetStockTransfers = "GetStockTransfers";

    // Sales / POS
    public const string CompleteSale = "CompleteSale";
    public const string GetSales = "GetSales";
    public const string GetSaleById = "GetSaleById";

    // Reports
    public const string GetReportsOverview = "GetReportsOverview";
    public const string GetProductsReport = "GetProductsReport";
    public const string GetCategoriesReport = "GetCategoriesReport";
    public const string GetUsersReport = "GetUsersReport";
    public const string GetSalesReport = "GetSalesReport";
    public const string GetStockReport = "GetStockReport";

    /// <summary>
    /// All permissions the system must have in the database.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        FullAccess,

        GetUsers,
        GetUserById,
        GetUsersInfo,
        CreateUser,
        UpdateUser,
        SetUserActive,
        AssignUserRoles,
        ChangePassword,
        GetUserPermissions,
        UpdateUserPermission,
        DeleteUser,

        GetAllRoles,
        GetRoleById,
        CreateRole,
        UpdateRole,
        DeleteRole,
        GetAllPermissions,
        GetRolePermissions,
        CreateRolePermission,

        GetAllCategories,
        GetCategoryById,
        CreateCategory,
        UpdateCategory,
        DeleteCategory,

        GetAllProductsAsync,
        GetProductByIdAsync,
        CreateProduct,
        UpdateProduct,
        DeleteProduct,
        GetProductByBarcode,
        GetProductStockLedger,
        GetLowStockProduct,
        GetProductsInfo,

        GetAllWarehouses,
        GetWarehouseById,
        AddWarehouse,
        EditWarehouse,
        DeleteWarehouse,

        GetAllSuppliers,
        GetSupplierById,
        AddSuppliers,
        EditSuppliers,
        DeleteSuppliers,

        AddStockTransaction,
        GetStockTransactions,
        CreateStockTransfer,
        GetStockTransfers,

        CompleteSale,
        GetSales,
        GetSaleById,

        GetReportsOverview,
        GetProductsReport,
        GetCategoriesReport,
        GetUsersReport,
        GetSalesReport,
        GetStockReport,
    ];
}
