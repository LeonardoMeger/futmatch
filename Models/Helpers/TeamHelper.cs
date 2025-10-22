namespace FutMatchApp.Models.Helpers
{
    public static class TeamHelper
    {
        private static readonly List<string> DefaultColors = new List<string>
        {
            "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8",
            "#6f42c1", "#e83e8c", "#fd7e14", "#20c997", "#6c757d",
            "#343a40", "#198754", "#0dcaf0", "#495057", "#f8f9fa"
        };

        public static string GetTeamColor(Team team, int? index = null)
        {
            if (!string.IsNullOrEmpty(team.CorPrimaria) && IsValidHexColor(team.CorPrimaria))
            {
                return team.CorPrimaria;
            }

            var colorIndex = index ?? (team.Id % DefaultColors.Count);
            return DefaultColors[Math.Abs(colorIndex) % DefaultColors.Count];
        }

        public static string GetTeamSecondaryColor(Team team, int? index = null)
        {
            if (!string.IsNullOrEmpty(team.CorSecundaria) && IsValidHexColor(team.CorSecundaria))
            {
                return team.CorSecundaria;
            }

            var primaryColor = GetTeamColor(team, index);
            return DarkenColor(primaryColor, 0.3);
        }

        public static string GetTeamDisplayName(Team team, int maxLength = 10)
        {
            if (string.IsNullOrEmpty(team.Nome))
                return "Time";

            if (team.Nome.Length <= maxLength)
                return team.Nome;

            var words = team.Nome.Split(' ');
            if (words.Length > 1 && words[0].Length <= maxLength)
                return words[0];

            return team.Nome.Substring(0, maxLength) + "...";
        }

        public static bool HasTeamLogo(Team team)
        {
            if (string.IsNullOrEmpty(team.FotoUrl))
                return false;

            return team.FotoUrl.StartsWith("http://") ||
                   team.FotoUrl.StartsWith("https://") ||
                   team.FotoUrl.StartsWith("/") ||
                   team.FotoUrl.StartsWith("data:image/");
        }

        public static string GetContrastColor(string backgroundColor)
        {
            if (string.IsNullOrEmpty(backgroundColor) || !IsValidHexColor(backgroundColor))
                return "#FFFFFF";

            try
            {
                var hex = backgroundColor.Replace("#", "");

                var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                var b = Convert.ToInt32(hex.Substring(4, 2), 16);

                var brightness = (r * 0.299 + g * 0.587 + b * 0.114);

                return brightness > 128 ? "#000000" : "#FFFFFF";
            }
            catch
            {
                return "#FFFFFF";
            }
        }

        private static string DarkenColor(string hexColor, double factor)
        {
            if (!IsValidHexColor(hexColor))
                return "#333333";

            try
            {
                var hex = hexColor.Replace("#", "");
                var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                var b = Convert.ToInt32(hex.Substring(4, 2), 16);

                r = Math.Max(0, (int)(r * (1 - factor)));
                g = Math.Max(0, (int)(g * (1 - factor)));
                b = Math.Max(0, (int)(b * (1 - factor)));

                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                return "#333333";
            }
        }

        private static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrEmpty(color))
                return false;

            if (!color.StartsWith("#"))
                return false;

            if (color.Length != 7)
                return false;

            return color.Substring(1).All(c =>
                (c >= '0' && c <= '9') ||
                (c >= 'A' && c <= 'F') ||
                (c >= 'a' && c <= 'f'));
        }

        public static string GenerateRandomTeamColor(int seed)
        {
            var random = new Random(seed);
            var colorIndex = random.Next(DefaultColors.Count);
            return DefaultColors[colorIndex];
        }

        public static bool IsValidImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            var lowerUrl = url.ToLower();

            return validExtensions.Any(ext => lowerUrl.Contains(ext)) ||
                   lowerUrl.StartsWith("data:image/");
        }
    }
}
