using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafWeighing.Domain.Entities;

[Table("UserSetup", Schema = "dbo")]
public class UserSetup
{
    [Key]
    [Column("Ind")]
    public int Ind { get; set; }

    [Column("FullName")]
    public string? FullName { get; set; }

    [Column("UserName")]
    public string? UserName { get; set; }

    [Column("Password")]
    public string? Password { get; set; }

    [Column("Admin")]
    public bool? Admin { get; set; }

    [Column("AdminLevel")]
    public int? AdminLevel { get; set; }

    [Column("Active")]
    public bool? Active { get; set; }

    [Column("TempWorker")]
    public bool? TempWorker { get; set; }

    [Column("Bl_Confirm")]
    public bool? BlConfirm { get; set; }

    [Column("Bl_Report")]
    public bool? BlReport { get; set; }

    [Column("Bl_Transfer")]
    public bool? BlTransfer { get; set; }

    [Column("Bl_LeafEditDel")]
    public bool? BlLeafEditDel { get; set; }

    [Column("BackDays")]
    public int? BackDays { get; set; }
}