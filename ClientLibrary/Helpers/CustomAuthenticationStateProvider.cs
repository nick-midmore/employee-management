using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
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

    private static CustomUserClaims DecryptToken(string jwtToken)
    {
        throw new NotImplementedException();
    }
}
