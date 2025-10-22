
using System.Text.Json.Serialization;

namespace FutMatchApp.Models.GooglePlaces
{
    public class GooglePlacesResponse
    {
        [JsonPropertyName("results")]
        public List<GooglePlace> Results { get; set; } = new List<GooglePlace>();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("next_page_token")]
        public string? NextPageToken { get; set; }
    }

    public class GooglePlace
    {
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("vicinity")]
        public string Vicinity { get; set; } = string.Empty;

        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }

        [JsonPropertyName("geometry")]
        public GoogleGeometry Geometry { get; set; } = new GoogleGeometry();

        [JsonPropertyName("rating")]
        public decimal? Rating { get; set; }

        [JsonPropertyName("user_ratings_total")]
        public int? UserRatingsTotal { get; set; }

        [JsonPropertyName("photos")]
        public List<GooglePhoto>? Photos { get; set; }

        [JsonPropertyName("opening_hours")]
        public GoogleOpeningHours? OpeningHours { get; set; }

        [JsonPropertyName("price_level")]
        public int? PriceLevel { get; set; }
    }

    public class GoogleGeometry
    {
        [JsonPropertyName("location")]
        public GoogleLocation Location { get; set; } = new GoogleLocation();
    }

    public class GoogleLocation
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public class GooglePhoto
    {
        [JsonPropertyName("photo_reference")]
        public string PhotoReference { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class GoogleOpeningHours
    {
        [JsonPropertyName("open_now")]
        public bool? OpenNow { get; set; }
    }

    public class GoogleFindPlaceResponse
    {
        [JsonPropertyName("candidates")]
        public List<GooglePlace> Candidates { get; set; } = new List<GooglePlace>();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}