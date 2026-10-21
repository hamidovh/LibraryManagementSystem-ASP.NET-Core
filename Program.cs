/* Program.cs: */

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies; // Bu using direktivi Cookie based log in əməliyyatlarını idarə etmək üçün Program.cs faylına əlavə olunmuş librarydir və ASP.NET Core-un Cookie Authentication ilə əlaqəli classlarını tətbiqdə istifadə etməyə imkan verir. Məsələn, aşağıdakı CookieAuthenticationDefaults və AddCookie(); kimi API-lar Cookie Authentication üçün istifadə olunurlar. Cookie Authentication istifadəçinin authenticated olub-olmadığını, yəni kimliyinin yoxlanılıb-yoxlanılmadığını müəyyənləşdirmək üçün bir vasitədir.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Tətbiqi qurmaq üçün ilkin builder objecti yaradılır:
var builder = WebApplication.CreateBuilder(args); // Bu sətir ASP.NET Core tətbiqinin başlanğıc quruluşunu yaradır. builder objecti vasitəsilə tətbiqin serviceləri, configuration məlumatları, environment məlumatları və s. pipelinea əlavə edilir. Buradakı args tətbiqə command-line vasitəsilə ötürülən arqumentləri təmsil edir.

// Add services to the container.
// Aşağıdakı sətirdə ASP.NET Core-un Controller, Action, View əsaslı MVC arxitekturasından istifadə olunacağı tətbiqə bildirilir. builder.Services Dependency Injectionə servicelər əlavə etmək üçün istifadə olunur.
builder.Services.AddControllersWithViews(options =>
{
    // Bütün POST/PUT/DELETE sorğularında anti-forgery token avtomatik yoxlanılır:
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Model binding xəta mesajları (default olaraq ingiliscədir):
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => "Bu sahə doldurulmalıdır!");
    messages.SetMissingBindRequiredValueAccessor(field => $"'{field}' sahəsi doldurulmalıdır!");
    messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"'{value}' dəyəri '{field}' sahəsi üçün yararsızdır!");
    messages.SetUnknownValueIsInvalidAccessor(field => $"'{field}' sahəsi üçün daxil edilən dəyər yararsızdır!");
    messages.SetValueIsInvalidAccessor(value => $"'{value}' dəyəri yararsızdır!");
    messages.SetValueMustBeANumberAccessor(field => $"'{field}' sahəsinə yalnız ədəd daxil edilməlidir!");
}); // gələcəkdə fetch/AJAX ilə POST yazılsa tokeni RequestVerificationToken header-i ilə göndərmək lazım olacaq.

// Şəkil servisi (wwwroot/Uploads qovluğu ilə işləyir):
builder.Services.AddSingleton<IImageStore, ImageStore>();

// DbContext tətbiqə əlavə edilir:
builder.Services.AddDbContext<LMSdbContext>(options => // Burada LMSdbContext ASP.NET Core-un Dependency Injection sisteminə əlavə edilir. LMSdbContext bu tətbiqin EF Core DbContext classıdır. Bu sayədə məsələn, Controllerlərdə Constructor Injection vasitəsilə DBə giriş əldə edilir. ASP.NET Core LMSdbContext objectini özü yaradıb controllerlərə ötürür.
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("appconnection"));
}); // Bu kod blokunun içərisində EF Core-a tətbiq tərəfindən SQL Server database provider istifadə ediləcəyi bildirilir. GetConnectionString("appconnection") isə appsettings.json faylının içərisindəki "ConnectionStrings": { "appconnection": "..." } dəyərini götürür. Yəni əlaqə belədir: appsettings.json - appconnection - Program.cs - UseSqlServer() - LMSdbContext - SQL Server.

