using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblBooks")]
[Index("BookIsbn", Name = "UQ_tblBooks_BookISBN", IsUnique = true)]
public partial class TblBook
{
    [Key]
    [Column("BookID")]
    public int BookId { get; set; }

    [StringLength(255)]
    public string BookName { get; set; } = null!;

    [Column("BookISBN")]
    [StringLength(17)]
    [Unicode(false)]
    public string BookIsbn { get; set; } = null!;

    public string? BookAbout { get; set; }

    public int BookStockQuantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BookFineAmount { get; set; }

    public bool BookIsActive { get; set; }

    public DateTime BookCreatedDate { get; set; }

    public DateTime? BookUpdatedDate { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<TblBookImage> TblBookImages { get; set; } = new List<TblBookImage>();

    [InverseProperty("Book")]
    public virtual ICollection<TblBorrow> TblBorrows { get; set; } = new List<TblBorrow>();

    [ForeignKey("BookId")]
    [InverseProperty("Books")]
    public virtual ICollection<TblAuthor> Authors { get; set; } = new List<TblAuthor>();

    [ForeignKey("BookId")]
    [InverseProperty("Books")]
    public virtual ICollection<TblCategory> Categories { get; set; } = new List<TblCategory>();
}
