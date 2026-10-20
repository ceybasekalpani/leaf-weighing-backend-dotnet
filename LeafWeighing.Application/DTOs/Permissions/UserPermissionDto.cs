namespace LeafWeighing.Application.DTOs.Permissions;

public class UserPermissionDto
{
    public int UserInd { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public bool Active { get; set; }
    public bool IsCanAddDeduction { get; set; }
    public bool IsCanAddCount { get; set; }
}

public class UpdateUserPermissionRequestDto
{
    public bool IsCanAddDeduction { get; set; } = true;
    public bool IsCanAddCount { get; set; } = true;
}
