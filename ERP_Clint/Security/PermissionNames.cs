namespace ERP_Clint.Security;

/// <summary>
/// Permission name constants — must match API SystemPermissions / Authorize roles.
/// </summary>
public static class PermissionNames
{
    public const string FullAccess = "FullAccess";

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

    public const string GetAllRoles = "GetAllRoles";
    public const string GetRoleById = "GetRoleById";
    public const string CreateRole = "CreateRole";
    public const string UpdateRole = "UpdateRole";
    public const string DeleteRole = "DeleteRole";
    public const string GetAllPermissions = "GetAllPermissions";
    public const string GetRolePermissions = "GetRolePermissions";
    public const string CreateRolePermission = "CreateRolePermission";

    public const string GetAllCategories = "GetAllCategories";
    public const string GetCategoryById = "GetCategoryById";
    public const string CreateCategory = "CreateCategory";
    public const string UpdateCategory = "UpdateCategory";
    public const string DeleteCategory = "DeleteCategory";

    public const string GetAllProductsAsync = "GetAllProductsAsync";
    public const string GetProductByIdAsync = "GetProductByIdAsync";
    public const string CreateProduct = "CreateProduct";
    public const string UpdateProduct = "UpdateProduct";
    public const string DeleteProduct = "DeleteProduct";
    public const string GetProductByBarcode = "GetProductByBarcode";
    public const string GetProductStockLedger = "GetProductStockLedger";
    public const string GetLowStockProduct = "GetLowStockProduct";
    public const string GetProductsInfo = "GetProductsInfo";

    public const string GetAllSuppliers = "GetAllSuppliers";
    public const string GetSupplierById = "GetSupplierById";
    public const string AddSuppliers = "AddSuppliers";
    public const string EditSuppliers = "EditSuppliers";
    public const string DeleteSuppliers = "DeleteSuppliers";

    public const string AddStockTransaction = "AddStockTransaction";
    public const string GetStockTransactions = "GetStockTransactions";

    public const string CompleteSale = "CompleteSale";
    public const string GetSales = "GetSales";
    public const string GetSaleById = "GetSaleById";

    public const string GetReportsOverview = "GetReportsOverview";
    public const string GetProductsReport = "GetProductsReport";
    public const string GetCategoriesReport = "GetCategoriesReport";
    public const string GetUsersReport = "GetUsersReport";
    public const string GetSalesReport = "GetSalesReport";
    public const string GetStockReport = "GetStockReport";
}
