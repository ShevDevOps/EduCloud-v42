using EduCloud_v42.Models;
using EduCloud_v42.Srevices.Loginer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Reflection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// --- ПОЧАТОК ДЕТАЛЬНОЇ ДІАГНОСТИКИ ---
Console.WriteLine("--- Starting Static File Diagnostics ---");

var assemblyLocation = Assembly.GetExecutingAssembly().Location;
Console.WriteLine($"[Debug] Assembly Location: {assemblyLocation}");

var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
Console.WriteLine($"[Debug] Assembly Directory: {assemblyDirectory}");

if (assemblyDirectory != null)
{
    var packageRoot = Directory.GetParent(assemblyDirectory)?
                               .Parent?
                               .Parent?
                               .FullName;
    Console.WriteLine($"[Debug] Calculated Package Root: {packageRoot}");

    if (packageRoot != null)
    {
        var staticWebAssetsPath = Path.Combine(packageRoot, "staticwebassets");
        Console.WriteLine($"[Debug] Target staticwebassets path: {staticWebAssetsPath}");

        // Перевіряємо, чи існує папка
        bool directoryExists = Directory.Exists(staticWebAssetsPath);
        Console.WriteLine($"[Debug] Directory.Exists check result: {directoryExists}");

        if (directoryExists)
        {
            Console.WriteLine("[Debug] Setting WebRootPath to staticwebassets...");
            builder.Environment.WebRootPath = staticWebAssetsPath;
        }
        else
        {
            Console.WriteLine("[Debug] staticwebassets directory NOT found. WebRootPath will not be set.");
        }
    }
    else
    {
        Console.WriteLine("[Debug] Could not determine Package Root.");
    }
}
else
{
    Console.WriteLine("[Debug] Could not determine Assembly Directory.");
}

Console.WriteLine($"[Debug] Final WebRootPath before build: {builder.Environment.WebRootPath}");
Console.WriteLine("--- End of Static File Diagnostics ---");


builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys")));

// Add services to the container.

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite(connection));
builder.Services.AddTransient<ILoginer, CookieLoginer>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();


var app = builder.Build();

Console.WriteLine($"[Debug] WebRootPath after build: {app.Environment.WebRootPath}");


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ïåðåêîíàéòåñÿ, ùî òóò íàçâà âàøîãî DbContext
        var context = services.GetRequiredService<EduCloud_v42.Models.LearningDbContext>();

        // Öÿ êîìàíäà çàñòîñîâóº ì³ãðàö³¿ òà ñòâîðþº òàáëèö³
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();

var finalWebRootPath = app.Environment.WebRootPath;
Console.WriteLine($"[Debug] Path for FileProvider: {finalWebRootPath}");

// Перевіряємо шлях ще раз, прямо перед створенням провайдера
if (Directory.Exists(finalWebRootPath))
{
    Console.WriteLine("[Debug] Creating PhysicalFileProvider and passing to UseStaticFiles.");

    // Ми ЯВНО створюємо FileProvider і передаємо його в UseStaticFiles
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(finalWebRootPath)
    });
}
else
{
    // Якщо раптом папки немає (чого не має бути, судячи з логів)
    Console.WriteLine("[Debug] Path not found AFTER build. Using default UseStaticFiles().");
    app.UseStaticFiles(); // Запускаємо як було, щоб побачити оригінальну помилку
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


namespace EduCloud_v42
{
    public partial class Program { }
}
