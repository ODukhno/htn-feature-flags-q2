using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using QEntitiesServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QResourceServer;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureServices((hostContext, services) =>
{
    services
        .Configure<ResourceAccessSettings>(hostContext.Configuration.GetSection(nameof(ResourceAccessSettings)))
        .AddOptions<ResourceAccessSettings>()
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var authConfigurationSection = builder.Configuration.GetSection("AzureAd");

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(authConfigurationSection);

    MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions();
    IMemoryCache memoryCache = new MemoryCache(Options.Create(memoryCacheOptions));

    services.AddSingleton(memoryCache);

    services
        .AddHealthChecks()
        .AddCheck<HealthCheck>("generic");

    services.AddControllers();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health/startup");
    endpoints.MapHealthChecks("/health/liveness");
    endpoints.MapHealthChecks("/health/readiness");
    endpoints.MapControllers();
});

app.Run();