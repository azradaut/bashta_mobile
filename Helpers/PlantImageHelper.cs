namespace bashta_mobile.Helpers;

public static class PlantImageHelper
{
    public static string GetDefaultImage(string? plantTypeName)
    {
        var value = plantTypeName?.Trim().ToLowerInvariant() ?? string.Empty;

        if (value.Contains("paradajz") || value.Contains("tomato"))
            return "plant_tomato.png";

        if (value.Contains("bosiljak") || value.Contains("basil"))
            return "plant_basil.png";

        if (value.Contains("jagoda") || value.Contains("strawberry"))
            return "plant_strawberry.png";

        if (value.Contains("paprika") || value.Contains("pepper"))
            return "plant_pepper.png";

        return "plant_default.png";
    }
}