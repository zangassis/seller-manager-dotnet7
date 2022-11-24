using Microsoft.EntityFrameworkCore;
using SellerManager.Models;


public class SellerDb : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
          options.UseSqlite("DataSource = sellers; Cache=Shared");

    public DbSet<Seller> Sellers { get; set; }
}