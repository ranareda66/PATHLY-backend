
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PATHLY_API.Data;
using PATHLY_API.JWT;
using PATHLY_API.Models;
using PATHLY_API.Services;
using PATHLY_API.Services.AuthServices;
using Serilog;
using System.Text;

namespace PATHLY_API
{
    public class Program
	{
		public static void Main(string[] args)
		{
            
            var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.Configure<jwt>(builder.Configuration.GetSection("JWT"));

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<UserService>();
			builder.Services.AddScoped<TripService>();
			builder.Services.AddScoped<ReportService>();
			builder.Services.AddScoped<PayPalService>();
			builder.Services.AddScoped<SearchService>();
			builder.Services.AddScoped<PaymentService>();
			builder.Services.AddScoped<LocationService>();

			builder.Services.AddIdentity<User, IdentityRole<int>>().AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{
                options.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PATHLY;Integrated Security=True;");
                //options.UseSqlServer("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");
            });

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
				.AddJwtBearer(o =>
				{
					o.RequireHttpsMetadata = false;
					o.SaveToken = false;
					o.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidIssuer = builder.Configuration["JWT:Issuer"],
						ValidAudience = builder.Configuration["JWT:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                        ClockSkew = TimeSpan.Zero


                    };
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
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();

		}
	}
}
