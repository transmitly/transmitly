using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerifyV2Quickstart.Models;

namespace VerifyV2Quickstart.Controllers
{
    public class UsersController : Controller
    {
        private UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            ViewData["Users"] = await _userManager.Users.ToListAsync();
            return View();
        }
    }
}