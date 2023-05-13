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
.ProducesValidationProblem()
.Produces<Supplier>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.WithName("DeleteSupplier")
.WithTags("Supplier");

app.Run();
