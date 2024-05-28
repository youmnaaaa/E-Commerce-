using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Interfaces;

namespace Talabat.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> _userManager)
        {
            var AuthClamis = new List<Claim>()
            {
                new Claim(ClaimTypes.GivenName, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var UserRoles = await _userManager.GetRolesAsync(user);
            foreach (var Role in UserRoles)
            {
                AuthClamis.Add(item: new Claim(ClaimTypes.Role, Role)); }

            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[key: "JWT:Key"]));
            var token = new JwtSecurityToken(
                  issuer: _configuration[key: "JWT:ValidIssuer"],
                  audience: _configuration[key: "JWT:ValidAudience"],
                  expires: DateTime.Now.AddDays(double.Parse(_configuration[key: "JWT:DurationInDays"])),
                  claims: AuthClamis,
                  signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );
            
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
