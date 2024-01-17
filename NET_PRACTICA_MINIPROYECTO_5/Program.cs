using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NET_PRACTICA_MINIPROYECTO_5.Middleware;
using NET_PRACTICA_MINIPROYECTO_5.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "myPolicyCors";
// Add services to the container.

/*
builder.Services.AddTransient(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("SqlServerConnection")));*/

builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlServerConnection")!;
    return new DbConnectionFactory(connectionString);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthTokenService>(provider =>
{
    IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
    IAntiforgery antiforgery = provider.GetRequiredService<IAntiforgery>();
    IDbConnectionFactory dbConnectionFactory = provider.GetRequiredService<IDbConnectionFactory>();

    return new AuthTokenService(httpContextAccessor, configuration, antiforgery, dbConnectionFactory);
});

builder.Services.AddTransient<IRefreshTokenService>(provider =>
{
    IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    IDbConnectionFactory DbConnectionFactory = provider.GetRequiredService<IDbConnectionFactory>();

    return new RefreshTokenService(httpContextAccessor, DbConnectionFactory);
});


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                          
                      });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//addswagergen esto configura la seguridad de la appi 
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


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddCookie(options =>
    {
        options.Cookie.Name = "JWT-TOKEN"; //el nombre con el cual se manejara en el cliente, no el backend
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
    options.Events = new JwtBearerEvents //esto maneja eventos, en este caso cuado lanzamos una solicitud y mandamos una cookie, obtenemos el token de la cookie
                                         
    {
        OnMessageReceived = context => //esto es un delegado, maneja el evento, y es un dump task que no devuelve nada
        {
            context.Token = context.Request.Cookies["JWT-TOKEN"];
            return Task.CompletedTask;
        }
    };
    
});


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    //options.Cookie.Expiration = TimeSpan.FromSeconds(10);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});


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

app.UseWhen(context => context.Request.Path.StartsWithSegments("/Show"), adminApp =>
{
    adminApp.UseMiddleware<JwtExpirationValidationMiddleware>();
});


app.UseWhen(context => context.Request.Path.StartsWithSegments("/Show"), adminApp =>
{
    adminApp.UseMiddleware<SessionExpirationValidationMiddleware>();

});

app.UseAuthorization();

app.MapControllers();

app.Run(app.Configuration["Data:Url"]);


//ver lo de como podemos hacer para que cuendo generemos los tokens nos saltemos el authorization middleware y seguir con el de los controller para que retorne la informacion aun caundo generamos tokens nuevos