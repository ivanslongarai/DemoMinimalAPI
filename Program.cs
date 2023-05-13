using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using DemoMinimalAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DemoMinimalAPI.Repositories;
using DemoMinimalAPI.Services;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var key = Encoding.ASCII.GetBytes(Settings.Secret);

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demo Minimal API",
        Description = "Developed by Ivan Longarai for study purposes",
        License = new OpenApiLicense { Name = "License MIT", Url = new Uri("https://opensource.org/licenses/MIT")}
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter with the JWT like this: Bearer {token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
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
            new string[] {}
        }
    });

});

builder.Services.AddDbContext<MinimalContextDb>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Admin", policy => policy.RequireRole("manager"));
    opt.AddPolicy("Employee", policy => policy.RequireRole("employee"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/* User rotes */

app.MapPost("/login", (User userParam) =>
{
    var user = UserRepository.Get(userParam.UserName!, userParam.Password!);

    if (user == null)
    {
        return Results.NotFound(new { message = "Invalid username or password" });
    }

    var token = TokenService.GenerateToken(user!);
    user!.Password = String.Empty;

    return Results.Ok(new
    {
        user,
        token
    });

})
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetTokenByLogIn")
.WithTags("User")
.AllowAnonymous();

app.MapGet("/authenticated", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as {user?.Identity!.Name}" });
})
.Produces(StatusCodes.Status200OK)
.WithName("GetAuthenticationStatus")
.WithTags("User")
.RequireAuthorization();

/* Suppiers rotes */

app.MapGet("/supplierList", async (MinimalContextDb ctx) =>
        await ctx.Suppliers.ToListAsync()
    )
    .RequireAuthorization()
    .WithName("GetSuppierList")
    .WithTags("Supplier");


app.MapGet("/supplier/{id}", async (Guid id, MinimalContextDb ctx) =>
        await ctx.Suppliers.FindAsync(id) is Supplier supplier ? Results.Ok(supplier)
            : Results.NotFound()
    )
    .RequireAuthorization()
    .Produces<Supplier>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetSuppierById")
    .WithTags("Supplier");

app.MapPost("/supplier", async (MinimalContextDb ctx, Supplier supplier) =>
{
    var validationResult = Supplier.GetValidator().Validate(supplier);

    if (validationResult.IsValid)
    {
        ctx.Suppliers.Add(supplier);
        var result = await ctx.SaveChangesAsync();

        /* It is just another way of returning this: 
            return result > 0 ? Results.Created($"/supplier/{supplier.Id}", supplier) : Results.BadRequest("Something went wrong");
        */

        return result > 0 ? Results.CreatedAtRoute("GetSuppierById", new {id = supplier.Id}, supplier) : Results.BadRequest("Something went wrong");
    }
    else
    {
        var errosToReturn = validationResult.Errors.Select(e => e.ErrorMessage);

        var result = new Dictionary<string, string[]>
        {
            { "Supplier", errosToReturn.ToArray() }
        };

        return Results.ValidationProblem(result);
    }
})
.RequireAuthorization()
.ProducesValidationProblem()
.Produces<Supplier>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostSupplier")
.WithTags("Supplier");

app.MapPut("/supplier/{id}", async (Guid id, MinimalContextDb ctx, Supplier supplier) =>
{
        var supplierStored = await ctx.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (supplierStored == null)
        {
            return Results.NotFound();
        }
        else
        {
            supplier.Id = id;
        }

        var validationResult = Supplier.GetValidator().Validate(supplier);

        if (validationResult.IsValid)
        {
            ctx.Suppliers.Update(supplier);
            var result = await ctx.SaveChangesAsync();
            return result > 0 ? Results.Ok(supplier) : Results.BadRequest("Something went wrong");
        }
        else
        {
            var errosToReturn = validationResult.Errors.Select(e => e.ErrorMessage);

            var result = new Dictionary<string, string[]>
            {
                { "Supplier", errosToReturn.ToArray() }
            };

            return Results.ValidationProblem(result);
        }
})
.RequireAuthorization("Admin")
.ProducesValidationProblem()
.Produces<Supplier>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutSupplier")
.WithTags("Supplier");

app.MapDelete("/supplier/{id}", async (Guid id, MinimalContextDb ctx) =>
{
    var supplierStored = await ctx.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    if (supplierStored == null)
    {
        return Results.NotFound();
    }
    else
    {
        ctx.Suppliers.Remove(supplierStored);
        var result = await ctx.SaveChangesAsync();
        return result > 0 ? Results.Ok($"Removed successfully the supplier id: {id}") : Results.BadRequest("Something went wrong");
    }    
})
.RequireAuthorization("Admin")
.ProducesValidationProblem()
.Produces<Supplier>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.WithName("DeleteSupplier")
.WithTags("Supplier");

app.Run();
