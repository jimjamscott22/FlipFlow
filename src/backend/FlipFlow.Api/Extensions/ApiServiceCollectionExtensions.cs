using System.Text.Json;
using System.Text.Json.Serialization;
using FlipFlow.Api.Security;
using FlipFlow.Application;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace FlipFlow.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FlipFlow API",
                Version = "v1",
                Description = "REST API for the FlipFlow reseller workflow app."
            });
        });

        services.AddSingleton<AccessPasscodeService>();

        return services;
    }

    public static IApplicationBuilder UseStaticFilesForUploads(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var uploadsPath = Path.Combine(environment.ContentRootPath, "storage", "uploads");

        Directory.CreateDirectory(uploadsPath);

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/storage/uploads" // must match the relativePath returned by LocalFileStorageService
        });
    }
}
