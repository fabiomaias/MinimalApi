using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinimalApi.Data;
using MinimalApi.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MinimalAPI", Description = "Teste com Minimal APIs", Version = "v1" });
});

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalAPI v1");
});

app.UseHttpsRedirection();


app.MapGet("/", () => "Teste da API");


app.MapGet("/provider", async (
    MinimalContextDb context) =>
    await context.Providers.ToListAsync())
    .WithName("GetProvider")
    .WithTags("Provider");


app.MapGet("provider/{id}", async (
    Guid id, MinimalContextDb context) =>
    await context.Providers.FindAsync(id)
        is Provider provider
            ? Results.Ok(provider)
            : Results.NotFound())
    .Produces<Provider>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetProviderById")
    .WithTags("Provider");


app.MapPost("provider", async (
    MinimalContextDb context, Provider provider) =>
    {
        if (!MiniValidator.TryValidate(provider, out var errors))
            return Results.ValidationProblem(errors);

        context.Providers.Add(provider);
        var result = await context.SaveChangesAsync();

        return result > 0
            //? Results.Created($"/provider/{provider.Id}", provider)
            ? Results.CreatedAtRoute("GetProviderById", new { id = provider.Id }, provider)
            : Results.BadRequest("Houve um problema ao salvar o registro");
    }).ProducesValidationProblem()
    .Produces<Provider>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostProvider")
    .WithTags("Provider");


app.MapPut("provider/{id}", async (
    Guid id,
    MinimalContextDb context,
    Provider provider) =>
    {
        var currentProvider = await context.Providers.FindAsync(id);
        if (currentProvider == null) return Results.NotFound();

        if (!MiniValidator.TryValidate(provider, out var errors))
            return Results.ValidationProblem(errors);

        context.Providers.Update(provider);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");

    }).ProducesValidationProblem()
    .Produces<Provider>(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutProvider")
    .WithTags("Provider");


app.MapDelete("provider/{id}", async (
    Guid id,
    MinimalContextDb context) =>
    {
        var currentProvider = await context.Providers.FindAsync(id);
        if (currentProvider == null) return Results.NotFound();

        context.Providers.Remove(currentProvider);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");
    })
    .Produces<Provider>(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("DeleteProvider")
    .WithTags("Provider");

app.Run();
