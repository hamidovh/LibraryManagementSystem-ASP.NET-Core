/* ~/Models/ViewModels/RoleVM.cs: */

using LibraryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class RoleVM
    {
        public int RoleId { get; set; }

        [Display(Name = "Rolun Adı")]
        [Required(ErrorMessage = "Rolun adı daxil edilməlidir!")]
        [StringLength(50, ErrorMessage = "Rolun adı maksimum 50 simvoldan ibarət ola bilər!")]
        public string RoleName { get; set; } = null!;

        [Display(Name = "Rol Aktivdir")]
        public bool RoleIsActive { get; set; }

        [Display(Name = "Rolun Əlavə Edilmə Tarixi")]
        public DateTime RoleCreatedDate { get; set; }

        [Display(Name = "Rolun Yenilənmə Tarixi")]
        public DateTime? RoleUpdatedDate { get; set; }

        // Rola aid istifadəçilər:
        public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>(); // Bu collection (entity type) heç yerdə istifadə olunmur.

        // Index/Delete səhifələrində rola aid istifadəçi sayını göstərmək üçün:
        public int UserCount { get; set; }
    }
}
