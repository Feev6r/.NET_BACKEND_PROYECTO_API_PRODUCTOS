using Azure.Storage.Blobs;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NET_PRACTICA_MINIPROYECTO_5.Attributes;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Repositories;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using DotNetEnv;
using NET_PRACTICA_MINIPROYECTO_5.Others;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobConnection")));
builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlServerConnection")!;
    return new DbConnectionFactory(connectionString);
});

#region Claudinary
Env.Load();


var cloudConfig = new CloudConfig
{
    Name = Env.GetString("CloudName"),
    ApiKey = Env.GetString("ApiKey"),
    ApiSecret = Env.GetString("ApiSecret")
};


Cloudinary cloudinary = new Cloudinary(new Account(
    cloudConfig.Name,
    cloudConfig.ApiKey,
    cloudConfig.ApiSecret
    ));
cloudinary.Api.Secure = true;


#endregion

builder.Services.AddSingleton(cloudinary);
builder.Services.AddScoped<IBlobRepository, BlobRepository>();
builder.Services.AddScoped<IProductsRepository, ProducstRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICloudinaryImgRepository, ClaudinaryImgRepository>();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: builder.Configuration["Data:CorsPolicy"]!,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithExposedHeaders("X-CSRF-Token")
                          .AllowCredentials();                                   
                      });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {

        In = ParameterLocation.Header, //donde se recivira el token
        Name = "Authorization", //el header del token
        Type = SecuritySchemeType.ApiKey //un tipo de seguridad, en este caso es apikey que hace que en una solicitud se le tenga que pasar como parametro un token
    });
  
    options.OperationFilter<SecurityRequirementsOperationFilter>();  
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RefreshCredentials>();
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddCookie(options =>
    {
        options.Cookie.Name = "JWT-TOKEN"; 
        options.Cookie.Path = "/";
    })
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidIssuer = "https://localhost:7777/",
        ValidAudience = "http://localhost:4200/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Appsettings:Token").Value!)),
    };
    options.Events = new JwtBearerEvents 
    {
        OnMessageReceived = context => 
        {
            context.Token = context.Request.Cookies["JWT-TOKEN"];
            return Task.CompletedTask;
        }
    };
});

//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromDays(15);
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    options.Cookie.HttpOnly = true;
//    options.Cookie.SameSite = SameSiteMode.None;
//});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

//----------------------------------------------

var app = builder.Build();

app.UseRouting();
app.UseCors(app.Configuration["Data:CorsPolicy"]!);
app.UseHttpsRedirection();
//app.UseStaticFiles();
app.UseAuthentication();

app.UseCsrfTokenValidator();

app.UseAuthorization();
app.MapControllers();


app.Run(app.Configuration["Data:Url"]);

