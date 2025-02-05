
using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Services;
using Serilog;

namespace PATHLY_API
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
			builder.Services.AddScoped<UserService>();
			builder.Services.AddScoped<UserLocationService>();
			builder.Services.AddScoped<UserPreferencesService>();
			builder.Services.AddScoped<PaymentService>();
			builder.Services.AddScoped<PayPalService>();
<<<<<<< HEAD
            builder.Services.AddScoped<RoadService>();
            builder.Services.AddScoped<RoadRecommendationService>();
			builder.Services.AddScoped<SearchHistoryService>();
            builder.Services.AddScoped<UserFeedbackService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
=======
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
>>>>>>> 5ff76ae5d40b85190b2deaf6173222ab9211dc6b
			{
				options.UseSqlServer("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");
			});

			Log.Logger = new LoggerConfiguration()
	           .WriteTo.File("logs/payments.log", rollingInterval: RollingInterval.Day)
	           .CreateLogger();

			builder.Host.UseSerilog();


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
