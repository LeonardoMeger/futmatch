using FutMatchApp.Models.GooglePlaces;
using FutMatchApp.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace FutMatchApp.Services
{
    public class GooglePlacesService : IGooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://maps.googleapis.com/maps/api/place";

        public GooglePlacesService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GooglePlaces:ApiKey"] ?? throw new ArgumentException("Google Places API Key not configured");

            Console.WriteLine($"GooglePlacesService inicializado com API Key: {!string.IsNullOrEmpty(_apiKey)}");
        }

        public async Task<List<Court>> SearchFootballCourtsAsync(string location, int radius = 15000)
        {
            try
            {
                Console.WriteLine($"Buscando quadras para: {location}");

                var geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?" +
                               $"address={Uri.EscapeDataString(location)}&" +
                               $"key={_apiKey}";

                Console.WriteLine($"Geocode URL: {geocodeUrl}");

                var geocodeResponse = await _httpClient.GetStringAsync(geocodeUrl);
                var geocodeData = JsonSerializer.Deserialize<GoogleGeocodeResponse>(geocodeResponse);

                if (geocodeData?.Results?.Any() == true)
                {
                    var firstResult = geocodeData.Results.First();
                    var lat = firstResult.Geometry.Location.Lat;
                    var lng = firstResult.Geometry.Location.Lng;

                    Console.WriteLine($"Coordenadas encontradas: {lat}, {lng}");

                    return await GetNearbyFootballCourtsAsync(lat, lng, radius);
                }
                else
                {
                    Console.WriteLine("Nenhuma coordenada encontrada para a localização");
                    return new List<Court>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar quadras: {ex.Message}");
                return new List<Court>();
            }
        }

        public async Task<List<Court>> GetNearbyFootballCourtsAsync(double lat, double lng, int radius = 15000)
        {
            var courts = new List<Court>();

            try
            {
                var searches = new[]
                {
                    new { keyword = "quadra de futebol society", priority = 1 },
                    new { keyword = "campo de futebol", priority = 2 },
                    new { keyword = "quadra esportiva", priority = 3 },
                    new { keyword = "centro esportivo", priority = 4 },
                    new { keyword = "arena esportiva", priority = 5 },
                    new { keyword = "futebol society", priority = 6 }
                };

                foreach (var search in searches)
                {
                    try
                    {
                        Console.WriteLine($"Buscando: {search.keyword}");

                        var textSearchUrl = $"{BaseUrl}/textsearch/json?" +
                                          $"query={Uri.EscapeDataString(search.keyword + " " + lat + "," + lng)}&" +
                                          $"location={lat},{lng}&" +
                                          $"radius={radius}&" +
                                          $"key={_apiKey}";

                        var response = await _httpClient.GetStringAsync(textSearchUrl);
                        var data = JsonSerializer.Deserialize<GooglePlacesResponse>(response);

                        if (data?.Status == "OK" && data.Results != null)
                        {
                            Console.WriteLine($"Encontradas {data.Results.Count} quadras para '{search.keyword}'");

                            foreach (var place in data.Results)
                            {
                                if (!courts.Any(c => c.GooglePlaceId == place.PlaceId))
                                {
                                    courts.Add(ConvertGooglePlaceToCourt(place));
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"API Status para '{search.keyword}': {data?.Status}");
                        }

                        if (courts.Count >= 6) break;

                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro na busca '{search.keyword}': {ex.Message}");
                    }
                }

                if (courts.Count < 6)
                {
                    await TryNearbySearch(lat, lng, radius, courts);
                }

                Console.WriteLine($"Total de quadras únicas encontradas: {courts.Count}");
                return courts.Take(6).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                return courts;
            }
        }

        private async Task TryNearbySearch(double lat, double lng, int radius, List<Court> courts)
        {
            try
            {
                var nearbyUrl = $"{BaseUrl}/nearbysearch/json?" +
                               $"location={lat},{lng}&" +
                               $"radius={radius}&" +
                               $"keyword=futebol&" +
                               $"type=establishment&" +
                               $"key={_apiKey}";

                var response = await _httpClient.GetStringAsync(nearbyUrl);
                var data = JsonSerializer.Deserialize<GooglePlacesResponse>(response);

                if (data?.Status == "OK" && data.Results != null)
                {
                    foreach (var place in data.Results)
                    {
                        if (!courts.Any(c => c.GooglePlaceId == place.PlaceId))
                        {
                            courts.Add(ConvertGooglePlaceToCourt(place));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no nearby search: {ex.Message}");
            }
        }

        private Court ConvertGooglePlaceToCourt(GooglePlace place)
        {
            decimal estimatedPrice = place.PriceLevel switch
            {
                1 => 45.00m,
                2 => 65.00m,
                3 => 85.00m,
                4 => 110.00m,
                5 => 150.00m,
                _ => DetermineEstimatedPrice(place)
            };

            return new Court
            {
                Id = 0, 
                Nome = place.Name,
                Descricao = BuildDescription(place),
                Localizacao = place.FormattedAddress ?? place.Vicinity,
                PrecoPorHora = estimatedPrice,
                Ativa = true,
                GooglePlaceId = place.PlaceId,
                Latitude = place.Geometry.Location.Lat,
                Longitude = place.Geometry.Location.Lng,
                GoogleRating = place.Rating,
                IsFromGoogle = true
            };
        }

        private decimal DetermineEstimatedPrice(GooglePlace place)
        {
            if (place.Rating.HasValue)
            {
                return place.Rating.Value switch
                {
                    >= 4.5m => 95.00m,
                    >= 4.0m => 80.00m,
                    >= 3.5m => 65.00m,
                    _ => 50.00m
                };
            }

            return 75.00m;
        }

        private string BuildDescription(GooglePlace place)
        {
            var description = new List<string>();

            if (place.Rating.HasValue)
            {
                description.Add($"⭐ {place.Rating:F1}");
            }

            if (place.UserRatingsTotal.HasValue && place.UserRatingsTotal > 0)
            {
                description.Add($"({place.UserRatingsTotal} avaliações)");
            }

            if (place.OpeningHours?.OpenNow == true)
            {
                description.Add("🟢 Aberto agora");
            }
            else if (place.OpeningHours?.OpenNow == false)
            {
                description.Add("🔴 Fechado");
            }

            return description.Any() ? string.Join(" • ", description) : "Quadra esportiva encontrada no Google";
        }
    }

    public class GoogleGeocodeResponse
    {
        [JsonPropertyName("results")]
        public List<GoogleGeocodeResult> Results { get; set; } = new List<GoogleGeocodeResult>();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoogleGeocodeResult
    {
        [JsonPropertyName("geometry")]
        public GoogleGeometry Geometry { get; set; } = new GoogleGeometry();
    }
}
