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

// Configurar localizaci�n para espa�ol (Colombia)
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

// Convertir DATABASE_URL de URI a formato clave-valor
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Configuration["DATABASE_URL"] is string databaseUrl && !string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Port={uri.Port};SSL Mode=Require";
}

// Configurar base de datos
builder.Services.AddDbContext<RifaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<RifaDbContext>();

// Configurar Identity con clase User personalizada
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<RifaDbContext>()
    .AddDefaultTokenProviders();

// Registrar IEmailSender
builder.Services.AddSingleton<IEmailSender, NullEmailSender>();

// Configurar redirecci�n de login
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

// Configurar el puerto solo en producci�n
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.WebHost.UseUrls($"http://+:{port}");
}

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar localizaci�n en el pipeline
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