using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportStore.Models;
namespace SportStore
{
    public class SportStoreDbContext:DbContext
    {
        public SportStoreDbContext(DbContextOptions<SportStoreDbContext> options) : base(options)
        {

        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Products> Products { get; set; }

        public DbSet<Shop> Shops { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<DeliveryStatus> DeliveryStatuses { get; set; }

        public DbSet<OrderStatus> OrderStatuses { get; set; }

        public DbSet<Receiver> Receivers { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<MallInfo> MallInfo { get; set; }

        public DbSet<Apply> Applies { get; set; }

        public DbSet<Reply> Replies { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasDiscriminator<string>("Discriminator")
                .HasValue<User>("User")
                .HasValue<Merchant>("Merchant")
                .HasValue<Administrator>("Administrator");
            modelBuilder.Entity<Products>().ToTable("Products").HasOne(x=>x.Shop).WithMany(x=>x.Products);
            modelBuilder.Entity<Shop>().ToTable("Shop").HasOne(x=>x.Merchant).WithOne(x=>x.Shop);
            modelBuilder.Entity<Image>().ToTable("Image").HasOne(x => x.Product).WithMany(x => x.Images);
            modelBuilder.Entity<Cart>().ToTable("Cart").HasOne(x=>x.Account).WithOne(x=>x.ShoppingCart).HasForeignKey<Cart>(x=>x.AccountID);
            modelBuilder.Entity<CartItem>().ToTable("CartItem").HasOne(x => x.Cart).WithMany(x => x.Item);
            modelBuilder.Entity<CartItem>().HasOne(x => x.Product).WithMany(x => x.CartItems);
            modelBuilder.Entity<Order>().ToTable("Order").HasOne(x => x.Customer).WithMany(x => x.Orders);
            modelBuilder.Entity<OrderItem>().ToTable("OrderItem").HasOne(x => x.Order).WithMany(x => x.OrderItems);
            modelBuilder.Entity<Delivery>().ToTable("Delivery").HasOne(x => x.Order).WithOne(x => x.Delivery).HasForeignKey<Delivery>(x => x.OrderID);
            modelBuilder.Entity<DeliveryStatus>().ToTable("DeliveryStatus").HasMany(x => x.Deliverys).WithOne(x => x.Status);
            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus").HasMany(x => x.Orders).WithOne(x => x.Status);
            modelBuilder.Entity<Receiver>().ToTable("Receiver").HasOne(x => x.Account).WithMany(x => x.Receivers);
            modelBuilder.Entity<Receiver>().HasMany(x =>x.Orders).WithOne(x => x.Receiver);
            modelBuilder.Entity<Order>().HasOne(x => x.Shop).WithMany(x => x.Orders);
            modelBuilder.Entity<Comment>().ToTable("Comment").HasOne(x => x.Product).WithMany(x => x.Comments);
            modelBuilder.Entity<Comment>().HasOne(x => x.Account).WithMany(x => x.Comments);
            modelBuilder.Entity<MallInfo>().ToTable("MallInfo").Property(x=>x.Discount).HasDefaultValue(10);
            modelBuilder.Entity<Apply>().ToTable("Apply").HasOne(x => x.Account).WithOne(x => x.Apply).HasForeignKey<Apply>(x => x.AccountID);
            modelBuilder.Entity<Reply>().ToTable("Reply").HasOne(x => x.Account).WithMany(x => x.Replies);
            modelBuilder.Entity<Reply>().HasOne(x => x.Comment).WithMany(x => x.Replies);
            modelBuilder.Entity<Payment>().ToTable("Payment").HasMany(x => x.Orders).WithOne(x => x.PayMent);
            modelBuilder.Entity<Payment>().HasOne(x => x.Account).WithMany(x => x.Payments);
        }

    }


    public static class DbInitializer
    {
        public static void Initialize(SportStoreDbContext context)
        {
            if (context.Accounts.Any())
            {
                return;
            }
            //User user = new User { CreateTime = DateTime.Now, Email = "2@qq.com", Name = "傅思欣", Password = "123456", Phone = "11111111111" };
            Administrator administrator= new Administrator { CreateTime = DateTime.Now, Email = "Admin@Admin.com", Name = "Admin", Password = "123456", Phone = "13727519274" };
           //Merchant merchant=new Merchant { CreateTime = DateTime.Now, Email = "2@qq.com", Name = "傅思欣", Password = "123456", Phone = "11111111111" };
            //context.Accounts.Add(user);
            context.Accounts.Add(administrator);
            //context.Accounts.Add(merchant);
            context.SaveChanges();

        }
    }
}
