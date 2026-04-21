namespace LeafWeighing.Application.DTOs.Deduction;

public class DeductionSummaryDto
{
    public decimal TotalBags { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalBagWeight { get; set; }
    public decimal TotalCoarse { get; set; }
    public decimal TotalWater { get; set; }
    public decimal TotalBoiled { get; set; }
    public decimal TotalRejected { get; set; }
    public decimal TotalNetWeight { get; set; }
    public int TransactionCount { get; set; }
}