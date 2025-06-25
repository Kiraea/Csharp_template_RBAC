using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connection to postgresql
builder.Services.AddDbContext<ApplicationDbContext>(options=>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddAuthentication((options) =>
{
    // this is only for the options for add authentication
    
    
    // basically doesnt return any status if not logged in etc passive basically
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    
    // actively checks if user is authoriized if not 403 
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    // actively checks if user is authencicated if not 401
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer((options) =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // who gave the u the jwt token
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        
        // purpose of this is basically what site are u allowing it to access to
        // cause there are lots of there may be 2 websites that use the same token and it prevents it from getting token from web 1 to use it to web 2
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
    };
    
    
});
// adding this basically adds the signinmanager, usermanager,
// but technically u still need to connect identity oto how it will talk to EF or db context so u add entity framework
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{

    options.User.RequireUniqueEmail = true;
    
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;
    
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<ITokenService, TokenService>();




builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
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

var app = builder.Build();
// code to test if connection was sucesfull
//bassically instantiating application db context in this scope context only
// can be done for anything that is injected like logger etc
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.CanConnect(); // or await db.Database.EnsureCreatedAsync() if it's empty
        Console.WriteLine("Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to connect to database: " + ex.Message);
    }
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();