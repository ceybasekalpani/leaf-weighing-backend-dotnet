namespace LeafWeighing.Application.Security;

public static class PermissionClaimTypes
{
    public const string IsAdmin = "is_admin";
    public const string CanAddDeduction = "permission:add_deduction";
    public const string CanAddCount = "permission:add_count";
}

public static class PermissionPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CanAddDeduction = "CanAddDeduction";
    public const string CanAddCount = "CanAddCount";
}
