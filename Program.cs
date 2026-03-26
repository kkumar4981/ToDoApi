
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ToDoApi.Configuration;
using ToDoApi.Models;
using ToDoApi.Repositories;
using ToDoApi.Services;
using ToDoApi.Stores;

namespace ToDoApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection(JwtOptions.SectionName));
        builder.Services.Configure<AdminSeedOptions>(
            builder.Configuration.GetSection(AdminSeedOptions.SectionName));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddDataProtection();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ToDoApi",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT token here."
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
        });

        builder.Services.AddAuthorization();
        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddUserStore<SqlUserStore>()
            .AddRoleStore<SqlRoleStore>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddScoped<IUserRepository, SqlServerUserRepository>();
        builder.Services.AddScoped<IRoleRepository, SqlServerRoleRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, SqlServerRefreshTokenRepository>();
        //builder.Services.AddScoped<IDatabaseInitializer, SqlServerDatabaseInitializer>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IEmailService, DevelopmentEmailService>();
        builder.Services.AddScoped<IIdentitySeedService, IdentitySeedService>();

        var app = builder.Build();

        //using (var scope = app.Services.CreateScope())
        //{
        //    var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        //    await databaseInitializer.InitializeAsync();

        //    var identitySeedService = scope.ServiceProvider.GetRequiredService<IIdentitySeedService>();
        //    await identitySeedService.SeedAsync();
        //}

        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
