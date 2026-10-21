using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class UserVM : IValidatableObject
    {
        public int UserId { get; set; }

        [Display(Name = "İstifadəçinin Adı")]
        [Required(ErrorMessage = "İstifadəçinin adı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "İstifadəçinin adı maksimum 100 simvoldan ibarət ola bilər!")]
        public string UserName { get; set; } = null!;

        [Display(Name = "İstifadəçinin Soyadı")]
        [Required(ErrorMessage = "İstifadəçinin soyadı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "İstifadəçinin soyadı maksimum 100 simvoldan ibarət ola bilər!")]
        public string UserSurname { get; set; } = null!;

        // <input type="date"> DateOnly ilə işləyir (.NET 7+):
        [Display(Name = "Doğum Tarixi")]
        [Required(ErrorMessage = "Doğum tarixi daxil edilməlidir!")]
        [DataType(DataType.Date)]
        public DateOnly UserDateOfBirth { get; set; }

        // DB-də yalnız NULL, "Kişi" və ya "Qadın" ola bilər (CK_tblUsers_UserSex):
        [Display(Name = "Cins")]
        [RegularExpression("^(Kişi|Qadın)$", ErrorMessage = "Cins yalnız 'Kişi' və ya 'Qadın' ola bilər!")]
        public string? UserSex { get; set; }

        // Azərbaycanda FİN kod 7 simvoldan ibarətdir (rəqəm və latın hərfləri):
        [Display(Name = "FİN Kod")]
        [Required(ErrorMessage = "FİN kod daxil edilməlidir!")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "FİN kod 7 simvoldan ibarət olmalıdır!")]
        [RegularExpression("^[A-Za-z0-9]{7}$", ErrorMessage = "FİN kod yalnız latın hərflərindən və rəqəmlərdən ibarət ola bilər!")]
        public string UserPersonalNo { get; set; } = null!;

        [Display(Name = "E-poçt")]
        [Required(ErrorMessage = "E-poçt ünvanı daxil edilməlidir!")]
        [EmailAddress(ErrorMessage = "E-poçt ünvanı düzgün formatda deyil!")]
        [StringLength(255, ErrorMessage = "E-poçt ünvanı maksimum 255 simvoldan ibarət ola bilər!")]
        public string UserEmail { get; set; } = null!;

        // DB-də VARCHAR(20)-dir, ona görə yalnız latın rəqəmləri və sadə işarələrə icazə verilir:
        [Display(Name = "Telefon Nömrəsi")]
        [Required(ErrorMessage = "Telefon nömrəsi daxil edilməlidir!")]
        [StringLength(20)]
        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$")]
        public string UserPhoneNo { get; set; } = null!;

        [Display(Name = "Ünvan")]
        [Required(ErrorMessage = "Ünvan daxil edilməlidir!")]
        [StringLength(255)]
        public string UserAddress { get; set; } = null!;

        [Display(Name = "İstifadəçi Adı")]
        [Required(ErrorMessage = "İstifadəçi adı daxil edilməlidir!")]
        [StringLength(100, ErrorMessage = "İstifadəçi adı maksimum 100 simvoldan ibarət ola bilər!")]
        public string UserUsername { get; set; } = null!;

        // Parol VM-də hash şəklində saxlanılmır. Yalnız formdan gələn açıq parol üçündür və DB-yə YAZILMIR, controller-də hash-lənir.
        // [Required] YOXDUR: Create-də controller məcburi yoxlayır, Edit-də boş buraxılsa mövcud parol dəyişmir.
        [Display(Name = "Parol")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Parol minimum 8 simvoldan ibarət olmalıdır!")]
        public string? UserPassword { get; set; }

        [Display(Name = "Parolun Təkrarı")]
        [DataType(DataType.Password)]
        [Compare(nameof(UserPassword), ErrorMessage = "Parollar eyni deyil!")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "İstifadəçi Aktivdir")]
        public bool UserIsActive { get; set; }

        [Display(Name = "İstifadəçinin Qeydiyyat Tarixi")]
        public DateTime UserRegistrationDate { get; set; }

        [Display(Name = "İstifadəçinin Yenilənmə Tarixi")]
        public DateTime? UserUpdatedDate { get; set; }

        // Rol (FK). Dropdown üçün rolların siyahısı controller-də ViewBag.Roles ilə ötürülür:
        [Display(Name = "Rol")]
        [Range(1, int.MaxValue, ErrorMessage = "Rol seçilməlidir!")]
        public int RoleId { get; set; }

        // Index/Delete səhifələrində rolun adını göstərmək üçün:
        public string? RoleName { get; set; }

        // View-larda ad və soyadı birlikdə göstərmək üçün:
        public string UserFullName => $"{UserName} {UserSurname}";

        // Atributlarla yoxlanıla bilməyən qaydalar (atributlar keçəndən sonra işləyir):
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (UserDateOfBirth > today)
            {
                yield return new ValidationResult("Doğum tarixi gələcəkdə ola bilməz!", new[] { nameof(UserDateOfBirth) });
            }
            else if (UserDateOfBirth < today.AddYears(-120))
            {
                yield return new ValidationResult("Doğum tarixi düzgün deyil!", new[] { nameof(UserDateOfBirth) });
            }
        }
    }
}
