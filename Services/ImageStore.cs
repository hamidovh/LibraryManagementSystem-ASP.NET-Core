/* ~/Services/ImageStore.cs: */

namespace LibraryManagementSystem.Services
{
    public interface IImageStore
    {
        // Xəta varsa xəta mesajını qaytarır, yoxdursa null qaytarır (fayl seçilməyibsə də null qaytarır):
        string? Validate(IFormFile? file);

        // Faylı wwwroot/Uploads qovluğuna yazır və fayl adını qaytarır:
        Task<string> SaveAsync(IFormFile file);

        // Faylı diskdən silir (ad boşdursa və ya fayl yoxdursa heç nə etmir):
        void Delete(string? fileName);
    }

    public class ImageStore : IImageStore
    {
        // Yüklənməsinə icazə verilən fayl (şəkil) uzantıları (extensionlar):
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" }; // string[] — yəni string-lərdən ibarət arrayi bildirir, arrayin içində icazə verilən fayl (şəkil) uzantıları var. Bu məlumatlar dəyişmədiyi üçün static-dir və bu arraydən istifadə etmək üçün bu arrayin base classına aid object yaratmağa ehtiyac yoxdur. private sözü bu arrayi yalnız bu class daxilində istifadə etməyə icazə verir, başqa classlar bu arrayə birbaşa giriş əldə edə bilməz. readonly sözü arrayə başqa yerdə yeni bir value verilməsinin qarşısını alır.

        // Faylın formatını və tipini (faylın məzmun növünü, yəni MIME type-ını) yoxlamaq üçün array (şəkil üçün icazə verilən ContentTypelar):
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        // Maksimum şəkil ölçüsü 5 MB:
        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB. Şəkil faylının ölçüsü maksimum 5 MB ola bilər (constant - sabit value). long böyük intdir (fayl ölçüsü bayt ilə ifadə olunduğu üçün data type olaraq long istifadə edilir).

        private readonly string _folder;

        public ImageStore(IWebHostEnvironment env)
        {
            _folder = Path.Combine(env.WebRootPath, "Uploads");
        }

