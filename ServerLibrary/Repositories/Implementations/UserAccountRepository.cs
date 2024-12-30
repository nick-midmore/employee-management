using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.Repositories.Implementations;
public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext context) : IUserAccount
{
    public async Task<GeneralResponse> RegisterAsync(Register user)
    {
        if (user is null) return new GeneralResponse(false, "Model is empty");

        var checkUser = await FindUserByEmail(user.Email!);
        if (checkUser != null) return new GeneralResponse(false, "User already exists");

        var applicationUser = await AddToDb(new ApplicationUser()
        {
            Email = user.Email,
            FullName = user.FullName,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
        });

        var checkAdminRole = await context.SystemRoles.FirstOrDefaultAsync(sr => sr.Name!.Equals(Constants.Admin));
        if (checkAdminRole is null)
        {
            var createAdminRole = await AddToDb(new SystemRole() { Name = Constants.Admin });
            await AddToDb(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });
            return new GeneralResponse(true, "Account created");
        }

        var checkUserRole = await context.SystemRoles.FirstOrDefaultAsync(sr => sr.Name!.Equals(Constants.User));
        SystemRole response = new();
        if (checkUserRole is null)
        {
            response = await AddToDb(new SystemRole() { Name = Constants.User });
            await AddToDb(new UserRole() { RoleId= response.Id, UserId = applicationUser.Id });
        }
        else await AddToDb(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationUser.Id });

        return new GeneralResponse(true, "Account created");
    }

    public async Task<LoginResponse> SignInAsync(Login login)
    {
        if (login is null) return new LoginResponse(false, "Model is empty");

        var user = await FindUserByEmail(login.Email);
        if (user is null) return new LoginResponse(false, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password)) return new LoginResponse(false, "Invalid email/password combination");

        var userRole = await GetUserRole(user.Id);
        if (userRole is null) return new LoginResponse(false, "User role not found");

        var roleName = await GetRoleName(userRole.RoleId);
        if (userRole is null) return new LoginResponse(false, "User role not found");

        string jwtToken = GenerateToken(user, roleName!.Name!);
        string refreshToken = GenerateRefreshToken();

        var findUser = await context.RefreshTokenInfo.FirstOrDefaultAsync(t => t.UserId == user.Id);
        if (findUser != null)
        {
            findUser!.Token = refreshToken;
            await context.SaveChangesAsync();
        }
        else
        {
            await AddToDb(new RefreshTokenInfo() { Token = refreshToken, UserId = user.Id });
        }
        
        return new LoginResponse(true, "Login successful", jwtToken, refreshToken);
    }

    private string GenerateToken(ApplicationUser user, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Role, role!)
        };

        var token = new JwtSecurityToken(
            issuer: config.Value.Issuer,
            audience: config.Value.Audience,
            claims: userClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private async Task<UserRole> GetUserRole(int userId) 
        => await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);

    private async Task<SystemRole> GetRoleName(int roleId) 
        => await context.SystemRoles.FirstOrDefaultAsync(sr => sr.Id == roleId);

    public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
    {
        if (token == null) return new LoginResponse(false, "Model is empty");

        var findToken = await context.RefreshTokenInfo.FirstOrDefaultAsync(ti => ti.Token!.Equals(token.Token));
        if (findToken == null) return new LoginResponse(false, "Invalid token");

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(au => au.Id == findToken.UserId);
        if (user == null) return new LoginResponse(false, "Refresh token does not match any user");

        var userRole = await GetUserRole(user.Id);
        var roleName = await GetRoleName(userRole.RoleId);
        string jwtToken = GenerateToken(user, roleName.Name!);
        string refreshToken = GenerateRefreshToken();

        var updateRefreshToken = await context.RefreshTokenInfo.FirstOrDefaultAsync(rt => rt.UserId == user.Id);
        if (updateRefreshToken == null) return new LoginResponse(false, "Refresh token not generated because user has not signed in");

        updateRefreshToken.Token = refreshToken;
        await context.SaveChangesAsync();
        return new LoginResponse(true, "Token refresh successful", jwtToken, refreshToken);
    }

    private async Task<ApplicationUser> FindUserByEmail(string email)
        => await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email!.ToLower()!.Equals(email!.ToLower()));

    private async Task<T> AddToDb<T>(T model)
    {
        var result = context.Add(model!);
        await context.SaveChangesAsync();
        return (T)result.Entity;
    }

}
