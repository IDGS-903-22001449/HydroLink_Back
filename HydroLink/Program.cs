using HydroLink;
using HydroLink.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuracion de JWT
var JWTSetting = builder.Configuration.GetSection("JWTSetting");

// Validar que la configuración JWT existe
if (!JWTSetting.Exists())
{
    throw new InvalidOperationException("JWTSetting configuration section is missing from appsettings.json");
}

var securityKey = JWTSetting.GetSection("securityKey").Value;
var validAudience = JWTSetting["ValidAudience"];
var validIssuer = JWTSetting["ValidIssuer"];

if (string.IsNullOrEmpty(securityKey))
{
    throw new InvalidOperationException("JWT securityKey is missing or empty");
}

if (string.IsNullOrEmpty(validAudience))
{
    throw new InvalidOperationException("JWT ValidAudience is missing or empty");
}

if (string.IsNullOrEmpty(validIssuer))
{
    throw new InvalidOperationException("JWT ValidIssuer is missing or empty");
}

var connectionString = builder.Configuration.GetConnectionString("cadenaSQL");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'cadenaSQL' is missing or empty");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Agregar DbContextFactory para servicios que necesitan contextos independientes
builder.Services.AddSingleton<IDbContextFactory<AppDbContext>>(provider =>
{
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseSqlServer(connectionString);
    return new DbContextFactory<AppDbContext>(optionsBuilder.Options);
});

// Registrar servicios personalizados
builder.Services.AddScoped<HydroLink.Services.IPrecioComponenteService, HydroLink.Services.PrecioComponenteService>();
builder.Services.AddScoped<HydroLink.Services.IInventarioService, HydroLink.Services.InventarioService>();
builder.Services.AddScoped<HydroLink.Services.ICostoPromedioService, HydroLink.Services.CostoPromedioService>();
builder.Services.AddScoped<HydroLink.Services.IProductoPrecioService, HydroLink.Services.ProductoPrecioService>();
builder.Services.AddScoped<HydroLink.Services.ICosteoPromedioService, HydroLink.Services.CosteoPromedioService>();
builder.Services.AddScoped<HydroLink.Services.IEmailService, HydroLink.Services.EmailService>();
builder.Services.AddScoped<HydroLink.Services.IPrecioActualizacionService, HydroLink.Services.PrecioActualizacionService>();


//Agregamos la configuración para ASP -Net Core Identity
builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidAudience = validAudience,
        ValidIssuer = validIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey))
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();

//definición de seguridad para el swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization Example : 'Bearer eyeleieieekeieieie",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
   {
       {
           new OpenApiSecurityScheme
           {
               Reference = new OpenApiReference
               {
                   Type = ReferenceType.SecurityScheme,
                   Id = "Bearer"
               },
               Scheme = "oauth2",
               Name = "Bearer",
               In = ParameterLocation.Header,
           },
           new List<string>()
       }
   });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
