using ApiRessource2.Helpers;
using ApiRessource2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Configuration;
using ApiRessource2.Helpers;
using System.Text.Json.Serialization;

namespace ApiRessource2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.AddScoped<IUserService, UserService>();
            
            ConfigurationHelper.Initialize(builder.Configuration);
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<DataContext>(
               options => {
                   options.UseInMemoryDatabase("RessourceDb");
               });
            } else
            {
                builder.Services.AddDbContext<DataContext>(
               options => {
                   string mySqlConnectionStr = builder.Configuration.GetConnectionString("connectionString");
                   options.UseMySql(mySqlConnectionStr, MariaDbServerVersion.AutoDetect(mySqlConnectionStr));
               });
            }
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            });

            var app = builder.Build();
            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    (scope.ServiceProvider.GetRequiredService(typeof(DataContext)) as DataContext).Database.EnsureDeleted();
                    (scope.ServiceProvider.GetRequiredService(typeof(DataContext)) as DataContext).Database.EnsureCreated();
                }
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseMiddleware<JwtMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}