using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeafWeighing.Domain.Entities;

[Table("UserPermissions", Schema = "dbo")]
public class UserPermission
{
    [Key]
    [Column("UserInd")]
    public int UserInd { get; set; }

    [Column("IsCanAddDeduction")]
    public bool IsCanAddDeduction { get; set; } = true;

    [Column("IsCanAddCount")]
    public bool IsCanAddCount { get; set; } = true;

    public UserSetup User { get; set; } = null!;
}
