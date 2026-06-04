using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using Microsoft.AspNetCore.Http; // Session handling ke liye zaroori hai
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Online_Sms_Project.Controllers
{
    // Temporary testing ke liye controller level se [Authorize] hata diya hai
    public class FriendsController : Controller
    {
        private readonly AppDbContext _context;

        // Injecting AppDbContext via Constructor
        public FriendsController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // UPGRADED INDEX: Ab yeh doston ki list aur pending requests load karega
        // ========================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // 1. Jo requests mujhe aayi hui hain aur pending hain
                var incomingRequests = await _context.FriendRequests
                    .Include(fr => fr.Sender)
                    .Where(fr => fr.ReceiverId == currentUserId && fr.Status == "Pending")
                    .ToListAsync();

                // 2. Jo log mere dost ban chuke hain (Status == "Accepted")
                var friendsList = await _context.FriendRequests
                    .Include(fr => fr.Sender)
                    .Include(fr => fr.Receiver)
                    .Where(fr => (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId) && fr.Status == "Accepted")
                    .Select(fr => fr.SenderId == currentUserId ? fr.Receiver : fr.Sender)
                    .ToListAsync();

                // Pending requests ko view mein bhejne ke liye ViewBag ka istemal
                ViewBag.IncomingRequests = incomingRequests;

                return View(friendsList); // Model ke taur par confirmed doston ki list jayegi
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        // ========================================================
        // NEW ACTION: Phone number se logon ko dhoondne ki logic
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> FindFriends(string searchPhone)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(searchPhone))
                {
                    return View(new List<User>());
                }

                // Apne alawa baki users ko phone number se search karna
                var foundUsers = await _context.Users
                    .Where(u => u.PhoneNumber.Contains(searchPhone) && u.Id != currentUserId)
                    .ToListAsync();

                ViewBag.SearchTerm = searchPhone;
                return View(foundUsers);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF
        public async Task<IActionResult> SendFriendRequest(int targetUserId)
        {
            try
            {
                var currentUserId = GetCurrentUserId(); // Real Session ID uthaye ga

                if (currentUserId == targetUserId)
                {
                    TempData["Error"] = "Aap khud ko friend request nahi bhej sakte.";
                    return RedirectToAction("Index");
                }

                // Check if an active request already exists
                var existingRequest = await _context.FriendRequests.AnyAsync(fr =>
                    (fr.SenderId == currentUserId && fr.ReceiverId == targetUserId) ||
                    (fr.SenderId == targetUserId && fr.ReceiverId == currentUserId));

                if (existingRequest)
                {
                    TempData["Error"] = "Aap dono ke darmiyan pehle se request pending ya accept ho chuki hai.";
                    return RedirectToAction("Index"); // Chaho toh FindFriends par bhi redirect kar sakte ho
                }

                var request = new FriendRequest
                {
                    SenderId = currentUserId,
                    ReceiverId = targetUserId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.FriendRequests.Add(request);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Friend request kamyabi se bhej di gayi hai!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var request = await _context.FriendRequests.FindAsync(requestId);

                if (request == null || request.ReceiverId != currentUserId)
                {
                    TempData["Error"] = "Friend request nahi mili ya aap yeh action nahi kar sakte.";
                    return RedirectToAction("Index");
                }

                request.Status = "Accepted";
                await _context.SaveChangesAsync();

                TempData["Success"] = "Friend request accept ho gayi!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        // FIXED: Claims mita kar sirf Session-based login helper rakha hai
        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value;
            }
            // Agar session expired ho toh login page par bhejne ke liye error throw karega
            throw new InvalidOperationException("Aapka login session khatam ho chuka hai. Dubara login karein.");
        }
    }
}