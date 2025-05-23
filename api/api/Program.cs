using Microsoft.EntityFrameworkCore;
using Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();


if (app.Environment.IsDevelopment()) // Check if you're running in Development mode
{
    app.UseSwagger();     // Enables the Swagger middleware
    app.UseSwaggerUI();   // Enables the Swagger UI middleware (serves the Swagger UI HTML/JS/CSS)
}



app.UseHttpsRedirection();

app.UseRouting(); // Identifies what endpoint to run
app.UseAuthentication(); // If you have authentication
app.UseAuthorization(); // If you have authorization
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ContextDb>();
        // This line applies any pending migrations to the database.
        context.Database.Migrate();
        // You can also add code here to seed initial data if needed.
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        // You might want to re-throw or handle the exception based on your needs.
    }
}

app.Run();
