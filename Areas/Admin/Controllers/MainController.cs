/* ~/Areas/Admin/Controllers/MainController.cs: */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize]
    public class MainController : Controller
    {
        public IActionResult MainAdminIndex()
        {
            return View();
        }
    }
}
