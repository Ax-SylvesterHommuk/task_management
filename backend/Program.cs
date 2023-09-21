using System.Net;
using System.Reflection;
using Microsoft.OpenApi.Models;
using task_backend.Data;
using task_backend.Helpers;

namespace task_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var allowedOrigin = configuration.GetValue<string>("CorsSettings:AllowedOrigin");

            builder.Services.AddControllers();
            builder.Services.AddScoped<DatabaseContext>();
            builder.Services.AddScoped<SecurityHelpers>(); 

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task Manager API",
                    Version = "v1",
                    Description = "Rest API for the Task Manager application."
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", builder =>
                {
                    builder.WithOrigins(allowedOrigin)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, 80); // Listen any IP (127.0.0.1) and port 80
            });

            // Add session support with an in-memory cache
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "SessionId";
                options.IdleTimeout = TimeSpan.FromMinutes(60); 
            });

            builder.Services.AddDistributedMemoryCache();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Loads the wwwroot/index.html
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404 && !context.Request.Path.Value.StartsWith("/api"))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseCors("AllowLocalhost");

            app.UseFileServer();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseSession();

            app.MapControllers();

            app.Run();
        }
    }
}