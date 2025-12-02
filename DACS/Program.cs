using DACS.Controllers;
using DACS.Models;
using DACS.Repositories;
using DACS.Repository;
using DACS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// ĐĂNG KÝ SERVICE
// ===========================
builder.Services.AddScoped<HomeController>(); // Chat
builder.Services.AddSingleton<SocketServer>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== FIX LỖI: Chỉ để lại 1 Identity duy nhất =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Google Login
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection =
            builder.Configuration.GetSection("Authentication:Google");

        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
        options.SaveTokens = true;
    });

builder.Services.AddRazorPages();

// Business services
builder.Services.AddSingleton<BlockchainService>();
builder.Services.AddScoped<INguoiMuaRepository, NguoiMuaRepository>();
builder.Services.AddScoped<IThuGomRepository, ThuGomRepository>();
builder.Services.AddScoped<ISanPhamRepository, EFSanPhamRepository>();
builder.Services.AddScoped<ITonKhoRepository, TonKhoRepository>();
builder.Services.AddScoped<IPhieuXuatRepository, PhieuXuatRepository>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.Configure<ESmsSettings>(builder.Configuration.GetSection("ESmsSettings"));
builder.Services.AddTransient<ISmsService, ESmsService>();
builder.Services.AddHttpClient();


// ===========================
// BUILD APP
// ===========================
var app = builder.Build();

// Chạy server socket (chat)
var socketServer = app.Services.GetRequiredService<SocketServer>();
_ = Task.Run(() => socketServer.StartAsync());


// ===========================
// PIPELINE
// ===========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.MapHub<ChatHub>("Hubs/ChatHub");

app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// ===========================
// ROUTES
// ===========================
app.UseEndpoints(endpoints => 
{ endpoints.MapControllerRoute( name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "Owner", pattern: "{area:exists}/{controller=Owner}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "KhachHang", pattern: "{area:exists}/{controller=KhachHang}/{action=Index}/{id?}");
    endpoints.MapControllerRoute( name: "QuanLyDH", pattern: "{area:exists}/{controller=QuanLyDH}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "QuanLyND", pattern: "{area:exists}/{controller=QuanLyND}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "QuanLyXNK", pattern: "{area:exists}/{controller=QuanLyXNK}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "QuanLySP", pattern: "{area:exists}/{controller=QuanLySP}/{action=Index}/{id?}"); 
    endpoints.MapControllerRoute( name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); 
});

// ===========================
// KHỞI ĐỘNG BLOCKCHAIN TEST
// ===========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var blockchainService = services.GetRequiredService<BlockchainService>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Đang kích hoạt TestBlockchainAsync() khi khởi động...");

        _ = blockchainService.TestBlockchainAsync(); // chạy nền không block app
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Không thể chạy BlockchainService test khi khởi động.");
    }
}

app.Run();
