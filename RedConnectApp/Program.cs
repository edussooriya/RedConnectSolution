using Microsoft.EntityFrameworkCore;
using RedConnect.DAL;
using RedConnectApp.DAL;
using RedConnectApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<MSSQLDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSql")));

builder.Services.AddScoped<MongoRepository>();
builder.Services.AddScoped<DonorMapService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordResetService>();

var app = builder.Build();

// ── Run seeder on startup ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
