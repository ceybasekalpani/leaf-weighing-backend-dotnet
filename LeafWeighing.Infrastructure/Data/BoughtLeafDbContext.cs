using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Infrastructure.Data;

public class BoughtLeafDbContext : DbContext
{
    public BoughtLeafDbContext(DbContextOptions<BoughtLeafDbContext> options)
        : base(options)
    {
    }

    public DbSet<TrLeafCollectionTemp> TrLeafCollectionTemp { get; set; }
    public DbSet<RegLeafCount> RegLeafCount { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Water, Coarse, Rejected: DB column is INT but entity declares decimal?
        // Without converter EF Core calls GetDecimal() on an int column → crash
        var intDbToDecimal = new ValueConverter<decimal?, int?>(
            v => v.HasValue ? (int?)v.Value : null,
            v => v.HasValue ? (decimal?)v.Value : null
        );

        // Spd, RouteDeduct, ExcessLeaf, Transfer, RouteDeductPre: DB column is MONEY but entity declares int?
        // Without converter EF Core calls GetInt32() on a money column → crash
        var moneyDbToInt = new ValueConverter<int?, decimal?>(
            v => v.HasValue ? (decimal?)v.Value : null,
            v => v.HasValue ? (int?)v.Value : null
        );

        modelBuilder.Entity<TrLeafCollectionTemp>(entity =>
        {
            entity.ToTable("Tr_LeafCollection_Temp", "dbo");
            entity.HasKey(e => e.Ind);
            entity.Property(e => e.Ind).HasColumnName("Ind").ValueGeneratedOnAdd();
            entity.Property(e => e.RegNo).HasColumnName("RegNo");
            entity.Property(e => e.Dealer).HasColumnName("Dealer");
            entity.Property(e => e.Route).HasColumnName("Route");
            entity.Property(e => e.LeafType).HasColumnName("LeafType");
            entity.Property(e => e.Qty).HasColumnName("Qty");

            // DB type: money → entity decimal? — no converter needed, money maps to decimal
            entity.Property(e => e.Gross).HasColumnName("Gross");
            entity.Property(e => e.BagWeight).HasColumnName("BagWeight");
            entity.Property(e => e.Boild).HasColumnName("Boild");
            entity.Property(e => e.NetWeight).HasColumnName("NetWeight");

            // DB type: int → entity decimal? — converter reads int, returns decimal?
            entity.Property(e => e.Water).HasColumnName("Water").HasConversion(intDbToDecimal);
            entity.Property(e => e.Coarse).HasColumnName("Coarse").HasConversion(intDbToDecimal);
            entity.Property(e => e.Rejected).HasColumnName("Rejected").HasConversion(intDbToDecimal);

            entity.Property(e => e.Shift).HasColumnName("Shift");
            entity.Property(e => e.UserName).HasColumnName("UserName");
            entity.Property(e => e.Mode).HasColumnName("Mode");
            entity.Property(e => e.PcName).HasColumnName("PC_Name");
            entity.Property(e => e.IsDeduction).HasColumnName("IsDeduction");
            entity.Property(e => e.MonthName).HasColumnName("MonthName");
            entity.Property(e => e.DayNo).HasColumnName("DayNo");
            entity.Property(e => e.LogTime).HasColumnName("LogTime");

            // DB type: money → entity int? — converter reads money (decimal), returns int?
            entity.Property(e => e.Spd).HasColumnName("Spd").HasConversion(moneyDbToInt);
            entity.Property(e => e.RouteDeduct).HasColumnName("RouteDeduct").HasConversion(moneyDbToInt);
            entity.Property(e => e.ExcessLeaf).HasColumnName("Excess_Leaf").HasConversion(moneyDbToInt);
            entity.Property(e => e.Transfer).HasColumnName("Transfer").HasConversion(moneyDbToInt);
            entity.Property(e => e.RouteDeductPre).HasColumnName("RouteDeductPre").HasConversion(moneyDbToInt);
        });

        modelBuilder.Entity<RegLeafCount>(entity =>
        {
            entity.ToTable("Reg_LeafCount", "dbo");
            entity.HasKey(e => e.Ind);
            entity.Property(e => e.Ind).HasColumnName("Ind");
            entity.Property(e => e.Date).HasColumnName("Date");
            entity.Property(e => e.Month).HasColumnName("Month");
            entity.Property(e => e.Route).HasColumnName("Route");
            entity.Property(e => e.BestLeaf).HasColumnName("BestLeaf");
            entity.Property(e => e.BelowBest).HasColumnName("BelowBest");
            entity.Property(e => e.Poor).HasColumnName("Poor");
            entity.Property(e => e.User).HasColumnName("User");
            entity.Property(e => e.LogTime).HasColumnName("LogTime");
            entity.Property(e => e.PcName).HasColumnName("PC_Name");
        });
    }
}
