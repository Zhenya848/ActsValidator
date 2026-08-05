using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using UserService.API.Middleware;
using UserService.Application;
using UserService.Domain.Shared.Payment;
using UserService.Implementation;
using UserService.Infrastructure;
using UserService.Infrastructure.Seeding;
using UserService.Presentation;
using UserService.Presentation.Grpc.Services;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5172);
    
    options.Listen(IPAddress.Any, 8081, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyTestService", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            []
        }
    });
});

var json = await File.ReadAllTextAsync("etc/products.json");
        
var seedData = JsonConvert.DeserializeObject<ProductData[]>(json)
               ?? throw new ApplicationException("Product Config is missing");

builder.Services.AddSingleton<Products>(new Products(seedData));

builder.Services
    .AddFromInfrastructure(builder.Configuration)
    .AddFromImplementation()
    .AddFromPresentation(builder.Configuration)
    .AddFromApplication();

var app = builder.Build();

var accountsSeeder = app.Services.GetRequiredService<AccountsSeeder>();
await accountsSeeder.SeedAsync();

app.UseMiddleware<ExceptionMiddleware>();

app.MapGrpcService<GreeterService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");

app.UseCors(config =>
{
    config.WithOrigins("http://localhost:5173")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();