// Aşağıdakı kod bloku tətbiqdə əsas Authentication mexanizmi kimi Cookie Authentication-ı təyin edir. Authentication Scheme ASP.NET Core-a istifadəçinin kimliyinin hansı authentication mexanizmi vasitəsilə müəyyən ediləcəyini bildirir. Məsələn, Cookies, JWT Bearer, Google, Microsoft, Facebook və s. kimi authentication mexanizmləri var. Bu layihədə authentication mexanizmi kimi Cookies istifadə olunur. CookieAuthenticationDefaults.AuthenticationScheme standart Cookies scheme-indən istifadə edir (yəni tətbiqin default authentication scheme-i Cookies olaraq təyin edilir).
/*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x => {
    x.LoginPath = "/Login";
    x.Cookie.Name = "AdminLogin";
});*/
// AddAuthentication(...) - Authentication üçün hansı schemedən istifadə ediləcəyini müəyyən edir.
// AddCookie(...) - bu Cookie Authentication-ın necə konfiqurasiya olunacağını təyin edir.
// x.LoginPath = "/Login"; - Login olmayan, yəni authenticated olmamış istifadəçi [Authorize] ilə qorunan səhifəyə daxil olmağa çalışdıqda yönləndiriləcəyi login səhifəsinin URL-ini (yolunu) müəyyən edir. Bu layihədə login səhifəsi "/Login" ünvanında yerləşir və Cookie Authentication mexanizmi authenticated olmamış istifadəçini həmin ünvana yönləndirir.
// x.Cookie.Name = "AdminLogin"; - Authentication zamanı yaradılan cookie-nin adını müəyyən edir. Login olmuş istifadəçinin authentication məlumatı bu cookie vasitəsilə brauzerdə saxlanılır, yəni authentication cookie-nin adı "AdminLogin" olur (cookie authentication üçün lazım olan authentication ticket cookie vasitəsilə saxlanılır).

builder.Services.AddScoped<IPasswordHasher<TblUser>, PasswordHasher<TblUser>>();

var app = builder.Build(); // Burada artıq yuxarıda builder objecti vasitəsilə konfiqurasiya edilmiş service və configuration quruluşlarından istifadə edilərək WebApplication objecti yaradılır: builder - configuration + services - Build() - app. app artıq tətbiqin özüdür.

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} // Bu if bloku əgər tətbiq Development mühitində (yəni, Production, Staging kimi environmentlarda) işləmirsə app.UseExceptionHandler("/Home/Error"); kod sətiri Production kimi mühitlərdə baş verən exception-lar üçün ümumi error səhifəsinə yönləndirmə mexanizmi qurur (/Home/Error - error action-ına yönləndirilir). app.UseHsts(); - HSTS (HTTP Strict Transport Security) Browser-ə saytla HTTPS üzərindən əlaqə qurmağa üstünlük verməsini bildirən təhlükəsizlik mexanizmidir.

app.UseHttpsRedirection(); // Bu middleware HTTP request-ləri HTTPS-ə (Məsələn, http://localhost:5000 və ya https://localhost:7000 kimi ünvanlara) yönləndirir.

app.UseStaticFiles(); // Bu middleware wwwroot daxilindəki static faylların browser tərəfindən əldə edilməsinə imkan verir. İstifadəçinin yüklədiyi wwwroot/Uploads/... şəkillərini UseStaticFiles() verir.

app.UseRouting(); // Bu middleware gələn request üçün uyğun endpoint/route-un müəyyən edilməsinə imkan yaradır. Məsələn, /Admin/Users/UserIndex gəldikdə ASP.NET Core bunun hansı controller/action-a uyğun olduğunu route sistemindən müəyyənləşdirir.

// Authentication (kimlik yoxlaması) - "Sən kimsən?" deyə soruşur, Authentication Cookie məlumatlarını yoxlayır və istifadəçinin kimliyini HttpContext.User daxilində müəyyən edir (app.UseAuthentication() istifadəçini login etmir, o, sadəcə mövcud authentication məlumatlarını yoxlayır):
app.UseAuthentication(); // Bu middleware hər HTTP request zamanı authentication məlumatlarının yoxlanılmasını təmin edən middlewaredir. Pipeline zamanı app.UseAuthentication(); cookieni oxuyur və login olmuş istifadəçinin kimliyini HttpContext.User objectində müəyyən edir. Pipeline-da app.UseAuthentication(); həmişə mütləq şəkildə app.UseAuthorization();-ın üstündə yerləşdirilməlidir. Çünki əvvəlcə sistem app.UseAuthentication(); vasitəsilə istifadəçini tanımalıdır, ondan sonra istifadəçinin sahib olduğu və olmadığı icazələri app.UseAuthorization(); vasitəsilə yoxlaya bilər. app.UseAuthorization(); istifadəçinin kimliyi (HttpContext.User) müəyyən edildikdən sonra həmin istifadəçinin tələb olunan icazələrə sahib olub-olmadığını yoxlayır.

