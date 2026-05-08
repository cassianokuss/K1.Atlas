using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configura o pipeline de middleware da aplicação
    /// </summary>
    public static WebApplication ConfigureMiddleware(this WebApplication app, string[]? corsPolicyNames = null, string? swaggerBasePath = null, bool? useAuthentication = true, bool? useAuthorization = true)
    {
        var environment = app.Environment;

        // Exception handling
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }app.MapHealthChecks("/health");


        // CORS
        if (corsPolicyNames != null && corsPolicyNames.Length > 0)
        {
            foreach (var policyName in corsPolicyNames)
            {
                app.UseCors(policyName);
            }
        }

        // Routing
        app.UseRouting();

        // Localização
        app.ConfigureLocalization();

        // Autenticação e Autorização
        if (useAuthentication == true)
            app.UseAuthentication();

        if (useAuthorization == true)
            app.UseAuthorization();

        // Controllers
        app.MapControllers();

        // Swagger>
        if (!string.IsNullOrEmpty(swaggerBasePath))
            app.UseOpenApiDocument(swaggerBasePath);

        app.MapHealthChecks("/health");

        return app;
    }

    /// <summary>
    /// Configura a localização da aplicação
    /// </summary>
    private static WebApplication ConfigureLocalization(this WebApplication app)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("pt-BR")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        return app;
    }
}
