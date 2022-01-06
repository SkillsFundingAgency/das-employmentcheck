using Microsoft.AspNetCore.Identity;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Infrastructure.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Services.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedDefaultUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var administratorRole = new IdentityRole("Administrator");

            if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await roleManager.CreateAsync(administratorRole);
            }

            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

            if (userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await userManager.CreateAsync(administrator, "Administrator1!");
                await userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        public static async Task SeedSampleDataAsync(ApplicationDbContext context)
        {
            // Seed, if necessary
            if (!context.ApprenticeEmploymentChecks.Any())
            {
                context.ApprenticeEmploymentChecks.Add(new ApprenticeEmploymentCheck
                {
                    Uln = 1000000001,
                    AccountId = 10000001,
                    CheckType = "Seed record 1",
                    CheckAllPayeSchemes = false,
                    ApprenticeshipId = 100001,
                    MinDate = new System.DateTime(2021, 12, 27),
                    MaxDate = new System.DateTime(2021, 12, 28)
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
