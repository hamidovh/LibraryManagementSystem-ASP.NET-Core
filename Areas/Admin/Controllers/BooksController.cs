/* ~/Areas/Admin/Controllers/BooksController.cs: */

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
    public class BooksController : Controller
    {
        private readonly LMSdbContext _context;
        private readonly IImageStore _images;

        public BooksController(LMSdbContext context, IImageStore images)
        {
            _context = context;
            _images = images;
        }

        // GET: Admin/Books/BookIndex
        [HttpGet]
        public async Task<IActionResult> BookIndex(string sortOrder = "date_desc", List<int>? authorIds = null, List<int>? categoryIds = null, int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum kitab sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            authorIds ??= new List<int>(); // ??= - "null-dursa yeni boş siyahı ver" deməkdir.
            categoryIds ??= new List<int>(); // ??= - "null-dursa yeni boş siyahı ver" deməkdir.

            var books = _context.TblBooks.AsNoTracking().AsQueryable();

            // Müəllifə görə filtr (seçilmiş müəlliflərdən ən azı birinə aid olan kitablar):
            if (authorIds.Any())
            {
                books = books.Where(b => b.Authors.Any(a => authorIds.Contains(a.AuthorId)));
            }

            // Kateqoriyaya görə filtr (seçilmiş kateqoriyalardan ən azı birinə aid olan kitablar):
            if (categoryIds.Any())
            {
                books = books.Where(b => b.Categories.Any(c => categoryIds.Contains(c.CategoryId)));
            }

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "name_asc":
                    books = books.OrderBy(b => b.BookName).ThenBy(b => b.BookId);
                    break;
                case "name_desc":
                    books = books.OrderByDescending(b => b.BookName).ThenByDescending(b => b.BookId);
                    break;
                case "date_asc":
                    books = books.OrderBy(b => b.BookCreatedDate).ThenBy(b => b.BookId);
                    break;
                default:
                    books = books.OrderByDescending(b => b.BookCreatedDate).ThenByDescending(b => b.BookId);
                    sortOrder = "date_desc";
                    break;
            }

            // Bütün kitabların ümumi sayı və ümumi səhifə sayı:
            int totalBooks = await books.CountAsync();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid kitablar ViewModelə çevrilir:
            var bookList = await books.Skip((page - 1) * pageSize).Take(pageSize).Select(b => new BookVM
            {
                BookId = b.BookId,
                BookName = b.BookName,
                BookIsbn = b.BookIsbn,
                BookAbout = b.BookAbout,
                BookStockQuantity = b.BookStockQuantity,
                BookFineAmount = b.BookFineAmount,
                BookIsActive = b.BookIsActive,
                BookCreatedDate = b.BookCreatedDate,
                BookUpdatedDate = b.BookUpdatedDate,
                TblBookImages = b.TblBookImages.OrderBy(i => i.ImageSortOrder).ToList(),
                Authors = b.Authors.ToList(),
                Categories = b.Categories.ToList()
            }).ToListAsync();

            // Filtr dropdown-ları üçün siyahılar (yalnız aktiv müəllif və kateqoriyalar):
            ViewBag.FilterAuthors = await _context.TblAuthors.Where(a => a.AuthorIsActive).OrderBy(a => a.AuthorName).ThenBy(a => a.AuthorSurname).ToListAsync();
            ViewBag.FilterCategories = await _context.TblCategories.Where(c => c.CategoryIsActive).OrderBy(c => c.CategoryName).ToListAsync();
            // Deaktiv bir müəllif və ya kateqoriya URL-dəki authorIds/categoryIds ilə gəlsə filtr yenə işləyər, sadəcə dropdown-da görünməz.

            // Məlumatlar Viewa ötürülür:
            ViewBag.SelectedAuthorIds = authorIds;
            ViewBag.SelectedCategoryIds = categoryIds;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBooks = totalBooks;

            return View(bookList);
        }

        // Create/Edit səhifələrində müəllif və kateqoriya siyahıları doldurulur (yüklənir) (keep...Ids: deaktiv olsalar belə siyahıda qalmalı olan (kitabın hazırkı) müəllif/kateqoriya ID-ləri):
        private async Task LoadAuthorsAndCategories(IEnumerable<int>? keepAuthorIds = null, IEnumerable<int>? keepCategoryIds = null)
        {
            var authorIds = keepAuthorIds?.ToList() ?? new List<int>();
            var categoryIds = keepCategoryIds?.ToList() ?? new List<int>();

            ViewBag.Authors = await _context.TblAuthors.Where(a => a.AuthorIsActive || authorIds.Contains(a.AuthorId)).OrderBy(a => a.AuthorName).ThenBy(a => a.AuthorSurname).ToListAsync();

            ViewBag.Categories = await _context.TblCategories.Where(c => c.CategoryIsActive || categoryIds.Contains(c.CategoryId)).OrderBy(c => c.CategoryName).ToListAsync();
        }

        // GET: Admin/Books/BookCreate
        [HttpGet]
        public async Task<IActionResult> BookCreate()
        {
            var model = new BookVM
            {
                BookIsActive = true
            };

            // Viewda göstərmək üçün müəllif və kateqoriyaların siyahısı gətirilir:
            await LoadAuthorsAndCategories();
            return View(model);
        }        

        // POST: Admin/Books/BookCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookCreate(BookVM model)
        {
            string? imageError = _images.Validate(model.ImageFile);

            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid)
            {
                await LoadAuthorsAndCategories();
                return View(model);
            }

            model.BookName = model.BookName.Trim(); // Boşluqları yığışdırır.
            model.BookIsbn = model.BookIsbn.Trim();
            model.BookAbout = string.IsNullOrWhiteSpace(model.BookAbout) ? null : model.BookAbout.Trim();

            // ISBN artıq mövcuddursa, yeni kitab yaratmağa icazə vermirik:
            bool isbnExists = await _context.TblBooks.AnyAsync(b => b.BookIsbn == model.BookIsbn);

            if (isbnExists)
            {
                ModelState.AddModelError(nameof(model.BookIsbn), "Bu ISBN ilə kitab artıq mövcuddur!");
                await LoadAuthorsAndCategories();
                return View(model);
            }

            // Yeni TblBook entity-si yaradılır:
            var book = new TblBook
            {
                BookName = model.BookName,
                BookIsbn = model.BookIsbn,
                BookAbout = model.BookAbout,
                BookStockQuantity = model.BookStockQuantity,
                BookFineAmount = model.BookFineAmount,
                BookIsActive = model.BookIsActive
            };

            // Seçilmiş müəlliflər tapılır:
            if (model.SelectedAuthorIds != null && model.SelectedAuthorIds.Any())
            {
                book.Authors = await _context.TblAuthors.Where(a => model.SelectedAuthorIds.Contains(a.AuthorId)).ToListAsync();
            }

            // Seçilmiş kateqoriyalar tapılır:
            if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
            {
                book.Categories = await _context.TblCategories.Where(c => model.SelectedCategoryIds.Contains(c.CategoryId)).ToListAsync();
            }

            string? newFileName = null;

            if (model.ImageFile is { Length: > 0 })
            {
                newFileName = await _images.SaveAsync(model.ImageFile);
                book.TblBookImages.Add(new TblBookImage { ImagePath = newFileName, ImageSortOrder = 0 });
            }

            // Kitab DB-ə əlavə olunur:
            _context.TblBooks.Add(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName);
                ModelState.AddModelError(string.Empty, "Kitabı əlavə etmək mümkün olmadı! Eyni ISBN-li kitab artıq mövcud ola bilər.");
                await LoadAuthorsAndCategories();
                return View(model);
            }

            TempData["Success"] = "Kitab uğurla əlavə edildi!";
            return RedirectToAction(nameof(BookIndex));
        }

        // GET: Admin/Books/BookEdit/5
        [HttpGet]
        public async Task<IActionResult> BookEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.TblBooks.AsNoTracking().Include(b => b.TblBookImages).Include(b => b.Authors).Include(b => b.Categories).FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            var model = new BookVM
            {
                BookId = book.BookId,
                BookName = book.BookName,
                BookIsbn = book.BookIsbn,
                BookAbout = book.BookAbout,
                BookStockQuantity = book.BookStockQuantity,
                BookFineAmount = book.BookFineAmount,
                BookIsActive = book.BookIsActive,
                BookCreatedDate = book.BookCreatedDate,
                BookUpdatedDate = book.BookUpdatedDate,
                TblBookImages = book.TblBookImages.OrderBy(i => i.ImageSortOrder).ToList(),
                Authors = book.Authors.ToList(),
                Categories = book.Categories.ToList(),
                SelectedAuthorIds = book.Authors.Select(a => a.AuthorId).ToList(),
                SelectedCategoryIds = book.Categories.Select(c => c.CategoryId).ToList()
            };

            // Kitabın hazırkı müəllif/kateqoriyası deaktiv olsa belə siyahıda qalır:
            await LoadAuthorsAndCategories(model.SelectedAuthorIds, model.SelectedCategoryIds);
            return View(model);
        }

        // Kitabın mövcud olub-olmadığını yoxlayan metod:
        private bool BookExists(int id)
        {
            return _context.TblBooks.Any(b => b.BookId == id);
        }

        // POST: Admin/Books/BookEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookEdit(int id, BookVM model)
        {
            if (id != model.BookId)
            {
                return NotFound();
            }

            var book = await _context.TblBooks.Include(b => b.TblBookImages).Include(b => b.Authors).Include(b => b.Categories).FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Formu yenidən göstərmək üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
            async Task<IActionResult> Redisplay()
            {
                model.BookCreatedDate = book.BookCreatedDate;
                model.BookUpdatedDate = book.BookUpdatedDate;
                model.TblBookImages = book.TblBookImages.OrderBy(i => i.ImageSortOrder).ToList();
                await LoadAuthorsAndCategories(book.Authors.Select(a => a.AuthorId), book.Categories.Select(c => c.CategoryId));
                return View(model);
            }

            string? imageError = _images.Validate(model.ImageFile);

            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid)
            {
                return await Redisplay();
            }

            model.BookName = model.BookName.Trim(); // Boşluqları yığışdırır.
            model.BookIsbn = model.BookIsbn.Trim();
            model.BookAbout = string.IsNullOrWhiteSpace(model.BookAbout) ? null : model.BookAbout.Trim();

            // Müəllif və kateqoriya seçimləri sıra fərqi nəzərə alınmadan müqayisə edilir:
            var oldAuthorIds = book.Authors.Select(a => a.AuthorId).OrderBy(x => x).ToList();
            var newAuthorIds = (model.SelectedAuthorIds ?? new List<int>()).Distinct().OrderBy(x => x).ToList();
            var oldCategoryIds = book.Categories.Select(c => c.CategoryId).OrderBy(x => x).ToList();
            var newCategoryIds = (model.SelectedCategoryIds ?? new List<int>()).Distinct().OrderBy(x => x).ToList();

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir. Ordinal müqayisə böyük-kiçik hərf dəyişikliyinin də dəyişiklik hesab edilməsi üçündür. Yeni şəkil seçilməyibsə və mövcud şəkil silinməyibsə, şəkil üzrə dəyişiklik yoxdur:
            bool noImageChange = (model.ImageFile is null || model.ImageFile.Length == 0) && !model.DeleteImage;

            bool isNotChanged = string.Equals(book.BookName, model.BookName, StringComparison.Ordinal) && string.Equals(book.BookIsbn, model.BookIsbn, StringComparison.Ordinal) && string.Equals(book.BookAbout ?? string.Empty, model.BookAbout ?? string.Empty, StringComparison.Ordinal) && book.BookStockQuantity == model.BookStockQuantity && book.BookFineAmount == model.BookFineAmount && book.BookIsActive == model.BookIsActive && oldAuthorIds.SequenceEqual(newAuthorIds) && oldCategoryIds.SequenceEqual(newCategoryIds) && noImageChange;

            // Stok miqdarı hazırda icarədə olan nüsxə sayından az ola bilməz:
            int activeCount = await _context.TblBorrows.CountAsync(b => b.BookId == id && b.BorrowReturnDate == null);

            if (model.BookStockQuantity < activeCount)
            {
                ModelState.AddModelError(nameof(model.BookStockQuantity), $"Stok miqdarı hazırda icarədə olan nüsxə sayından ({activeCount}) az ola bilməz!");
                return await Redisplay();
            }

            if (isNotChanged)
            {                
                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return await Redisplay();
            }

            // ISBN-in başqa kitaba aid olub-olmadığı yoxlanılır:
            bool isbnExists = await _context.TblBooks.AnyAsync(b => b.BookIsbn == model.BookIsbn && b.BookId != id);

            if (isbnExists)
            {
                ModelState.AddModelError(nameof(model.BookIsbn), "Bu ISBN başqa kitaba aiddir!");
                return await Redisplay();
            }

            // Əsas kitab məlumatları yenilənir:
            book.BookName = model.BookName;
            book.BookIsbn = model.BookIsbn;
            book.BookAbout = model.BookAbout;
            book.BookStockQuantity = model.BookStockQuantity;
            book.BookFineAmount = model.BookFineAmount;
            book.BookIsActive = model.BookIsActive;
            book.BookUpdatedDate = DateTime.Now;

            // Mövcud müəllif əlaqələri təmizlənir, daha sonra müəllif əlaqələri yenilənir:
            book.Authors.Clear();
            // Yeni seçilmiş müəlliflər əlavə edilir:
            if (newAuthorIds.Any())
            {
                var selectedAuthors = await _context.TblAuthors.Where(a => newAuthorIds.Contains(a.AuthorId)).ToListAsync();
                foreach (var author in selectedAuthors)
                {
                    book.Authors.Add(author);
                }
            }

            // Mövcud kateqoriya əlaqələri təmizlənir, daha sonra kateqoriya əlaqələri yenilənir:
            book.Categories.Clear();
            // Yeni seçilmiş kateqoriyalar əlavə edilir:
            if (newCategoryIds.Any())
            {
                var selectedCategories = await _context.TblCategories.Where(c => newCategoryIds.Contains(c.CategoryId)).ToListAsync();
                foreach (var category in selectedCategories)
                {
                    book.Categories.Add(category);
                }
            }

            // Kitabın hazırkı ilk şəkli tapılır:
            var oldImage = book.TblBookImages.OrderBy(i => i.ImageSortOrder).FirstOrDefault();
            var filesToDelete = new List<string>();
            string? newFileName = null;

            // İstifadəçi mövcud şəkli silmək istəyirsə:
            if (model.DeleteImage && oldImage != null)
            {
                filesToDelete.Add(oldImage.ImagePath);
                _context.TblBookImages.Remove(oldImage);
                oldImage = null;
            }

            // Yeni şəkil yüklənibsə:
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
                    book.TblBookImages.Add(new TblBookImage { ImagePath = newFileName, ImageSortOrder = 0 });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _images.Delete(newFileName);

                if (!BookExists(id))
                {
                    TempData["Error"] = "Bu kitab artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(BookIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName);
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı! Eyni ISBN-li kitab yaranmış ola bilər, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(BookEdit), new { id });
            }

            foreach (var fileName in filesToDelete)
            {
                _images.Delete(fileName);
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(BookIndex));
        }

        // GET: Admin/Books/BookDelete/5
        [HttpGet]
        public async Task<IActionResult> BookDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.TblBooks.AsNoTracking().Include(b => b.TblBookImages).Include(b => b.Authors).Include(b => b.Categories).FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            var model = new BookVM
            {
                BookId = book.BookId,
                BookName = book.BookName,
                BookIsbn = book.BookIsbn,
                BookAbout = book.BookAbout,
                BookStockQuantity = book.BookStockQuantity,
                BookFineAmount = book.BookFineAmount,
                BookIsActive = book.BookIsActive,
                BookCreatedDate = book.BookCreatedDate,
                BookUpdatedDate = book.BookUpdatedDate,
                TblBookImages = book.TblBookImages.OrderBy(i => i.ImageSortOrder).ToList(),
                Authors = book.Authors.ToList(),
                Categories = book.Categories.ToList()
            };

            return View(model);
        }

        // POST: Admin/Books/BookDelete/5
        [HttpPost, ActionName("BookDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookDeleteConfirmed(int id)
        {
            var book = await _context.TblBooks.Include(b => b.TblBookImages).FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Kitaba bağlı aktiv icarə olub-olmadığı yoxlanılır:
            bool hasActiveBorrow = await _context.TblBorrows.AnyAsync(b => b.BookId == id && b.BorrowReturnDate == null);

            if (hasActiveBorrow)
            {
                TempData["DeleteError"] = "Bu kitabı silmək mümkün deyil! Kitab hazırda icarədədir.";
                return RedirectToAction(nameof(BookIndex));
            }

            // Qaytarılmış icarələr də FK ilə (NO ACTION) silməni bloklayır:
            bool hasBorrowHistory = await _context.TblBorrows.AnyAsync(b => b.BookId == id);

            if (hasBorrowHistory)
            {
                TempData["DeleteError"] = "Bu kitabı silmək mümkün deyil! Kitaba aid icarə tarixçəsi mövcuddur.";
                return RedirectToAction(nameof(BookIndex));
            }

            var imagePaths = book.TblBookImages.Select(i => i.ImagePath).ToList();

            // Kitabı DBdən silirik:
            _context.TblBooks.Remove(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu kitabı silmək mümkün deyil! Kitaba bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(BookIndex));
            }

            // DBdən silindikdən sonra fiziki şəkillər silinir:
            foreach (var imagePath in imagePaths)
            {
                _images.Delete(imagePath);
            }

            TempData["Success"] = "Kitab uğurla silindi!";
            return RedirectToAction(nameof(BookIndex));
        }
    }
}
