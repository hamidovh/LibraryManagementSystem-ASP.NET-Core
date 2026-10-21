using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblUsers")]
[Index("RoleId", Name = "IX_tblUsers_RoleID")]
[Index("UserEmail", Name = "UQ_tblUsers_UserEmail", IsUnique = true)]
[Index("UserPersonalNo", Name = "UQ_tblUsers_UserPersonalNo", IsUnique = true)]
[Index("UserPhoneNo", Name = "UQ_tblUsers_UserPhoneNo", IsUnique = true)]
[Index("UserUsername", Name = "UQ_tblUsers_UserUsername", IsUnique = true)]
public partial class TblUser
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    public string UserSurname { get; set; } = null!;

    public DateOnly UserDateOfBirth { get; set; }

    [StringLength(20)]
    public string? UserSex { get; set; }

    [StringLength(7)]
    [Unicode(false)]
    public string UserPersonalNo { get; set; } = null!;

    [StringLength(255)]
    public string UserEmail { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string UserPhoneNo { get; set; } = null!;

    [StringLength(255)]
    public string UserAddress { get; set; } = null!;

    [StringLength(100)]
    public string UserUsername { get; set; } = null!;

    [StringLength(255)]
    public string UserPasswordHash { get; set; } = null!;

    public bool UserIsActive { get; set; }

    public DateTime UserRegistrationDate { get; set; }

    public DateTime? UserUpdatedDate { get; set; }

    [Column("RoleID")]
    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("TblUsers")]
    public virtual TblRole Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<TblBorrow> TblBorrows { get; set; } = new List<TblBorrow>();
}
