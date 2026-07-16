using UBP.Auth;
using UBP.Booking.API.Extensions;
using UBP.Booking.API.Services;
using UBP.Booking.Application;
using UBP.Endpoints;
using UBP.Logging;
using UBP.OpenApi;
using UBP.OpenApi.Scalar;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.ConfigureAppOptions();
builder.Services.AddApplication();

builder.Services.AddIamAuthentication(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"]!;
    options.Audience = "booking_api";
});
builder.Services.AddAuthorization();

builder.Services.AddOpenApiDocumentation("v1", options =>
{
    options.Title = "Booking API";
    options.Version = "v1";
    options.Description = "Abstract booking engine API";
});

builder.Services.AddHostedService<BookingCompletionBackgroundService>();
builder.Services.AddEndpoints();

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApiDocumentation();
app.MapEndpoints("/api");

await app.RunAsync();
