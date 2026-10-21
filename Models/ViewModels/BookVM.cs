/* ~/Models/ViewModels/BookVM.cs: */

using LibraryManagementSystem.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class BookVM
    {
        public int BookId { get; set; }

        [Display(Name = "Kitabın Adı")]
        [Required(ErrorMessage = "Kitabın adı daxil edilməlidir!")]
        [StringLength(255, ErrorMessage = "Kitabın adı maksimum 255 simvoldan ibarət ola bilər!")]
        public string BookName { get; set; } = null!;

        [Display(Name = "ISBN")]
        [Required(ErrorMessage = "ISBN daxil edilməlidir!")]
        [StringLength(17, ErrorMessage = "ISBN maksimum 17 simvoldan ibarət ola bilər!")]
        public string BookIsbn { get; set; } = null!;

        [Display(Name = "Kitab haqqında")]
        public string? BookAbout { get; set; }

        [Display(Name = "Stok miqdarı")]
        [Required(ErrorMessage = "Stok miqdarı daxil edilməlidir!")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miqdarı 0 və ya daha böyük olmalıdır!")]        
        public int BookStockQuantity { get; set; }

        [Display(Name = "Cərimə məbləği")]
        [Required(ErrorMessage = "Cərimə məbləği daxil edilməlidir!")]
        [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Cərimə məbləği 0 ilə 999.99 arasında olmalıdır!")]        
        public decimal BookFineAmount { get; set; }

        [Display(Name = "Kitab aktivdir")]
        public bool BookIsActive { get; set; }

        [Display(Name = "Kitabın Əlavə Edilmə Tarixi")]
        public DateTime BookCreatedDate { get; set; }

        [Display(Name = "Kitabın Yenilənmə Tarixi")]
        public DateTime? BookUpdatedDate { get; set; }

        // Mövcud kitab şəkilləri:
        public virtual ICollection<TblBookImage> TblBookImages { get; set; } = new List<TblBookImage>();

        // Kitaba aid müəlliflər:
        public virtual ICollection<TblAuthor> Authors { get; set; } = new List<TblAuthor>();

        // Kitaba aid kateqoriyalar:
        public virtual ICollection<TblCategory> Categories { get; set; } = new List<TblCategory>();

        // Yeni kitab şəkli yükləmək üçün:
        [Display(Name = "Kitabın Şəkli")]
        public IFormFile? ImageFile { get; set; }

        // Şəkil silmə property-si:
        public bool DeleteImage { get; set; }

        // Create/Edit zamanı seçilən müəllif ID-ləri:
        [Display(Name = "Müəlliflər")]
        public List<int> SelectedAuthorIds { get; set; } = new List<int>();

        // Create/Edit zamanı seçilən kateqoriya ID-ləri:
        [Display(Name = "Kateqoriyalar")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}
