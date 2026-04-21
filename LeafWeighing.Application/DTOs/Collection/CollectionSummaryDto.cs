namespace LeafWeighing.Application.DTOs.Collection;

public class CollectionSummaryDto
{
    public int RegNo { get; set; }
    public string? SupplierName { get; set; }
    public string? Route { get; set; }
    public string Bags { get; set; } = "0";
    public string Gross { get; set; } = "0";
    public string TotalBagWeight { get; set; } = "0";
    public string TotalCoarce { get; set; } = "0";
    public string TotalWater { get; set; } = "0";
    public string TotalBoiled { get; set; } = "0";
    public string TotalRejected { get; set; } = "0";
    public string NetWeight { get; set; } = "0";
    public int TransactionCount { get; set; }
    public int CollectionCount { get; set; }
    public int DeductionCount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
}