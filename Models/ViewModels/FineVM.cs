/* ~/Models/ViewModels/FineVM.cs: */

using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class FineVM
    {
        public int FineId { get; set; }

        [Display(Name = "İcarə")]
        [Range(1, int.MaxValue, ErrorMessage = "İcarə seçilməlidir!")]
        public int BorrowId { get; set; }

        // DB-də yalnız "Gecikir" və ya "İtirilib" ola bilər (CK_tblFines_FineReason):
        [Display(Name = "Cərimənin Səbəbi")]
        [Required(ErrorMessage = "Cərimənin səbəbi seçilməlidir!")]
        public string FineReason { get; set; } = null!;

        [Display(Name = "Cərimənin Yaranma Tarixi")]
        public DateTime FineDate { get; set; }

        // Range(double, double) istifadə olunur ki, limitlər string kimi culture ilə parse olunmasın:
        [Display(Name = "Cərimə Məbləği")]
        [Range(0.01, 99999.99, ErrorMessage = "Cərimə məbləği 0.01 ilə 99999.99 arasında olmalıdır!")]
        public decimal FineAmount { get; set; }

        [Display(Name = "Cərimə Ödənilib")]
        public bool FineIsPaid { get; set; }

        [Display(Name = "Ödəniş Tarixi")]
        public DateTime? FinePaymentDate { get; set; }

        // Index/Edit/Pay/Delete səhifələrində göstərmək üçün (DB-yə yazılmır, formdan gəlmir):
        public string? UserFullName { get; set; }

        public string? UserUsername { get; set; }

        public string? BookName { get; set; }

        public string? BookIsbn { get; set; }

        public DateTime BorrowDate { get; set; }

        public DateTime BorrowDueDate { get; set; }

        public DateTime? BorrowReturnDate { get; set; }

        // View-larda status göstərmək üçün:
        public string FineStatus => FineIsPaid ? "Ödənilib" : "Ödənilməyib";

        // Cərimənin səbəbi itirilmə olub-olmadığını göstərir:
        public bool IsLost => FineReason == "İtirilib";
    }
}
