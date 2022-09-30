using Api.Middlewares;
using Infrastructure.Seeds;

namespace Microsoft.Extensions.DependencyInjection;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ApiExceptionHandlingMiddleware>();

    public static async Task UseDbInitializer(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseContextInitializer>();

        await initializer.InitializeAsync();
        await initializer.SeedAsync();
    }
}
