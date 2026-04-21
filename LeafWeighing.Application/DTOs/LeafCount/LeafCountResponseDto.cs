using System;

namespace LeafWeighing.Application.DTOs.LeafCount;

public class LeafCountResponseDto
{
    public int Ind { get; set; }
    public int? Date { get; set; }
    public string? Month { get; set; }
    public string? Route { get; set; }
    public int? BestLeaf { get; set; }
    public int? BellowBest { get; set; }
    public int? Poor { get; set; }
    public string? User { get; set; }
    public DateTime? LogTime { get; set; }
    public string? PcName { get; set; }
}