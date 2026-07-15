using ERP_API.Domin.CategoriesEntity;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.PermissionsEntity;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Domin.SalesEntity;
using ERP_API.Domin.SuppliersEntity;
using ERP_API.Domin.UsersEntity;
using ERP_API.Domin.WarehouseEntity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ORM;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new CategoriesMap());
        modelBuilder.ApplyConfiguration(new ProductMap());
        modelBuilder.ApplyConfiguration(new ProductUnitMap());
        modelBuilder.ApplyConfiguration(new ProductBarcodeMap());
        modelBuilder.ApplyConfiguration(new RoleMap());
        modelBuilder.ApplyConfiguration(new PermissionsMap());
        modelBuilder.ApplyConfiguration(new StockTransactionsMap());
        modelBuilder.ApplyConfiguration(new UserRolesMap());
        modelBuilder.ApplyConfiguration(new UserPermissionsMap());
        modelBuilder.ApplyConfiguration(new RolePermissionsMap());
        modelBuilder.ApplyConfiguration(new SuppliersMap());
        modelBuilder.ApplyConfiguration(new SaleMap());
        modelBuilder.ApplyConfiguration(new SaleLineMap());
        modelBuilder.ApplyConfiguration(new WarehouseMap());
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductUnit> ProductUnits { get; set; }
    public DbSet<ProductBarcode> ProductBarcodes { get; set; }
    public DbSet<StockTransactions> StockTransactions { get; set; }
    public DbSet<UserRoles> UserRoles { get; set; }
    public DbSet<UserPermissions> UserPermissions { get; set; }
    public DbSet<RolePermissions> RolePermissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Suppliers> Suppliers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleLine> SaleLines { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
}
