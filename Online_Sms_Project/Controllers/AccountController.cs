using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Online_Sms_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public static HashSet<int> ActiveLoggedInUsers = new HashSet<int>();

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // --- AUTH ACTIONS ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string phoneNumber, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email || u.PhoneNumber == phoneNumber))
            {
                TempData["Error"] = "Email ya Phone Number pehle se register hai!";
                return View();
            }

            var user = new User { Username = username, Email = email, PhoneNumber = phoneNumber, PasswordHash = password, PlanType = "Free", FreeMessagesSent = 0 };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            var existingUserId = HttpContext.Session.GetInt32("UserId");
            if (existingUserId.HasValue) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usernameOrEmail, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.PasswordHash == password);
            if (user == null) { TempData["Error"] = "Galat Username/Email ya Password!"; return View(); }

            lock (ActiveLoggedInUsers) { if (!ActiveLoggedInUsers.Contains(user.Id)) ActiveLoggedInUsers.Add(user.Id); }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index", "Home");
        }

        // --- PROFILE ACTIONS ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            return View(new ProfileViewModel { Id = user.Id, Username = user.Username, Email = user.Email, PhoneNumber = user.PhoneNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login");

            if (!ModelState.IsValid) return View("Profile", model);

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            user.Username = model.Username;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("Username", user.Username);
            TempData["Success"] = "Profile update ho gayi!";
            return RedirectToAction("Profile");
        }

        // --- LOGOUT ACTION ---
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue) { lock (ActiveLoggedInUsers) { ActiveLoggedInUsers.Remove(userId.Value); } }
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}