using Api.Data;
using Microsoft.EntityFrameworkCore;
using Api.EndPoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var connString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

await DbInitializer.SeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapProductEndpoints();
app.MapServiceEndpoints();

app.Run();
