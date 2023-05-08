using MassTransit;
using Sample.Components.Consumers;
using Sample.Contracts;

namespace Sample_Twitch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample API Site");
            builder.Services.AddMassTransit(mt =>
            {
                mt.UsingRabbitMq((context, cfg) => 
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });

               // mt.AddConsumer<SubmitOrderConsumer>();
            });

            //builder.Services.AddScoped<SubmitOrderConsumer>();
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi3();
                //app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.MapControllers();
            
            app.Run();

            
        }
    }
}