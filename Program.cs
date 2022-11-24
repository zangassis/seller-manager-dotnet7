using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SellerManager.Data;
using SellerManager.Helpers;
using SellerManager.Models;
using SellerManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SellerDb>();
builder.Logging.AddConsole();
builder.Services.AddSingleton<TokenService>();
var secretKey = ApiSettings.GenerateSecretByte();

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("manager", policy => policy.RequireRole("manager"));
    options.AddPolicy("operator", policy => policy.RequireRole("operator"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

#region Endpoint filters

app.MapPost("sellers/create/{sellerName}", (string sellerName, SellerDb db) =>
{
    var newSeller = new Seller(Guid.NewGuid(), sellerName);
    db.Add(newSeller);
    db.SaveChanges();
    return Results.Ok("Seller created successfully");
})
.AddEndpointFilter(async (invocationContext, next) =>
{
    var sellerName = invocationContext.GetArgument<string>(0);

    if (sellerName.Length > 10)
    {
        app.Logger.LogInformation($"Error when creating the new seller");
        return Results.Problem("Seller Name must be a maximum of 10 characters!");
    }
    return await next(invocationContext);
});

app.MapPut("/sellers/update/{id}", async (Seller seller, int id, SellerDb db) =>
{
    var dbSeller = await db.Sellers.FindAsync(id);

    if (dbSeller is null) return Results.NotFound();

    dbSeller.Name = seller.Name;

    await db.SaveChangesAsync();

    return Results.NoContent();
}).AddEndpointFilter(async (efiContext, next) =>
{
    var tdparam = efiContext.GetArgument<Seller>(0);

    var validationError = Utilities.IsValid(tdparam);

    if (!string.IsNullOrEmpty(validationError))
    {
        app.Logger.LogInformation($"Error when updating the new seller: {validationError}");
        return Results.Problem(validationError);
    }
    return await next(efiContext);
});

#endregion

#region Typed Results
app.MapGet("sellers/getSeller/{sellerId}", (Guid id, SellerDb db) =>
{
    return GetSeller(id, db);
});

static async Task<Results<Ok<Seller>, NotFound>> GetSeller(Guid id, SellerDb db) =>
    await db.Sellers.FirstOrDefaultAsync(x => x.Id == id) is Seller item ? TypedResults.Ok(item) : TypedResults.NotFound();

#endregion

#region Route Groups
app.MapGroup("/public/sellers")
    .MapSellersApi()
    .WithTags("Public");

app.MapGroup("/private/sellers")
    .MapSellersApi()
    .WithTags("Private")
    .RequireAuthorization("Manager");

app.MapPost("/login", (User userModel, TokenService service) =>
{
    var user = UserRepository.Find(userModel.Username, userModel.Password);

    if (user is null)
        return Results.NotFound(new { message = "Invalid username or password" });

    var token = service.GenerateToken(user);

    user.Password = string.Empty;

    return Results.Ok(new { user = user, token = token });
});

app.MapGet("sellers/getAll", (SellerDb db) =>
{
    var sellers = db.Sellers;

    if (!sellers.Any())
        return Results.NotFound();
    else
        return TypedResults.Ok(sellers);
});

#endregion

app.Run();


