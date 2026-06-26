using ProductCatalog.Api.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductCatalog.Api.Middleware;
using ProductCatalog.Api.OpenApi;
using ProductCatalog.Api.Options;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("JWT settings are not configured.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1"
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token in the format: Bearer {token}"
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .ValidateDataAnnotations()
    .Validate(options => options.ExpirationMinutes > 0, "ExpirationMinutes must be greater than zero.")
    .ValidateOnStart();
builder.Services
    .AddOptions<DemoUserOptions>()
    .Bind(builder.Configuration.GetSection(DemoUserOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IProductRepository, TxtProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(FrontendCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
