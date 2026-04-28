using System.Globalization;
using System.Net.Http.Json;

namespace SistemaMPN.Client.Modules.Miembros.Services
{
    // === Modela el JSON ===
    public class CountriesRoot
    {
        public List<CountryItem>? countries { get; set; }
    }

    public class CountryItem
    {
        public string? name { get; set; }     // Inglés
        public string? es_name { get; set; }  // Español
    }

    public interface INacionalidadesService
    {
        Task<IReadOnlyList<string>> GetAsync(CancellationToken ct = default);
    }

    public class NacionalidadesService : INacionalidadesService
    {
        private readonly HttpClient _http;
        private IReadOnlyList<string>? _cache;
        private static readonly StringComparer EsComparer =
            StringComparer.Create(new CultureInfo("es-AR"), ignoreCase: true);

        public NacionalidadesService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<string>> GetAsync(CancellationToken ct = default)
        {
            if (_cache is not null) return _cache;

            try
            {
                var root = await _http.GetFromJsonAsync<CountriesRoot>(
                    "data/countries.json", ct);

                var list = (root?.countries ?? new List<CountryItem>())
                    .Select(c =>
                        !string.IsNullOrWhiteSpace(c.es_name)
                            ? c.es_name!.Trim()
                            : (c.name ?? string.Empty).Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(EsComparer)
                    .OrderBy(s => s, EsComparer)
                    .ToList();

                // Si quisieras evitar tildes al buscar más adelante:
                // list = list.Select(RemoveDiacritics).Distinct(EsComparer).ToList();

                _cache = list;
                return _cache;
            }
            catch
            {
                _cache = new List<string> { "Argentina", "Uruguay", "Paraguay", "Brasil", "Chile" };
                return _cache;
            }
        }

        // Útil si más adelante querés búsquedas sin tildes
        // private static string RemoveDiacritics(string text)
        // {
        //     var normalized = text.Normalize(NormalizationForm.FormD);
        //     var sb = new StringBuilder();
        //     foreach (var c in normalized)
        //     {
        //         var uc = CharUnicodeInfo.GetUnicodeCategory(c);
        //         if (uc != UnicodeCategory.NonSpacingMark) sb.Append(c);
        //     }
        //     return sb.ToString().Normalize(NormalizationForm.FormC);
        // }
    }
}
