using ActsValidator.API.Middleware;
using ActsValidator.Application;
using ActsValidator.Infrastructure;
using ActsValidator.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddFromInfrastructure(builder.Configuration)
    .AddFromPresentation(builder.Configuration)
    .AddFromApplication();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();