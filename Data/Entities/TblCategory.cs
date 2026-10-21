using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblCategories")]
[Index("CategoryName", Name = "UQ_tblCategories_CategoryName", IsUnique = true)]
public partial class TblCategory
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    public string? CategoryDescription { get; set; }

    public bool CategoryIsActive { get; set; }

    public DateTime CategoryCreatedDate { get; set; }

    public DateTime? CategoryUpdatedDate { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<TblCategoryImage> TblCategoryImages { get; set; } = new List<TblCategoryImage>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Categories")]
    public virtual ICollection<TblBook> Books { get; set; } = new List<TblBook>();
}
