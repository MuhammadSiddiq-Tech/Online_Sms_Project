using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Online_Sms_Project.Controllers
{
    public class MessagesController : Controller
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context) => _context = context;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSMS(string recipientNumber, string messageBody)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            if (!senderId.HasValue) return RedirectToAction("Login", "Account");

            // User dhoondhein (Receiver)
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == recipientNumber);

            if (receiver == null)
            {
                TempData["Error"] = "Yeh phone number kisi user se linked nahi hai!";
                return RedirectToAction("Index");
            }

            var message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiver.Id,
                Content = messageBody,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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
            var msgs = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.SenderId == senderId && !m.IsRead)
                .ToListAsync();

            foreach (var m in msgs) m.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}