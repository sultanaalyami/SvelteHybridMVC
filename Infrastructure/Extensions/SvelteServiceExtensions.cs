using HRCE.Core;

namespace HRCE.Infrastructure.Extensions;

public static class SvelteServiceExtensions
{
    public static IServiceCollection AddSvelteHybrid(
        this IServiceCollection services,
        Action<SvelteOptions>? configure = null)
    {
        var options = new SvelteOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ISvelteRenderer, SvelteRenderer>();

        return services;
    }
}
