using Application;
using Infrastructure;
using Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using moduleTest.Controllers;
using Persistence;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// JWT Authentication'ı yapılandır
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"]);
//builder.Services.AddAuthentication(options =>
//{
//	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//	options.TokenValidationParameters = new TokenValidationParameters
//	{
//		ValidateIssuer = true,
//		ValidateAudience = true,
//		ValidateLifetime = true,
//		ValidateIssuerSigningKey = true,
//		ValidIssuer = builder.Configuration["Jwt:Issuer"],
//		ValidAudience = builder.Configuration["Jwt:Audience"],
//		IssuerSigningKey = new SymmetricSecurityKey(key)
//	};
//});



builder.Services.AddControllers();



builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// Swagger'ı yapılandır	
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Windchill Integration API",
		Version = "v1",
		Description = "PLM Integration Services",
		Contact = new OpenApiContact
		{
			Name = "Developer Support Team",
			Email = "o.ozcelik@designtech.com.tr"
		}
	});

	//Hata verdiği için yorum satırına alındı
	// XML comments
	//var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	//var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	//c.IncludeXmlComments(xmlPath);
});
// Swagger'ı yapılandır	

// JWT Authentication'ı yapılandır
builder.Services.AddHostedService<IntegrationBackgroundService>();
builder.Services.AddSingleton<IPerformanceService, PerformanceService>();
var app = builder.Build();

// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI();
//}

	app.UseSwagger();
	app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
