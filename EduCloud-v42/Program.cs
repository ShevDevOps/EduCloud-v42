using EduCloud_v42.Models;
using EduCloud_v42.Srevices.Loginer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

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

            staticWebAssetsPath = Path.Combine(packageRoot, "wwwroot");
            Console.WriteLine($"[Debug] Target staticwebassets path: {staticWebAssetsPath}");
            directoryExists = Directory.Exists(staticWebAssetsPath);
            Console.WriteLine($"[Debug] Directory.Exists check result: {directoryExists}");
            if (directoryExists)
            {
                Console.WriteLine("[Debug] Setting WebRootPath to wwwroot...");
                builder.Environment.WebRootPath = staticWebAssetsPath;
            }
            else
            {
                Console.WriteLine("[Debug] staticwebassets directory NOT found. WebRootPath will not be set.");
            }
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

string? DBChoose = builder.Configuration["DB"];
string? connection = "";
switch (DBChoose)
{
    case "SqLite":
        connection = builder.Configuration.GetConnectionString("SqliteConnection");
        if (!string.IsNullOrEmpty(connection))
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite(connection));
        else
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite("Data Source=siteData.db;"));
        break;
    case "MsSql":
        connection = builder.Configuration.GetConnectionString("MsSqlConnection");
        if (!string.IsNullOrEmpty(connection))
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlServer(connection));
        else
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite("Data Source=siteData.db;"));
        break;
    case "Postgres":
        connection = builder.Configuration.GetConnectionString("PostgresConnection");
        if (!string.IsNullOrEmpty(connection))
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseNpgsql(connection));
        else
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite("Data Source=siteData.db;"));
        break;
    case "InMemory":
        connection = builder.Configuration.GetConnectionString("InMemoryConnection");
        if (!string.IsNullOrEmpty(connection))
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseInMemoryDatabase(connection));
        else
            builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite("Data Source=siteData.db;"));
        break;
    default:
        builder.Services.AddDbContext<LearningDbContext>(options => options.UseSqlite("Data Source=siteData.db;"));
        break;
}

builder.Services.AddTransient<ILoginer, CookieLoginer>();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Це критично для повернення сутностей EF Core напряму (API v1)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Налаштування Swagger (Пункт 1, 4)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EduCloud API v1", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "EduCloud API v2", Version = "v2" });
});

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

        //context.Database.Migrate();
        bool dropDB = builder.Configuration.GetValue<bool>("DropDB");
        if(dropDB)
        {
            context.Database.EnsureDeleted();
        }
        context.Database.EnsureCreated();
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


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduCloud API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "EduCloud API v2");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapControllers();

app.Run();


namespace EduCloud_v42
{
    public partial class Program { }
}
