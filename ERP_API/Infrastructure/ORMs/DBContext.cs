using ERP_API.Domin.CategoriesEntity;
using ERP_API.Domin.PermissionsEntity;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Domin.UsersEntity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ORM;

public class DBContext:DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new CategoriesMap());
        modelBuilder.ApplyConfiguration(new ProductMap());
        modelBuilder.ApplyConfiguration(new StockTransactionsMap());
        modelBuilder.ApplyConfiguration(new UserRolesMap());
        modelBuilder.ApplyConfiguration(new UserPermissionsMap());
        modelBuilder.ApplyConfiguration(new RolePermissionsMap());
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockTransactions> StockTransactions { get; set; }
    public DbSet<UserRoles> UserRoles { get; set; }
    public DbSet<UserPermissions> UserPermissions { get; set; }
    public DbSet<RolePermissions> RolePermissions { get; set; }
}
