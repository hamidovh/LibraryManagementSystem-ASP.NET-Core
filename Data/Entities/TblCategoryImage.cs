using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblCategoryImages")]
[Index("CategoryId", Name = "IX_tblCategoryImages_CategoryID")]
public partial class TblCategoryImage
{
    [Key]
    [Column("CategoryImageID")]
    public int CategoryImageId { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(500)]
    public string ImagePath { get; set; } = null!;

    public int ImageSortOrder { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("TblCategoryImages")]
    public virtual TblCategory Category { get; set; } = null!;
}
