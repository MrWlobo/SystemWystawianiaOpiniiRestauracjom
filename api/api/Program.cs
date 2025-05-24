using Microsoft.EntityFrameworkCore;
using Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var expiresInMinutes = double.Parse(jwtSettings["ExpiresInMinutes"] ?? "60");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? string.Empty)),
        ClockSkew = TimeSpan.Zero 
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Admin")); 
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });

    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {} 
        }
    });
});

builder.Services.AddDbContext<ContextDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();


if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();     
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name V1");
    });
}



app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ContextDb>();
        
        context.Database.Migrate();
        

        if (!context.Users.Any(u => u.Login == "admin"))
    {
        var adminUser = new User
        {
            Login = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin"),
            isAdmin = true
        };
        context.Users.Add(adminUser);
        context.SaveChanges();
        Console.WriteLine("Admin user 'admin' seeded.");
    }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        
    }

    
}

app.Run();
