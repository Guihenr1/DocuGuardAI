using System.Text;
using DocuGuardAI.Api;
using DocuGuardAI.Application;
using DocuGuardAI.Application.Settings;
using DocuGuardAI.Infrastructure;
using DocuGuardAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SystemAdminOnly", policy => policy.RequireRole("SystemAdmin"))
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SystemAdmin"))
    .AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Admin", "Editor"))
    .AddPolicy("AnyRole", policy =>
        policy.RequireRole("Admin", "Editor", "Viewer"));

var app = builder.Build();

if (app.Environment.IsDevelopment() || 
    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENABLE_SCALAR")))
{
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Lina API Documentation")
            .WithTheme(ScalarTheme.DeepSpace) 
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

if (app.Environment.IsDevelopment() || 
    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUTO_MIGRATE")))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DocuGuardAIDbContext>();

    await dbContext.Database.MigrateAsync();

// seed default data (company + system admin)
await DocuGuardAI.Infrastructure.Seed.DataSeeder.EnsureSeedAsync(app.Services);
}

// app.UseHttpsRedirection();
// app.Use((context, next) =>
// {
//     context.Request.Scheme = "https";   
//     return next();
// });
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));
app.MapControllers();

app.Run();