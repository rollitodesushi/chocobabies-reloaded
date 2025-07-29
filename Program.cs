using AspNetCoreRateLimit;
using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Hubs;
using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configurar localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
    {
        // 🔥 Esto permite que enums como estadoTiquete se deserialicen desde strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("es-CR") };
    options.DefaultRequestCulture = new RequestCulture("es-CR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configurar base de datos
string connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : Environment.GetEnvironmentVariable("DATABASE_URL") is string databaseUrl
        ? $"Host={new Uri(databaseUrl).Host};Port=5432;Database={new Uri(databaseUrl).Segments.Last().Trim('/')};Username={new Uri(databaseUrl).UserInfo.Split(':')[0]};Password={new Uri(databaseUrl).UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true"
        : throw new InvalidOperationException("No se pudo obtener el connection string.");
Console.WriteLine($"Connection String: {connectionString}");
builder.Services.AddDbContext<RifaDbContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

// Configurar Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<RifaDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Configurar Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<RifaDbContext>()
    .SetApplicationName("ChocobabiesReloaded");

// Servicios adicionales
builder.Services.AddSingleton<IEmailSender, NullEmailSender>();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddRazorPages();

// Puerto para producción
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.WebHost.UseUrls($"http://+:{port}");
}

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-CR"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("es-CR") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("es-CR") }
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<RifaHub>("/rifaHub");

// Seed inicial
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
    }

    var user = await userManager.FindByEmailAsync("admin@chocobabies.com");
    if (user == null)
    {
        user = new User { UserName = "admin@chocobabies.com", Email = "admin@chocobabies.com", FullName = "Admin User", EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

app.Run();