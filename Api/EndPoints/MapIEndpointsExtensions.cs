using Api.Abstractions;

namespace Api.EndPoints;

public static class MapIEndpointsExtensions
{
    public static void MapIEndpoints(this WebApplication app)
    {
        var endpoints = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t)
                && !t.IsInterface
                && !t.IsAbstract)
            .Select(type => Activator.CreateInstance(type) as IEndpoint)
            .ToArray();

        foreach (var endpoint in endpoints)
        {
            endpoint!.MapEndpoint(app);
        }
    }
}
