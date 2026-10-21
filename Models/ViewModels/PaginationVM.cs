/* ~/Models/ViewModels/PaginationVM.cs: */

namespace LibraryManagementSystem.Models.ViewModels
{
    public class PaginationVM
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        // Verilən səhifə nömrəsi üçün tam URL qaytaran funksiya:
        public Func<int, string> PageUrl { get; set; } = _ => "#";
    }
}
