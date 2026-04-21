namespace LeafWeighing.Application.DTOs.Deduction;

public class SaveDeductionRequestDto
{
    public int RegNo { get; set; }
    public string? SupplierName { get; set; }
    public string? Route { get; set; }
    public string? LeafType { get; set; }
    public decimal BagWeight { get; set; }
    public decimal Coarse { get; set; }
    public decimal Water { get; set; }
    public decimal Boiled { get; set; }
    public decimal Rejected { get; set; }
    public string? UserName { get; set; }
    public string? Mode { get; set; }
    public string? PcName { get; set; }
    public string? Month { get; set; }
}