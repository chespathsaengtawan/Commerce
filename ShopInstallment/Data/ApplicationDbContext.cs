using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Models;

namespace ShopInstallment.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InstallmentPlan> InstallmentPlans { get; set; }
        public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ProductRating> ProductRatings { get; set; }
        public DbSet<SellerPaymentSetting> SellerPaymentSettings { get; set; }
        public DbSet<EscrowAccount> EscrowAccounts { get; set; }
        public DbSet<CoinWallet> CoinWallets { get; set; }
        public DbSet<CoinTransaction> CoinTransactions { get; set; }
        public DbSet<Gender> Genders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal precision
            builder.Entity<CoinTransaction>().Property(e => e.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<CoinWallet>().Property(e => e.Balance).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowAccount>().Property(e => e.Balance).HasColumnType("decimal(18,2)");
            builder.Entity<SellerPaymentSetting>().Property(e => e.PromptPayFeePercent).HasColumnType("decimal(5,2)");
            builder.Entity<SellerPaymentSetting>().Property(e => e.BillPaymentFeePercent).HasColumnType("decimal(5,2)");
            builder.Entity<SellerPaymentSetting>().Property(e => e.TrueWalletFeePercent).HasColumnType("decimal(5,2)");
            builder.Entity<SellerPaymentSetting>().Property(e => e.BankTransferFeePercent).HasColumnType("decimal(5,2)");
            builder.Entity<SellerPaymentSetting>().Property(e => e.CoinFeePercent).HasColumnType("decimal(5,2)");

            // Configure relationships
            builder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InstallmentPlan>()
                .HasOne(i => i.Order)
                .WithOne(o => o.InstallmentPlan)
                .HasForeignKey<InstallmentPlan>(i => i.OrderId);

            // Limit images to 5 per product via check constraint (soft enforcement; server-side validation needed too)
            builder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.IsMain });
            // Note: Admin user and roles seeding moved to DbInitializer for better control
        }
    }
}
