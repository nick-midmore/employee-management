﻿using BaseLibrary.DTOs;
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
    public async Task<GeneralResponse> CreateAsync(Register user)
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

    public async Task<LoginResponse> SignInAsync(Login user)
    {
        if (user is null) return new LoginResponse(false, "Model is empty");

        var applicationUser = await FindUserByEmail(user.Email);
        if (applicationUser is null) return new LoginResponse(false, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password)) return new LoginResponse(false, "Invalid email/password combination");

        var getUserRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == applicationUser.Id);
        if (getUserRole is null) return new LoginResponse(false, "User role not found");

        var getRoleName = await context.SystemRoles.FirstOrDefaultAsync(sr => sr.Id == getUserRole.RoleId);
        if (getUserRole is null) return new LoginResponse(false, "User role not found");

        string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
        string refreshToken = GenerateRefreshToken();
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


    private async Task<ApplicationUser> FindUserByEmail(string email)
        => await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email!.ToLower()!.Equals(email!.ToLower()));

    private async Task<T> AddToDb<T>(T model)
    {
        var result = context.Add(model!);
        await context.SaveChangesAsync();
        return (T)result.Entity;
    }
}
