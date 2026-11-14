using Microsoft.AspNetCore.Authentication.Cookies;
using TechMove.Services.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();
builder.Services.AddTransient<JwtBearerHandler>();
builder.Services.AddHttpClient<IClientApiService, ClientApiService>(ConfigureApiClient).AddHttpMessageHandler<JwtBearerHandler>();
builder.Services.AddHttpClient<IContractApiService, ContractApiService>(ConfigureApiClient).AddHttpMessageHandler<JwtBearerHandler>();
builder.Services.AddHttpClient<IServiceRequestApiService, ServiceRequestApiService>(ConfigureApiClient).AddHttpMessageHandler<JwtBearerHandler>();
builder.Services.AddHttpClient<IExchangeRateApiService, ExchangeRateApiService>(ConfigureApiClient).AddHttpMessageHandler<JwtBearerHandler>();
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigureApiClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

static void ConfigureApiClient(IServiceProvider services, HttpClient client)
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/");
    client.Timeout = TimeSpan.FromSeconds(10);
}

app.Run();
