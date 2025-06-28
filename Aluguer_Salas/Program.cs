// Ficheiro: Program.cs
using Aluguer_Salas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Aluguer_Salas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Aluguer_Salas.Services.Email;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE SERVIÇOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    Console.WriteLine("AVISO: Connection String 'DefaultConnection' não encontrada. O DbContext não foi configurado.");
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Utilizador, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();

builder.Services.AddTransient<ICustomEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministradorPolicy", policy =>
        policy.RequireRole("Administrador"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gestão de Salas e Materiais",
        Version = "v1",
        Description = "API para gestão de salas, materiais, reservas e requisições de material"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

// --- 2. CONSTRUÇÃO DA APLICAÇÃO ---
var app = builder.Build();

// --- 3. SEED DA BASE DE DADOS (Executado em todos os ambientes) ---
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // ...
            await SeedData.Initialize(services, builder.Configuration);
            // ...
        }
        catch (Exception ex)
        {
            // ...
        }
    }
}

// --- 4. CONFIGURAÇÃO DO PIPELINE HTTP ---

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Adiciona este bloco de código para proteger o Swagger
app.Use(async (context, next) =>
{
    // Verifica se o caminho do pedido começa com /swagger
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        // Verifica se o utilizador está autenticado e se tem a função "Administrador"
        // Se não tiver, retorna um erro 403 (Acesso Proibido)
        if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Administrador"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Acesso negado. Apenas Administradores podem aceder ao Swagger.");
            return; // Termina o pedido aqui
        }
    }

    // Se o utilizador for administrador ou o caminho não for /swagger, continua para o próximo middleware
    await next.Invoke();
});

// --- 5. EXECUÇÃO DA APLICAÇÃO ---
app.Run();