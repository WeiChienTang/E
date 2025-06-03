using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<IndustryType> IndustryTypes { get; set; }
        public DbSet<ContactType> ContactTypes { get; set; }
        public DbSet<AddressType> AddressTypes { get; set; }
        public DbSet<CustomerContact> CustomerContacts { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
          protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Customer Entity - Only Delete Behaviors (Index moved to Entity)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasOne(e => e.CustomerType)
                      .WithMany(ct => ct.Customers)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.IndustryType)
                      .WithMany(i => i.Customers)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure CustomerContact Entity - Only Delete Behaviors
            modelBuilder.Entity<CustomerContact>(entity =>
            {
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.CustomerContacts)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ContactType)
                      .WithMany(ct => ct.CustomerContacts)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure CustomerAddress Entity - Only Delete Behaviors
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.CustomerAddresses)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.AddressType)
                      .WithMany(at => at.CustomerAddresses)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
