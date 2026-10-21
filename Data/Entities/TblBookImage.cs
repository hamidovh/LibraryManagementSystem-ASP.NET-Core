using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblBookImages")]
[Index("BookId", Name = "IX_tblBookImages_BookID")]
public partial class TblBookImage
{
    [Key]
    [Column("BookImageID")]
    public int BookImageId { get; set; }

    [Column("BookID")]
    public int BookId { get; set; }

    [StringLength(500)]
    public string ImagePath { get; set; } = null!;

    public int ImageSortOrder { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("TblBookImages")]
    public virtual TblBook Book { get; set; } = null!;
}
