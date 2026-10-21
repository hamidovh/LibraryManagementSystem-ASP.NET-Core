/* ~/Areas/Admin/Controllers/CategoriesController.cs: */

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
    //[Authorize] // [Authorize] attributeunun vəzifəsi yazıldığı Controller və ya Actiona yalnız authenticationdan keçmiş istifadəçilərin daxil ola bilməsinə icazə verməkdir.
    //[Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly LMSdbContext _context; // _context LMSdbContext classına aid objectdir. Yəni EF Core vasitəsilə DBə giriş əldə etdiyimiz əsas object _contextdir.
        private readonly IImageStore _images;

        public CategoriesController(LMSdbContext context, IImageStore images) // Constructor metod
        {
            _context = context;
            _images = images;
        }

        // GET: Admin/Categories/CategoryIndex
        [HttpGet]
        public async Task<IActionResult> CategoryIndex(string sortOrder = "date_desc", int page = 1)
        {
            // Bir səhifədə göstəriləcək maksimum kateqoriya sayı:
            int pageSize = 20;

            // Səhifə nömrəsinin 1-dən kiçik olmasının qarşısı alınır:
            if (page < 1)
            {
                page = 1;
            }

            // Kateqoriyalar şəkilləri ilə birlikdə DBdən götürülür:
            var categories = _context.TblCategories.AsNoTracking().Include(c => c.TblCategoryImages).AsQueryable();
            // _context.TblCategories - LMSdbContext classının _context adlı instanceı vasitəsilə DBdəki tblCategories tableına giriş edilir. TblCategories EF Core-da DBin tblCategories tableını təmsil edən DbSet-dir.
            // Include() kateqoriyanı gətirərkən onun əlaqəli şəkillərini də gətirir.

            // Seçilmiş filtrə uyğun sıralama aparılır (default: yenidən köhnəyə) (ThenBy ilə tarixlər eyni olanda belə səhifələmə sabit qalır):
            switch (sortOrder)
            {
                case "name_asc":
                    categories = categories.OrderBy(c => c.CategoryName).ThenBy(c => c.CategoryId);
                    break;
                case "name_desc":
                    categories = categories.OrderByDescending(c => c.CategoryName).ThenByDescending(c => c.CategoryId);
                    break;
                case "date_asc":
                    categories = categories.OrderBy(c => c.CategoryCreatedDate).ThenBy(c => c.CategoryId);
                    break;
                default:
                    categories = categories.OrderByDescending(c => c.CategoryCreatedDate).ThenByDescending(c => c.CategoryId);
                    sortOrder = "date_desc";
                    break;
            }
            // Burada sortOrder parametrinin standart qiyməti date_desc olduğu üçün səhifə ilk açıldıqda kateqoriyalar yenidən köhnəyə sıralanacaq. OrderByDescending() əks istiqamətdə sıralama aparır.

            // Bütün kateqoriyaların ümumi sayı əldə edilir:
            int totalCategories = await categories.CountAsync(); // CountAsync() — verilənlər bazasındakı kateqoriyaların ümumi sayını hesablayır.

            // Ümumi səhifə sayı hesablanır:
            int totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize); // Math.Ceiling() — ümumi saydan asılı olaraq səhifə sayını müəyyən edir. Məsələn, 45 kateqoriya varsa, 3 səhifə yaranır.

            // Mövcud olmayan sonrakı səhifəyə keçidin qarşısı alınır:
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Yalnız cari səhifəyə aid kateqoriyalar sıralanır. Sıralanmış kateqoriyalar ViewModelə çevrilir:
            var categoryList = await categories.Skip((page - 1) * pageSize).Take(pageSize).Select(c => new CategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription,
                CategoryIsActive = c.CategoryIsActive,
                CategoryCreatedDate = c.CategoryCreatedDate,
                CategoryUpdatedDate = c.CategoryUpdatedDate,
                TblCategoryImages = c.TblCategoryImages.OrderBy(i => i.ImageSortOrder).ToList()  // Kateqoriyanın şəkillərini də gətirir.
            }).ToListAsync();
            // Skip((page - 1) * pageSize) — əvvəlki səhifələrdəki kateqoriyaları ötürür.
            // Take(pageSize) — növbəti maksimum 20 kateqoriyanı əldə edir.
            // Select() TblCategory objektlərinin olduğu kimi gəlməsinə imkan varmir, onun əvəzinə hər bir TblCategory objectini götürüb CategoryVM objektinə çevirir (manual mapping). Məsələn, TblCategory-dəki CategoryId propertysinin valuesunu CategoryVM-in CategoryId propertysinə verir (eyni şey bütün propertylərin valueları üçün keçərlidir). Birbaşa TblCategory qaytarmamağın səbəbi CategoryIndex View-sunda Model olaraq CategoryVM çağırılmasıdır. c — TblCategory objektinin hər bir valuesunu müvəqqəti olaraq təmsil edir. Select() projection-da Include ignor olunur.
            // ToListAsync() LINQ sorğusunu DB-də icra edir. DB-dən gələn cavabı List<CategoryVM>-ə çevirir və categories variableına verir.

            // Məlumatlar Viewa ötürülür:
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCategories = totalCategories;

            return View(categoryList);
        }        

        // GET: Admin/Categories/CategoryCreate
        [HttpGet] // GET CategoryCreate.cshtml səhifəsini göstərir, yəni bu action yalnız HTTP GET request üçün istifadə olunur (CategoryCreate.cshtml açılanda GET metodu olan actiona request göndərilir).        
        public IActionResult CategoryCreate() // IActionResult sözü bu actionın istənilən fərqli tipdə nəticə qaytara biləcəyini və nəticənin HTTP response action result olduğunu bildirir bildirir. Yəni, action məsələn return View() və ya return RedirectToAction() və ya NotFound() və ya bir başqa View Result Type qaytara bilər.
        {
            var model = new CategoryVM // CategoryVM classına aid yeni object yaradılır. Objectin adı: "model". 
            {
                CategoryIsActive = true // CategoryCreate.cshtml səhifəsi açıldıqda kateqoriya aktiv vəziyyətdə olur, bunu göstərən checkbox varsa default olaraq işarələnmiş görünür.
            };

            return View(model); // Action CategoryVM classına aid model objectini CategoryCreate.cshtml View-suna göndərir. Çünki View-da Model olaraq CategoryVM çağırılıb.
        }

        // POST: Admin/Categories/CategoryCreate
        [HttpPost] // Post - formdan gələn məlumatları yoxlayır, kateqoriyanı DB-ə əlavə edir, şəkil varsa onu wwwroot/Uploads folderinə və DB-in tblCategoryImages tableına save edir. Üstünə [HttpPost] yazılmış action HTTP POST request qəbul edir, yəni bu actiona http tərəfindən form göndərilir, form action tərəfindən validate olunur, validationlardan keçərsə DB-ə yeni valuelar save edilir.
        // [HttpPost] sözü Actionın yalnız HTTP POST request üçün nəzərdə tutulduğunu bildirir. (GET formu açır, POST formdan gələn dataları qəbul edir və DB-i modify edir.) POST metodu form submit edildikdən (Save buttonuna basıldıqdan) sonra işləyir. Save buttonuna basıldıqda Browser HTTP POST request göndərir və bu action işə düşür.
        [ValidateAntiForgeryToken] // ASP.NET Core avtomatik olaraq POST metodları üçün xüsusi token yaradır və POST request zamanı həmin tokeni yoxlayır. Bu token bu actiona başqa yerdən POST request gəlməsinin qarşısını alır (Anti-forgery token, serverin formla birlikdə göndərdiyi təsadüfi sətirdir. Server onu POST zamanı müqayisə edir, başqa saytdan göndərilən saxta formda bu token olmur və request rədd edilir (CSRF qorunması)).
        // [ValidateAntiForgeryToken] təhlükəsizlik üçündür. Server form üçün (POST metodları üçün) xüsusi, random bir təhlükəsizlik kodu yaradır. Form submit ediləndə həmin kod da göndərilir. Server gələn kodu yoxlayır, gələn kod serverin yaratdığı kod deyilsə server formun başqa yerdən submit olunduğunu "düşünərək" dataları qəbul etmir. Yəni məsələn, başqa bir sayt istifadəçisi öz Browserindən bu proqrama icazəsiz POST göndərməyə çalışanda token yoxlanışı bu requesti bloklayır.
        public async Task<IActionResult> CategoryCreate(CategoryVM model) // Formdan gələn datalar CategoryVM classının model adlı objectinə yerləşdirilir (model binding - form datalarının class propertylərinə doldurulmasıdır (Model binding formdan gələn datanın avtomatik olaraq objectə doldurulmasıdır)). async sözü metodun daxilində DB-dən asinxron şəkildə data alınacağını və ya DB-ə data veriləcəyini bildirir (async sayəsində metodun içərisində await istifadə olunur). Task sözü metodun (actionın) gələcəkdə (əməliyyatlar bitdikdən sonra) nəticə qaytaracağını bildirir.
        {
            // Şəkil validasiyası (şəkil seçilməyibsə null qayıdır):
            string? imageError = _images.Validate(model.ImageFile);
            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            // Form data validation:
            if (!ModelState.IsValid) // ModelState formdan gələn məlumatların validation nəticələrini (daxil edilmiş valueların tələb olunan formatlarda olub-olmadığını) yoxlayır və saxlayır. Əgər daxil edilmiş datalardan ən az biri validationdan keçməyibsə bu if true gəlir, nəticədə action bu if blokuna düşür, form yenidən açılır, istifadəçinin daxil etdiyi məlumatlar itmir, ancaq DB-ə də heç nə save olunmur.
            {
                return View(model);
            }

            model.CategoryName = model.CategoryName.Trim(); // Boşluqları yığışdırır.
            model.CategoryDescription = string.IsNullOrWhiteSpace(model.CategoryDescription) ? null : model.CategoryDescription.Trim();

            // Eyni adlı kateqoriya artıq varsa, yenisi yaradılmır (DB-də UNIQUE constraint var):
            bool categoryExists = await _context.TblCategories.AnyAsync(c => c.CategoryName == model.CategoryName);
            if (categoryExists)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "Bu adda kateqoriya artıq mövcuddur!");
                return View(model);
            }

            var category = new TblCategory // Entity yaradılır (Entity TblCategory classına aid, CategoryVM-dən valueları götürmüş objectdir). Yuxarıdakı model adlı variable CategoryVM ViewModelinin objectidir, buradakı category variableı isə DB Entity-sidir.
            {
                CategoryName = model.CategoryName, // Formdan gələn və CategoryVM-in model adlı objectinə verilmiş CategoryName dəyəri database Entity-sinin (TblCategory classının) CategoryName propertysinə verilir.
                CategoryDescription = model.CategoryDescription,
                CategoryIsActive = model.CategoryIsActive
            };

            // Şəkil varsa əvvəl fayl yazılır, record kateqoriyaya bağlanır (EF FK-ni özü qoyur):
            string? newFileName = null;
            if (model.ImageFile is { Length: > 0 })
            {
                newFileName = await _images.SaveAsync(model.ImageFile);
                category.TblCategoryImages.Add(new TblCategoryImage { ImagePath = newFileName, ImageSortOrder = 0 });
            }

            _context.TblCategories.Add(category); // Entity TblCategory tableına əlavə olunur, EF Core category objectini DB-ə əlavə olunacaq Entity kimi saxlayır.
            try
            {
                // Kateqoriya və şəkil qeydi tək əməliyyatda yazılır:
                await _context.SaveChangesAsync(); // EF Core Entity-ni DB-ə insert edir (valuelar tblCategories tableına save olunur).
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName); // Yetim fayl qalmasın deyə.
                ModelState.AddModelError(string.Empty, "Kateqoriyanı əlavə etmək mümkün olmadı! Eyni adlı kateqoriya artıq mövcud ola bilər.");
                return View(model);
            }

            TempData["Success"] = "Kateqoriya uğurla əlavə edildi!";
            return RedirectToAction(nameof(CategoryIndex)); // CategoryCreate View qaytarılmır. Save olunmuş datalar, yəni yenilənmiş CategoryIndex göstərilir.
        }

        // GET: Admin/Categories/CategoryEdit/5
        [HttpGet]
        public async Task<IActionResult> CategoryEdit(int? id) // Bu action Edit səhifəsini açır və seçilmiş kateqoriyanın mövcud məlumatlarını View-ya göndərir. id URL-dən gələn kateqoriyanın ID-sidir və null ola bilər (int?).
        {
            if (id == null) // URL-dən Category ID gəlib-gəlmədiyini yoxlayır, gəlməyibsə 404 qaytarır.
            {
                return NotFound();
            }

            var category = await _context.TblCategories.AsNoTracking().Include(c => c.TblCategoryImages).FirstOrDefaultAsync(c => c.CategoryId == id); // Bu sətir CategorID valuesu URL-dən gələn id-yə bərabər olan Kateqoriyanı LINQ kodu vasitəsilə DB-dən tapıb gətirir və category adlı variable-a bərabər edir. LINQ kodu Edit səhifəsinə kateqoriyanın mövcud şəklini də gətirir. (TblCategories DB-dəki tblCategories tableının EF Core-dakı qarşılığıdır (PMC-da həyata keçirilmiş Scaffolding nəticəsində yaranıb). Include() kateqoriyanın şəkillərini də gətirir, Include() yazılmasaydı təkcə TblCategory Entity-si gələcəkdi, Include Kateqoriyanı gətirərkən ona bağlı TblCategoryImages məlumatlarını da gətirir. c - TblCategory objectinin təmsilçisidir. FirstOrDefaultAsync() URL ilə göndərilən id-yə bərabər olan Kateqoriyanı DB-də axtarıb uyğun gələn nəticələrdən yalnız birincini gətirir, əgər heç nə tapmasa null qaytarır və category variableının valuesu null olur.)

            if (category == null) // category variableının valuesu null olarsa, yəni FirstOrDefaultAsync() əmri DB-dən heç nə tapmazsa action bu if-ə düşür və 404 qaytarır.
            {
                return NotFound();
            }

            var model = new CategoryVM // Entity-dən (TblCategory classına aid category adlı objectdən) ViewModel yaradılır və model adlı variablea value olaraq verilir. category adlı variable DB-dən gələn TblCategory Entity-sidir. View-da isə CategoryVM çağırılır. Ona görə də, DB Entity-si CategoryVM-in objectinə çevrilir (verilir).
            {
                // Model binding (manual mapping): DB-dən gələn məlumatlar CategorVM ViewModelinin propertylərinə bərabər edilir ki, Action və View onları istifadə edə bilsin.
                CategoryId = category.CategoryId, // Edit üçün ID vacibdir, çünki POST zamanı sistem hansı category-nin edit edildiyini dəqiq bilməlidir.
                CategoryName = category.CategoryName, // DB-dəki category adı ViewModel-ə köçürülür.
                CategoryDescription = category.CategoryDescription,
                CategoryIsActive = category.CategoryIsActive,
                CategoryCreatedDate = category.CategoryCreatedDate,
                CategoryUpdatedDate = category.CategoryUpdatedDate,
                TblCategoryImages = category.TblCategoryImages.OrderBy(i => i.ImageSortOrder).ToList() // Kateqoriyanın mövcud şəkli (şəkilləri) də ViewModel-ə ötürülür və ImageSortOrderə əsasən sıralanırlar.
            };

            return View(model); // CategoryVM-ə aid model adlı object View-a ötürülür və CategoryEdit.cshtml səhifəsi açılır.
        }

        // Kateqoriyanın mövcud olub-olmadığını yoxlayan metod:
        private bool CategoryExists(int id) // CategoryExists metodu verilən id-yə sahib category olub-olmadığını yoxlayır, varsa true qaytarır.
        {
            return _context.TblCategories.Any(c => c.CategoryId == id);
        }

        // POST: Admin/Categories/CategoryEdit/5
        [HttpPost] // Actionın yalnız HTTP POST request üçün nəzərdə tutulduğunu bildirir. (GET formu açır, POST formdan gələn dataları qəbul edir və DB-i modify edir.) POST metodu form submit edildikdən (Save buttonuna basıldıqdan) sonra işləyir. Save buttonuna basıldıqda Browser HTTP POST request göndərir və bu action işə düşür.
        [ValidateAntiForgeryToken] // Bu təhlükəsizlik üçündür. Server form üçün xüsusi, random bir təhlükəsizlik kodu yaradır. Form submit ediləndə həmin kod da göndərilir. Server gələn kodu yoxlayır, gələn kod serverin yaratdığı kod deyilsə server formun başqa yerdən submit olunduğunu "düşünərək" dataları qəbul etmir. Yəni məsələn, başqa bir sayt istifadəçisi öz Browserindən bu proqrama icazəsiz POST göndərməyə çalışanda token yoxlanışı bu requesti bloklayır.
        public async Task<IActionResult> CategoryEdit(int id, CategoryVM model) // Bu action iki dənə parameter qəbul edir, biri URL-dən gələn id, digəri CategoryVM-in model adlı objectinə doldurulmuş (model binding - manual mapping edilmiş) formdan gələn məlumatlar.
        {
            if (id != model.CategoryId) // URL-dən gələn id-nin formdan gələn id-yə (CategoryId-yə) bərabər olub-olmadığı yoxlanılır, bərabər deyilsə action 404 qaytarır.
            {
                return NotFound(); // Uyğunsuzluq varsa POST request dayandırılır.
            } // URL-in göstərdiyi category ilə formun göstərdiyi category eyni olmalıdır. URL-də 5-ci category-ni edit etməyə gəlinibsə, amma form 10-cu category-ni göstərirsə bu düzgün deyil. Ona görə də, bu yoxlama aparılır.

            var category = await _context.TblCategories.Include(c => c.TblCategoryImages).FirstOrDefaultAsync(c => c.CategoryId == id); // Edit olunacaq real DB Entity-sini tapmaq üçün DB-dən category yenidən axtarılır. Çünki, model CategoryVM ViewModel-inin objectidir və DB-i birbaşa ViewModel ilə update etmək olmaz. Əvvəl DB Entity-si tapılmalıdır, sonra onun məlumatları yenilənməlidir.
            // Burada Kateqoriya yenidən tapılır və gətirilir, çünki GET-də tapılıb category objectinə verilmiş Entity artıq istifadə edilmir (o Entity ViewModelə çevrildi). POST yeni HTTP requestdir, ona görə də DB-dən kateqoriyanı POST üçün yenidən götürürük.
            // POST-da yenidən yükləməyin əsl səbəbi DbContext-in hər HTTP request üçün ayrıca yaranmasıdır (scoped lifetime).

            if (category == null) // category tapılmasa, yəni ID DB-də yoxdursa: 404.
            {
                return NotFound();
            }

            // POST zamanı formdan gəlməyən, amma View-ya lazım olan sahələri doldurur:
            void Refill()
            {
                model.CategoryCreatedDate = category.CategoryCreatedDate;
                model.CategoryUpdatedDate = category.CategoryUpdatedDate;
                model.TblCategoryImages = category.TblCategoryImages.OrderBy(i => i.ImageSortOrder).ToList();
            }

            // Heç nə silinməzdən ƏVVƏL şəkil yoxlanılır:
            string? imageError = _images.Validate(model.ImageFile);

            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid) // Form məlumatlarının validation-dan keçib-keçmədiyini yoxlayır. Məsələn, tələb olunan sahələrin (inputların) boş saxlanılıb-saxlanılmadığına baxır.
            {
                Refill();

                return View(model); // Validation uğursuzdursa form yenidən açılır, istifadəçinin yazdığı məlumatlar itmir, modeldə qalır, ancaq DB-ə heç nə save olunmur.
            }

            model.CategoryName = model.CategoryName.Trim(); // Boşluqları yığışdırır.
            model.CategoryDescription = string.IsNullOrWhiteSpace(model.CategoryDescription) ? null : model.CategoryDescription.Trim();

            // Dəyişiklik yoxlaması (Ordinal: "kitab" --> "Kitab" da dəyişiklik sayılır). Heç bir dəyişiklik edilməyibsə POST icra olunmur (DB-ə heç nə save edilmir) və Edit səhifəsində xəbərdarlıq mesajı göstərilir. Ordinal müqayisə böyük-kiçik hərf dəyişikliyinin də dəyişiklik hesab edilməsi üçündür. Yeni şəkil seçilməyibsə və mövcud şəkil silinməyibsə, şəkil üzrə dəyişiklik yoxdur:
            bool noImageChange = (model.ImageFile is null || model.ImageFile.Length == 0) && !model.DeleteImage;

            bool isNotChanged = string.Equals(category.CategoryName, model.CategoryName, StringComparison.Ordinal) && string.Equals(category.CategoryDescription ?? string.Empty, model.CategoryDescription ?? string.Empty, StringComparison.Ordinal) && category.CategoryIsActive == model.CategoryIsActive && noImageChange;

            if (isNotChanged)
            {
                // Formda göstərilməsi üçün DB-dəki məlumatlar modelə yenidən doldurulur (POST zamanı bu sahələr formdan gəlmir):
                Refill();

                ModelState.AddModelError(string.Empty, "Heç bir dəyişiklik edilməyib!");
                return View(model);
            }

            // Eyni adın başqa kateqoriyaya aid olub-olmadığı yoxlanılır:
            bool categoryExists = await _context.TblCategories.AnyAsync(c => c.CategoryName == model.CategoryName && c.CategoryId != id);
            if (categoryExists)
            {
                Refill();

                ModelState.AddModelError(nameof(model.CategoryName), "Bu adda başqa kateqoriya artıq mövcuddur!");
                return View(model);
            }

            // Edit əməliyyatı başlayır, Kateqoriya məlumatları yenilənir (dəyişilir). Formdan gələn və CategoryVM-in model adlı objectində saxlanılan yeni məlumatlar DB Entity-sinə, yəni TblCategory classının category adlı objectinə verilir:
            category.CategoryName = model.CategoryName;
            category.CategoryDescription = model.CategoryDescription;
            category.CategoryIsActive = model.CategoryIsActive;
            category.CategoryUpdatedDate = DateTime.Now; // Kateqoriyanın son dəyişdirildiyi vaxt olaraq indiki tarix və saat qeyd olunur.

            // Mövcud kateqoriya şəkilləri (köhnə, əvvəldən var olan şəkillər) tapılır və birincidən sonuncuya doğru sıralanırlar, oldImage variableına bərabər edilirlər (əgər şəkil yoxdursa oldImage = null olur):
            var oldImage = category.TblCategoryImages.OrderBy(i => i.ImageSortOrder).FirstOrDefault();
            var filesToDelete = new List<string>(); // DB uğurlu olandan SONRA silinəcək fayllar
            string? newFileName = null;

            // İstifadəçi mövcud şəkli silmək istəyirsə (fayl hələ silinmir, yalnız qeyd olunur):
            if (model.DeleteImage && oldImage != null)
            {
                filesToDelete.Add(oldImage.ImagePath);
                _context.TblCategoryImages.Remove(oldImage);
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
                    category.TblCategoryImages.Add(new TblCategoryImage { ImagePath = newFileName, ImageSortOrder = 0 });
                }
            }

            try
            {
                await _context.SaveChangesAsync(); // Bütün dəyişikliklər DB-ə göndərilir və save olunur.
            }
            catch (DbUpdateConcurrencyException) // Update zamanı gözlənilməz uyğunsuzluq baş verərsə action catch-ə düşür.
            {
                _images.Delete(newFileName); // Yalnız yeni fayl silinir, köhnələrə toxunulmur.

                if (!CategoryExists(id))
                {
                    TempData["Error"] = "Bu kateqoriya artıq mövcud deyil (silinib)!";
                    return RedirectToAction(nameof(CategoryIndex)); // CategoryExists metodu false olarsa tək səfərlik error mesaj görsənir və CategoryIndex səhifəsi açılır.
                }
                throw; // CategoryExists metodu true olarsa exception yenidən yuxarı ötürülür (throw; exception-ı yuxarı ötürür və istifadəçi 500 xətası görür).              
            }
            catch (DbUpdateException)
            {
                _images.Delete(newFileName);

                TempData["Error"] = "Dəyişiklikləri yadda saxlamaq mümkün olmadı! Eyni adlı kateqoriya yaranmış ola bilər, zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(CategoryEdit), new { id });
            }

            // DB uğurlu olduqdan sonra köhnə fiziki fayllar silinir:
            foreach (var fileName in filesToDelete)
            {
                _images.Delete(fileName);
            }

            TempData["Success"] = "Dəyişikliklər uğurla yadda saxlanıldı!";
            return RedirectToAction(nameof(CategoryIndex)); // Edit əməliyyatı uğurla bitdiyi halda proqram Edit View-dan çıxır və yenilənmiş CategoryIndex səhifəsinə gedir.
        }

        // GET: Admin/Categories/CategoryDelete/5
        [HttpGet]
        public async Task<IActionResult> CategoryDelete(int? id) // GET CategoryDelete - silinəcək kateqoriyanın məlumatlarını və şəkillərini tapıb təsdiq səhifəsində göstərir.
        {
            if (id == null) // Bu yoxlama vacibdir, çünki POST metodu konkret kateqoriya axtaracaq və ID-si olmadan hansı kateqoriyanın silinəcəyi bilinməz.
            {
                return NotFound(); // 404
            }

            var category = await _context.TblCategories.AsNoTracking().Include(c => c.TblCategoryImages).FirstOrDefaultAsync(c => c.CategoryId == id); // Kateqoriya DB-dən tapılır və category adlı variableın valuesu olur. category objecti TblCategory tutur.

            if (category == null) // FirstOrDefaultAsync() heç nə tapmasa (yəni id gəlib (action yuxarıdakı if blokuna düşməyib), amma gəlmiş id-yə uyğun kateqoriya DB-də yoxdur): 404.
            {
                return NotFound();
            }

            var model = new CategoryVM // DB-dən gələn Entity ViewModelə çevrilir və CategoryVM-ə aid model adlı objectə verilir.
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryIsActive = category.CategoryIsActive,
                CategoryCreatedDate = category.CategoryCreatedDate,
                CategoryUpdatedDate = category.CategoryUpdatedDate,
                TblCategoryImages = category.TblCategoryImages.OrderBy(i => i.ImageSortOrder).ToList() // i hər bir şəkil objectini təmsil edir.
            };

            return View(model); // CategoryVM-in model adlı objecti hazırdır və View-ya göndərilir.
        }

        // POST: Admin/Categories/CategoryDelete/5
        [HttpPost, ActionName("CategoryDelete")] // GET metodu silmə səhifəsini açır, POST metodu silmə əməliyyatını təsdiqləyir və icra edir. C# metod adı CategoryDeleteConfirmed(), MVC action adı CategoryDelete olacaq, çünki C#-da eyni parametrləri gözləyən metod adları eyni ola bilməz (overloading xaric).
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDeleteConfirmed(int id) // POST CategoryDeleteConfirmed - istifadəçi təsdiqlədikdən (delete düyməsinə basdıqdan) sonra bu action işləyir və kateqoriyanı DB-dən və ona aid şəkilləri wwwroot/Uploads qovluğundan fiziki olaraq silir.
        {
            var category = await _context.TblCategories.Include(c => c.TblCategoryImages).FirstOrDefaultAsync(c => c.CategoryId == id); // Kateqoriya yenidən tapılır və gətirilir, çünki GET-də tapılıb category objectinə verilmiş DbSet artıq istifadə edilmir (o DbSet ViewModelə çevrildi). POST yeni HTTP requestdir, ona görə də DB-dən kateqoriyanı POST üçün yenidən götürürük.

            if (category == null)
            {
                return NotFound(); // Əgər istifadəçi olmayan ID ilə request göndəribsə: 404.
            }

            // Kateqoriyaya bağlı kitab olub-olmadığını yoxlanılır (tblBookCategories-də ON DELETE CASCADE olduğu üçün bu yoxlama olmasa, kateqoriya silinəndə kitab-kateqoriya əlaqələri səssizcə silinərdi).:
            bool hasBooks = await _context.TblBooks.AnyAsync(b => b.Categories.Any(c => c.CategoryId == id)); // DB-də Kateqoriyaya bağlı kitab(lar) varsa hasBooks variableının valuesu true olur. _context.TblBooks - DB-dəki tblBooks cədvəlinə giriş əldə edir, AnyAsync() əlaqəli tabledə (tblCategoriesdə) ən az bir uyğun qeydin olub-olmadığına baxır, varsa true qaytarır. b - bir kitabdır, b.Categories - həmin kitabın aid olduğu kateqoriyalardır. Any(c => c.CategoryId == id) - kitabın aid olduğu kateqoriyaların arasında silinmək istənilən id-yə bərabər CategoryId-yə sahib kateqoriyanın olub-olmadığını yoxlayır, varsa true qaytarır.

            if (hasBooks) // Bu kateqoriyanın heç olmasa 1 kitabı var? Əgər varsa silmə.
            {
                TempData["DeleteError"] = "Bu kateqoriyanı silmək mümkün deyil! Kateqoriyaya bağlı kitablar mövcuddur."; // TempData[] vasitəsilə istifadəçinin cəmi bir dəfə görəcəyi mesaj tutulur və bu mesaj TempData[] vasitəsilə View-a daşınacaq.
                return RedirectToAction(nameof(CategoryIndex)); // İstifadəçi heç bir dəyişikliyin baş vermədiyi CategoryIndex səhifəsinə qaytarılır.
            }

            // Silinəcək şəkillərin fayl adları silinmədən əvvəl yadda saxlanılır:
            var imagePaths = category.TblCategoryImages.Select(i => i.ImagePath).ToList();            

            _context.TblCategories.Remove(category); // Burada EF Core category adlı DbSet tutan Entity-ni silir.

            try
            {
                await _context.SaveChangesAsync(); // Kateqoriya DB-dən silinir və dəyişikliklər save olunur (dəyişikliklər DB-ə tətbiq olunur).
            }
            catch (DbUpdateException) // DB update zamanı problem yaranarsa action catch-ə düşür.
            {
                // Kateqoriyaya bağlı kitab varsa FK constraint səbəbindən silinməyə bilər.
                TempData["DeleteError"] = "Bu kateqoriyanı silmək mümkün deyil! Kateqoriyaya bağlı məlumatlar mövcuddur.";
                return RedirectToAction(nameof(CategoryIndex)); // İstifadəçi error mesajı görür və heç bir dəyişiklik baş verməyən CategoryIndex səhifəsinə qaytarılır.
            }

            // DB-dən silindikdən sonra fiziki şəkillər silinir:
            foreach (var imagePath in imagePaths)
            {
                _images.Delete(imagePath);
            } // Kateqoriya silindikdən sonra kateqoriyanın şəkilləri fiziki olaraq diskdən silinir. Kateqoriya silinməzdən əvvəl onun şəkilləri imagePaths objectində list olaraq saxlanılmışdı və həmin saxlanılmış fiziki şəkil faylları bu foreach daxilində diskdən tamamilə silinir. SaveChangesAsync() uğurlu olduğu halda fiziki şəkilləri silməkdə məqsəd FK səbəbindən kateqoriya DB-dən silinməsə onun şəkillərinin də silinməməsini təmin etməkdir.

            TempData["Success"] = "Kateqoriya uğurla silindi!";
            return RedirectToAction(nameof(CategoryIndex)); // Nəticə uğurlu olarsa istifadəçi dəyişikliklərin baş tutduğu CategoyIndex səhifəsinə yönləndirilir.
        }
    }
}

