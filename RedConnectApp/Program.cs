using Microsoft.EntityFrameworkCore;
using RedConnect.DAL;
using RedConnectApp.DAL;

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

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Register}/{id?}");

app.Run();
