using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NET_PRACTICA_MINIPROYECTO_5.Attributes;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Repositories;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "myPolicyCors";


builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobConnection")));

builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlServerConnection")!;
    return new DbConnectionFactory(connectionString);
});


builder.Services.AddScoped<IBlobRepository, BlobRepository>();

builder.Services.AddScoped<IProductsRepository, ProducstRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
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

//Configures the security of the api
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

//Configures stuff from controllers like filters and other things.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TestAuthorizationFilter>();
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddCookie(options =>
    {
        options.Cookie.Name = "JWT-TOKEN"; 
        options.Cookie.Path += "/Show/Prodcutos|/Show/test";
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
        ValidAudience = "https://localhost:7777/Show/",
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




builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    //options.IdleTimeout = TimeSpan.FromDays(1);
    //options.Cookie.Expiration = TimeSpan.FromDays(30);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
});

builder.Services.AddAntiforgery();

//----------------------------------------------

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseSession();

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();


app.Run(app.Configuration["Data:Url"]);