        // Yüklənən faylın icazə verilən şəkil formatında olub-olmadığını, yəni şəklin düzgün olub-olmadığını yoxlayan metod:
        public string? Validate(IFormFile? file) // Bu metod ya null ya da error mesajı (string) qaytaracaq (metodun tipi stringdir, ? işarəsi null qaytara biləcəyini də göstərir). IFormFile interfacei istifadəçinin upload etmək üçün seçdiyi faylı value kimi tutur və file adlı variablea verir (file variableının data typeı IFormFiledır). Yəni, IFormFile ASP.NET Core-da HTTP request ilə göndərilən upload edilmiş faylı təmsil edir.
        {
            // Fayl (şəkil) seçilməyibsə xəta yoxdur, validation lazım deyil:
            if (file is null || file.Length == 0)
            {
                return null; // Şəkil seçmək məcburi olmadığı üçün şəkil seçilmədikdə metod bu if blokuna düşür və null qaytarır.
            } // Bu if istifadəçi tərəfindən fayl seçilib-seçilmədiyini yoxlayır. file variableı nulldırsa və ya ölçüsü 0-dırsa bu if true olur və metod null qaytarır.

            // Fayl ölçüsü yoxlanılır:
            if (file.Length > MaxImageSize)
            {
                return "Şəklin ölçüsü maksimum 5 MB ola bilər!";
            } // Bu if faylın ölçüsünü yoxlayır, seçilmiş faylın ölçüsü 5 MB-dan (yəni MaxImageSize variableının valuesundan) böyük olarsa metod bu if blokuna düşür və buradakı error mesajını qaytarır. Metod bu if-ə düşərsə, yəni bu if true olarsa bu if error mesajı qaytaracaq və metod burda dayanacaq, çünki seçilmiş şəkil faylı validationdan keçmədi, ona görə də, method aşağıdakı sətirləri run etdirə bilməz. Metodun aşağıdakı sətirləri run etdirə bilməsi (yəni dayanmayıb öz işinə davam edə bilməsi) üçün bu if-in şərti false gəlməlidir. Bu if-in şərti true gələrsə metod error mesajı qaytaracaq və dayanacaq.

            // Faylın extension-ını götürülür:
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant(); // file.FileName faylın adını verir (FileName IFormFile interfaceinin propertysidir), Path.GetExtension() faylın adının yalnız extension hissəsini götürür, ToLowerInvariant() extension hissəsinin bütün hərflərini kiçik hərflərə çevirir (lower case). Bütün hərfləri kiçik etməkdə məqsəd bütün yazılış formalarını (böyük-kiçik) eyni hala salmaqdır. Nəticədə extension variableının valuesu seçilmiş faylın uzantısının (nöqtədən sonrakı hissəsinin) kiçik hərflərlə yazılmış forması olur.

            // Extension icazə verilən formatlar arasında deyilsə:
            if (!AllowedExtensions.Contains(extension)) // Bu if extension variableının valuesunun AllowedExtensions arrayindəki valuelardan biri ilə üst-üstə düşüb-düşmədiyini yoxlayır, əgər üst-üstə düşmürsə bu if true gəlir və error mesajı qaytarır, yəni metod false olur və dayanır, çünki seçilmiş fayl validationdan keçə bilmədi. 
            {
                return "Yalnız JPG, JPEG, PNG və WEBP uzantılarına sahib şəkil yükləyə bilərsiniz!";
            }

            // ContentType yoxlanılır:
            string contentType = (file.ContentType ?? string.Empty).ToLowerInvariant(); // file.ContentType seçilmiş faylın formatını (məsələn, şəkildi yoxsa tekstdi) verir (ContentType IFormFile interfaceinin propertysidir), ToLowerInvariant() faylın content-type mətnini kiçildir. Əgər fayl image formatında deyilsə (allowedContentTypes arrayinin valuelarından biri ilə üst-üstə düşmürsə) bu if true gəlir, metod false olur, fayl validationdan keçə bilmir və metod dayanır, bu ifdən gələn error mesajı görünür:
            if (!AllowedContentTypes.Contains(contentType))
            {
                return "Yüklənən fayl şəkil formatında deyil!";
            }
            // Həm extension, həm də format (MIME) yoxlanması həqiqətən şəkil faylı upload edilib-edilmədiyini bilmək üçün aparılır.

            // Fayl bütün validationlardan keçibsə (yəni ilk if bloku xaric yuxarıdakı if blokları false gəlibsə) metod null qaytarır. Fayl seçilməyibsə metod null qaytarır. Seçilibsə və seçilmiş faylın ölçüsü 5 MB-dan (yəni MaxImageSize variableının valuesundan) kiçikdirsə, extensionı və formatı yuxarıdakı arraylərdəki kimidirsə metod null qaytarır:
            return null;
        }