// Authorization (icazə yoxlaması) - "Sənin buna icazən varmı?" deyə soruşur.
app.UseAuthorization(); // app.UseAuthorization(); istifadəçinin Authentication nəticəsində müəyyən edilmiş kimliyinə əsasən tələb olunan resursa daxil olmaq üçün lazımi icazəyə sahib olub-olmadığını yoxlayır. Məsələn, [Authorize], [Authorize(Roles = "Admin")] kimi atributlar ilə qorunan controller və action-ların yalnız authenticated olmuş istifadəçilər tərəfindən açılmasını təmin edir ([Authorize] "Authenticated olmalısan", [Authorize(Roles = "Admin")] isə "Authenticated olmalısan və Admin roluna sahib olmalısan" deyir). Yəni Authorization deyir: "İstifadəçinin kim olduğunu bildim. İndi görüm bu resursa daxil olmağa icazəsi varmı?".

app.MapStaticAssets(); // ASP.NET Core-un static asset-lərini endpoint routing sistemi ilə endpoint kimi xəritələndirməsi üçün istifadə olunan API-dir. MapStaticAssets() yalnız build vaxtı məlum olan faylları verir.

app.MapControllers(); // Attribute routing (məsələn, [Route("Login")]) istifadə edən controller-ləri endpoint kimi xəritələndirir və route-un işləməsinə kömək edir.

// Area route:
app.MapControllerRoute(
        name: "areas", //name: "admin",
        pattern: "{area:exists}/{controller=Main}/{action=MainAdminIndex}/{id?}"
    ); // app.MapControllerRoute() MVC controller route-u təyin edir. name: "areas" və ya name: "admin" Route-un adıdır. Burada name: "areas" yazılıb, çünki bu Route yalnız Admin yox, ümumilikdə Area-lar üçündür. {area:exists} məsələn Admin kimi bir areanın mütləq mövcud olmalı olduğunu bildirir, onun ardınca isə bu areanın default controller və actionı qeyd olunub. id? id-nin istəyə bağlı olduğunu, id parametrinin qeyd edilməsinin məcburi olmadığını bildirir.

// Default route:
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomeIndex}/{id?}")
    .WithStaticAssets(); // Bu normal MVC controller-lər üçün default routedur. .WithStaticAssets(); bu route-un static assetlərlə əlaqəli endpoint mexanizmlərindən istifadə etməsinə imkan verir.

app.Run(); // ASP.NET Core tətbiqini işə salır və HTTP request-ləri qəbul etməyə başlayır.


// Program.cs - builder - services - Build() - middleware - routes - app.Run() - APPLICATION RUNNING.

