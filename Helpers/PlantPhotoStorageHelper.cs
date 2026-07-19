using bashta_mobile.ViewModels;

namespace bashta_mobile.Helpers;

public static class PlantPhotoStorageHelper
{
    private const string Prefix = "plant_photo_";

    public static string GetPreferenceKey(int plantId)
    {
        return $"{Prefix}{plantId}";
    }

    public static string? GetCustomPhotoPath(int? plantId)
    {
        if (plantId is null)
            return null;

        var key = GetPreferenceKey(plantId.Value);
        var path = Preferences.Get(key, string.Empty);

        if (string.IsNullOrWhiteSpace(path))
            return null;

        return File.Exists(path) ? path : null;
    }

    public static async Task<string> SaveCustomPhotoAsync(int plantId, FileResult image)
    {
        var appDataPath = FileSystem.AppDataDirectory;
        var fileName = $"plant_{plantId}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
        var destinationPath = Path.Combine(appDataPath, fileName);

        await using var sourceStream = await image.OpenReadAsync();
        await using var destinationStream = File.Create(destinationPath);

        await sourceStream.CopyToAsync(destinationStream);

        Preferences.Set(GetPreferenceKey(plantId), destinationPath);

        return destinationPath;
    }

    public static void RemoveCustomPhoto(int plantId)
    {
        var key = GetPreferenceKey(plantId);
        var path = Preferences.Get(key, string.Empty);

        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            File.Delete(path);
        }

        Preferences.Remove(key);
    }

    public static ImageSource GetPlantImageSource(
        int? plantId,
        string? plantTypeName)
    {
        var customPath = GetCustomPhotoPath(plantId);

        if (!string.IsNullOrWhiteSpace(customPath))
            return ImageSource.FromFile(customPath);

        return PlantImageHelper.GetDefaultImage(plantTypeName);
    }
}