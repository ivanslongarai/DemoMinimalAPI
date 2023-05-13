using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/supplierList", async (MinimalContextDb ctx) =>
        await ctx.Suppliers.ToListAsync()
    )
    .WithName("GetSuppierList")
    .WithTags("Supplier");

app.MapGet("/supplier/{id}", async (Guid id, MinimalContextDb ctx) =>
        await ctx.Suppliers.FindAsync(id) is Supplier supplier ? Results.Ok(supplier)
            : Results.NotFound()
    )
    .Produces<Supplier>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetSuppierById")
    .WithTags("Supplier");

app.Run();
