using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SistemaMPN.Client.Providers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILocalStorageService _storageService;
        private readonly HttpClient _httpClient;
        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient, ILocalStorageService storageService)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _storageService = storageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string token = await _storageService.GetItemAsync<String>("Token");
                var identity = new ClaimsIdentity();

                if (!string.IsNullOrEmpty(token) && await ValidateToken(token))
                {
                    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var user = new ClaimsPrincipal(identity);

                var state = new AuthenticationState(user);
                NotifyAuthenticationStateChanged(Task.FromResult(state));
                return state;
            }
            catch (Exception ex)
            {   
                Console.Error.WriteLine($"Error al obtener el estado de autentificacion: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

        }

        //Chequeo que el token se pueda leer y que no haya expirado
        private async Task<bool> ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return false;

            var jwtToken = handler.ReadToken(token);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "Token");
                return false;
            }

            return true;
        }
    }
}
