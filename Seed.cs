using Ilmhub.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Ilmhub.Identity;

public class Seed
{
    public static async Task InitializeRolesAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Seed>();

        var roles = config.GetSection("Identity:IdentityServer:Roles").Get<string[]>();

        foreach(var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
                var newRole = new IdentityRole(role);
                var result = await roleManager.CreateAsync(newRole);

                if(!result.Succeeded)
                {
                    logger.LogWarning($"Seed role {role} failed. Error: {result.Errors.First().Description}");
                }
                else
                {
                    logger.LogInformation("Seed role {0} succeeded.", role);
                }
            }
            else
            {
                logger.LogInformation($"Role {role} already exists.");
            }
        }

        logger.LogInformation("Role seed is finished.");
    }

    public static async Task InitializeUserAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Seed>();

        var users = config.GetSection("Identity:IdentityServer:TestUsers").Get<TestUser[]>();

        foreach(var user in users)
        {
            var newUser = new IdentityUser(user.UserName);

            var result = await userManager.CreateAsync(newUser, user.Password);

            if(result.Succeeded)
            {
                var roleResult = await userManager.AddToRolesAsync(newUser, user.Roles);

                if(roleResult.Succeeded)
                {
                    logger.LogInformation($"{user.UserName}ga rollar qo'shildi.");
                }
                else
                {
                    logger.LogInformation($"{user.UserName}ga rollar qo'shilmadi.");
                }
            }
            else
            {
                logger.LogInformation($"{user.UserName} user yaratilmadi");
            }
        }
    }
}