using Domain.Catalogs.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Format> Formats => Set<Format>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<CatalogAuditLog> CatalogChanges => Set<CatalogAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company", "Catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.OwnsOne(x => x.Code, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Code").HasMaxLength(150).IsRequired();
            });
            entity.OwnsOne(x => x.Description, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Description").HasMaxLength(250).IsRequired();
            });
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ModifiedAt).IsRequired();
            entity.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<Format>(entity =>
        {
            entity.ToTable("Format", "Catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.OwnsOne(x => x.Code, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Code").HasMaxLength(150).IsRequired();
            });
            entity.OwnsOne(x => x.Description, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Description").HasMaxLength(250).IsRequired();
            });
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ModifiedAt).IsRequired();
            entity.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.ToTable("Discipline", "Catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.OwnsOne(x => x.Code, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Code").HasMaxLength(150).IsRequired();
            });
            entity.OwnsOne(x => x.Description, owned =>
            {
                owned.Property(v => v.Value).HasColumnName("Description").HasMaxLength(250).IsRequired();
            });
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ModifiedAt).IsRequired();
            entity.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<CatalogAuditLog>(entity =>
        {
            entity.ToTable("CatalogChanges", "Audit");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(20).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(200);
            entity.Property(x => x.OldValue).HasMaxLength(4000);
            entity.Property(x => x.NewValue).HasMaxLength(4000);
        });
    }
}
