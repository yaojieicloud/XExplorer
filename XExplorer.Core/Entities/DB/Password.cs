using System.ComponentModel.DataAnnotations.Schema;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Modes;

[Table("Passwords")]
public class Password : ModeBase
{
    [Column("Password")]
    public string? Pwd { get; set; }
}