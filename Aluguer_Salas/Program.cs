using Aluguer_Salas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // Para IEmailSender
using Aluguer_Salas.Models;
using Microsoft.EntityFrameworkCore;
using Aluguer_Salas.Services.Email;


var builder = WebApplication.CreateBuilder(args);

// 1. Connection String e DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Adiciona o filtro de exceções de desenvolvimento para o Entity Framework
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Configuração do Identity
builder.Services.AddIdentity<Utilizador, IdentityRole>(options => // Use AddIdentity para configurar roles e opções
{
    // Configurações de Senha
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Defina como true para mais segurança se desejar

    // Configurações de Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configurações de Utilizador
    options.User.RequireUniqueEmail = true;

    // Configurações de SignIn (Confirmação de Email)
    options.SignIn.RequireConfirmedEmail = true; // Obrigatório para login
    // A linha options.SignIn.RequireConfirmedAccount = true; é redundante se RequireConfirmedEmail = true
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders() // Para tokens de reset de senha, confirmação de email, etc.
    .AddRoles<IdentityRole>(); // Certifica que AddRoles é chamado aqui se não estiver já no AddIdentity acima

// Nota: Você tinha duas chamadas a AddDefaultIdentity. A segunda foi integrada na primeira AddIdentity.
// A chamada .AddRoles<IdentityRole>() pode ser encadeada diretamente após .AddDefaultTokenProviders()
// ou já estar coberta pela chamada AddIdentity<Utilizadores, IdentityRole> se IdentityRole for especificado.
// Para clareza, deixei o .AddRoles<IdentityRole>() explícito, mas verifique se não está duplicado.
// Se `builder.Services.AddIdentity<Utilizadores, IdentityRole>` já está ali, o `.AddRoles<IdentityRole>()` separado
// não é estritamente necessário, mas não prejudica.

// 3. Regista o serviço de envio de e-mail
builder.Services.AddTransient<ICustomEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// 4. Personaliza opções do Cookie de Autenticação do Identity (Opcional, mas bom para ter)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Caminho padrão para páginas Identity UI
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 5. Configuração de Autorização (Políticas, se necessário no futuro)
builder.Services.AddAuthorization(options =>
{
    // Exemplo de como adicionar uma policy, se você usar AuthorizeFolder com policy
    options.AddPolicy("AdministradorPolicy", policy =>
        policy.RequireRole("Administrador"));
});

// 6. Serviços da Aplicação (MVC e/ou Razor Pages)
 builder.Services.AddControllersWithViews(); 
builder.Services.AddRazorPages(options =>
{
    // Convenções de autorização para pastas, se desejar (alternativa a [Authorize] em cada página)
    // Exemplo: Proteger todas as páginas dentro de /Admin para que exijam o role "Administrador"
    // options.Conventions.AuthorizeFolder("/Admin", "AdministradorPolicy");
    // Ou para Razor Pages dentro de Áreas:
    // options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage").RequireAuthenticatedUser();
});


// --- Construção do App ---
var app = builder.Build();

// --- Seed de Dados (Apenas em Desenvolvimento ou uma vez na produção, gerido com cuidado) ---
// É uma boa prática executar migrações e seed aqui.
if (app.Environment.IsDevelopment()) // Condicionar o seed pode ser útil
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("A aplicar migrações e a semear a base de dados...");

            var context = services.GetRequiredService<ApplicationDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate(); // Aplica migrações pendentes
                logger.LogInformation("Migrações aplicadas com sucesso.");
            }
            else
            {
                logger.LogInformation("Nenhuma migração pendente.");
            }

            var configuration = services.GetRequiredService<IConfiguration>();
            await SeedData.Initialize(services, configuration); // Chamar o método de seed
            logger.LogInformation("Seed da base de dados concluído com sucesso.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocorreu um erro durante a migração ou ao semear a base de dados.");
            // Considerar lançar a exceção ou parar a aplicação se o seed for crítico para o arranque
        }
    }
}

// --- Pipeline HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Middleware para ajudar com migrações EF em desenvolvimento
    app.UseDeveloperExceptionPage(); // Mostra erros detalhados em desenvolvimento
}
else
{
    app.UseExceptionHandler("/Error"); // Caminho para a página de erro de Razor Pages
    // app.UseExceptionHandler("/Home/Error"); // Para MVC
    app.UseHsts(); // Adiciona o header Strict-Transport-Security
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir arquivos de wwwroot (CSS, JS, imagens)

app.UseRouting(); // Habilita o roteamento

// Middlewares de Autenticação e Autorização (A ORDEM É IMPORTANTE)
app.UseAuthentication(); // Identifica quem é o utilizador
app.UseAuthorization();  // Verifica se o utilizador tem permissão

// Mapeamento de Endpoints
 app.MapControllerRoute( // Descomente e ajuste se tiver Controllers MVC como endpoint principal
    name: "default",
   pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Para Razor Pages e Identity UI

app.Run();