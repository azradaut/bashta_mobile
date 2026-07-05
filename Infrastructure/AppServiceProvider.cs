namespace bashta_mobile.Infrastructure;

public static class AppServiceProvider
{
    public static IServiceProvider? Current { get; private set; }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        Current = serviceProvider;
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        if (Current is null)
            throw new InvalidOperationException("Service provider is not initialized.");

        var service = Current.GetService<T>();

        if (service is null)
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");

        return service;
    }
}