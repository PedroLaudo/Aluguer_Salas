using Aluguer_Salas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Aluguer_Salas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Aluguer_Salas.Services.Email;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Obter a connection string do ficheiro de configuração
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar o DbContext com SQL Server
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

// Configurar o Identity com regras de segurança e suporte a roles
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

// Registar serviço de envio de emails
builder.Services.AddTransient<ICustomEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Definir caminhos de login, logout e acesso negado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Definir política de autorização para administradores
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministradorPolicy", policy =>
        policy.RequireRole("Administrador"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Ativar documentação da API com Swagger
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

// Ativar telemetria com Application Insights
builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

var app = builder.Build();

// Executar seed de dados em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await SeedData.Initialize(services, builder.Configuration);
        }
        catch (Exception)
        {
            // Ignorar erros de seed para evitar falha na execução
        }
    }
}

// Ativar Swagger (documentação da API)
app.UseSwagger();
app.UseSwaggerUI();

// Configuração de tratamento de erros
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

// Middleware padrão da aplicação
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Rota por omissão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Restringir acesso ao Swagger a utilizadores com o perfil de Administrador
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Administrador"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Acesso negado. Apenas Administradores podem aceder ao Swagger.");
            return;
        }
    }

    await next.Invoke();
});

app.Run();
