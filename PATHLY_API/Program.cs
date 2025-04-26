using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PATHLY_API.Data;
using PATHLY_API.JWT;
using PATHLY_API.Models;
using PATHLY_API.Services;
using PATHLY_API.Services.AuthServices;
using PATHLY_API.Services.EmailServices;
using System.Text;
using System.Text.Json.Serialization;

namespace PATHLY_API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.Configure<Jwt>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<TripService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<SearchService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PayPalService>();
            builder.Services.AddScoped<SubscriptionPlanService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddHostedService<SubscriptionExpirationService>();
            builder.Services.AddScoped<IRoadService, RoadService>();
            builder.Services.AddHttpClient<GooglePlacesService>();
            builder.Services.AddHttpClient<GoogleTrafficService>();
            builder.Services.AddScoped<GoogleTrafficService>();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<GoogleTripService>();  // Changed from GoogleTrafficService
            builder.Services.AddScoped<TripService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<IRoadPredictionService, RoadPredictionService>();

			builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
              .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            var connectionString = builder.Configuration.GetConnectionString("cs");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
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

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");
            app.MapControllers();
            app.Run();

        }
    }
}
