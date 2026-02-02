using WEB.Handlers.HttpHandlers;
using WEB.Hubs;
using WEB.Interfaces;
using WEB.Middleware;
using WEB.Repositories;
using WEB.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Authentication: Hem Cookie hem JWT kullanılıyor, default Cookie olacak.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Auth/Login";
		options.AccessDeniedPath = "/Auth/Login";
		options.ExpireTimeSpan = TimeSpan.FromHours(1);
		options.SlidingExpiration = true;
	})
	.AddJwtBearer("Api", options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"]))
		};

		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				// Hata loglama veya farklı işlem yapılabilir
				return Task.CompletedTask;
			}
		};
	});

// Controller, View, HttpClient, middleware ve diğer servis kayıtları.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddHttpClient<ApiRepository>();
builder.Services.AddScoped<IGetTokenService, GetTokenRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IApiService, ApiRepository>();

builder.Services.AddHttpClient("ApiClient")
	  .AddHttpMessageHandler<DynamicApiAddressHandler>()
	  .AddHttpMessageHandler<AuthHeaderHandler>()      // Token iletici handler
	  .AddHttpMessageHandler<RefreshTokenHandler>();     // Refresh token handler

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAllOrigins", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// SignalR servisini ekliyoruz
builder.Services.AddSignalR();

// Session ve diğer middleware konfigürasyonları
builder.Services.AddSession(opt =>
{
	opt.IdleTimeout = TimeSpan.FromMinutes(30);
	opt.Cookie.HttpOnly = true;
	opt.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseStaticFiles();


app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseMiddleware<SetupCheckMiddleware>();
app.UseMiddleware<CustomAuthorizationMiddleware>();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Auth}/{action=Login}/{id?}");

// SignalR hub yolu
app.MapHub<WTPartHub>("/wTPartHub");

app.Run();

