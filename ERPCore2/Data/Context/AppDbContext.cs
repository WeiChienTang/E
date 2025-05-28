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
            
            // Configure Customer Entity
            modelBuilder.Entity<Customer>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.CustomerId);
                
                // Required Fields
                entity.Property(e => e.CustomerCode)
                      .IsRequired()
                      .HasMaxLength(20);
                      
                entity.Property(e => e.CompanyName)
                      .IsRequired()
                      .HasMaxLength(100);
                
                // Optional Fields
                entity.Property(e => e.ContactPerson)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.TaxNumber)
                      .HasMaxLength(20);
                      
                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.ModifiedBy)
                      .HasMaxLength(50);
                  // Default Values
                entity.Property(e => e.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
                
                // Indexes
                entity.HasIndex(e => e.CustomerCode)
                      .IsUnique();
                
                // Relationships
                entity.HasOne(e => e.CustomerType)
                      .WithMany(ct => ct.Customers)
                      .HasForeignKey(e => e.CustomerTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
                        entity.HasOne(e => e.IndustryType)
                      .WithMany(i => i.Customers)
                      .HasForeignKey(e => e.IndustryTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure CustomerType Entity
            modelBuilder.Entity<CustomerType>(entity =>
            {
                entity.HasKey(e => e.CustomerTypeId);
                
                entity.Property(e => e.TypeName)
                      .IsRequired()
                      .HasMaxLength(50);
                        entity.Property(e => e.Description)
                      .HasMaxLength(200);
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
            });
              // Configure IndustryType Entity
            modelBuilder.Entity<IndustryType>(entity =>
            {
                entity.HasKey(e => e.IndustryTypeId);
                
                entity.Property(e => e.IndustryTypeName)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.Property(e => e.IndustryTypeCode)
                      .HasMaxLength(10);
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
            });
              // Configure ContactType Entity
            modelBuilder.Entity<ContactType>(entity =>
            {
                entity.HasKey(e => e.ContactTypeId);
                
                entity.Property(e => e.TypeName)
                      .IsRequired()
                      .HasMaxLength(20);
                      
                entity.Property(e => e.Description)
                      .HasMaxLength(100);
                      
                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.ModifiedBy)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
            });
            
            // Configure AddressType Entity
            modelBuilder.Entity<AddressType>(entity =>
            {
                entity.HasKey(e => e.AddressTypeId);
                
                entity.Property(e => e.TypeName)
                      .IsRequired()
                      .HasMaxLength(20);
                        entity.Property(e => e.Description)
                      .HasMaxLength(100);
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
            });
            
            // Configure CustomerContact Entity
            modelBuilder.Entity<CustomerContact>(entity =>
            {
                entity.HasKey(e => e.ContactId);
                  entity.Property(e => e.ContactValue)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.Property(e => e.IsPrimary)
                      .HasDefaultValue(false);
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
                
                // Relationships
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.CustomerContacts)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.ContactType)
                      .WithMany(ct => ct.CustomerContacts)
                      .HasForeignKey(e => e.ContactTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure CustomerAddress Entity
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(e => e.AddressId);
                
                entity.Property(e => e.PostalCode)
                      .HasMaxLength(10);
                      
                entity.Property(e => e.City)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.District)
                      .HasMaxLength(50);
                        entity.Property(e => e.Address)
                      .HasMaxLength(200);
                      
                entity.Property(e => e.IsPrimary)
                      .HasDefaultValue(false);
                      
                entity.Property(e => e.Status)
                      .HasDefaultValue(EntityStatus.Active)
                      .HasSentinel(EntityStatus.Default);
                
                // Relationships
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.CustomerAddresses)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.AddressType)
                      .WithMany(at => at.CustomerAddresses)
                      .HasForeignKey(e => e.AddressTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
