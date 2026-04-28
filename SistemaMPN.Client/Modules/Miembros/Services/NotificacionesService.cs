using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Interfaces;
using SistemaMPN.Shared.DTO;
using System;
using System.Net.Http.Json;
using System.Security.Claims;

namespace SistemaMPN.Client.Modules.Miembros.Services
{
    public class NotificacionesService : IAsyncDisposable
    {
        private readonly NavigationManager _navigation;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly HttpClient _http;

        private HubConnection? _hubConnection;
        private bool _initialized;

        private readonly List<NotificacionDto> _items = new();
        private string? _currentUserId;

        public event Action? OnChange;

        public IReadOnlyList<NotificacionDto> Items => _items;
        public int NotificacionesSinLeer => _items.Count(n => !n.Leida.Value);

        public NotificacionesService(
            NavigationManager navigation,
            ILocalStorageService localStorage,
            HttpClient http,
            AuthenticationStateProvider authStateProvider)
        {
            _navigation = navigation;
            _localStorage = localStorage;
            _http = http;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            _initialized = true;
            _authStateProvider.AuthenticationStateChanged += OnAuthStateChanged;

            await CargarNotificaciones();
            await InicializarSignalRAsync();
        }

        private async Task CargarNotificaciones()
        {
            var lista = await _http.GetFromJsonAsync<List<NotificacionDto>>("api/Notificacion/GetNotificaciones");

            if (lista == null) return;

            _items.Clear();
            _items.AddRange(lista);

            NotificarCambios();
        }

        public async Task MarcarTodasLeidasAsync()
        {
            var ids = _items.Where(n => !n.Leida.Value).Select(n => n.Id).ToList();

            if (ids.Count == 0) return;

            await _http.PatchAsJsonAsync("api/Notificacion/LeerNotificacion", ids);

            foreach (var n in _items)
                n.Leida = true;

            NotificarCambios();
        }
        private async Task InicializarSignalRAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("Token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return;
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigation.ToAbsoluteUri("/notificacioneshub"), options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                        options.SkipNegotiation = true;
                        options.Transports =
                            Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30) })
                    .Build();

                _hubConnection.On<NotificacionDto>(
                    "RecibirNotificacion",
                    AlRecibirNotificacion);

                _hubConnection.Closed += async (error) =>
                {
                    await Task.Delay(5000);
                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        try { await _hubConnection.StartAsync(); }
                        catch { }
                    }
                };

                _hubConnection.Reconnecting += (error) =>
                {
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += (connectionId) =>
                {
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
            }
            catch (Exception)
            {
            }
        }

        private async void OnAuthStateChanged(Task<AuthenticationState> task)
        {
            var state = await task;
            var newUserId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (newUserId != _currentUserId)
            {
                _currentUserId = newUserId;
                _items.Clear();
                if (!string.IsNullOrEmpty(newUserId))
                    await CargarNotificaciones();
            }
        }
        private void AlRecibirNotificacion(NotificacionDto notification)
        {
            if (_items.Any(n => n.Id == notification.Id)) return;

            _items.Insert(0, notification);
            NotificarCambios();
        }

        private void NotificarCambios()
        {
            OnChange?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            _authStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
        }
    }
}
