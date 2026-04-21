using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafWeighing.Domain.Entities;

[Table("Reg_LeafCount", Schema = "dbo")]
public class RegLeafCount
{
    [Key]
    [Column("Ind")]
    public int Ind { get; set; }

    [Column("Date")]
    public int? Date { get; set; }

    [Column("Month")]
    public string? Month { get; set; }

    [Column("Route")]
    public string? Route { get; set; }

    [Column("BestLeaf")]
    public int? BestLeaf { get; set; }

    [Column("BelowBest")]
    public int? BelowBest { get; set; }

    [Column("Poor")]
    public int? Poor { get; set; }

    [Column("User")]
    public string? User { get; set; }

    [Column("LogTime")]
    public DateTime? LogTime { get; set; }

    [Column("PC_Name")]
    public string? PcName { get; set; }
}