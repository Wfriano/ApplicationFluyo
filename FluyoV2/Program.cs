using FluentValidation;
using FluyoV2.Features.Accounts.Repositories;
using FluyoV2.Features.Accounts.Services;
using FluyoV2.Features.Auth.Repositories;
using FluyoV2.Features.Auth.Services;
using FluyoV2.Features.Categories.Services;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Commitments.Services;
using FluyoV2.Features.Dashboard.Services;
using FluyoV2.Features.Goals.Repositories;
using FluyoV2.Features.Goals.Services;
using FluyoV2.Features.Transactions.Repositories;
using FluyoV2.Features.Transactions.Services;
using FluyoV2.Features.Transfers.Repositories;
using FluyoV2.Features.Transfers.Services;
using FluyoV2.Infrastructure.Persistence;
using FluyoV2.Middleware;
using FluyoV2.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Mongo Settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value);

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fluyo API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese únicamente el token JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MongoDb Context
builder.Services.AddSingleton<MongoDbContext>();

// Dependency Injection
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<AccountsRepository>();
builder.Services.AddScoped<AccountsService>();

builder.Services.AddSingleton<CategoriesService>();

builder.Services.AddScoped<TransactionsRepository>();
builder.Services.AddScoped<TransactionsService>();

builder.Services.AddScoped<TransfersRepository>();
builder.Services.AddScoped<TransfersService>();

builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<GoalsRepository>();
builder.Services.AddScoped<GoalsService>();

builder.Services.AddScoped<CommitmentsRepository>();
builder.Services.AddScoped<CommitmentsService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fluyo API v1");
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();