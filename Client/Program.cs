using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using ClientLibrary.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using ClientLibrary.Services.Contracts;
using ClientLibrary.Services.Implementations;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CustomHttpHandler>();
builder.Services.AddHttpClient("SystemApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7286");
}).AddHttpMessageHandler<CustomHttpHandler>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7286") });
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<GetHttpClient>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();

builder.Services.AddScoped<SfDialogService>();
builder.Services.AddSyncfusionBlazor();

await builder.Build().RunAsync();
