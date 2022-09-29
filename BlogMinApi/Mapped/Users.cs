using BlogMinApi.Dto;
using Cust.Areas.Identity.Data;
using Cust.Data;
using Cust.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogMinApi.Mapped;

public class Users
{
    public static async Task<IResult> CreateUser(UserManager<CustUser> userMgr, UserDto user)
    {
        var identityUser = new CustUser()
        {
            UserName = user.UserName,
            Alias = user.Alias,
            Email = user.UserName
        };

        bool userExists = userMgr.FindByEmailAsync(user.UserName).Result != null;
        bool aliasExists = userMgr.Users.Where(u => u.Alias == user.Alias).Any();
        if (userExists || aliasExists) { return Results.Text("User or alias already exist"); } // Или тут уместнее другой код?

        var result = await userMgr.CreateAsync(identityUser, user.Password);

        if (result.Succeeded)
        {
            return Results.Ok();
        }
        else
        {
            return Results.BadRequest();
        }
    }

    public static async Task<IResult> GetToken(WebApplicationBuilder builder, UserManager<CustUser> userMgr, UserDto user)
    {
        var identityUsr = await userMgr.FindByNameAsync(user.UserName);

        if (await userMgr.CheckPasswordAsync(identityUsr, user.Password))
        {
            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                // Expires = DateTime.Now.AddMinutes(5),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = credentials
            };

            var adminClaim = userMgr.GetClaimsAsync(identityUsr).Result.Where(c => c.Type == "IsAdmin").FirstOrDefault();
            if (adminClaim != null)
            {
                tokenDescriptor.Subject.Claims.Append(new Claim("IsAdmin", ""));
            }

            var editorClaim = userMgr.GetClaimsAsync(identityUsr).Result.Where(c => c.Type == "IsEditor").FirstOrDefault();
            if (editorClaim != null)
            {
                tokenDescriptor.Subject.AddClaim(new Claim("IsEditor", ""));
            }

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            //var jwtToken = jwtTokenHandler.WriteToken(token);

            //var token = new JwtSecurityToken(issuer: issuer, audience: audience,signingCredentials: credentials);
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var stringToken = tokenHandler.WriteToken(token);
            var stringToken = jwtTokenHandler.WriteToken(token);

            return Results.Ok(stringToken);
        }
        else
        {
            return Results.Unauthorized();
        }

    }


}
