using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using EShop.Data;
using EShop.Repositories;
using EShop.Services;
using EShop.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EshoppingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["Key"];
if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key must be at least 256 bits (32 characters) long.");
}

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" // Ensure this matches your token's role claim type
        };
    });

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development CORS policy - restricted to localhost for security
        options.AddPolicy("AllowAll",
            policy => policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5000", "https://localhost:5001")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
    }
    else
    {
        // Production CORS policy
        options.AddPolicy("AllowAll",
            policy => policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                            .WithMethods("GET", "POST", "PUT", "DELETE")
                            .WithHeaders("Content-Type", "Authorization"));
    }
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EshoppingZone API",
        Version = "v1",
        Description = "E-commerce API for managing products, orders, and authentication."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <your_token>' (without quotes). Example: 'Bearer abc123xyz'",
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
            new List<string>()
        }
    });

    // Include XML comments if available for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddScoped<IRepositoryCollection, RepositoryCollection>();
builder.Services.AddScoped<RepositoryService>();
builder.Services.AddScoped<IChatbotOperationsService, ChatbotOperationsService>();
async Task SeedAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<EshoppingDbContext>();

    // If there's already an Admin user, do nothing
    if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
    {
        var admin = new User
        {
            FullName = "admin",
            Email = "admin@example.com",
            Role = UserRole.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123") 
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Admin user seeded.");
    }
    else
    {
        Console.WriteLine("ℹ️ Admin user already exists.");
    }
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
await SeedAdminUserAsync(app.Services);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EshoppingZone API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

