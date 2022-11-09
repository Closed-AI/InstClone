using API;
using API.Configs;
using API.Middlewares;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        // there starts old code style OnConfigureServices() method
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // get auth configuration params from appsetings.json
        var authSection = builder.Configuration.GetSection(AuthConfig.Position);
        var authConfig = authSection.Get<AuthConfig>();

        builder.Services.Configure<AuthConfig>(authSection);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "¬ведите токен пользовател€",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme,

            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme,

                        },
                        Scheme = "oauth2",
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        // add data context as scoped service, connect with PostgreSQL with Npgsql package;
        // connection string goes from appsetings.json
        builder.Services.AddDbContext<DAL.DataContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), sql => { });
        });

        // add automaper
        builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);
        // add our user service
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<PostService>();
        builder.Services.AddTransient<AttachService>();

        builder.Services.AddAuthentication(o =>
        {
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = authConfig.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = authConfig.GetSymmetricSecurityKey(),
                ClockSkew = TimeSpan.Zero,
            };
        });

        builder.Services.AddAuthorization(o =>
        {
            o.AddPolicy("ValidAccessToken", p =>
            {
                p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                p.RequireAuthenticatedUser();
            });
        });


        // Application building (there starts old code style OnConfigure() method)
        var app = builder.Build();

        // Configure the HTTP request pipeline.

        // add auto migrations
        using (var serviceScope = ((IApplicationBuilder)app)
            .ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
        {
            if (serviceScope != null)
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DAL.DataContext>();
                context.Database.Migrate();
            }
        }

        // in "PROD")) version make this section working oly in (debug) mode
        //                                                  [ development ]
        // now we have no GUI and need to test our API from Swagger
        app.UseSwagger();
        app.UseSwaggerUI();
        //------------------------------------------------------------------

        app.UseHttpsRedirection();

        // authorization and autentification middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseTokenValidator();
        app.MapControllers();

        app.Run();
    }
}