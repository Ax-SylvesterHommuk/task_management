using System.Net;

namespace task_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 3000); // Listen on localhost (127.0.0.1) and port 3000
            });

            // Add session support with an in-memory cache
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "SessionId";
                options.IdleTimeout = TimeSpan.FromMinutes(60); 
            });

            // Add in-memory distributed cache
            builder.Services.AddDistributedMemoryCache();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseSession();

            app.MapControllers();

            app.Run();
        }
    }
}