// [Authorize] attribute-unun vəzifəsi yazıldığı Controller və ya Action-a yalnız authentication-dan keçmiş istifadəçilər daxil ola bilməsinə icazə verməkdir. [Authorize] istifadəçini login etmir, sadəcə login olub-olmadığını yoxlayır. [Authorize] Program.cs-dəki app.UseAuthentication(); vasitəsilə öz işini yerinə yetirir, yəni istifadəçinin login olub-olmadığını öyrənir. app.UseAuthentication(); Authentication Middleware-dir və bu middleware request gələndə authentication cookie-ni yoxlayır. Middleware HTTP request və response arasında işləyən proqram komponentidir; Middleware request-i emal edən aralıq proqram komponentidir, Request controller-ə çatmamış müəyyən yoxlamalar aparır. Hər middleware request-i yoxlaya bilər, dəyişdirə bilər, məlumat əlavə edə bilər, request-i dayandıra bilər, növbəti middleware-ə ötürə bilər. Response geri qayıdanda da middleware-lərdən keçə bilər. Middleware-lərin ardıcıllığına pipeline deyilir, məsələn, Program.cs-də alt-alta yazılmış app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.MapStaticAssets(); app.MapControllerRoute(...); kimi hissələr var. Bunların birlikdə əmələ gətirdiyi request-processing ardıcıllığı HTTP request pipeline adlanır. Yəni, pipeline request-in tətbiq daxilində keçdiyi middleware-lərin ardıcıllığıdır, middleware-lərin ardıcıl işləmə sistemidir.
