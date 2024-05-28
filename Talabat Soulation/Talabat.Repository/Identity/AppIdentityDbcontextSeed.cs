using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Repository.Identity
{
    public static class AppIdentityDbcontextSeed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> _userManager)
        {
            if(_userManager.Users.Count() == 0)
            {
                var users = new AppUser()
                {
                    DisplayName = "Yomna Magdy",
                    Email = "yomna@gmail.com",
                    UserName = "yomna.magdy",
                    PhoneNumber = "0123456789",
                };
                await _userManager.CreateAsync(users, password: "Pa$$W0rd");
            }
        }

    }
}
