using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblRoles")]
[Index("RoleName", Name = "UQ_tblRoles_RoleName", IsUnique = true)]
public partial class TblRole
{
    [Key]
    [Column("RoleID")]
    public int RoleId { get; set; }

    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    public bool RoleIsActive { get; set; }

    public DateTime RoleCreatedDate { get; set; }

    public DateTime? RoleUpdatedDate { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
