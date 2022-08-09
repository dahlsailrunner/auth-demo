using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationDemo.Auth
{
    public class AppUser : IdentityUser
    {
        public string? ExternalSubjectId { get; set; }
    }

    public class UserContext : IdentityDbContext<AppUser>
    {
        public string DbPath { get; set; }

        public UserContext(IConfiguration config)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DbPath = Path.Join(path, config.GetConnectionString("UserDbSQLiteFilename"));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public static class DevUserHelper
    {
        public static async Task CreateDevUsers(UserManager<AppUser> userMgr)
        {
            foreach (var user in userMgr.Users)
            {
                await userMgr.DeleteAsync(user); // remove existing users, start from scratch
            }

            var appUser = new AppUser { UserName = "erik@acme.com", EmailConfirmed = true };
            await userMgr.CreateAsync(appUser, "Password123!");
            await userMgr.AddClaimAsync(appUser, new Claim("CompanyId", "1234"));

            appUser = new AppUser { UserName = "ali@mars.com", EmailConfirmed = true };
            await userMgr.CreateAsync(appUser, "Password123!");
            await userMgr.AddClaimAsync(appUser, new Claim("CompanyId", "7890"));

            appUser = new AppUser { UserName = "mega@local.com", EmailConfirmed = true };
            await userMgr.CreateAsync(appUser, "Password123!");
            await userMgr.AddClaimsAsync(appUser, new List<Claim>
            {
                new("CompanyId", "1234"),
                new("CompanyId", "7890")

            });

            appUser = new AppUser { UserName = "bob@someplace.com", EmailConfirmed = true, ExternalSubjectId = "2" };
            await userMgr.CreateAsync(appUser);
            await userMgr.AddClaimAsync(appUser, new Claim("CompanyId", "7890"));
        }
    }
}