// using Microsoft.AspNetCore.Authentication.Cookies; - Bu using direktivi ASP.NET Core-un Cookie Authentication ilə əlaqəli API-lərini istifadə etməyə imkan verir. Məsələn, CookieAuthenticationDefaults.AuthenticationScheme və AddCookie() kimi classlar Cookie Authentication librarysinə aiddir (CookieAuthenticationDefaults və AddCookie(); Cookie Authentication konfiqurasiyasında istifadə olunurlar). Cookie Authentication mexanizmi istifadəçinin login vəziyyətini cookie vasitəsilə yadda saxlamağa imkan verir.
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) - Bu kod sətiri sistemə deyir: "Bu tətbiqdə istifadəçilərin kimliyini yoxlamaq üçün əsas Authentication mexanizmi Cookie Authentication olacaq.", yəni tətbiqin default Authentication Scheme-ini Cookie Authentication olaraq təyin edir. CookieAuthenticationDefaults.AuthenticationScheme standart olaraq "Cookies" dəyərini ifadə edir. Yəni ASP.NET Core-a deyilir: "Authentication üçün Cookies mexanizmindən istifadə et.".
// Authentication Scheme nədir? Authentication Scheme ASP.NET Core-a istifadəçinin kimliyinin hansı authentication mexanizmi vasitəsilə müəyyən ediləcəyini bildirir. Məsələn, müxtəlif authentication mexanizmləri ola bilər: Cookies, JWT Bearer, Google, Microsoft, Facebook və s.. Bu layihədə Cookies (Cookie Authentication) istifadə olunur.
// .AddCookie(x => { x.LoginPath = "/Login"; x.Cookie.Name = "AdminLogin"; }); - Bu hissə Cookie Authentication-ın konfiqurasiyasını müəyyən edir. Yəni, AddAuthentication(...) əsas authentication scheme-ni seçir, yəni deyir: "Authentication istifadə edəcəyəm.". AddCookie(...) isə Cookie Authentication-ın davranışını konfiqurasiya edir, yəni deyir: "Authentication-ı Cookie vasitəsilə necə işlətməliyəm?".
// x.LoginPath = "/Login"; sistemə deyir: Login olmamış istifadəçi [Authorize] ilə qorunan resursa, yəni səhifəyə (admin panelinə) daxil olmağa çalışdıqda, onu /Login (URL-i /Login olan səhifəyə) ünvanına düzgün LoginPath ilə yönləndir. Yəni, x.LoginPath = "/Login"; [Authorize] ilə qorunan resursa authenticated olmayan istifadəçi daxil olmağa çalışdıqda yönləndiriləcəyi login səhifəsinin yolunu müəyyən edir.
// x.Cookie.Name = "AdminLogin"; Authentication zamanı istifadə olunan cookienin adını müəyyən edir. Login olduqdan sonra brauzerdə authentication cookie yaranır. Bu layihədə authentication cookie-nin adı "AdminLogin" olacaq (Cookie-yə istənilən adı vermək olar).
// [Authorize], [Authorize(Roles = "Admin")] kimi authorization qaydaları admin panelinə yalnız adminlərin girə bilməsini təmin etmək kimi məqsədlərə xidmət edir. [Authorize(Roles = "Admin")] attributeunun İstifadəçinin yalnız authenticated olmasını deyil, həm də "Admin" roluna sahib olmasını tələb edir.
// [Authorize] attribute-unun vəzifəsi yazıldığı Controller və ya Action-a yalnız authenticationdan keçmiş istifadəçilərin daxil ola bilməsinə icazə verməkdir. [Authorize] istifadəçini login etmir, o, qorunan controller və ya actiona daxil olmaq üçün istifadəçinin authenticated olmasını tələb edir, yəni sadəcə istifadəçinin login olub-olmadığını yoxlayır. [Authorize] Program.cs-dəki app.UseAuthentication(); vasitəsilə öz işini yerinə yetirir, yəni istifadəçinin login olub-olmadığını öyrənir. Authentication məlumatı isə app.UseAuthentication(); middleware tərəfindən müəyyən edilir. app.UseAuthentication(); Authentication Middleware-dir və bu middleware request gələndə authentication cookie-ni yoxlayır.

// Middleware HTTP request və response arasında işləyən proqramdır; Middleware request-i emal edən aralıq proqram komponentidir, Request controller-ə çatmamış müəyyən yoxlamalar aparır. Hər middleware request-i emal edə bilər, yoxlaya bilər, dəyişdirə bilər, məlumat əlavə edə bilər, request-i dayandıra bilər, response yarada və ya request-i növbəti middleware-ə ötürə bilər və s.. Response geri qayıdanda da middlewarelərdən keçir.
// HTTP Request Pipeline - Middleware-lərin ardıcıllığına pipeline deyilir, məsələn, Program.cs-də alt-alta yazılmış app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.MapStaticAssets(); app.MapControllerRoute(...); kimi hissələr var. Bunların birlikdə əmələ gətirdiyi request-processing ardıcıllığı HTTP request pipeline adlanır. Yəni, pipeline requestin tətbiq daxilində keçdiyi middlewarelərin ardıcıllığıdır, ardıcıl işləmə sistemidir.
// Tətbiqə gələn HTTP request-in middlewarelər və endpointlər vasitəsilə ardıcıl şəkildə emal olunma prosesinə request pipeline deyilir. Middlewarelərin tətbiqdə hansı ardıcıllıqla yerləşdirilməsi onların işləmə qaydasına təsir edir.
// [Authorize] - Authorization tələb edir; UseAuthentication() - istifadəçinin kimliyi müəyyən edilir; UseAuthorization() - [Authorize] qaydası yoxlanılır. Yəni [Authorize] Authentication middleware-in özü deyil. [Authorize] authorization tələbini müəyyən edir; UseAuthentication() isə authentication məlumatını hazırlayır, UseAuthorization() isə həmin authorization tələblərini tətbiq edir.
// Login - Authentication Cookie yaradılır - Növbəti request - UseAuthentication() - HttpContext.User - UseAuthorization() - [Authorize] - Admin panelinə girişə icazə / rədd
