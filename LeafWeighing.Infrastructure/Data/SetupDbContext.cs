using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Infrastructure.Data;

public class SetupDbContext : DbContext
{
    public SetupDbContext(DbContextOptions<SetupDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserSetup> UserSetup { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserSetup>(entity =>
        {
            entity.ToTable("UserSetup", "dbo");
            entity.HasKey(e => e.Ind);
            entity.Property(e => e.Ind).HasColumnName("Ind");
            entity.Property(e => e.FullName).HasColumnName("FullName");
            entity.Property(e => e.UserName).HasColumnName("UserName");
            entity.Property(e => e.Password).HasColumnName("Password");
            entity.Property(e => e.Admin).HasColumnName("Admin");
            entity.Property(e => e.AdminLevel).HasColumnName("AdminLevel");
            entity.Property(e => e.Active).HasColumnName("IsActive");
            entity.Property(e => e.TempWorker).HasColumnName("TempWorker");
            entity.Property(e => e.BlConfirm).HasColumnName("Bl_Confirm");
            entity.Property(e => e.BlReport).HasColumnName("Bl_Report");
            entity.Property(e => e.BlTransfer).HasColumnName("Bl_Transfer");
            entity.Property(e => e.BlLeafEditDel).HasColumnName("Bl_LeafEditDel");
            entity.Property(e => e.BackDays).HasColumnName("BackDays");
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions", "dbo");
            entity.HasKey(e => e.UserInd);
            entity.Property(e => e.UserInd).HasColumnName("UserInd");
            entity.Property(e => e.IsCanAddDeduction)
                .HasColumnName("IsCanAddDeduction")
                .HasDefaultValue(true);
            entity.Property(e => e.IsCanAddCount)
                .HasColumnName("IsCanAddCount")
                .HasDefaultValue(true);
            entity.HasOne(e => e.User)
                .WithOne(e => e.AppPermissions)
                .HasForeignKey<UserPermission>(e => e.UserInd)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
