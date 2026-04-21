namespace LeafWeighing.Application.DTOs.LeafCount;

public class RouteTotalWeightDto
{
    public string? Route { get; set; }
    public int Date { get; set; }
    public string? Month { get; set; }
    public decimal TotalWeight { get; set; }
    public string Formula { get; set; } = "Gross - (Coarse + Water + BagWeight + Spd + Boiled + Rejected + RouteDeduct + ExcessLeaf + Transfer + RouteDeductPre)";
}