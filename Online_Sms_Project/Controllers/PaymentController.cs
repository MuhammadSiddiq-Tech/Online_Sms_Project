using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // FIXED: Session handling ke liye zaroori hai
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data;
using Online_Sms_Project.Models;
using Stripe;
using Stripe.Checkout;

namespace Online_Sms_Project.Controllers
{
    // Temporary testing ke liye controller level se [Authorize] hata diya hai
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var domain = $"{Request.Scheme}://{Request.Host}/";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 999, // $9.99 in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Premium Unlimited SMS Plan"
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "Payment/Cancel",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                TempData["Error"] = "Invalid payment session.";
                return RedirectToAction("Index");
            }

            try
            {
                var service = new SessionService();
                Session session = await service.GetAsync(session_id);

                if (session.PaymentStatus == "paid")
                {
                    var userId = GetCurrentUserId(); // Real active session ID
                    var user = await _context.Users.FindAsync(userId);

                    if (user != null)
                    {
                        // FIXED: Hamare User model ke PlanType string field ko "Paid" me convert kardiya
                        user.PlanType = "Paid";
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Thank you! Your account has been upgraded to Premium.";
                    }
                }
                else
                {
                    TempData["Error"] = "Payment verification failed or is still pending.";
                    return RedirectToAction("Index");
                }

                return View();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment process was canceled.";
            return RedirectToAction("Index");
        }

        // FIXED: Shifted from Claims to HTTP Session State
        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value;
            }
            throw new InvalidOperationException("Aapka login session expire ho chuka hai. Dubara login karein.");
        }
    }
}