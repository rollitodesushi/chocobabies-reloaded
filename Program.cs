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
string connectionString = null;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else // Production (Render)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        connectionString = $"Host={databaseUri.Host};Port=5432;Database={databaseUri.Segments.Last().Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se pudo obtener el connection string para el entorno actual.");
}

Console.WriteLine($"Connection String: {connectionString}");
builder.Services.AddDbContext<RifaDbContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())); // Opcional: logging para depuración

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



// Configurar localización en el pipeline
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-CO"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("es-CO") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("es-CO") }
});


app.UseStaticFiles(); // Asegúrate de que esta línea esté presente y no esté comentada
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<RifaHub>("/rifaHub");

// Asegurar creación de roles y asignación inicial (opcional, descomentar si necesitas)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    // Crear rol "Admin" si no existe
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
    }

    // Asignar rol "Admin" al primer usuario registrado (ajústalos con el email usado)
    var user = await userManager.FindByEmailAsync("admin@chocobabies.com");
    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

app.Run();