// Localização: Aluguer_Salas/Data/SeedData.cs

// Usings necessários para as classes e métodos utilizados neste arquivo
using Aluguer_Salas.Data; // Para ApplicationDbContext e Utilizadores
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;     // Para IConfiguration
using Microsoft.Extensions.DependencyInjection; // Para IServiceProvider, GetRequiredService
using Microsoft.Extensions.Logging;         // Para ILogger (opcional, se quiser usar logging formal)
using System;                                 // Para IServiceProvider, Console, Exception
using System.Linq;                            // Para .Select() na formatação de erros
using System.Threading.Tasks;                 // Para Task

namespace Aluguer_Salas.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            // Obter os serviços necessários
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Opcional: Obter um logger para um logging mais estruturado
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Aluguer_Salas.Data.SeedData");

            logger.LogInformation("--- A iniciar o processo de SeedData ---");

            // Nome do Role de Administrador
            string adminRoleName = "Administrador";

            // 1. Criar o Role "Administrador" se não existir
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                logger.LogInformation($"Role '{adminRoleName}' não encontrado. A tentar criar...");
                var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                if (roleResult.Succeeded)
                {
                    logger.LogInformation($"✔️ Role '{adminRoleName}' criado com sucesso.");
                }
                else
                {
                    logger.LogError($"❌ Erro ao criar role '{adminRoleName}': {string.Join(", ", roleResult.Errors.Select(e => $"{e.Code} - {e.Description}"))}");
                }
            }
            else
            {
                logger.LogInformation($"ℹ️ Role '{adminRoleName}' já existe.");
            }

            // 2. Definir os dados dos administradores a partir da configuração
            string[] adminEmails = new string[] {
                configuration.GetValue<string>("AdminUsers:Admin1:Email") ?? "admin1_fallback@example.com",
                configuration.GetValue<string>("AdminUsers:Admin2:Email") ?? "admin2_fallback@example.com",
                configuration.GetValue<string>("AdminUsers:Admin3:Email") ?? "admin3_fallback@example.com"
            };

            for (int i = 0; i < adminEmails.Length; i++)
            {
                string adminEmail = adminEmails[i];
                // Lê a configuração específica para este admin dentro do loop
                string adminPassword = configuration.GetValue<string>($"AdminUsers:Admin{i + 1}:Password") ?? "SenhaPadraoExtraForte123!"; // Use um fallback seguro ou trate o erro
                string adminNome = configuration.GetValue<string>($"AdminUsers:Admin{i + 1}:Nome") ?? $"Utilizador Admin {i + 1}";

                logger.LogInformation($"--- A processar utilizador: {adminEmail} ---");

                // 3. Verificar se o utilizador já existe
                var existingUser = await userManager.FindByEmailAsync(adminEmail);

                if (existingUser == null)
                {
                    logger.LogInformation($"Utilizador '{adminEmail}' não encontrado. A tentar criar...");
                    var adminUser = new Utilizador
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Nome = adminNome,
                        EmailConfirmed = true // Confirmar automaticamente para admins criados internamente
                    };

                    var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                    if (createUserResult.Succeeded)
                    {
                        logger.LogInformation($"✔️ Utilizador '{adminEmail}' criado com sucesso.");
                        existingUser = adminUser; // Usar o utilizador recém-criado para atribuição de role
                    }
                    else
                    {
                        logger.LogError($"❌ Erro ao criar utilizador '{adminEmail}':");
                        foreach (var error in createUserResult.Errors)
                        {
                            logger.LogError($"   - {error.Code}: {error.Description}");
                        }
                        continue; // Pula para o próximo admin se a criação falhar
                    }
                }
                else
                {
                    logger.LogInformation($"ℹ️ Utilizador '{adminEmail}' já existe.");
                }

                // 4. Atribuir o role "Administrador" ao utilizador (novo ou existente) se ainda não o tiver
                if (existingUser != null && !await userManager.IsInRoleAsync(existingUser, adminRoleName))
                {
                    logger.LogInformation($"A tentar atribuir role '{adminRoleName}' a '{adminEmail}'...");
                    var addToRoleResult = await userManager.AddToRoleAsync(existingUser, adminRoleName);
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation($"🔐 Role '{adminRoleName}' atribuído com sucesso a '{adminEmail}'.");
                    }
                    else
                    {
                        logger.LogError($"❌ Erro ao atribuir role '{adminRoleName}' a '{adminEmail}': {string.Join(", ", addToRoleResult.Errors.Select(e => $"{e.Code} - {e.Description}"))}");
                    }
                }
                else if (existingUser != null)
                {
                    logger.LogInformation($"ℹ️ Utilizador '{adminEmail}' já possui o role '{adminRoleName}'.");
                }
            }
            logger.LogInformation("--- Processo de SeedData concluído ---");
        }
    }
}