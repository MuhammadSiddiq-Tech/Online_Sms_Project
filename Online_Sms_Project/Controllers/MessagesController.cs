using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Online_Sms_Project.Controllers
{
    public class MessagesController : Controller
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpGet]
        public IActionResult GetUnreadCounts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return Json(new { });

            var unread = _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToList();

            return Json(unread);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int senderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.SenderId == senderId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in messages) msg.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}