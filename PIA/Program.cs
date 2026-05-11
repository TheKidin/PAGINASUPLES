using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PIA.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la conexión a la base de datos de tus suplementos (AHORA CON SQLITE)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

// AQUI ESTA EL CAMBIO: Cambiamos UseSqlServer por UseSqlite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Configurar ASP.NET Core Identity (El sistema de Usuarios)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddTransient<PIA.Services.EmailSenderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 3. Activar la Autenticación
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 4. Mapear las páginas de Razor
app.MapRazorPages();

app.Run();