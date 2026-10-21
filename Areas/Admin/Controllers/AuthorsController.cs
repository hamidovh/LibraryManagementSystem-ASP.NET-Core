/* ~/Areas/Admin/Controllers/AuthorsController.cs: */

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
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
    public class AuthorsController : Controller
    {
        private readonly LMSdbContext _context;
        private readonly IImageStore _images;

        public AuthorsController(LMSdbContext context, IImageStore images)
        {
            _context = context;
            _images = images;
        }

        // GET: Admin/Authors/AuthorIndex
        [HttpGet]
        public async Task<IActionResult> AuthorIndex(string sortOrder = "date_desc", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum müəllif sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            var authors = _context.TblAuthors.AsNoTracking().Include(a => a.TblAuthorImages).AsQueryable();

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "name_asc":
                    authors = authors.OrderBy(a => a.AuthorName).ThenBy(a => a.AuthorSurname).ThenBy(a => a.AuthorId);
                    break;
                case "name_desc":
                    authors = authors.OrderByDescending(a => a.AuthorName).ThenByDescending(a => a.AuthorSurname).ThenByDescending(a => a.AuthorId);
                    break;
                case "date_asc":
                    authors = authors.OrderBy(a => a.AuthorCreatedDate).ThenBy(a => a.AuthorId);
                    break;
                default:
                    authors = authors.OrderByDescending(a => a.AuthorCreatedDate).ThenByDescending(a => a.AuthorId);
                    sortOrder = "date_desc";
                    break;
            }

            // Bütün müəlliflərin ümumi sayı və ümumi səhifə sayı:
            int totalAuthors = await authors.CountAsync();
            int totalPages = (int)Math.Ceiling(totalAuthors / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid müəlliflər ViewModelə çevrilir:
            var authorList = await authors.Skip((page - 1) * pageSize).Take(pageSize).Select(a => new AuthorVM
            {
                AuthorId = a.AuthorId,
                AuthorName = a.AuthorName,
                AuthorSurname = a.AuthorSurname,
                AuthorInfo = a.AuthorInfo,
                AuthorIsActive = a.AuthorIsActive,
                AuthorCreatedDate = a.AuthorCreatedDate,
                AuthorUpdatedDate = a.AuthorUpdatedDate,
                TblAuthorImages = a.TblAuthorImages.OrderBy(i => i.ImageSortOrder).ToList()
            }).ToListAsync();

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAuthors = totalAuthors;

            return View(authorList);
        }

        // GET: Admin/Authors/AuthorCreate
        [HttpGet]
        public IActionResult AuthorCreate()
        {
            var model = new AuthorVM
            {
                AuthorIsActive = true
            };

            return View(model);
        }

        // POST: Admin/Authors/AuthorCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorCreate(AuthorVM model)
        {
            string? imageError = _images.Validate(model.ImageFile);

            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.AuthorName = model.AuthorName.Trim(); // Boşluqları yığışdırır.
            model.AuthorSurname = model.AuthorSurname.Trim();
            model.AuthorInfo = string.IsNullOrWhiteSpace(model.AuthorInfo) ? null : model.AuthorInfo.Trim();

            // Eyni ad və soyadlı müəllif artıq varsa, yenisi yaradılmır (DB-də UNIQUE constraint var):
            bool authorExists = await _context.TblAuthors.AnyAsync(a => a.AuthorName == model.AuthorName && a.AuthorSurname == model.AuthorSurname);

            if (authorExists)
            {
                ModelState.AddModelError(nameof(model.AuthorName), "Bu ad və soyadda müəllif artıq mövcuddur!");
                return View(model);
            }

            // Yeni TblAuthor entity-si yaradılır:
            var author = new TblAuthor
            {
                AuthorName = model.AuthorName,
                AuthorSurname = model.AuthorSurname,
                AuthorInfo = model.AuthorInfo,
                AuthorIsActive = model.AuthorIsActive
            };

            string? newFileName = null;

            if (model.ImageFile is { Length: > 0 })
            {
                newFileName = await _images.SaveAsync(model.ImageFile);
                author.TblAuthorImages.Add(new TblAuthorImage { ImagePath = newFileName, ImageSortOrder = 0 });
            }

            // Müəllif DB-ə əlavə olunur:
            _context.TblAuthors.Add(author);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName);

                ModelState.AddModelError(string.Empty, "Müəllifi əlavə etmək mümkün olmadı! Eyni ad və soyadlı müəllif artıq mövcud ola bilər.");
                return View(model);
            }

            TempData["Success"] = "Müəllif uğurla əlavə edildi!";
            return RedirectToAction(nameof(AuthorIndex));
        }

        // GET: Admin/Authors/AuthorEdit/5
        [HttpGet]
        public async Task<IActionResult> AuthorEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.TblAuthors.AsNoTracking().Include(a => a.TblAuthorImages).FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            var model = new AuthorVM
            {
                AuthorId = author.AuthorId,
                AuthorName = author.AuthorName,
                AuthorSurname = author.AuthorSurname,
                AuthorInfo = author.AuthorInfo,
                AuthorIsActive = author.AuthorIsActive,
                AuthorCreatedDate = author.AuthorCreatedDate,
                AuthorUpdatedDate = author.AuthorUpdatedDate,
                TblAuthorImages = author.TblAuthorImages.OrderBy(i => i.ImageSortOrder).ToList()
            };

            return View(model);
        }

        // Müəllifin mövcud olub-olmadığını yoxlayan metod:
        private bool AuthorExists(int id)
        {
            return _context.TblAuthors.Any(a => a.AuthorId == id);
        }

        // POST: Admin/Authors/AuthorEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorEdit(int id, AuthorVM model)
        {
            if (id != model.AuthorId)
            {
                return NotFound();
            }

            var author = await _context.TblAuthors.Include(a => a.TblAuthorImages).FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            // Formda göstərilməsi üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
            void Refill()
            {
                model.AuthorCreatedDate = author.AuthorCreatedDate;
                model.AuthorUpdatedDate = author.AuthorUpdatedDate;
                model.TblAuthorImages = author.TblAuthorImages.OrderBy(i => i.ImageSortOrder).ToList();
            }

            string? imageError = _images.Validate(model.ImageFile);

            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid)
            {
                Refill();

                return View(model);
            }

            model.AuthorName = model.AuthorName.Trim(); // Boşluqları yığışdırır.
            model.AuthorSurname = model.AuthorSurname.Trim();
            model.AuthorInfo = string.IsNullOrWhiteSpace(model.AuthorInfo) ? null : model.AuthorInfo.Trim();

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir. Ordinal müqayisə böyük-kiçik hərf dəyişikliyinin də dəyişiklik hesab edilməsi üçündür. Yeni şəkil seçilməyibsə və mövcud şəkil silinməyibsə, şəkil üzrə dəyişiklik yoxdur:
            bool noImageChange = (model.ImageFile is null || model.ImageFile.Length == 0) && !model.DeleteImage;

            bool isNotChanged = string.Equals(author.AuthorName, model.AuthorName, StringComparison.Ordinal) && string.Equals(author.AuthorSurname, model.AuthorSurname, StringComparison.Ordinal) && string.Equals(author.AuthorInfo ?? string.Empty, model.AuthorInfo ?? string.Empty, StringComparison.Ordinal) && author.AuthorIsActive == model.AuthorIsActive && noImageChange;

            if (isNotChanged)
            {
                Refill();

                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return View(model);
            }

            // Eyni ad və soyadın başqa müəllifə aid olub-olmadığı yoxlanılır:
            bool authorExists = await _context.TblAuthors.AnyAsync(a => a.AuthorName == model.AuthorName && a.AuthorSurname == model.AuthorSurname && a.AuthorId != id);

            if (authorExists)
            {
                Refill();

                ModelState.AddModelError(nameof(model.AuthorName), "Bu ad və soyadda başqa müəllif artıq mövcuddur!");
                return View(model);
            }

            // Əsas müəllif məlumatları yenilənir:
            author.AuthorName = model.AuthorName;
            author.AuthorSurname = model.AuthorSurname;
            author.AuthorInfo = model.AuthorInfo;
            author.AuthorIsActive = model.AuthorIsActive;
            author.AuthorUpdatedDate = DateTime.Now;

            // Müəllifin hazırkı ilk şəkli tapılır:
            var oldImage = author.TblAuthorImages.OrderBy(i => i.ImageSortOrder).FirstOrDefault();
            var filesToDelete = new List<string>();
            string? newFileName = null;

            // İstifadəçi mövcud şəkli silmək istəyirsə:
            if (model.DeleteImage && oldImage != null)
            {
                filesToDelete.Add(oldImage.ImagePath);
                _context.TblAuthorImages.Remove(oldImage);
                oldImage = null;
            }

            // Yeni şəkil seçilibsə:
            if (model.ImageFile is { Length: > 0 })
            {
                newFileName = await _images.SaveAsync(model.ImageFile);

                if (oldImage != null)
                {
                    filesToDelete.Add(oldImage.ImagePath);
                    oldImage.ImagePath = newFileName;
                    oldImage.ImageSortOrder = 0;
                }
                else
                {
                    author.TblAuthorImages.Add(new TblAuthorImage { ImagePath = newFileName, ImageSortOrder = 0 });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _images.Delete(newFileName);

                if (!AuthorExists(id))
                {
                    TempData["Error"] = "Bu müəllif artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(AuthorIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName);
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı! Eyni ad və soyadlı müəllif yaranmış ola bilər, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(AuthorEdit), new { id });
            }

            foreach (var fileName in filesToDelete)
            {
                _images.Delete(fileName);
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(AuthorIndex));
        }

        // GET: Admin/Authors/AuthorDelete/5
        [HttpGet]
        public async Task<IActionResult> AuthorDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.TblAuthors.AsNoTracking().Include(a => a.TblAuthorImages).FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            var model = new AuthorVM
            {
                AuthorId = author.AuthorId,
                AuthorName = author.AuthorName,
                AuthorSurname = author.AuthorSurname,
                AuthorInfo = author.AuthorInfo,
                AuthorIsActive = author.AuthorIsActive,
                AuthorCreatedDate = author.AuthorCreatedDate,
                AuthorUpdatedDate = author.AuthorUpdatedDate,
                TblAuthorImages = author.TblAuthorImages.OrderBy(i => i.ImageSortOrder).ToList()
            };

            return View(model);
        }

        // POST: Admin/Authors/AuthorDelete/5
        [HttpPost, ActionName("AuthorDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorDeleteConfirmed(int id)
        {
            var author = await _context.TblAuthors.Include(a => a.TblAuthorImages).FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            // Müəllifə bağlı kitab olub-olmadığı yoxlanılır (tblBookAuthors-da ON DELETE CASCADE olduğu üçün bu yoxlama olmasa, müəllif silinəndə kitab-müəllif əlaqələri səssizcə silinərdi).
            bool hasBooks = await _context.TblBooks.AnyAsync(b => b.Authors.Any(a => a.AuthorId == id));

            if (hasBooks)
            {
                TempData["DeleteError"] = "Bu müəllifi silmək mümkün deyil! Müəllifə bağlı kitablar mövcuddur.";
                return RedirectToAction(nameof(AuthorIndex));
            }

            // Silinəcək şəkillərin fayl adları əvvəlcədən saxlanılır:
            var imagePaths = author.TblAuthorImages.Select(i => i.ImagePath).ToList();

            _context.TblAuthors.Remove(author);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu müəllifi silmək mümkün deyil! Müəllifə bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(AuthorIndex));
            }

            // DB-dən silindikdən sonra fiziki şəkillər silinir:
            foreach (var imagePath in imagePaths)
            {
                _images.Delete(imagePath);
            }

            TempData["Success"] = "Müəllif uğurla silindi!";
            return RedirectToAction(nameof(AuthorIndex));
        }
    }
}
