using Microsoft.EntityFrameworkCore;

namespace TelegramShopBot
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ShopClient> ShopClients { get; set; } = null!;
        public DbSet<ProductOrder> ProductsOrders { get; set; } = null!;
        public DbSet<ProductsType> ProductsTypes { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Directory.Exists(@"C:\\ProgramData\CDEV_TelegramShopBot\") == false)
            {
                Directory.CreateDirectory(@"C:\\ProgramData\CDEV_TelegramShopBot\");
            }
            optionsBuilder.UseSqlite(@"Data Source=C:\\ProgramData\CDEV_TelegramShopBot\DataBase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShopClient>().HasKey(sc => sc.ChatId);

            modelBuilder.Entity<ShopClient>()
                .HasOne(sc => sc.BeingCreatedProductOrder)
                .WithOne(o => o.ShopClientOnCreating)
                .HasForeignKey<ProductOrder>(o1 => o1.ShopClientIdOnCreating)
                .HasPrincipalKey<ShopClient>(sc1 => sc1.ChatId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShopClient>()
                .HasMany(sc => sc.ProductOrders)
                .WithOne(o => o.ShopClientOnStoring)
                .HasForeignKey(o1 => o1.ShopClientIdOnStoring)
                .HasPrincipalKey(sc1 => sc1.ChatId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}