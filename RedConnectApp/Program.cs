using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RedConnect.DAL;
using RedConnect.Interfaces;
using RedConnect.Middleware;
using RedConnect.Services;
using RedConnectApp.DAL;
using RedConnectApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<MSSQLDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSql")));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["Mongo:Connection"]);
});

builder.Services.AddScoped<IMongoRepository, MongoRepository>();

builder.Services.AddScoped<IAppDbContext, MSSQLDBContext>();
builder.Services.AddScoped<DonorMapService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<IMedicalReportService,MedicalReportService>();
builder.Services.AddScoped<IBloodBankService, BloodBankService>();

var app = builder.Build();

// ── Run seeder on startup ─────────────────────────────────────────────────


app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
