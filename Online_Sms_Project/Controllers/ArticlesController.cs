using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data; // Is namespace ko confirm karein ke ApplicationDbContext isi me hai
using Online_Sms_Project.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Online_Sms_Project.Controllers
{
    public class ArticlesController : Controller
    {
        // AppDbContext badal kar ApplicationDbContext kar diya hai
        private readonly AppDbContext _context;

        public ArticlesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Articles/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                TempData["Error"] = "Article likhne ke liye pehle login karein.";
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Title aur Content dono likhna zaroori hai!";
                return View();
            }

            try
            {
                var article = new Article
                {
                    Title = title,
                    Content = content,
                    AuthorId = currentUserId.Value,
                    PublishedAt = DateTime.UtcNow
                };

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Article kamyabi se publish ho gaya!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kuch masla hua hai: " + ex.Message;
                return View();
            }
        }
    }
}