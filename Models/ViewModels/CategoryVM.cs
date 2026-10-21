/* ~/Models/ViewModels/CategoryVM.cs: */

using LibraryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class CategoryVM
    {
        public int CategoryId { get; set; }

        [Display(Name = "Kateqoriyanın Adı")]
        [Required(ErrorMessage = "Kateqoriya adı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "Kateqoriyanın adı maksimum 100 simvoldan ibarət ola bilər!")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Kateqoriyanın Təsviri")]
        public string? CategoryDescription { get; set; }

        [Display(Name = "Kateqoriya Aktivdir")]
        public bool CategoryIsActive { get; set; }

        [Display(Name = "Kateqoriyanın Əlavə Edilmə Tarixi")]
        public DateTime CategoryCreatedDate { get; set; }

        [Display(Name = "Kateqoriyanın Yenilənmə Tarixi")]
        public DateTime? CategoryUpdatedDate { get; set; }

        // Mövcud kateqoriya şəkilləri:
        public virtual ICollection<TblCategoryImage> TblCategoryImages { get; set; } = new List<TblCategoryImage>();

        // Kateqoriyaya aid kitablar:
        public virtual ICollection<TblBook> Books { get; set; } = new List<TblBook>(); // Bu collection heç yerdə istifadə olunmur.

        // Yeni şəkil yükləmək üçün:
        [Display(Name = "Kateqoriyanın Şəkli")]
        public IFormFile? ImageFile { get; set; }

        // Şəkil silmə property-si:
        public bool DeleteImage { get; set; }
    }
}

// [NotMapped] Entity Framework Core-a deyir ki, bu property-ni database-də sütun kimi yaratma və database-dən map etmə (bu property C# proqramında lazımdır, amma database-də column olmasın). [NotMapped] propertylər yalnız Form üçün lazım olan məlumatlardır. Mapped - xəritələnmiş / database ilə əlaqələndirilmiş, NotMapped - database ilə əlaqələndirmə deməkdir.

/*
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
*/
