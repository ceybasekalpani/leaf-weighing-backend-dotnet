using System;

namespace LeafWeighing.Application.DTOs.Deduction;

public class TransactionDto
{
    public int Ind { get; set; }
    public int? RegNo { get; set; }
    public string? SupplierName { get; set; }
    public string? Route { get; set; }
    public string? LeafType { get; set; }
    public int? Bags { get; set; }
    public decimal? Gross { get; set; }
    public decimal? BagWeight { get; set; }
    public decimal? Water { get; set; }
    public decimal? Coarce { get; set; }
    public decimal? Rejected { get; set; }
    public decimal? Boiled { get; set; }
    public decimal? NetWeight { get; set; }
    public string? Shift { get; set; }
    public string? UserName { get; set; }
    public DateTime? LogTime { get; set; }
}