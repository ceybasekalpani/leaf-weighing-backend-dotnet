namespace LeafWeighing.Application.DTOs.LeafCount;

public class LeafCountRequestDto
{
    public int Date { get; set; }
    public string? Month { get; set; }
    public string? Route { get; set; }
    public int BestLeaf { get; set; }
    public int BellowBest { get; set; }
    public int Poor { get; set; }
    public string? UserName { get; set; }
}