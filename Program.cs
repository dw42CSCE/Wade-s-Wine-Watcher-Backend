using wwwbackend.data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add DbContext
builder.Services.AddDbContext<WineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();

// Apply migrations and verify connection
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WineDbContext>();

    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Migrations applied successfully!");

        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("✅ Successfully connected to the database!");
        }
        else
        {
            Console.WriteLine("❌ Could not connect to the database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration or connection failed: {ex.Message}");
    }
}

app.Run();
