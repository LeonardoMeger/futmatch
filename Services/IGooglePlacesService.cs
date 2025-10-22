using FutMatchApp.Models;
using FutMatchApp.Models.GooglePlaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FutMatchApp.Services
{
    public interface IGooglePlacesService
    {
        Task<List<Court>> SearchFootballCourtsAsync(string location, int radius = 15000);
        Task<List<Court>> GetNearbyFootballCourtsAsync(double lat, double lng, int radius = 15000);

    }
}