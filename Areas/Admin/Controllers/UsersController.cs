/* ~/Areas/Admin/Controllers/UsersController.cs: */

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    //[Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly LMSdbContext _context;
        private readonly IPasswordHasher<TblUser> _passwordHasher;

        // Parolu hash edən sinif (əlavə paket tələb etmir, hər hash-ə özü unikal "salt" əlavə edir):
        //private readonly PasswordHasher<TblUser> _passwordHasher = new PasswordHasher<TblUser>();

        public UsersController(LMSdbContext context, IPasswordHasher<TblUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: Admin/Users/UserIndex
        [HttpGet]
        public async Task<IActionResult> UserIndex(string sortOrder = "date_desc", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum istifadəçi sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            var users = _context.TblUsers.AsNoTracking().AsQueryable();

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "name_asc":
                    users = users.OrderBy(u => u.UserName).ThenBy(u => u.UserSurname).ThenBy(u => u.UserId);
                    break;
                case "name_desc":
                    users = users.OrderByDescending(u => u.UserName).ThenByDescending(u => u.UserSurname).ThenByDescending(u => u.UserId);
                    break;
                case "date_asc":
                    users = users.OrderBy(u => u.UserRegistrationDate).ThenBy(u => u.UserId);
                    break;
                default:
                    users = users.OrderByDescending(u => u.UserRegistrationDate).ThenByDescending(u => u.UserId);
                    sortOrder = "date_desc";
                    break;
            }

            // Bütün istifadəçilərin ümumi sayı və ümumi səhifə sayı:
            int totalUsers = await users.CountAsync();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid istifadəçilər ViewModelə çevrilir (UserPasswordHash heç vaxt VM-ə köçürülmür):
            var userList = await users.Skip((page - 1) * pageSize).Take(pageSize).Select(u => new UserVM
            {
                UserId = u.UserId,
                UserName = u.UserName,
                UserSurname = u.UserSurname,
                UserDateOfBirth = u.UserDateOfBirth,
                UserSex = u.UserSex,
                UserPersonalNo = u.UserPersonalNo,
                UserEmail = u.UserEmail,
                UserPhoneNo = u.UserPhoneNo,
                UserAddress = u.UserAddress,
                UserUsername = u.UserUsername,
                UserIsActive = u.UserIsActive,
                UserRegistrationDate = u.UserRegistrationDate,
                UserUpdatedDate = u.UserUpdatedDate,
                RoleId = u.RoleId,
                RoleName = u.Role.RoleName
            }).ToListAsync();

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;

            return View(userList);
        }

        // Create/Edit səhifələrində rolların siyahısı doldurulur (yüklənir) (keepRoleId: deaktiv olsa belə siyahıda qalmalı olan (istifadəçinin hazırkı) rolun ID-si):
        private async Task LoadRoles(int? keepRoleId = null)
        {
            ViewBag.Roles = await _context.TblRoles.Where(r => r.RoleIsActive || r.RoleId == keepRoleId).OrderBy(r => r.RoleName).ToListAsync();
        }

        // Formdan gələn mətn sahələri təmizlənir:
        private static void NormalizeModel(UserVM model)
        {
            model.UserName = model.UserName.Trim(); // Boşluqları yığışdırır.
            model.UserSurname = model.UserSurname.Trim();
            model.UserPersonalNo = model.UserPersonalNo.Trim().ToUpperInvariant(); // FİN kod böyük hərflərlə saxlanılır.
            model.UserEmail = model.UserEmail.Trim();
            model.UserPhoneNo = model.UserPhoneNo.Trim();
            model.UserAddress = model.UserAddress.Trim();
            model.UserUsername = model.UserUsername.Trim();
            model.UserSex = string.IsNullOrWhiteSpace(model.UserSex) ? null : model.UserSex.Trim();
            // Parol Trim olunmur (boşluqlar parolun hissəsi ola bilər).
        }

        // Rolun seçilə bilən olması və 4 UNIQUE sahənin başqa istifadəçiyə aid olmaması yoxlanılır (xətalar ModelState-ə əlavə olunur).
        // ignoreUserId: Edit zamanı istifadəçinin öz qeydi yoxlamadan çıxarılır (Create-də 0, çünki ID-lər 1-dən başlayır).
        // currentRoleId: Edit zamanı istifadəçinin hazırkı rolu deaktiv olsa belə seçilə bilər.
        private async Task ValidateUserFields(UserVM model, int ignoreUserId = 0, int? currentRoleId = null)
        {
            bool isEdit = ignoreUserId != 0;

            bool roleOk = await _context.TblRoles.AnyAsync(r => r.RoleId == model.RoleId && (r.RoleIsActive || r.RoleId == currentRoleId));
            if (!roleOk)
            {
                ModelState.AddModelError(nameof(model.RoleId), "Seçilmiş rol mövcud deyil və ya deaktivdir!");
            }

            if (await _context.TblUsers.AnyAsync(u => u.UserPersonalNo == model.UserPersonalNo && u.UserId != ignoreUserId))
            {
                ModelState.AddModelError(nameof(model.UserPersonalNo), isEdit ? "Bu FİN kod başqa istifadəçiyə aiddir!" : "Bu FİN kodla istifadəçi artıq mövcuddur!");
            }

            if (await _context.TblUsers.AnyAsync(u => u.UserEmail == model.UserEmail && u.UserId != ignoreUserId))
            {
                ModelState.AddModelError(nameof(model.UserEmail), isEdit ? "Bu e-poçt ünvanı başqa istifadəçiyə aiddir!" : "Bu e-poçt ünvanı ilə istifadəçi artıq mövcuddur!");
            }

            if (await _context.TblUsers.AnyAsync(u => u.UserPhoneNo == model.UserPhoneNo && u.UserId != ignoreUserId))
            {
                ModelState.AddModelError(nameof(model.UserPhoneNo), isEdit ? "Bu telefon nömrəsi başqa istifadəçiyə aiddir!" : "Bu telefon nömrəsi ilə istifadəçi artıq mövcuddur!");
            }

            if (await _context.TblUsers.AnyAsync(u => u.UserUsername == model.UserUsername && u.UserId != ignoreUserId))
            {
                ModelState.AddModelError(nameof(model.UserUsername), isEdit ? "Bu istifadəçi adı başqa istifadəçiyə aiddir!" : "Bu istifadəçi adı artıq istifadə olunur!");
            }
        }

        // GET: Admin/Users/UserCreate
        [HttpGet]
        public async Task<IActionResult> UserCreate()
        {
            var model = new UserVM
            {
                UserIsActive = true
            };

            // Viewda göstərmək üçün rolların siyahısı gətirilir:
            await LoadRoles();
            return View(model);
        }

        // POST: Admin/Users/UserCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCreate(UserVM model)
        {
            // UserVM-də UserPassword üçün [Required] yoxdur (Edit-də boş buraxıla bilsin deyə), buna görə Create-də parol burada məcburi edilir:
            if (string.IsNullOrWhiteSpace(model.UserPassword))
            {
                ModelState.AddModelError(nameof(model.UserPassword), "Parol daxil edilməlidir!");
            }

            if (!ModelState.IsValid)
            {
                await LoadRoles();
                return View(model);
            }

            NormalizeModel(model);

            await ValidateUserFields(model);

            if (!ModelState.IsValid)
            {
                await LoadRoles();
                return View(model);
            }

            // Yeni TblUser entity-si yaradılır:
            var user = new TblUser
            {
                UserName = model.UserName,
                UserSurname = model.UserSurname,
                UserDateOfBirth = model.UserDateOfBirth,
                UserSex = model.UserSex,
                UserPersonalNo = model.UserPersonalNo,
                UserEmail = model.UserEmail,
                UserPhoneNo = model.UserPhoneNo,
                UserAddress = model.UserAddress,
                UserUsername = model.UserUsername,
                UserIsActive = model.UserIsActive,
                RoleId = model.RoleId
            };

            // Açıq parol DB-yə yazılmır, yalnız hash saxlanılır:
            user.UserPasswordHash = _passwordHasher.HashPassword(user, model.UserPassword!);

            // İstifadəçi DB-ə əlavə olunur:
            _context.TblUsers.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "İstifadəçini əlavə etmək mümkün olmadı! FİN kod, e-poçt, telefon nömrəsi və ya istifadəçi adı artıq mövcud ola bilər.");
                await LoadRoles();
                return View(model);
            }

            TempData["Success"] = "İstifadəçi uğurla əlavə edildi!";
            return RedirectToAction(nameof(UserIndex));
        }

        // GET: Admin/Users/UserEdit/5
        [HttpGet]
        public async Task<IActionResult> UserEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.TblUsers.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // UserPasswordHash VM-ə köçürülmür, parol sahələri boş qalır:
            var model = new UserVM
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserSurname = user.UserSurname,
                UserDateOfBirth = user.UserDateOfBirth,
                UserSex = user.UserSex,
                UserPersonalNo = user.UserPersonalNo,
                UserEmail = user.UserEmail,
                UserPhoneNo = user.UserPhoneNo,
                UserAddress = user.UserAddress,
                UserUsername = user.UserUsername,
                UserIsActive = user.UserIsActive,
                UserRegistrationDate = user.UserRegistrationDate,
                UserUpdatedDate = user.UserUpdatedDate,
                RoleId = user.RoleId
            };

            // İstifadəçinin hazırkı rolu deaktiv olsa belə siyahıda qalır:
            await LoadRoles(user.RoleId);
            return View(model);
        }

        // İstifadəçinin mövcud olub-olmadığını yoxlayan metod:
        private bool UserExists(int id)
        {
            return _context.TblUsers.Any(u => u.UserId == id);
        }

        // POST: Admin/Users/UserEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(int id, UserVM model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Formu yenidən göstərmək üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
            async Task<IActionResult> Redisplay()
            {
                model.UserRegistrationDate = user.UserRegistrationDate;
                model.UserUpdatedDate = user.UserUpdatedDate;
                await LoadRoles(user.RoleId);
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return await Redisplay();
            }

            NormalizeModel(model);

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir. Ordinal müqayisə böyük-kiçik hərf dəyişikliyinin də dəyişiklik hesab edilməsi üçündür. Parol sahəsi boşdursa parol dəyişdirilmir:
            bool isNotChanged = string.Equals(user.UserName, model.UserName, StringComparison.Ordinal) && string.Equals(user.UserSurname, model.UserSurname, StringComparison.Ordinal) && user.UserDateOfBirth == model.UserDateOfBirth && string.Equals(user.UserSex ?? string.Empty, model.UserSex ?? string.Empty, StringComparison.Ordinal) && string.Equals(user.UserPersonalNo, model.UserPersonalNo, StringComparison.Ordinal) && string.Equals(user.UserEmail, model.UserEmail, StringComparison.Ordinal) && string.Equals(user.UserPhoneNo, model.UserPhoneNo, StringComparison.Ordinal) && string.Equals(user.UserAddress, model.UserAddress, StringComparison.Ordinal) && string.Equals(user.UserUsername, model.UserUsername, StringComparison.Ordinal) && user.UserIsActive == model.UserIsActive && user.RoleId == model.RoleId && string.IsNullOrWhiteSpace(model.UserPassword);

            if (isNotChanged)
            {
                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return await Redisplay();
            }

            // Rol və 4 UNIQUE sahə yoxlanılır (istifadəçinin öz qeydi çıxarılır):
            await ValidateUserFields(model, id, user.RoleId);

            if (!ModelState.IsValid)
            {
                return await Redisplay();
            }

            // Əsas istifadəçi məlumatları yenilənir:
            user.UserName = model.UserName;
            user.UserSurname = model.UserSurname;
            user.UserDateOfBirth = model.UserDateOfBirth;
            user.UserSex = model.UserSex;
            user.UserPersonalNo = model.UserPersonalNo;
            user.UserEmail = model.UserEmail;
            user.UserPhoneNo = model.UserPhoneNo;
            user.UserAddress = model.UserAddress;
            user.UserUsername = model.UserUsername;
            user.UserIsActive = model.UserIsActive;
            user.RoleId = model.RoleId;
            user.UserUpdatedDate = DateTime.Now;

            // Parol yalnız yeni parol daxil edilibsə yenilənir (boşdursa UserPasswordHash-ə toxunulmur):
            if (!string.IsNullOrWhiteSpace(model.UserPassword))
            {
                user.UserPasswordHash = _passwordHasher.HashPassword(user, model.UserPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    TempData["Error"] = "Bu istifadəçi artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(UserIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı! FİN kod, e-poçt, telefon nömrəsi və ya istifadəçi adı artıq başqa istifadəçiyə aid ola bilər, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(UserEdit), new { id });
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(UserIndex));
        }

        // GET: Admin/Users/UserDelete/5
        [HttpGet]
        public async Task<IActionResult> UserDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.TblUsers.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserVM
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserSurname = user.UserSurname,
                UserDateOfBirth = user.UserDateOfBirth,
                UserSex = user.UserSex,
                UserPersonalNo = user.UserPersonalNo,
                UserEmail = user.UserEmail,
                UserPhoneNo = user.UserPhoneNo,
                UserAddress = user.UserAddress,
                UserUsername = user.UserUsername,
                UserIsActive = user.UserIsActive,
                UserRegistrationDate = user.UserRegistrationDate,
                UserUpdatedDate = user.UserUpdatedDate,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName
            };

            return View(model);
        }

        // POST: Admin/Users/UserDelete/5
        [HttpPost, ActionName("UserDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDeleteConfirmed(int id)
        {
            var user = await _context.TblUsers.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // İstifadəçiyə bağlı aktiv icarə olub-olmadığı yoxlanılır:
            bool hasActiveBorrow = await _context.TblBorrows.AnyAsync(b => b.UserId == id && b.BorrowReturnDate == null);

            if (hasActiveBorrow)
            {
                TempData["DeleteError"] = "Bu istifadəçini silmək mümkün deyil! İstifadəçinin hazırda icarədə olan kitabı var.";
                return RedirectToAction(nameof(UserIndex));
            }

            // Qaytarılmış icarələr də FK ilə (NO ACTION) silməni bloklayır:
            bool hasBorrowHistory = await _context.TblBorrows.AnyAsync(b => b.UserId == id);

            if (hasBorrowHistory)
            {
                TempData["DeleteError"] = "Bu istifadəçini silmək mümkün deyil! İstifadəçiyə aid icarə tarixçəsi mövcuddur.";
                return RedirectToAction(nameof(UserIndex));
            }

            // İstifadəçini DBdən silirik:
            _context.TblUsers.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu istifadəçini silmək mümkün deyil! İstifadəçiyə bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(UserIndex));
            }

            TempData["Success"] = "İstifadəçi uğurla silindi!";
            return RedirectToAction(nameof(UserIndex));
        }
    }
}