        public async Task<string> SaveAsync(IFormFile file)
        {
            Directory.CreateDirectory(_folder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLowerInvariant();
            string filePath = Path.Combine(_folder, fileName);
            await using var stream = new FileStream(filePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return fileName;
        }

        public void Delete(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }
            // Path.GetFileName yalnız fayl adını saxlayır (başqa qovluğa çıxışın qarşısını alır):
            string filePath = Path.Combine(_folder, Path.GetFileName(fileName));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}

// Əməliyyat sırası: əvvəl validasiya, sonra yeni faylı yaz, sonra SaveChanges, ən sonda köhnə faylları sil. Uğursuz olsa yalnız yeni fayl silinir, köhnələrə toxunulmur.

/*
        // Yüklənməsinə icazə verilən fayl (şəkil) uzantıları:
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" }; // string[] — yəni string-lərdən ibarət arrayi bildirir, arrayin içində icazə verilən fayl (şəkil) uzantıları var. Bu məlumatlar dəyişmədiyi üçün static-dir və bu arraydən istifadə etmək üçün classa aid object yaratmağa ehtiyac yoxdur. private sözü bu arrayi yalnız bu class daxilində istifadə etməyə icazə verir, başqa classlar bu arrayə birbaşa giriş əldə edə bilməz. readonly sözü arrayə başqa yerdə yeni bir value verilməsinin qarşısını alır.

        // Faylın formatını və tipini (faylın məzmun növünü) (MIME type-ını) yoxlamaq üçün array:
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        private const long MaxImageSize = 5 * 1024 * 1024; // Şəkil faylının ölçüsü maksimum 5 MB ola bilər (constant - sabit value). long böyük intdir (fayl ölçüsü bayt ilə ifadə olunduğu üçün data type olaraq long istifadə edilir).

        // Yüklənən faylın icazə verilən şəkil formatında olub-olmadığını yoxlayan metod:
        private bool ValidateImage(IFormFile imageFile, out string errorMessage) // Bu metod ya true, ya da false və error mesajı (string) qaytaracaq (metodun tipi booldur, ancaq error mesajının tipi stringdir, ona görə də booldan başqa tipdə valuenun gələ bilməsi üçün stringin qabağında out sözü istifadə olunub, out sözü metodun booldan (true və ya falsedan) başqa tərəfə (false olduqda error mesajını da çağıran tərəfə) çıxa bilməsini təmin edir).
        // IFormFile interfacei istifadəçinin upload etmək üçün seçdiyi faylı value (parametr) kimi tutur və imageFile adlı variablea verir (imageFile variableının data typeı IFormFile-dır). Yəni, IFormFile ASP.NET Core-da HTTP request ilə göndərilən upload edilmiş faylı təmsil edir.
        {
            errorMessage = string.Empty; // Error mesajı ilk başda boşaldılır: errorMessage = "";.

            // Fayl seçilməyibsə xəta yoxdur, validation lazım deyil:
            if (imageFile == null || imageFile.Length == 0)
            {
                return true; // Şəkil seçmək məcburi olmadığı üçün şəkil seçilmədikdə bu if və metod true qaytarır.
            } // Bu if istifadəçi tərəfindən fayl seçilib-seçilmədiyini yoxlayır. imageFile nulldırsa və ya ölçüsü 0-dırsa bu if true olur və metod true qaytarır.

            // Fayl ölçüsünü yoxlanılır:
            if (imageFile.Length > MaxImageSize)
            {
                errorMessage = "Şəklin ölçüsü maksimum 5 MB ola bilər!";
                return false;
            } // Bu if faylın ölçüsünü yoxlayır, seçilmiş faylın ölçüsü 5 MB-dan (yəni MaxImageSize variableının valuesundan) böyük olarsa error mesajı və false qaytarır. Metod bu if-ə düşərsə, yəni bu if true olarsa bu if error mesajı və false qaytaracaq və metod burda dayanacaq, çünki seçilmiş şəkil faylı validationdan keçmədi, ona görə də, method aşağıdakı sətirləri run etdirə bilməz. Metodun aşağıdakı sətirləri run etdirə bilməsi (yəni dayanmayıb öz işinə davam edə bilməsi) üçün bu if-in şərti false gəlməlidir. Bu if-in şərti true gələrsə metod error mesajı ilə false qaytaracaq və dayanacaq.

            // Faylın extension-ını götürülür:
            string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant(); // imageFile.FileName faylın adını verir (FileName IFormFile interfaceinin propertysidir), Path.GetExtension() faylın yalnız extension hissəsini götürür, ToLowerInvariant() extension hissəsinin bütün hərflərini kiçik hərflərə çevirir (lower case). Bütün hərfləri kiçik etməkdə məqsəd bütün yazılış formalarını (böyük-kiçik) eyni hala salmaqdır. Nəticədə extension variableının valuesu seçilmiş faylın uzantısının (nöqtədən sonrakı hissəsinin) kiçik hərflərlə yazılmış adı olur.

            // Extension icazə verilən formatlar arasında deyilsə:
            if (!AllowedImageExtensions.Contains(extension)) // extension variableının valuesunun AllowedImageExtensions arrayindəki valuelardan biri ilə üst-üstə düşüb-düşmədiyini yoxlayır, əgər üst-üstə düşmürsə bu if true gəlir, error mesajı və false qaytarır, yəni metod false olur və dayanır, çünki seçilmiş fayl validationdan keçə bilmədi. 
            {
                errorMessage = "Yalnız JPG, JPEG, PNG və WEBP uzantılarına sahib şəkil yükləyə bilərsiniz!";
                return false;
            }

            // ContentType yoxlanılır:
            if (!AllowedContentTypes.Contains(imageFile.ContentType.ToLowerInvariant())) // imageFile.ContentType seçilmiş faylın formatını (məsələn, şəkildi yoxsa tekstdi) verir (ContentType IFormFile interfaceinin propertysidir), ToLowerInvariant() faylın adındakı bütün hərfləri (content-type mətnini) kiçildir. Əgər fayl image formatında deyilsə (allowedContentTypes arrayinin valuelarından biri ilə üst-üstə düşmürsə) bu if true gəlir, metod false olur, fayl validationdan keçə bilmir və metod dayanır, bu ifdən gələn error mesajı görünür.
            {
                errorMessage = "Yüklənən fayl şəkil formatında deyil!";
                return false;
            }
            // Həm extension, həm də format (MIME) yoxlanması həqiqətən şəkil faylı upload edilib-edilmədiyini bilmək üçün aparılır.

            return true; // Fayl bütün validationlardan keçibsə (yəni ilk if bloku xaric yuxarıdakı if blokları false gəlibsə) metod true qaytarır. Fayl seçilməyibsə metod true qaytarır. Seçilibsə və seçilmiş faylın ölçüsü 5 MB-dan (yəni MaxImageSize variableının valuesundan) kiçikdirsə, extensionı və formatı yuxarıdakı arraylərdəki kimidirsə metod true qaytarır.
        }

        [HttpPost]
        [ValidateAntiFoorgeryToken]
        public async Task<IActionResult> CategoryCreate(CategoryVM model)
        {
            await _context.SaveChangesAsync(); // EF Core Entity-ni DB-ə insert edir (valuelar tblCategories tableına save olunur) (bu insert əməliyyatı ilə şəkildən əvvəl kateqoriyanın ID-si save olunur ki, sonra şəklin hansı kateqoriyaya aid olacağı sistem tərəfindən dəqiq bilinsin).

            if (model.ImageFile is not null && model.ImageFile.Length > 0) // Şəkil seçilibmi və ölçüsü 0-dan böyükdürmü?
            {
                if (!ValidateImage(model.ImageFile, out string errorMessage)) // Yuxarıdakı ValidateImage metodu çağırılır, bu metod iki parametr gözləyir, biri modeldən gələn şəkil faylıdır, digəri fayl validationları keçə bilmədikdə metodun öz daxilindən gələn error mesajını təmsil edir, əgər metod false qaytararsa bu if true olacaq və faylın adı ilə metoddan gələn error mesajı ekranda görsənəcək, forma yazılmış məlumatlar itməyəcək amma DB-ə heçnə insert olunmayacaq.
                {
                    ModelState.AddModelError(nameof(model.ImageFile), errorMessage); // Validation erroru ModelStateə əlavə olunur və seçilmiş faylın adı ilə metoddan gələn error mesajı ekranda görsənir.
                    return View(model); // Create formu yenidən göstərilir, DB-ə heç nə save olunmur.
                }
            }

            // Əgər şəkil seçilibsə:
            if (model.ImageFile is not null && model.ImageFile.Length > 0) // Şəkil seçilibmi və ölçüsü 0-dan böyükdürmü?
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName); // Unique file name yaradılır və fileName adlı data tipi string olan variablea value olaraq verilir. Guid.NewGuid() - Guid classı random unikal ad yaradır. Bu ad ToString() vasitəsilə stringə çevrilir və upload üçün seçilmiş faylın uzantısı Path.GetExtension() ilə tutularaq Guid.NewGuid() metodunun yaratdığı random unikal ada əlavə olunur. Nəticədə insert edilən faylların adları fərqli olur.
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads"); // Upload edilmiş fiziki faylın saxlanılacağı qovluğun yolunun adı yaradılır və folder adlı data tipi string olan variableda saxlanılır. Directory.GetCurrentDirectory() tətbiqin işlədiyi əsas qovluğu qaytarır (E:\LMS\LibraryManagementSystem). Sonra bu qovluğa "wwwroot" və "Uploads" qovluqları əlavə edilir. Nəticə: E:\LMS\LibraryManagementSystem\wwwroot\Uploads. Bu yol adı string tipində olan folder adlı variablea verilir. Path.Combine() yolları düzgün şəkildə birləşdirir, ona görə də, E:\\LMS\\LibraryManagementSystem\\wwwroot\\Uploads adını əl ilə yazmaq əvəzinə Path.Combine()-dan istifadə olunur.

                // Qovluq yoxdursa yaradır:
                if (!Directory.Exists(folder)) // Bu if folder adlı variableın saxladığı adda olan yola sahib qovluğun mövcud olub-olmadığını yoxlayır. Əgər elə bir qovluq yoxdursa bu if true gəlir və proqram bu if blokuna düşür.
                {
                    Directory.CreateDirectory(folder); // Əgər qovluq yoxdursa bu sətir tərəfindən qovluq yaradılır (folder variableının saxladığı (bərabər olduğu) adda olan yola sahib qovluq).
                }

                string filePath = Path.Combine(folder, fileName); // Burada faylın tam yolunun adı yaradılır və filePath adlı data tipi string olan variablea bərabər edilir: filePath = E:\LMS\LibraryManagementSystem\wwwroot\Uploads\image.jpg = folder (E:\LMS\LibraryManagementSystem\wwwroot\Uploads) + fileName (image.jpg (əslində image.jpg əvəzinə Guid.NewGuid()-in yaratdığı unikal ad olur)).

                using (var stream = new FileStream(filePath, FileMode.Create)) // Burada FileStream classına aid stream adlı object yaradılır. filePath faylın özünün olacağı yolun adını bildirir (yəni harada yaradılacağını deyir), FileMode.Create o adda file yoxdursa faylı yaradır, varsa üstünə yazır (Guid istifadə edildiyi üçün eyni adlı faylın yaranma ehtimalı yoxdur). (Yəni FileStream-in FileMode.Create metodu filePath variableının tutduğu adda olan yola sahib fiziki fayl yaradır və onu stream adlı variablea verir.) using FileStream classını avtomatik açır (open), FileStream classının görəcəyi iş icra olunub bitdikdən sonra onu avtomatik olaraq bağlayır (dispose edir, yəni resurslarını istifadə etməyi dayandırır (dispose etmək artıq ehtiyac duyulmayan resursları RAMdan silmək deməkdir, yəni using FileStream öz işini bitirdikdən sonra ona aid stream adlı instanceı RAMdan silir)).
                {
                    await model.ImageFile.CopyToAsync(stream); // İstifadəçinin upload etdiyi şəkil, yəni IFormFile interfaceinə aid object diskdə yaradılmış stream adlı FileStream objektinə kopyalanır. CopyToAsync() IFormFile interfaceinin metodudur və istifadəçinin seçdiyi şəkli fiziki olaraq E:\LMS\LibraryManagementSystem\wwwroot\Uploads folderinin içinə save edir (İstifadəçinin upload etmək üçün seçdiyi faylın kontentini (yəni şəkli) stream adlı variablea kopyalayır).
                }

                var categoryImage = new TblCategoryImage // DB üçün şəkil Entity-si yaradılır. Bu yeni TblCategoryImage Entitysidir. Bu Entity DB-dəki tblCategoryImages tableına save olunacaq.
                {
                    CategoryId = category.CategoryId,
                    ImagePath = fileName,
                    ImageSortOrder = 0 // FK vasitəsilə CategoryID tblCategoryImage classına save olunan sətirə də verilir. DB-ə fiziki faylın adı yazılır: ImagePath = fileName. Şəklin sıralanma nömrəsi təyin olunur: ImageSortOrder = 0.
                };

                _context.TblCategoryImages.Add(categoryImage); // Şəkil Entity-si EF Corea əlavə olunur ki, EF Core DB-ə şəklin fayl adını (categoryImage objectini) insert edə bilsin.
                await _context.SaveChangesAsync(); // Databasedə faylın adı (path) saxlanılır (EF Core database-ə INSERT göndərir).

                // Nəticə olaraq şəklin özü, yəni fiziki fayl FileStream classı vasitəsilə E:\LMS\LibraryManagementSystem\wwwroot\Uploads folderinin içinə save edilir. DB-ə isə şəklin adı (file path) insert edilir (categoryImage objecti yaradılaraq).
            }
        }
        public async Task<IActionResult> CategoryEdit(int id, CategoryVM model)
        {
            // Şəkillərin saxlanılacağı folderin fiziki yolunun adı yaradılır və tipi string olan folder adlı variablea verilir:
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads"); // E:\LMS\LibraryManagementSystem\wwwroot\Uploads.

            // İstifadəçi mövcud şəkli silmək istəyirsə:
            if (model.DeleteImage && oldImage != null) // oldImage variableı null olmamalı (yəni həqiqətən köhnə şəkil(lər) var olmalı) və istifadəçi "Şəkli Sil" seçimini etmiş olmalıdır (CategoryVM classındakı DeleteImage propertysi istifadə olunur) ki, köhnə şəkil silinsin.
            {
                if (!string.IsNullOrWhiteSpace(oldImage.ImagePath)) // TblCategoryImage classının ImagePath propertysində value olub-olmadığı (yəni DB-də köhnə şəklə aid filename/path məlumatının olub-olmadığı) yoxlanılır, əgər value varsa bu if true gəlir.
                {
                    string oldFilePath = Path.Combine(folder, oldImage.ImagePath); // Fiziki faylın yolunun adı yaradılır və oldFilePath adlı variablea verilir. Nümunə nəticə: E:\LMS\LibraryManagementSystem\wwwroot\Uploads\image.jpg.
                    if (System.IO.File.Exists(oldFilePath)) // Diskdə həqiqətən həmin adda faylın mövcud olub-olmaması yoxlanılır, əgər varsa fayl silinir.
                    {
                        System.IO.File.Delete(oldFilePath); // Fayl fiziki olaraq wwwroot/Uploads folderindən silinir.
                    }
                }
                _context.TblCategoryImages.Remove(oldImage); // DB-dəki image recordu silinir (tblCategoryImages tableından URL-dən gəlmiş id-yə bərabər CategoryID-yə sahib sətirdə olan ImagePath valuesu başda olmaqla bütün valuelar silinir).
                oldImage = null; // oldImage variableı null olur (köhnə şəkil faylı fiziki olaraq həm folderdən, həm DB-dən yox olur).
                // System.IO.File.Delete(oldFilePath); sətri fiziki faylı olduğu yerdən silir, _context.TblCategoryImages.Remove(oldImage); sətrində isə EF Core DB-dəki şəkil qeydini silir.
            }

            // Yeni şəkil seçilibsə:
            if (model.ImageFile is not null && model.ImageFile.Length > 0) // Yeni şəklin seçilib-seçilmədiyi yoxlanılır. Yeni şəkil seçilməsə bu if false olacaq və CategoryVM classındakı DeleteImage propertysi istifadə olunmasa köhnə şəkil olduğu kimi qalacaq.
            {
                if (!ValidateImage(model.ImageFile, out string errorMessage))
                {
                    ModelState.AddModelError(nameof(model.ImageFile), errorMessage); // Validation erroru ModelStateə ələvə olunur və ValidateImage() metodundan gələn error mesajı göstərilir.
                    return View(model); // Edit səhifəsi yenidən açılır, DB-ə heç bir dəyişiklik save olunmur.
                } // Yeni şəkil validate olunur. Yeni şəkil validationlardan keçərsə bu if false, keçməzsə true olur və yeni şəkil əlavə edilmir.

                if (!Directory.Exists(folder)) // Burada wwwroot/Uploads folderi (folder adlı data tipi string olan variableın saxladığı adda olan folder) yoxdursa yaradılır.
                {
                    Directory.CreateDirectory(folder);
                }

                // Köhnə şəkil varsa, fiziki faylı silinir:
                if (oldImage != null && !string.IsNullOrWhiteSpace(oldImage.ImagePath)) // Bu if köhnə şəkli yenisi ilə əvəz etmək üçündür. // Əgər əvvəlki image recordu hələ də qalıbsa yeni şəkil gələndə əvvəlki fiziki şəkil silinir.
                {

                    string oldFilePath = Path.Combine(folder, oldImage.ImagePath); // Köhnə faylın yolu tapılır.
                    if (System.IO.File.Exists(oldFilePath)) // Folderdə köhnə faylın mövcudluğu yoxlanılır.
                    {
                        System.IO.File.Delete(oldFilePath); // Əgər köhnə fayl mövcuddursa fiziki olaraq silinir.
                    }
                }

                // Yeni fayl adı:
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName); // Yeni unikal fayl name yaradılır (FileName IFormFile interfaceinin propertysidir.).
                string filePath = Path.Combine(folder, fileName); // Yeni file path (fayl yolu) yaradılır: E:\LMS\LibraryManagementSystem\wwwroot\Uploads\uploadedImage.jpg.

                // Yeni şəkil fiziki qovluğa yazılır (fiziki olaraq save edilir):
                using (var stream = new FileStream(filePath, FileMode.Create)) // Yeni FileStream-in FileMode.Create metodu filePath variableının tutduğu adda fiziki fayl yaradır və onu stream adlı variablea bərabər edir.
                {
                    await model.ImageFile.CopyToAsync(stream); // İstifadəçinin upload etmək üçün seçdiyi şəkli stream adlı variablea kopyalayır.
                } // (Öncə IFormFile işləyir (istifadəçinin upload etdiyi şəkli tutur), daha sonra FileStream işləyir (IFormFile-ın tutduğu şəkli wwwroot/Uploads folderinə yazır)).

                // Mövcud DB qeydi yenilənir:
                if (oldImage != null) // Əgər köhnə image record varsa bu if true gəlir.
                {
                    oldImage.ImagePath = fileName; // Köhnə image recordu varsa yeni image record yaratmağa ehtiyac yoxdur, mövcud record dəyişdirilir (yenisi köhnənin üstünə (yoluna) yazılır).
                    oldImage.ImageSortOrder = 0; // Şəklin sıralama nömrəsi 0 edilir.
                }
                else // Əgər əvvəl heç şəkil yox idisə proqram elseə düşür.
                {
                    // Şəkil ümumiyyətlə yox idisə yeni qeyd (Entity) yaradılır:
                    var categoryImage = new TblCategoryImage
                    {
                        CategoryId = category.CategoryId,
                        ImagePath = fileName,
                        ImageSortOrder = 0 // Yeni şəklin hansı Kateqoriyaya aid olduğu bildirilir. DB-ə yeni şəklin fileName-i yazılır və sıralama nömrəsi verilir.
                    };
                    _context.TblCategoryImages.Add(categoryImage); // categoryImage adlı yeni object (entity) EF Corea əlavə olunur.
                }
            }
        }
*/
