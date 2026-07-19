using bashta_mobile.Constants;

namespace bashta_mobile.Helpers;

public static class PlantImageSourceHelper
{
    public static string GetImageSource(string? imagePath, string? plantTypeName)
    {
        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return imagePath;

            if (imagePath.StartsWith('/'))
                return $"{ApiConstants.BaseHost}{imagePath}";

            return $"{ApiConstants.BaseHost}/{imagePath}";
        }

        return PlantImageHelper.GetDefaultImage(plantTypeName);
    }
}