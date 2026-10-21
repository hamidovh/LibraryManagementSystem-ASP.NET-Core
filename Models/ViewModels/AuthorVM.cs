/* ~/Models/ViewModels/AuthorVM.cs: */

using LibraryManagementSystem.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class AuthorVM
    {
        public int AuthorId { get; set; }

        [Display(Name = "Müəllifin Adı")]
        [Required(ErrorMessage = "Müəllifin adı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "Müəllifin adı maksimum 100 simvoldan ibarət ola bilər!")]
        public string AuthorName { get; set; } = null!;

        [Display(Name = "Müəllifin Soyadı")]
        [Required(ErrorMessage = "Müəllifin soyadı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "Müəllifin soyadı maksimum 100 simvoldan ibarət ola bilər!")]
        public string AuthorSurname { get; set; } = null!;

        [Display(Name = "Müəllif Haqqında")]
        public string? AuthorInfo { get; set; }

        [Display(Name = "Müəllif Aktivdir")]
        public bool AuthorIsActive { get; set; }

        [Display(Name = "Müəllifin Əlavə Edilmə Tarixi")]
        public DateTime AuthorCreatedDate { get; set; }

        [Display(Name = "Müəllifin Yenilənmə Tarixi")]
        public DateTime? AuthorUpdatedDate { get; set; }

        // Mövcud müəllif şəkilləri:
        public virtual ICollection<TblAuthorImage> TblAuthorImages { get; set; } = new List<TblAuthorImage>();

        // Müəllifə aid kitablar:
        public virtual ICollection<TblBook> Books { get; set; } = new List<TblBook>(); // Bu collection heç yerdə istifadə olunmur.

        // Yeni şəkil yükləmək üçün:
        [Display(Name = "Müəllifin Şəkli")]
        public IFormFile? ImageFile { get; set; }

        // Şəkil silmə property-si:
        public bool DeleteImage { get; set; }

        // View-larda ad və soyadı birlikdə göstərmək üçün:
        public string AuthorFullName => $"{AuthorName} {AuthorSurname}";
    }
}
