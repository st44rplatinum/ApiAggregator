using System.Text;
using ApiAggregator.Endpoints;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"] ?? "ApiAggregator";
var audience = jwt["Audience"] ?? "ApiAggregator";
var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is required in config.");

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<StatsService>();
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddSingleton<NewsService>();
builder.Services.AddSingleton<CryptoService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAggregateEndpoints();

app.Run();

public partial class Program { }
