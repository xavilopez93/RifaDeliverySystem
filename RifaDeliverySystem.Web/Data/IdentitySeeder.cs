using Microsoft.AspNetCore.Identity;

namespace RifaDeliverySystem.Web.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider svc)
        {
            var roleMgr = svc.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = svc.GetRequiredService<UserManager<ApplicationUser>>();

            // Crea rol "Admin"
            if (!await roleMgr.RoleExistsAsync("Admin"))
                await roleMgr.CreateAsync(new IdentityRole("Admin"));

            // Crea usuario admin
            var admin = await userMgr.FindByEmailAsync("admin@site.com");
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = "admin@site.com", Email = "admin@site.com"};
                await userMgr.CreateAsync(admin, "P4ssw0rd!");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
