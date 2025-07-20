using wwwbackend.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Read secret from environment variable
var jwtSecret = builder.Configuration["JWT_SECRET"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new Exception("JWT_SECRET is not set in configuration! Please configure it in Azure App Service.");
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:4200",
                "https://dw42csce.github.io")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var connectionString =
    builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ??
    Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new Exception("SQL connection string is missing.");

builder.Services.AddDbContext<WineDbContext>(options =>
    options.UseSqlServer(connectionString));


// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "WadeWineAPI",
            ValidAudience = "WadeWineClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and verify connection
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WineDbContext>();
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        Console.WriteLine($"Pending Migrations: {string.Join(", ", pendingMigrations)}");

        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
        }

}

app.Run();
