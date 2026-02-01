using EmployeeManager.Application.Security;
using EmployeeManager.Domain.Roles;
using EmployeeManager.Infrastructure.Data;
using EmployeeManager.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Api.Seed;

public static class DirectorSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        // 1) Lê config
        var seed = app.Configuration.GetSection("Seed");
        var enabled = seed.GetValue<bool>("Enabled");
        if (!enabled) return;

        var email = seed.GetValue<string>("DirectorEmail")?.Trim().ToLowerInvariant();
        var password = seed.GetValue<string>("DirectorPassword");
        var doc = seed.GetValue<string>("DirectorDocNumber")?.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(doc))
        {
            throw new InvalidOperationException("Seed config is missing required values. Check appsettings.json -> Seed.");
        }

        // 2) Cria scope pra pegar serviços do DI
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // 3) Garante que o banco está com migrations aplicadas
        // (em DEV isso é ótimo; em PROD normalmente se faz fora do app)
        await db.Database.MigrateAsync();

        // 4) Se já existe esse diretor por email, atualiza a senha para o valor do seed
        var existing = await db.Employees.FirstOrDefaultAsync(e => e.Email == email);
        if (existing is not null)
        {
            existing.PasswordHash = hasher.Hash(password);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.IsActive = true;
            existing.DeactivatedAt = null;

            await db.SaveChangesAsync();
            Console.WriteLine($">>> Seed: Director already exists ({email}). Password reset from Seed.");
            return;
        }

        // 5) Cria registro
        var directorId = Guid.NewGuid();

        var entity = new EmployeeEntity
        {
            Id = directorId,
            FirstName = "Director",
            LastName = "Seed",
            Email = email,
            DocNumber = doc,
            BirthDate = new DateOnly(1990, 1, 1),
            Role = Role.Director,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            DeactivatedAt = null,
            PasswordHash = hasher.Hash(password),
            ManagerEmployeeId = null,
            Phones = new List<EmployeePhoneEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = directorId,
                    Number = "999990000",
                    Type = "seed"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = directorId,
                    Number = "333300000",
                    Type = "seed"
                }
            }
        };

        db.Employees.Add(entity);
        await db.SaveChangesAsync();

        Console.WriteLine($">>> Seed: Director created ({email}) with DocNumber={doc}.");
    }
}