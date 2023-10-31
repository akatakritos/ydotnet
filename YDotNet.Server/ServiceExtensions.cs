using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using YDotNet.Server;
using YDotNet.Server.Storage;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static YDotnetRegistration AddYDotNet(this IServiceCollection services)
    {
        services.AddOptions<DocumentManagerOptions>();
        services.TryAddSingleton<IDocumentManager, DefaultDocumentManager>();
        services.TryAddSingleton<IDocumentStorage, InMemoryDocumentStorage>();

        services.AddSingleton<IHostedService>(x =>
            x.GetRequiredService<IDocumentManager>());

        return new YDotnetRegistration
        {
            Services = services
        };
    }

    public static YDotnetRegistration AddCallback<T>(this YDotnetRegistration registration) where T : class, IDocumentCallback
    {
        registration.Services.AddSingleton<IDocumentCallback, T>();
        return registration;
    }
}

public sealed class YDotnetRegistration
{
    required public IServiceCollection Services { get; init; }
}
