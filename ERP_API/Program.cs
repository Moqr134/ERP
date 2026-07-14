using AutoMapper;
using ERP_API.App.IService;
using ERP_API.App.Service;
using ERP_API.Infrastructure.Permissions;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.Mapping;
using Infrastructure.Middleware;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

DBConn.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new InvalidOperationException(
        "Jwt:SecretKey is not configured. Set it via environment variable Jwt__SecretKey, User Secrets, or appsettings.Development.json.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.Register<ISingleton>();

builder.Services.Register<IScopped>();

builder.Services.AddDbContext<DBContext>(option => option.UseSqlServer(DBConn.ConnectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    cfg.SaveToken = true;

                    byte[] symmetrickey = Convert.FromBase64String(jwtSettings.SecretKey);
                    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(symmetrickey);

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = securityKey,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["AuthToken"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://localhost:7107"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWepApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();

        });
});
var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DBContext>();
        await db.Database.MigrateAsync();
    }

    await PermissionSeeder.EnsurePermissionsAsync(app.Services, app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to apply migrations or seed system permissions on startup.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandler>();

app.UseHttpsRedirection();

app.UseCors("AllowWepApp");

app.UseMiddleware<CsrfHeaderMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
