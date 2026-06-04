using Microsoft.EntityFrameworkCore;
using Online_Sms_Project.Data; // Your data layer namespace
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllersWithViews();

// 2. Register your AppDbContext BEFORE builder.Build()
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. FIXED: Permanent login ke liye Session aur Cookie settings ko 1 saal par set kiya hai
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromDays(365); // 1 Saal tak session server par active rahega
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".OnlineSms.Session";

    // CRITICAL: Yeh browser ko batata hai ke browser close hone par cookie delete nahi karni
    options.Cookie.MaxAge = TimeSpan.FromDays(365);
});

// 4. Initialize Stripe API key BEFORE builder.Build()
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// ========================================================
var app = builder.Build();
// ========================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 5. Enable Session Middleware BEFORE Authentication/Authorization
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

// 6. Route ko automatic Account/Login par set kar diya
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();