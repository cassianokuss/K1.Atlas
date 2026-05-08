using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSender(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
        });

        services.AddScoped<ISender, Mediator>();
        services.AddScoped<IPublisher, Mediator>();
        services.AddScoped<IMediator, Mediator>();

        return services;
    }

    public static IServiceCollection AddOpenApiDocument(this IServiceCollection services, string title, string version)
    {
        services.AddHealthChecks();

        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = version });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Por favor insira um token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, string defaultScheme, string authority, string audience)
    {
        services
            .AddAuthentication(defaultScheme)
            .AddJwtBearer(defaultScheme, options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        return services;
    }

    public static IApplicationBuilder UseOpenApiDocument(this IApplicationBuilder app, string basePath)
    {
        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((swagger, httpReq) =>
            {
                if (httpReq.Headers.ContainsKey("X-Forwarded-Host"))
                {
                    var serverUrl = $"{httpReq.Scheme}://{httpReq.Headers["X-Forwarded-Host"]}/{basePath}";

                    swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = serverUrl.Replace("http:", "https:") } };
                }
            });
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/{basePath}/v1/swagger.json", "v1");
            options.RoutePrefix = basePath;
        });

        return app;
    }
}
