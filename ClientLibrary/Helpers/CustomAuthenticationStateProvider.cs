﻿using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientLibrary.Helpers;
public class CustomAuthenticationStateProvider(LocalStorageService localStorageService) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var stringToken = await localStorageService.GetToken();
        if (string.IsNullOrEmpty(stringToken)) return await Task.FromResult(new AuthenticationState(anonymous));

        var deserialiseToken = Serialisations.DeserializeJsonString<UserSession>(stringToken);
        if (deserialiseToken == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var getUserClaims = DecryptToken(deserialiseToken.Token!);
        if (getUserClaims == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var claimsPrincipal = SetClaimPrincipal(getUserClaims);
        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    private static ClaimsPrincipal SetClaimPrincipal(CustomUserClaims claims)
    {
        if (claims.Email == null) return new ClaimsPrincipal();
        return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, claims.Id!),
                new(ClaimTypes.Name, claims.Name),
                new(ClaimTypes.Email, claims.Email),
                new(ClaimTypes.Role, claims.Role)
            }, "JwtAuth"));
    }

    private static CustomUserClaims DecryptToken(string jwtToken)
    {
        if(string.IsNullOrEmpty(jwtToken)) return new CustomUserClaims();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        var userId = token.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier);
        var name = token.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Name);
        var email = token.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Email);
        var role = token.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Role);
        return new CustomUserClaims(userId!.Value, name!.Value, email!.Value, role!.Value);
    }
}
