using API;
using API.Services;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        // there starts old code style OnConfigureServices() method
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

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
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}