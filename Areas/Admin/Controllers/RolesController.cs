/* ~/Areas/Admin/Controllers/RolesController.cs: */

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class RolesController : Controller
    {
        private readonly LMSdbContext _context;

        public RolesController(LMSdbContext context)
        {
            _context = context;
        }

        // GET: Admin/Roles/RoleIndex
        [HttpGet]
        public async Task<IActionResult> RoleIndex(string sortOrder = "date_desc", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum rol sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            var roles = _context.TblRoles.AsNoTracking().AsQueryable();

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "name_asc":
                    roles = roles.OrderBy(r => r.RoleName).ThenBy(r => r.RoleId);
                    break;
                case "name_desc":
                    roles = roles.OrderByDescending(r => r.RoleName).ThenByDescending(r => r.RoleId);
                    break;
                case "date_asc":
                    roles = roles.OrderBy(r => r.RoleCreatedDate).ThenBy(r => r.RoleId);
                    break;
                default:
                    roles = roles.OrderByDescending(r => r.RoleCreatedDate).ThenByDescending(r => r.RoleId);
                    sortOrder = "date_desc";
                    break;
            }

            // Bütün rolların ümumi sayı və ümumi səhifə sayı:
            int totalRoles = await roles.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRoles / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid rollar ViewModelə çevrilir:
            var roleList = await roles.Skip((page - 1) * pageSize).Take(pageSize).Select(r => new RoleVM
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                RoleIsActive = r.RoleIsActive,
                RoleCreatedDate = r.RoleCreatedDate,
                RoleUpdatedDate = r.RoleUpdatedDate,
                UserCount = r.TblUsers.Count()
            }).ToListAsync();

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRoles = totalRoles;

            return View(roleList);
        }

        // GET: Admin/Roles/RoleCreate
        [HttpGet]
        public IActionResult RoleCreate()
        {
            var model = new RoleVM
            {
                RoleIsActive = true
            };

            return View(model);
        }

        // POST: Admin/Roles/RoleCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleCreate(RoleVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.RoleName = model.RoleName.Trim(); // Boşluqları yığışdırır.

            // Eyni adlı rol artıq varsa, yenisi yaradılmır (DB-də UNIQUE constraint var):
            bool roleExists = await _context.TblRoles.AnyAsync(r => r.RoleName == model.RoleName);

            if (roleExists)
            {
                ModelState.AddModelError(nameof(model.RoleName), "Bu adda rol artıq mövcuddur!");
                return View(model);
            }

            // Yeni TblRole entity-si yaradılır:
            var role = new TblRole
            {
                RoleName = model.RoleName,
                RoleIsActive = model.RoleIsActive
            };

            // Rol DB-ə əlavə olunur:
            _context.TblRoles.Add(role);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Rolu yadda saxlamaq mümkün olmadı! Eyni adlı rol artıq mövcud ola bilər.");
                return View(model);
            }

            TempData["Success"] = "Rol uğurla əlavə edildi!";
            return RedirectToAction(nameof(RoleIndex));
        }

        // GET: Admin/Roles/RoleEdit/5
        [HttpGet]
        public async Task<IActionResult> RoleEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.TblRoles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleVM
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleIsActive = role.RoleIsActive,
                RoleCreatedDate = role.RoleCreatedDate,
                RoleUpdatedDate = role.RoleUpdatedDate,
            };

            return View(model);
        }

        // Rolun mövcud olub-olmadığını yoxlayan metod:
        private bool RoleExists(int id)
        {
            return _context.TblRoles.Any(r => r.RoleId == id);
        }

        // POST: Admin/Roles/RoleEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleEdit(int id, RoleVM model)
        {
            if (id != model.RoleId)
            {
                return NotFound();
            }

            var role = await _context.TblRoles.FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            void Refill()
            {
                model.RoleCreatedDate = role.RoleCreatedDate;
                model.RoleUpdatedDate = role.RoleUpdatedDate;
            }

            if (!ModelState.IsValid)
            {
                Refill();

                return View(model);
            }

            model.RoleName = model.RoleName.Trim(); // Boşluqları yığışdırır.

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir. Ordinal müqayisə böyük-kiçik hərf dəyişikliyinin də (məsələn, "admin"-dən  "Admin"-ə) dəyişiklik hesab edilməsi üçündür.
            bool isNotChanged = string.Equals(role.RoleName, model.RoleName, StringComparison.Ordinal) && role.RoleIsActive == model.RoleIsActive;

            if (isNotChanged)
            {
                // Formda göstərilməsi üçün DB-dəki tarixlər modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
                Refill();

                // Boş key ilə əlavə olunan error ValidationSummary-də (asp-validation-summary="ModelOnly") göstərilir:
                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return View(model);
            }

            // Eyni adın başqa rola aid olub-olmadığı yoxlanılır:
            bool roleExists = await _context.TblRoles.AnyAsync(r => r.RoleName == model.RoleName && r.RoleId != id);

            if (roleExists)
            {
                Refill();
                ModelState.AddModelError(nameof(model.RoleName), "Bu adda başqa rol artıq mövcuddur!");
                return View(model);
            }

            // Əsas rol məlumatları yenilənir:
            role.RoleName = model.RoleName;
            role.RoleIsActive = model.RoleIsActive;
            role.RoleUpdatedDate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    TempData["Error"] = "Bu rol artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(RoleIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı! Eyni adlı rol yaranmış ola bilər, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(RoleEdit), new { id });
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(RoleIndex));
        }

        // GET: Admin/Roles/RoleDelete/5
        [HttpGet]
        public async Task<IActionResult> RoleDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.TblRoles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleVM
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleIsActive = role.RoleIsActive,
                RoleCreatedDate = role.RoleCreatedDate,
                RoleUpdatedDate = role.RoleUpdatedDate,
                UserCount = await _context.TblUsers.CountAsync(u => u.RoleId == role.RoleId)
            };

            return View(model);
        }

        // POST: Admin/Roles/RoleDelete/5
        [HttpPost, ActionName("RoleDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleDeleteConfirmed(int id)
        {
            var role = await _context.TblRoles.FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            // Rola bağlı istifadəçi olub-olmadığı yoxlanılır (tblUsers - tblRoles FK-sı ON DELETE NO ACTION-dur):
            bool hasUsers = await _context.TblUsers.AnyAsync(u => u.RoleId == id);

            if (hasUsers)
            {
                TempData["DeleteError"] = "Bu rolu silmək mümkün deyil! Rola bağlı istifadəçilər mövcuddur.";
                return RedirectToAction(nameof(RoleIndex));
            }

            _context.TblRoles.Remove(role);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu rolu silmək mümkün deyil! Rola bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(RoleIndex));
            }

            TempData["Success"] = "Rol uğurla silindi!";
            return RedirectToAction(nameof(RoleIndex));
        }
    }
}
