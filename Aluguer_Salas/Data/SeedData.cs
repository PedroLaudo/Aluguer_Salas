// Localização: Aluguer_Salas/Data/SeedData.cs
using Aluguer_Salas.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Aluguer_Salas.Models; // Contém Utilizador e Utente

namespace Aluguer_Salas.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>(); // Utilizador é sua classe que herda de IdentityUser
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // Para obter o logger para a classe SeedData corretamente se SeedData for estática:
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Aluguer_Salas.Data.SeedData"); // Usar uma categoria de string

            logger.LogInformation("--- A iniciar o processo de SeedData ---");

            // 1. CRIAR ROLES ESSENCIAIS
            string[] roleNames = { "Administrador", "Professor", "Aluno" }; // Adicione outros roles conforme necessário
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    logger.LogInformation($"Role '{roleName}' não encontrado. A tentar criar...");
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (roleResult.Succeeded) logger.LogInformation($"✔️ Role '{roleName}' criado com sucesso.");
                    else logger.LogError($"❌ Erro ao criar role '{roleName}': {FormatErrors(roleResult.Errors)}");
                }
                else logger.LogInformation($"ℹ️ Role '{roleName}' já existe.");
            }

            // 2. CRIAR UTILIZADORES ADMINISTRADORES E ATRIBUIR ROLE
            string adminRoleName = "Administrador";
            for (int i = 0; i < 3; i++) // Supondo 3 admins
            {
                string adminEmail = configuration.GetValue<string>($"AdminUsers:Admin{i + 1}:Email") ?? $"admin{i + 1}@example.com";
                string adminPassword = configuration.GetValue<string>($"AdminUsers:Admin{i + 1}:Password") ?? "SenhaAdminSuperSegura123!"; // Use senhas fortes
                string adminNome = configuration.GetValue<string>($"AdminUsers:Admin{i + 1}:Nome") ?? $"Admin User {i + 1}";

                logger.LogInformation($"--- A processar utilizador Admin: {adminEmail} ---");
                // O parâmetro 'nome' aqui é para Utilizador.Nome (IdentityUser)
                await CreateUserWithUtenteAndAssignRole(userManager, adminEmail, adminPassword, adminNome, adminRoleName, logger, context);
            }

            // 3. CRIAR UTILIZADOR PROFESSOR E ATRIBUIR ROLE
            string professorRoleName = "Professor";
            string professorEmail = "pedro2004laudo@gmail.com";
            string professorPassword = "SenhaProfessorSuperSegura123!"; // Use senhas fortes
            string professorNome = "Pedro Caetano Laudo"; // Este é o Utilizador.Nome (IdentityUser)

            logger.LogInformation($"--- A processar utilizador Professor: {professorEmail} ---");
            // O parâmetro 'nome' aqui é para Utilizador.Nome (IdentityUser)
            await CreateUserWithUtenteAndAssignRole(userManager, professorEmail, professorPassword, professorNome, professorRoleName, logger, context);

            logger.LogInformation("--- Processo de SeedData concluído ---");
        }

        private static async Task CreateUserWithUtenteAndAssignRole(
            UserManager<Utilizador> userManager,
            string email,
            string password,
            string nomeParaIdentityUser, // Nome que será definido em Utilizador.Nome (IdentityUser)
            string roleName,             // Nome do IdentityRole a ser atribuído
            ILogger logger,
            ApplicationDbContext dbContext)
        {
            var existingIdentityUser = await userManager.FindByEmailAsync(email);

            if (existingIdentityUser == null)
            {
                logger.LogInformation($"Utilizador Identity '{email}' não encontrado. A tentar criar...");
                existingIdentityUser = new Utilizador // Sua classe Aluguer_Salas.Models.Utilizador que herda de IdentityUser
                {
                    UserName = email,
                    Email = email,
                    Nome = nomeParaIdentityUser, // Define o Nome no IdentityUser
                    EmailConfirmed = true // Confirme automaticamente para usuários semeados
                };
                var createUserResult = await userManager.CreateAsync(existingIdentityUser, password);

                if (createUserResult.Succeeded)
                {
                    logger.LogInformation($"✔️ Utilizador Identity '{email}' criado com sucesso.");
                }
                else
                {
                    logger.LogError($"❌ Erro ao criar utilizador Identity '{email}': {FormatErrors(createUserResult.Errors)}");
                    return; // Não prosseguir se a criação do IdentityUser falhar
                }
            }
            else
            {
                logger.LogInformation($"ℹ️ Utilizador Identity '{email}' já existe.");
                // Opcional: atualizar o Nome no IdentityUser se for diferente
                if (existingIdentityUser.Nome != nomeParaIdentityUser)
                {
                    existingIdentityUser.Nome = nomeParaIdentityUser;
                    var updateResult = await userManager.UpdateAsync(existingIdentityUser);
                    if (updateResult.Succeeded)
                    {
                        logger.LogInformation($"Nome do Utilizador Identity '{email}' atualizado para '{nomeParaIdentityUser}'.");
                    }
                    else
                    {
                        logger.LogError($"Erro ao atualizar nome do Utilizador Identity '{email}': {FormatErrors(updateResult.Errors)}");
                    }
                }
            }

            // Atribuir o IdentityRole ao IdentityUser se ainda não o tiver
            if (!await userManager.IsInRoleAsync(existingIdentityUser, roleName))
            {
                logger.LogInformation($"A tentar atribuir IdentityRole '{roleName}' a '{email}'...");
                var addToRoleResult = await userManager.AddToRoleAsync(existingIdentityUser, roleName);
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation($"🔐 IdentityRole '{roleName}' atribuído com sucesso a '{email}'.");
                }
                else
                {
                    logger.LogError($"❌ Erro ao atribuir IdentityRole '{roleName}' a '{email}': {FormatErrors(addToRoleResult.Errors)}");
                }
            }
            else
            {
                logger.LogInformation($"ℹ️ Utilizador '{email}' já possui o IdentityRole '{roleName}'.");
            }

            // Gerenciar a entidade Utente (seu perfil customizado que NÃO tem 'Nome')
            var utente = await dbContext.Utentes
                                .FirstOrDefaultAsync(u => u.UtilizadorIdentityId == existingIdentityUser.Id);

            if (utente == null)
            {
                logger.LogInformation($"A criar entrada Utente para '{email}'...");
                utente = new Utente
                {
                    Email = email,                                // Copia do email
                    Tipo = roleName,                              // O Tipo do Utente é definido com base no roleName do Identity
                    UtilizadorIdentityId = existingIdentityUser.Id  // Chave estrangeira para o IdentityUser
                    // A propriedade 'Nome' NÃO existe em Utente, então não é definida aqui.
                };
                dbContext.Utentes.Add(utente);
            }
            else
            {
                logger.LogInformation($"Entrada Utente para '{email}' já existe. A verificar/atualizar Tipo.");
                bool utenteChanged = false;
                if (utente.Tipo != roleName)
                {
                    utente.Tipo = roleName; // Atualiza o Tipo se o roleName do IdentityUser for diferente
                    utenteChanged = true;
                }
                // Nenhuma verificação ou atualização para utente.Nome, pois Utente não tem essa propriedade.
                if (utenteChanged)
                {
                    dbContext.Utentes.Update(utente);
                }
            }

            // Salvar mudanças na entidade Utente
            try
            {
                await dbContext.SaveChangesAsync(); // Salva o Utente (novo ou atualizado)
                if (utente != null) logger.LogInformation($"✔️ Entrada Utente para '{email}' salva/atualizada com sucesso.");
            }
            catch (DbUpdateException dbEx) // Ser mais específico com a exceção
            {
                logger.LogError(dbEx, $"❌ Erro de banco de dados ao salvar/atualizar Utente para '{email}'. Detalhes: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ Erro geral ao salvar/atualizar Utente para '{email}'.");
            }
        }

        private static string FormatErrors(System.Collections.Generic.IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(e => $"{e.Code}: {e.Description}"));
        }
    }
}