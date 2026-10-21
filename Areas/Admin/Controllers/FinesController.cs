/* ~/Areas/Admin/Controllers/FinesController.cs: */

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
    public class FinesController : Controller
    {
        // DB-dəki CK_tblFines_FineReason constraint-i ilə eyni dəyərlər (BorrowsController da gecikmə cəriməsini "Gecikir" ilə yaradır):
        private const string ReasonLate = "Gecikir";
        private const string ReasonLost = "İtirilib";

        private readonly LMSdbContext _context;

        public FinesController(LMSdbContext context)
        {
            _context = context;
        }

        // GET: Admin/Fines/FineIndex
        [HttpGet]
        public async Task<IActionResult> FineIndex(string sortOrder = "date_desc", string status = "", string reason = "", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum cərimə sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            var fines = _context.TblFines.AsNoTracking().AsQueryable();

            // Ödəniş statusuna görə filtr (paid / unpaid, boşdursa hamısı):
            switch (status)
            {
                case "paid":
                    fines = fines.Where(f => f.FineIsPaid);
                    break;
                case "unpaid":
                    fines = fines.Where(f => !f.FineIsPaid);
                    break;
                default:
                    status = "";
                    break;
            }

            // Səbəbə görə filtr (late / lost, boşdursa hamısı):
            switch (reason)
            {
                case "late":
                    fines = fines.Where(f => f.FineReason == ReasonLate);
                    break;
                case "lost":
                    fines = fines.Where(f => f.FineReason == ReasonLost);
                    break;
                default:
                    reason = "";
                    break;
            }

            // Filtrə uyğun cərimələrin sayı və məbləği, həmçinin bütün ödənilməmiş cərimələrin məbləği:
            int totalFines = await fines.CountAsync();
            decimal totalAmount = await fines.SumAsync(f => f.FineAmount);
            decimal totalUnpaidAmount = await _context.TblFines.Where(f => !f.FineIsPaid).SumAsync(f => f.FineAmount);

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə):
            switch (sortOrder)
            {
                case "date_asc":
                    fines = fines.OrderBy(f => f.FineDate).ThenBy(f => f.FineId);
                    break;
                case "amount_desc":
                    fines = fines.OrderByDescending(f => f.FineAmount).ThenByDescending(f => f.FineId);
                    break;
                case "amount_asc":
                    fines = fines.OrderBy(f => f.FineAmount).ThenBy(f => f.FineId);
                    break;
                default:
                    fines = fines.OrderByDescending(f => f.FineDate).ThenByDescending(f => f.FineId);
                    sortOrder = "date_desc";
                    break;
            }

            // Ümumi səhifə sayı:
            int totalPages = (int)Math.Ceiling(totalFines / (double)pageSize);

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid cərimələr ViewModelə çevrilir:
            var fineList = await fines.Skip((page - 1) * pageSize).Take(pageSize).Select(f => new FineVM
            {
                FineId = f.FineId,
                BorrowId = f.BorrowId,
                FineReason = f.FineReason,
                FineDate = f.FineDate,
                FineAmount = f.FineAmount,
                FineIsPaid = f.FineIsPaid,
                FinePaymentDate = f.FinePaymentDate,
                UserFullName = f.Borrow.User.UserName + " " + f.Borrow.User.UserSurname,
                UserUsername = f.Borrow.User.UserUsername,
                BookName = f.Borrow.Book.BookName,
                BookIsbn = f.Borrow.Book.BookIsbn,
                BorrowDate = f.Borrow.BorrowDate,
                BorrowDueDate = f.Borrow.BorrowDueDate,
                BorrowReturnDate = f.Borrow.BorrowReturnDate
            }).ToListAsync();

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.Status = status;
            ViewBag.Reason = reason;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalFines = totalFines;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.TotalUnpaidAmount = totalUnpaidAmount;

            return View(fineList);
        }

        // Create səhifəsində cəriməsi olmayan icarələrin siyahısı doldurulur (hər icarəyə maksimum 1 cərimə ola bilər - UQ_tblFines_BorrowID):
        private async Task LoadBorrows()
        {
            var now = DateTime.Now;

            var borrows = await _context.TblBorrows.AsNoTracking().Where(b => !_context.TblFines.Any(f => f.BorrowId == b.BorrowId)).OrderByDescending(b => b.BorrowDate).Select(b => new
            {
                b.BorrowId,
                BookName = b.Book.BookName,
                UserName = b.User.UserName,
                UserSurname = b.User.UserSurname,
                b.BorrowDate,
                b.BorrowDueDate,
                b.BorrowReturnDate
            }).ToListAsync();

            ViewBag.Borrows = borrows.Select(b =>
            {
                string borrowStatus = b.BorrowReturnDate.HasValue ? "qaytarılıb" : (now > b.BorrowDueDate ? "gecikir" : "aktiv");

                return new SelectListItem
                {
                    Value = b.BorrowId.ToString(),
                    Text = $"{b.BookName} — {b.UserName} {b.UserSurname} (icarə: {b.BorrowDate:dd.MM.yyyy}, {borrowStatus})"
                };
            }).ToList();
        }

        // Create səhifəsində səbəb siyahısı doldurulur:
        private void LoadReasons()
        {
            ViewBag.Reasons = new List<SelectListItem>
            {
                new SelectListItem { Value = ReasonLate, Text = ReasonLate },
                new SelectListItem { Value = ReasonLost, Text = ReasonLost }
            };
        }

        // GET: Admin/Fines/FineCreate
        [HttpGet]
        public async Task<IActionResult> FineCreate()
        {
            var model = new FineVM
            {
                FineReason = ReasonLate
            };

            await LoadBorrows();
            LoadReasons();
            return View(model);
        }

        // POST: Admin/Fines/FineCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FineCreate(FineVM model)
        {
            async Task<IActionResult> Redisplay()
            {
                await LoadBorrows();
                LoadReasons();
                return View(model);
            }

            // Səbəb yalnız DB-nin icazə verdiyi dəyərlərdən biri ola bilər:
            if (model.FineReason != ReasonLate && model.FineReason != ReasonLost)
            {
                ModelState.AddModelError(nameof(model.FineReason), "Cərimənin səbəbi yalnız 'Gecikir' və ya 'İtirilib' ola bilər!");
            }

            if (!ModelState.IsValid)
            {
                return await Redisplay();
            }

            // İcarə mövcud olmalıdır (kitabın stokunu dəyişmək üçün Book da gətirilir):
            var borrow = await _context.TblBorrows.Include(b => b.Book).FirstOrDefaultAsync(b => b.BorrowId == model.BorrowId);

            if (borrow == null)
            {
                ModelState.AddModelError(nameof(model.BorrowId), "Seçilmiş icarə mövcud deyil!");
            }
            else
            {
                // Hər icarəyə maksimum 1 cərimə ola bilər:
                bool hasFine = await _context.TblFines.AnyAsync(f => f.BorrowId == model.BorrowId);

                if (hasFine)
                {
                    ModelState.AddModelError(nameof(model.BorrowId), "Bu icarəyə artıq cərimə yazılıb!");
                }

                // İtirilmə cəriməsi yalnız hələ qaytarılmamış icarə üçün yaradıla bilər:
                if (model.FineReason == ReasonLost && borrow.BorrowReturnDate != null)
                {
                    ModelState.AddModelError(nameof(model.BorrowId), "İtirilmə cəriməsi yalnız hələ qaytarılmamış icarə üçün yaradıla bilər!");
                }
            }

            if (!ModelState.IsValid)
            {
                return await Redisplay();
            }

            // Yeni TblFine entity-si yaradılır (FineDate DB-nin default SYSDATETIME() dəyərini alır):
            var fine = new TblFine
            {
                BorrowId = model.BorrowId,
                FineReason = model.FineReason,
                FineAmount = model.FineAmount,
                FineIsPaid = false
            };

            _context.TblFines.Add(fine);

            // Kitab itirilibsə icarə bağlanır və itirilmiş nüsxə stokdan çıxarılır (stok >= aktiv icarə sayı qaydası pozulmasın deyə hər ikisi eyni SaveChanges-də edilir):
            bool isLost = model.FineReason == ReasonLost;

            if (isLost)
            {
                var now = DateTime.Now;
                borrow!.BorrowReturnDate = now;

                if (borrow.Book.BookStockQuantity > 0)
                {
                    borrow.Book.BookStockQuantity -= 1;
                }

                borrow.Book.BookUpdatedDate = now;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Cəriməni əlavə etmək mümkün olmadı! Bu icarəyə artıq cərimə yazılmış ola bilər.");
                return await Redisplay();
            }

            TempData["Success"] = isLost ? "Cərimə uğurla əlavə edildi! İcarə bağlandı və itirilmiş nüsxə stokdan çıxarıldı." : "Cərimə uğurla əlavə edildi!";
            return RedirectToAction(nameof(FineIndex));
        }

        // Cəriməyə aid göstərilən məlumatlar modelə doldurulur (Borrow, Borrow.User və Borrow.Book Include olunmalıdır):
        private static void FillDisplay(FineVM model, TblFine fine)
        {
            model.BorrowId = fine.BorrowId;
            model.FineReason = fine.FineReason;
            model.FineDate = fine.FineDate;
            model.FineIsPaid = fine.FineIsPaid;
            model.FinePaymentDate = fine.FinePaymentDate;
            model.UserFullName = fine.Borrow.User.UserName + " " + fine.Borrow.User.UserSurname;
            model.UserUsername = fine.Borrow.User.UserUsername;
            model.BookName = fine.Borrow.Book.BookName;
            model.BookIsbn = fine.Borrow.Book.BookIsbn;
            model.BorrowDate = fine.Borrow.BorrowDate;
            model.BorrowDueDate = fine.Borrow.BorrowDueDate;
            model.BorrowReturnDate = fine.Borrow.BorrowReturnDate;
        }

        // Entity-dən ViewModel yaradılır:
        private static FineVM MapToVM(TblFine fine)
        {
            var model = new FineVM
            {
                FineId = fine.FineId,
                FineAmount = fine.FineAmount
            };

            FillDisplay(model, fine);
            return model;
        }

        // Cərimənin mövcud olub-olmadığını yoxlayan metod:
        private bool FineExists(int id)
        {
            return _context.TblFines.Any(f => f.FineId == id);
        }

        // GET: Admin/Fines/FineEdit/5 (yalnız ödənilməmiş cəriməninin məbləğini dəyişmək mümkündür)
        [HttpGet]
        public async Task<IActionResult> FineEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fine = await _context.TblFines.AsNoTracking().Include(f => f.Borrow).ThenInclude(b => b.User).Include(f => f.Borrow).ThenInclude(b => b.Book).FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            // Ödənilmiş cərimə dəyişdirilə bilməz:
            if (fine.FineIsPaid)
            {
                TempData["Error"] = "Ödənilmiş cəriməni redaktə etmək mümkün deyil!";
                return RedirectToAction(nameof(FineIndex));
            }

            return View(MapToVM(fine));
        }

        // POST: Admin/Fines/FineEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FineEdit(int id, FineVM model)
        {
            if (id != model.FineId)
            {
                return NotFound();
            }

            var fine = await _context.TblFines.Include(f => f.Borrow).ThenInclude(b => b.User).Include(f => f.Borrow).ThenInclude(b => b.Book).FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            if (fine.FineIsPaid)
            {
                TempData["Error"] = "Ödənilmiş cəriməni redaktə etmək mümkün deyil!";
                return RedirectToAction(nameof(FineIndex));
            }

            // Formu yenidən göstərmək üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
            IActionResult Redisplay()
            {
                FillDisplay(model, fine);
                return View(model);
            }

            // BorrowId və FineReason bu formda dəyişmir, ona görə onların validasiya xətaları nəzərə alınmır:
            ModelState.Remove(nameof(model.BorrowId));
            ModelState.Remove(nameof(model.FineReason));

            if (!ModelState.IsValid)
            {
                return Redisplay();
            }

            // Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir:
            if (fine.FineAmount == model.FineAmount)
            {
                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return Redisplay();
            }

            fine.FineAmount = model.FineAmount;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FineExists(id))
                {
                    TempData["Error"] = "Bu cərimə artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(FineIndex));
                }
                throw;
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(FineEdit), new { id });
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(FineIndex));
        }

        // GET: Admin/Fines/FinePay/5 (cəriməni ödənilmiş kimi qeyd etmə təsdiq səhifəsi)
        [HttpGet]
        public async Task<IActionResult> FinePay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fine = await _context.TblFines.AsNoTracking().Include(f => f.Borrow).ThenInclude(b => b.User).Include(f => f.Borrow).ThenInclude(b => b.Book).FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            if (fine.FineIsPaid)
            {
                TempData["Error"] = "Bu cərimə artıq ödənilib!";
                return RedirectToAction(nameof(FineIndex));
            }

            return View(MapToVM(fine));
        }

        // POST: Admin/Fines/FinePay/5
        [HttpPost, ActionName("FinePay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinePayConfirmed(int id)
        {
            var fine = await _context.TblFines.FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            // Eyni cərimə iki dəfə ödənilə bilməz:
            if (fine.FineIsPaid)
            {
                TempData["Error"] = "Bu cərimə artıq ödənilib!";
                return RedirectToAction(nameof(FineIndex));
            }

            // CK_tblFines_PaymentDateAfterFineDate: ödəniş tarixi cərimə tarixindən əvvəl ola bilməz (DB serveri ilə vaxt fərqi olsa belə pozulmasın deyə):
            var paymentDate = DateTime.Now;

            if (paymentDate < fine.FineDate)
            {
                paymentDate = fine.FineDate;
            }

            fine.FineIsPaid = true;
            fine.FinePaymentDate = paymentDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Ödənişi yadda saxlamaq mümkün olmadı, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(FineIndex));
            }

            TempData["Success"] = "Cərimə ödənilmiş kimi qeyd edildi!";
            return RedirectToAction(nameof(FineIndex));
        }

        // GET: Admin/Fines/FineDelete/5
        [HttpGet]
        public async Task<IActionResult> FineDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fine = await _context.TblFines.AsNoTracking().Include(f => f.Borrow).ThenInclude(b => b.User).Include(f => f.Borrow).ThenInclude(b => b.Book).FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            return View(MapToVM(fine));
        }

        // POST: Admin/Fines/FineDelete/5
        [HttpPost, ActionName("FineDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FineDeleteConfirmed(int id)
        {
            var fine = await _context.TblFines.FirstOrDefaultAsync(f => f.FineId == id);

            if (fine == null)
            {
                return NotFound();
            }

            // Ödənilmiş cərimə maliyyə qeydidir, silinə bilməz:
            if (fine.FineIsPaid)
            {
                TempData["DeleteError"] = "Ödənilmiş cəriməni silmək mümkün deyil!";
                return RedirectToAction(nameof(FineIndex));
            }

            _context.TblFines.Remove(fine);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["DeleteError"] = "Bu cəriməni silmək mümkün deyil! Cəriməyə bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(FineIndex));
            }

            TempData["Success"] = "Cərimə uğurla silindi!";
            return RedirectToAction(nameof(FineIndex));
        }
    }
}
