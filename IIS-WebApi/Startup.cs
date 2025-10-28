using Microsoft.OpenApi.Models;

namespace IIS_WebApi;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // Register services
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IIS Host API",
                Version = "v1",
                Description = "A comprehensive API for IIS hosting management and operations"
            });
        });
    }

    // Configure HTTP pipeline
    public void Configure(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IIS Host API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}
