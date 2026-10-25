using InventoryOrders.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    options.AddSecurityRequirement(document =>
    {
        var requirement = new Microsoft.OpenApi.OpenApiSecurityRequirement();

        var schemeReference =
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                "Bearer", document, null);

        requirement.Add(schemeReference, new List<string>());

        return requirement;
    });
});


var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "JWT secret is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? "InventoryOrders.Api";

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? "InventoryOrders.Client";


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthorization();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapPost("/api/auth/login", (LoginRequest request) =>
       {
           // Development-only demo credentials
           if (request.Username != "demo" ||
               request.Password != "ChangeMe123!")
           {
               return Results.Unauthorized();
           }

           var claims = new[]
           {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "User")
       };

           var key = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(jwtSecret));

           var credentials = new SigningCredentials(
               key,
               SecurityAlgorithms.HmacSha256);

           var token = new JwtSecurityToken(
               issuer: jwtIssuer,
               audience: jwtAudience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(30),
               signingCredentials: credentials);

           return Results.Ok(new
           {
               token = new JwtSecurityTokenHandler()
                   .WriteToken(token),
               expiresAt = token.ValidTo
           });
       });
//}


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    db.Database.Migrate();
}

app.Run();
public record LoginRequest(string Username, string Password);