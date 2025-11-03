using Esferas.Data;
using Esferas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<EmailSender>();


// ✅ Configuración única de Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

// builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PreguntaImportService>();
builder.Services.AddScoped<LinkUnicoService>();
builder.Services.AddScoped<ResultadosEmpresaService>();
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "public-result",
    pattern: "r/{token:guid}",
    defaults: new { controller = "Resultados", action = "Personal" });

// app.MapRazorPages();

// ✅ Crear usuario de prueba si no existe
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    string email = "admin@esferas.com";
    string password = "123456";

    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        var newUser = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"❌ Error creando usuario: {error.Description}");
            }
        }
        else
        {
            Console.WriteLine("✅ Usuario creado correctamente.");
        }
    }
    else
    {
        Console.WriteLine("ℹ️ Usuario ya existe.");
    }
}

app.Run();
