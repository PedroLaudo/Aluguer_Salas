using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aluguer_Salas.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Aluguer_Salas.Data.SeedData");

            logger.LogInformation("--- A iniciar o processo de SeedData ---");

            // Criar as roles principais, caso ainda não existam
            string[] roleNames = { "Administrador", "Professor", "Aluno" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"✔️ Role '{roleName}' criado com sucesso.");
                }
            }

            // Ler utilizadores da configuração e criar os que ainda não existem
            var usersToSeed = configuration.GetSection("AdminUsers").Get<List<SeedUserConfig>>();

            if (usersToSeed == null || !usersToSeed.Any())
            {
                logger.LogInformation("Nenhum utilizador encontrado na configuração 'AdminUsers'.");
                return;
            }

            foreach (var userConfig in usersToSeed)
            {
                await CreateUserWithUtenteAndAssignRole(
                    userManager,
                    context,
                    logger,
                    userConfig.Email,
                    userConfig.Password,
                    userConfig.Nome,
                    userConfig.Role
                );
            }

            // Guardar alterações na base de dados, se existirem
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Alterações do SeedData guardadas com sucesso na base de dados.");
            }

            logger.LogInformation("--- Processo de SeedData concluído ---");
        }

        // Cria um utilizador com uma entrada Utente e atribui-lhe uma role
        private static async Task CreateUserWithUtenteAndAssignRole(
            UserManager<Utilizador> userManager,
            ApplicationDbContext dbContext,
            ILogger logger,
            string email,
            string password,
            string nome,
            string roleName)
        {
            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                identityUser = new Utilizador
                {
                    UserName = email,
                    Email = email,
                    Nome = nome,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(identityUser, password);
                if (!result.Succeeded)
                {
                    logger.LogError($"❌ Erro ao criar utilizador '{email}': {FormatErrors(result.Errors)}");
                    return;
                }
                logger.LogInformation($"✔️ Utilizador '{email}' criado com sucesso.");
            }

            if (!await userManager.IsInRoleAsync(identityUser, roleName))
            {
                await userManager.AddToRoleAsync(identityUser, roleName);
                logger.LogInformation($"🔐 Role '{roleName}' atribuída a '{email}'.");
            }

            var utente = await dbContext.Utentes.FirstOrDefaultAsync(u => u.UtilizadorIdentityId == identityUser.Id);
            if (utente == null)
            {
                utente = new Utente
                {
                    Email = email,
                    Tipo = roleName,
                    UtilizadorIdentityId = identityUser.Id
                };
                dbContext.Utentes.Add(utente);
                logger.LogInformation($"✔️ Utente associado ao utilizador '{email}' adicionado.");
            }
        }

        // Formata erros de criação do Identity
        private static string FormatErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(e => e.Description));
        }
    }

    // Modelo auxiliar para carregar utilizadores do appsettings.json
    public class SeedUserConfig
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
