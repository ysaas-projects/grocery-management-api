using grocery_management.Data;
using grocery_management.Services.Implementation;
using grocery_management.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// ---------- Swagger ----------
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Grocery Management API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
            Array.Empty<string>()
        }
    });
});



// ---------- Database ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);



// ---------- Services ----------
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();



// ---------- CORS (VERY IMPORTANT) ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://10.0.2.2:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // REQUIRED for cookies
    });
});




// ---------- Authentication ----------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "SmartScheme";
        options.DefaultChallengeScheme = "SmartScheme";
    })
    .AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                return JwtBearerDefaults.AuthenticationScheme;

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })

    //  Cookie Auth (for browser sessions)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "accessToken";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.LoginPath = "/signin";
        options.AccessDeniedPath = "/signin";

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    })

    //  JWT Auth (for API / mobile / cookie-based JWT)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:AccessTokenSecret"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };

        //  THIS IS THE MISSING PART
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Read JWT from cookie
                var token = context.Request.Cookies["accessToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });



// ---------- Kestrel ----------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP only
});




// ==============================
// PIPELINE
// ==============================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
