namespace LeafWeighing.Application.DTOs.Deduction;

public class SaveDeductionRequestDto
{
    public int RegNo { get; set; }
    public string? SupplierName { get; set; }
    public string? Route { get; set; }
    public string? LeafType { get; set; }
    public int BagWeight { get; set; }
    public int Coarse { get; set; }
    public int Water { get; set; }
    public int Boiled { get; set; }
    public int Rejected { get; set; }
    public string? UserName { get; set; }
    public string? Mode { get; set; }
    public string? PcName { get; set; }
    public string? Month { get; set; }
}
