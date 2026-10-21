/* ~/Models/ViewModels/BorrowVM.cs: */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class BorrowVM
    {
        public int BorrowId { get; set; }

        [Display(Name = "İstifadəçi")]
        [Range(1, int.MaxValue, ErrorMessage = "İstifadəçi seçilməlidir!")]
        public int UserId { get; set; }

        [Display(Name = "Kitab")]
        [Range(1, int.MaxValue, ErrorMessage = "Kitab seçilməlidir!")]
        public int BookId { get; set; }

        [Display(Name = "İcarə Tarixi")]
        public DateTime BorrowDate { get; set; }

        // <input type="date"> yalnız tarixi göndərir, controller bunu həmin günün sonuna (23:59:59) çevirir:
        [Display(Name = "Qaytarılma Müddəti")]
        [Required(ErrorMessage = "Qaytarılma müddəti daxil edilməlidir!")]
        [DataType(DataType.Date)]
        public DateTime BorrowDueDate { get; set; }

        [Display(Name = "Qaytarılma Tarixi")]
        public DateTime? BorrowReturnDate { get; set; }

        // Index/Edit/Delete/Return səhifələrində göstərmək üçün (DB-yə yazılmır, formdan gəlmir):
        public string? UserFullName { get; set; }

        public string? UserUsername { get; set; }

        public string? BookName { get; set; }

        public string? BookIsbn { get; set; }

        // Borrow-a bağlı cərimə (varsa):
        public bool HasFine { get; set; }

        public decimal? FineAmount { get; set; }

        // Return səhifəsində göstərmək üçün - gecikmə günü və hesablanmış cərimə:
        public int LateDays { get; set; }

        public decimal FinePreview { get; set; }

        // BorrowReturnDate IS NOT NULL → Qaytarılıb; IS NULL və vaxt keçməyibsə → Aktiv; IS NULL və vaxt keçibsə → Gecikir:
        public string BorrowStatus
        {
            get
            {
                if (BorrowReturnDate.HasValue)
                {
                    return "Qaytarılıb";
                }

                return DateTime.Now > BorrowDueDate ? "Gecikir" : "Aktiv";
            }
        }
    }
}
