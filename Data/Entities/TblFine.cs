using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblFines")]
[Index("BorrowId", Name = "UQ_tblFines_BorrowID", IsUnique = true)]
public partial class TblFine
{
    [Key]
    [Column("FineID")]
    public int FineId { get; set; }

    [Column("BorrowID")]
    public int BorrowId { get; set; }

    [StringLength(20)]
    public string FineReason { get; set; } = null!;

    public DateTime FineDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FineAmount { get; set; }

    public bool FineIsPaid { get; set; }

    public DateTime? FinePaymentDate { get; set; }

    [ForeignKey("BorrowId")]
    [InverseProperty("TblFine")]
    public virtual TblBorrow Borrow { get; set; } = null!;
}
