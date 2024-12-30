using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

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

    public Task<LoginResponse> SignInAsync(Login user)
    {
        throw new NotImplementedException();
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
