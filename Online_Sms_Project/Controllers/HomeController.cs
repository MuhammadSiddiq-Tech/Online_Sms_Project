using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Online_Sms_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor Injection: Database context ko controller mein le kar aana
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // UPGRADED INDEX: Ab yeh home page par saare latest articles dikhaye ga
        public async Task<IActionResult> Index()
        {
            // 1. Database se saare articles nikalna aur sath unke Author (User) ka data bhi include karna
            // 2. OrderByDescending lagane se sab se naya article sab se upar aayega (Just like LinkedIn/Facebook feed)
            var latestArticles = await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();

            // Articles ki list ko view (HTML Page) par bhejna
            return View(latestArticles);
        }
    }
}