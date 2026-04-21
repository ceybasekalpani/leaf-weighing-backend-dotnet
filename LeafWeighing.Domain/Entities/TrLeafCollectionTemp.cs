using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafWeighing.Domain.Entities;

[Table("Tr_LeafCollection_Temp", Schema = "dbo")]
public class TrLeafCollectionTemp
{
    [Key]
    [Column("Ind")]
    public int Ind { get; set; }

    [Column("RegNo")]
    public int? RegNo { get; set; }

    [Column("Dealer")]
    public string? Dealer { get; set; }

    [Column("Route")]
    public string? Route { get; set; }

    [Column("LeafType")]
    public string? LeafType { get; set; }

    [Column("Qty")]
    public int? Qty { get; set; }

    [Column("Gross")]
    public decimal? Gross { get; set; }

    [Column("BagWeight")]
    public decimal? BagWeight { get; set; }

    [Column("Water")]
    public decimal? Water { get; set; }

    [Column("Coarse")]
    public decimal? Coarse { get; set; }

    [Column("Rejected")]
    public decimal? Rejected { get; set; }

    [Column("Boild")]
    public decimal? Boild { get; set; }

    [Column("NetWeight")]
    public decimal? NetWeight { get; set; }

    [Column("Shift")]
    public string? Shift { get; set; }

    [Column("UserName")]
    public string? UserName { get; set; }

    [Column("Mode")]
    public string? Mode { get; set; }

    [Column("PC_Name")]
    public string? PcName { get; set; }

    [Column("IsDeduction")]
    public bool? IsDeduction { get; set; }

    [Column("MonthName")]
    public string? MonthName { get; set; }

    [Column("DayNo")]
    public int? DayNo { get; set; }

    [Column("LogTime")]
    public DateTime? LogTime { get; set; }

    // Additional deduction fields
    [Column("Spd")]
    public decimal? Spd { get; set; }

    [Column("RouteDeduct")]
    public decimal? RouteDeduct { get; set; }

    [Column("Excess_Leaf")]
    public decimal? ExcessLeaf { get; set; }

    [Column("Transfer")]
    public decimal? Transfer { get; set; }

    [Column("RouteDeductPre")]
    public decimal? RouteDeductPre { get; set; }
}