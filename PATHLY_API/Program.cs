
using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Services;

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
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");
			});

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
