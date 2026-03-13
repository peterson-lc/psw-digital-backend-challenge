using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PswChallenge.Api.Endpoints.Auth;
using PswChallenge.Api.Endpoints.Holidays;
using PswChallenge.Api.HealthChecks;
using PswChallenge.Application.Queries.GetHolidays;
using PswChallenge.Api.ExceptionHandler;
using ApiExceptionMiddleware = PswChallenge.Api.Middlewares.ExceptionHandlerMiddleware;
using PswChallenge.Application.Configuration;
using PswChallenge.Application.Services;
using PswChallenge.Application.Services.Interfaces;
using PswChallenge.Infra.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<ApiExceptionMiddleware>();
builder.Services.ConfigureInfrastructure(builder.Configuration);

// Configure Swagger/OpenAPI with JWT Bearer support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PswChallenge API",
        Version = "v1",
        Description = "API for the PswChallenge application with JWT authentication."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter your JWT token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Bind configuration sections
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AdminCredentialsOptions>(
    builder.Configuration.GetSection(AdminCredentialsOptions.SectionName));

// Register application services
builder.Services.AddOutputCache();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetHolidaysQuery).Assembly));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<BrasilApiHealthCheck>(
        name: "brasilapi",
        failureStatus: HealthStatus.Degraded,
        tags: ["external", "api"])
    .AddCheck(
        name: "self",
        () => HealthCheckResult.Healthy("Application is running"),
        tags: ["self"]);

// Configure JWT Bearer authentication
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PswChallenge API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Apply CORS policy (must be before authentication/authorization)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// Map Health Check endpoints (no authentication required)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data,
                tags = e.Value.Tags
            })
        }, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();

// Simplified health check endpoint
app.MapHealthChecks("/healthz").AllowAnonymous();

app.MapAuthEndpoints();
app.MapHolidaysEndpoints();

await app.RunAsync();