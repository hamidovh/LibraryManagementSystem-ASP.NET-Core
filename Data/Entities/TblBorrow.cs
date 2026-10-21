using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblBorrows")]
[Index("UserId", "BorrowDate", Name = "IX_tblBorrows_UserID_BorrowDate", IsDescending = new[] { false, true })]
public partial class TblBorrow
{
    [Key]
    [Column("BorrowID")]
    public int BorrowId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("BookID")]
    public int BookId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime BorrowDueDate { get; set; }

    public DateTime? BorrowReturnDate { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("TblBorrows")]
    public virtual TblBook Book { get; set; } = null!;

    [InverseProperty("Borrow")]
    public virtual TblFine? TblFine { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TblBorrows")]
    public virtual TblUser User { get; set; } = null!;
}
