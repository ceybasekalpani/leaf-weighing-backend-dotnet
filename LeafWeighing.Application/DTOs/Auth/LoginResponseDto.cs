namespace LeafWeighing.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public int Ind { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public bool? Admin { get; set; }
    public int? AdminLevel { get; set; }
    public bool? Active { get; set; }
    public bool? TempWorker { get; set; }
    public UserPermissionsDto Permissions { get; set; } = new();
}

public class UserPermissionsDto
{
    public bool? Confirm { get; set; }
    public bool? Report { get; set; }
    public bool? Transfer { get; set; }
    public bool? LeafEditDel { get; set; }
    public int? BackDays { get; set; }
}