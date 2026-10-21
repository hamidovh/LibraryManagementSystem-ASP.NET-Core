/* ~/Areas/Admin/Controllers/BorrowsController.cs: */

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    //[Authorize(Roles = "Admin")]
    public class BorrowsController : Controller
    {
        private readonly LMSdbContext _context;

        public BorrowsController(LMSdbContext context)
        {
            _context = context;
        }

        // GET: Admin/Borrows/BorrowIndex
        [HttpGet]
        public async Task<IActionResult> BorrowIndex(string sortOrder = "date_desc", string status = "", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum icarə sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            var now = DateTime.Now; // Bütün sorğuda eyni "indi" istifadə olunsun deyə bir dəfə götürülür.

            var borrows = _context.TblBorrows.AsNoTracking().AsQueryable();

            // Statusa görə filtr (active / overdue / returned, boşdursa hamısı):
            switch (status)
            {
                case "active":
                    borrows = borrows.Where(b => b.BorrowReturnDate == null && b.BorrowDueDate >= now);
                    break;
                case "overdue":
                    borrows = borrows.Where(b => b.BorrowReturnDate == null && b.BorrowDueDate < now);
                    break;
                case "returned":
                    borrows = borrows.Where(b => b.BorrowReturnDate != null);
                    break;
                default:
                    status = "";
                    break;
            }

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "date_asc":
                    borrows = borrows.OrderBy(b => b.BorrowDate).ThenBy(b => b.BorrowId);
                    break;
                case "due_asc":
                    borrows = borrows.OrderBy(b => b.BorrowDueDate).ThenBy(b => b.BorrowId);
                    break;
                case "due_desc":
                    borrows = borrows.OrderByDescending(b => b.BorrowDueDate).ThenByDescending(b => b.BorrowId);
                    break;
                default:
                    borrows = borrows.OrderByDescending(b => b.BorrowDate).ThenByDescending(b => b.BorrowId);
                    sortOrder = "date_desc";
                    break;
            }

            // Bütün icarələrin ümumi sayı və ümumi səhifə sayı:
            int totalBorrows = await borrows.CountAsync();
            int totalPages = (int)Math.Ceiling(totalBorrows / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid icarələr ViewModelə çevrilir:
            var borrowList = await borrows.Skip((page - 1) * pageSize).Take(pageSize).Select(b => new BorrowVM
            {
                BorrowId = b.BorrowId,
                UserId = b.UserId,
                BookId = b.BookId,
                BorrowDate = b.BorrowDate,
                BorrowDueDate = b.BorrowDueDate,
                BorrowReturnDate = b.BorrowReturnDate,
                UserFullName = b.User.UserName + " " + b.User.UserSurname,
                UserUsername = b.User.UserUsername,
                BookName = b.Book.BookName,
                BookIsbn = b.Book.BookIsbn,
                HasFine = b.TblFine != null,
                FineAmount = b.TblFine != null ? b.TblFine.FineAmount : (decimal?)null
            }).ToListAsync();

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBorrows = totalBorrows;

            return View(borrowList);
        }

        // Create səhifəsində istifadəçi və kitab siyahıları doldurulur (yalnız aktiv istifadəçilər və stokda boş nüsxəsi olan aktiv kitablar):
        private async Task LoadUsersAndBooks()
        {
            var users = await _context.TblUsers.AsNoTracking().Where(u => u.UserIsActive).OrderBy(u => u.UserName).ThenBy(u => u.UserSurname).Select(u => new { u.UserId, u.UserName, u.UserSurname, u.UserUsername }).ToListAsync();

            ViewBag.Users = users.Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = $"{u.UserName} {u.UserSurname} ({u.UserUsername})"
            }).ToList();

            // Mövcud nüsxə sayı = stok miqdarı − hazırda icarədə olan nüsxələr:
            var books = await _context.TblBooks.AsNoTracking().Where(b => b.BookIsActive).OrderBy(b => b.BookName).Select(b => new
            {
                b.BookId,
                b.BookName,
                b.BookIsbn,
                Available = b.BookStockQuantity - b.TblBorrows.Count(x => x.BorrowReturnDate == null)
            }).ToListAsync();

            ViewBag.Books = books.Where(b => b.Available > 0).Select(b => new SelectListItem
            {
                Value = b.BookId.ToString(),
                Text = $"{b.BookName} (ISBN: {b.BookIsbn}) — stok miqdarı: {b.Available}"
            }).ToList();
        }

        // Qaytarılma müddəti həmin günün sonuna çevrilir (CK_tblBorrows_DueDate: BorrowDueDate >= BorrowDate olmalıdır, bugünü seçəndə də düzgün işləsin deyə):
        private static DateTime ToEndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        // Gecikmə günü (qaytarılma müddətinin keçdiyi tam təqvim günləri):
        private static int CalculateLateDays(DateTime dueDate, DateTime returnDate)
        {
            return Math.Max(0, (returnDate.Date - dueDate.Date).Days);
        }

        // GET: Admin/Borrows/BorrowCreate
        [HttpGet]
        public async Task<IActionResult> BorrowCreate()
        {
            var model = new BorrowVM
            {
                BorrowDueDate = DateTime.Today.AddDays(14) // Default qaytarılma müddəti: 14 gün sonra (formda dəyişdirilə bilər).
            };

            await LoadUsersAndBooks();
            return View(model);
        }

        // POST: Admin/Borrows/BorrowCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowCreate(BorrowVM model)
        {
            if (model.BorrowDueDate == default)
            {
                ModelState.AddModelError(nameof(model.BorrowDueDate), "Qaytarılma müddəti daxil edilməlidir!");
            }
            else if (model.BorrowDueDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BorrowDueDate), "Qaytarılma müddəti keçmişdə ola bilməz!");
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAndBooks();
                return View(model);
            }

            // İstifadəçi mövcud və aktiv olmalıdır:
            bool userOk = await _context.TblUsers.AnyAsync(u => u.UserId == model.UserId && u.UserIsActive);

            if (!userOk)
            {
                ModelState.AddModelError(nameof(model.UserId), "Seçilmiş istifadəçi mövcud deyil və ya deaktivdir!");
            }

            // Kitab mövcud və aktiv olmalıdır:
            var book = await _context.TblBooks.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == model.BookId && b.BookIsActive);

            if (book == null)
            {
                ModelState.AddModelError(nameof(model.BookId), "Seçilmiş kitab mövcud deyil və ya deaktivdir!");
            }
            else
            {
                // Stokda boş nüsxə olmalıdır:
                int activeCount = await _context.TblBorrows.CountAsync(b => b.BookId == model.BookId && b.BorrowReturnDate == null);

                if (book.BookStockQuantity - activeCount <= 0)
                {
                    ModelState.AddModelError(nameof(model.BookId), "Bu kitabın hazırda boş nüsxəsi yoxdur!");
                }
            }

            // Eyni istifadəçi eyni kitabı eyni anda iki dəfə götürə bilməz (DB-də filtered UNIQUE index var):
            bool alreadyBorrowed = await _context.TblBorrows.AnyAsync(b => b.UserId == model.UserId && b.BookId == model.BookId && b.BorrowReturnDate == null);

            if (alreadyBorrowed)
            {
                ModelState.AddModelError(nameof(model.BookId), "Bu istifadəçi bu kitabı artıq icarəyə götürüb və hələ qaytarmayıb!");
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAndBooks();
                return View(model);
            }

            // Yeni TblBorrow entity-si yaradılır (BorrowDate DB-nin default SYSDATETIME() dəyərini alır):
            var borrow = new TblBorrow
            {
                UserId = model.UserId,
                BookId = model.BookId,
                BorrowDueDate = ToEndOfDay(model.BorrowDueDate)
            };

            _context.TblBorrows.Add(borrow);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "İcarəni əlavə etmək mümkün olmadı! Bu istifadəçi bu kitabı artıq icarəyə götürmüş ola bilər.");
                await LoadUsersAndBooks();
                return View(model);
            }

            TempData["Success"] = "İcarə uğurla əlavə edildi!";
            return RedirectToAction(nameof(BorrowIndex));
        }

        // GET: Admin/Borrows/BorrowEdit/5 (yalnız qaytarılma müddətini uzatmaq/dəyişmək mümkündür, istifadəçi və kitab dəyişmir)
        [HttpGet]
        public async Task<IActionResult> BorrowEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.TblBorrows.AsNoTracking().Include(b => b.User).Include(b => b.Book).FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            // Qaytarılmış icarə dəyişdirilə bilməz:
            if (borrow.BorrowReturnDate != null)
            {
                TempData["Error"] = "Qaytarılmış icarəni redaktə etmək mümkün deyil!";
                return RedirectToAction(nameof(BorrowIndex));
            }

            return View(MapToVM(borrow));
        }

        // Entity-dən ViewModel yaradılır (User və Book Include olunmalıdır):
        private static BorrowVM MapToVM(TblBorrow borrow)
        {
            return new BorrowVM
            {
                BorrowId = borrow.BorrowId,
                UserId = borrow.UserId,
                BookId = borrow.BookId,
                BorrowDate = borrow.BorrowDate,
                BorrowDueDate = borrow.BorrowDueDate,
                BorrowReturnDate = borrow.BorrowReturnDate,
                UserFullName = borrow.User.UserName + " " + borrow.User.UserSurname,
                UserUsername = borrow.User.UserUsername,
                BookName = borrow.Book.BookName,
                BookIsbn = borrow.Book.BookIsbn,
                HasFine = borrow.TblFine != null,
                FineAmount = borrow.TblFine?.FineAmount
            };
        }

        // İcarənin mövcud olub-olmadığını yoxlayan metod:
        private bool BorrowExists(int id)
        {
            return _context.TblBorrows.Any(b => b.BorrowId == id);
        }

        // POST: Admin/Borrows/BorrowEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowEdit(int id, BorrowVM model)
        {
            if (id != model.BorrowId)
            {
                return NotFound();
            }

            var borrow = await _context.TblBorrows.Include(b => b.User).Include(b => b.Book).FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            if (borrow.BorrowReturnDate != null)
            {
                TempData["Error"] = "Qaytarılmış icarəni redaktə etmək mümkün deyil!";
                return RedirectToAction(nameof(BorrowIndex));
            }

            // Formu yenidən göstərmək üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
            IActionResult Redisplay()
            {
                model.UserId = borrow.UserId;
                model.BookId = borrow.BookId;
                model.BorrowDate = borrow.BorrowDate;
                model.BorrowReturnDate = borrow.BorrowReturnDate;
                model.UserFullName = borrow.User.UserName + " " + borrow.User.UserSurname;
                model.UserUsername = borrow.User.UserUsername;
                model.BookName = borrow.Book.BookName;
                model.BookIsbn = borrow.Book.BookIsbn;
                return View(model);
            }

            // UserId və BookId bu formda dəyişmir, ona görə onların validasiya xətaları nəzərə alınmır:
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.BookId));

            if (model.BorrowDueDate == default)
            {
                ModelState.AddModelError(nameof(model.BorrowDueDate), "Qaytarılma müddəti daxil edilməlidir!");
            }
            else if (model.BorrowDueDate.Date < borrow.BorrowDate.Date)
            {
                ModelState.AddModelError(nameof(model.BorrowDueDate), "Qaytarılma müddəti icarə tarixindən əvvəl ola bilməz!");
            }

            if (!ModelState.IsValid)
            {
                return Redisplay();
            }

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir:
            if (borrow.BorrowDueDate.Date == model.BorrowDueDate.Date)
            {
                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return Redisplay();
            }

            borrow.BorrowDueDate = ToEndOfDay(model.BorrowDueDate);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowExists(id))
                {
                    TempData["Error"] = "Bu icarə artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(BorrowIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(BorrowEdit), new { id });
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(BorrowIndex));
        }

        // GET: Admin/Borrows/BorrowReturn/5 (kitabın qaytarılmasını təsdiq səhifəsi, gecikmə və cərimə əvvəlcədən göstərilir)
        [HttpGet]
        public async Task<IActionResult> BorrowReturn(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.TblBorrows.AsNoTracking().Include(b => b.User).Include(b => b.Book).Include(b => b.TblFine).FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            if (borrow.BorrowReturnDate != null)
            {
                TempData["Error"] = "Bu icarə artıq qaytarılıb!";
                return RedirectToAction(nameof(BorrowIndex));
            }

            var model = MapToVM(borrow);
            model.LateDays = CalculateLateDays(borrow.BorrowDueDate, DateTime.Now);
            model.FinePreview = model.LateDays * borrow.Book.BookFineAmount;

            return View(model);
        }

        // POST: Admin/Borrows/BorrowReturn/5
        [HttpPost, ActionName("BorrowReturn")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowReturnConfirmed(int id)
        {
            var borrow = await _context.TblBorrows.Include(b => b.Book).Include(b => b.TblFine).FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            // Eyni icarə iki dəfə qaytarıla bilməz:
            if (borrow.BorrowReturnDate != null)
            {
                TempData["Error"] = "Bu icarə artıq qaytarılıb!";
                return RedirectToAction(nameof(BorrowIndex));
            }

            var returnDate = DateTime.Now;
            int lateDays = CalculateLateDays(borrow.BorrowDueDate, returnDate);
            decimal fineAmount = lateDays * borrow.Book.BookFineAmount;

            borrow.BorrowReturnDate = returnDate;

            // Gecikmə varsa və kitabın günlük cərimə məbləği 0-dan böyükdürsə cərimə yaradılır (hər icarəyə maksimum 1 cərimə - UQ_tblFines_BorrowID):
            bool fineCreated = false;

            if (lateDays > 0 && fineAmount > 0 && borrow.TblFine == null)
            {
                borrow.TblFine = new TblFine
                {
                    FineReason = "Gecikir",
                    FineAmount = fineAmount,
                    FineIsPaid = false
                };
                fineCreated = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Qaytarılmanı yadda saxlamaq mümkün olmadı, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(BorrowIndex));
            }

            TempData["Success"] = fineCreated ? $"Kitab qaytarıldı! {lateDays} gün gecikmə üçün {fineAmount:0.00} AZN cərimə yaradıldı." : "Kitab uğurla qaytarıldı!";
            return RedirectToAction(nameof(BorrowIndex));
        }

        // GET: Admin/Borrows/BorrowDelete/5
        [HttpGet]
        public async Task<IActionResult> BorrowDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.TblBorrows.AsNoTracking().Include(b => b.User).Include(b => b.Book).Include(b => b.TblFine).FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            return View(MapToVM(borrow));
        }

        // POST: Admin/Borrows/BorrowDelete/5
        [HttpPost, ActionName("BorrowDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowDeleteConfirmed(int id)
        {
            var borrow = await _context.TblBorrows.FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
            {
                return NotFound();
            }

            // İcarəyə bağlı cərimə olub-olmadığı yoxlanılır (tblFines - tblBorrows FK-sı ON DELETE NO ACTION-dur):
            bool hasFine = await _context.TblFines.AnyAsync(f => f.BorrowId == id);

            if (hasFine)
            {
                TempData["DeleteError"] = "Bu icarəni silmək mümkün deyil! İcarəyə bağlı cərimə mövcuddur.";
                return RedirectToAction(nameof(BorrowIndex));
            }

            _context.TblBorrows.Remove(borrow);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu icarəni silmək mümkün deyil! İcarəyə bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(BorrowIndex));
            }

            TempData["Success"] = "İcarə uğurla silindi!";
            return RedirectToAction(nameof(BorrowIndex));
        }
    }
}
