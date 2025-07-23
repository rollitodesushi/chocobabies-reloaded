using AspNetCoreRateLimit;
using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Hubs;
using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;



var builder = WebApplication.CreateBuilder(args);

// Configurar localización para español (Colombia)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("es-CO") };
    options.DefaultRequestCulture = new RequestCulture("es-CO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configurar base de datos
// *** Changed: Hardcode correct Render DATABASE_URL with SSL and logging ***  FUNCIONA ACTUALMENTE
/*var connectionString = "Host=dpg-d1vkklumcj7s73ffglh0-a.oregon-postgres.render.com;Database=chocobabies_h1i5;Username=admin;Password=TD70XHZmA1TWWk5ApBmdEcF6reNfC7Lu;Port=5432;SSL Mode=Require";
Console.WriteLine($"Connection String: {connectionString}");
builder.Services.AddDbContext<RifaDbContext>(options =>
    options.UseNpgsql(connectionString));*/

var connectionString = builder.Environment.IsDevelopment()
    ? "Host=localhost;Database=chocobabies;Username=postgres;Password=Jouikb_1996"
    : "Host=dpg-d1vkklumcj7s73ffglh0-a.oregon-postgres.render.com;Database=chocobabies_h1i5;Username=admin;Password=TD70XHZmA1TWWk5ApBmdEcF6reNfC7Lu;Port=5432;SSL Mode=Require";
Console.WriteLine($"Connection String: {connectionString}");
builder.Services.AddDbContext<RifaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configurar Identity con clase User personalizada
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<RifaDbContext>()
    .AddDefaultTokenProviders();

// Registrar IEmailSender
builder.Services.AddSingleton<IEmailSender, NullEmailSender>();

// Configurar redirección de login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Configurar Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<RifaDbContext>()
    .SetApplicationName("ChocobabiesReloaded");

// Configurar servicios adicionales
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddResponseCaching();

// Configurar Rate Limiting
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddRazorPages();

// Configurar el puerto solo en producción
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.WebHost.UseUrls($"http://+:{port}");
}

var app = builder.Build();

// Configurar pipeline HTTP
// *** Changed: Disable UseHttpsRedirection in production ***
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

// Configurar localización en el pipeline
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-CO"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("es-CO") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("es-CO") }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<RifaHub>("/rifaHub");

app.Run();
