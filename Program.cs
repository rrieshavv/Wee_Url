using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using WeeUrl.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connStr));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/shorturl", async(UrlDto url, ApiDbContext db, HttpContext ctx) =>
{
    //validation the input url
    if(!Uri.TryCreate(url.Url, UriKind.Absolute, out var InputUrl))
    {
        return Results.BadRequest("Invalid url has been provided.");
    }

    //creating a short version of the provided url
    var random = new Random();
    const string chars = "ABCDEFHIJKLMNOPQRSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyz";
    var randomStr = new string(Enumerable.Repeat(chars, 8)
        .Select(x => x[random.Next(x.Length)]).ToArray());
});



app.Run();

class ApiDbContext : DbContext
{
    public virtual DbSet<UrlSchema> Urls { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options):base(options)
    {
        
    }
}