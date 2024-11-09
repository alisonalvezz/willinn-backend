using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

public class ConfigureSwaggerOptions(IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CRUD Usuarios - Prueba Tecnica Willinn",
            Version = "v0.0.01",
            Description = "Prueba tecnica Willinn",
            Contact = new OpenApiContact
            {
                Name = "Alison Alvez"
            },
        });
        
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        
    }
}