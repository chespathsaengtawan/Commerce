using Microsoft.AspNetCore.Identity;
using ShopInstallment.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopInstallment.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // สร้าง Roles
            string[] roleNames = { "Admin", "Seller", "Buyer" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // สร้าง Admin user
            var adminEmail = "support@99baht.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Support",
                    CustomerCode = "ADMIN001",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@99baht");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // ตรวจสอบว่า Admin มี Role แล้วหรือยัง
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            string[] genderNames = { "ชาย", "หญิง", "อื่นๆ" };
            foreach (var genderName in genderNames)
            {
                var genderExist = await context.Genders.FirstOrDefaultAsync(g => g.Name == genderName);
                if (genderExist == null)
                {
                    context.Genders.Add(new Gender { Name = genderName });
                    await context.SaveChangesAsync();
                }
            }

            var existingCategories = await context.Categories.AnyAsync();
            foreach (var category in context.Categories)
            {
                if (category.GenderId != null)
                {
                    category.GenderId = 1;
                    context.Categories.Update(category);
                    await context.SaveChangesAsync();
                }
            }
            
        }
    }
}
