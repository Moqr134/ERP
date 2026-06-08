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

    }
    public DbSet<Users> Users { get; set; }
}
