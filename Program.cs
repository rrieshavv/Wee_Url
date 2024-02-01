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

    //mapping short url with the long one
    var sUrl = new UrlSchema()
    {
        Url = url.Url,
        ShortUrl = randomStr
    };

    //saving the mapping to the database
    db.Urls.Add(sUrl);
    await db.SaveChangesAsync();

    //construct url
    var result = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{sUrl.ShortUrl}";

    return Results.Ok(new ShortUrlResponseDto()
    {
        Url = result
    });
});
/*
app.MapFallback(async context =>
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

    var path = context.Request.Path.ToUriComponent().Trim('/');
    var urlMatch = await dbContext.Urls.FirstOrDefaultAsync(x =>
        x.ShortUrl.ToLower().Trim() == path);

   if (urlMatch == null)
    {
       context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid request yo");
        return;
    }

    context.Response.Redirect(urlMatch.Url); // Redirect to the original URL
});
*/
app.MapGet("/{shortUrl}", async (string shortUrl, ApiDbContext dbContext, HttpContext context) =>
{
    var urlMatch = await dbContext.Urls.FirstOrDefaultAsync(x => x.ShortUrl.ToLower() == shortUrl.ToLower());

    if (urlMatch == null)
    {
        context.Response.StatusCode = 404; // Not Found
        await context.Response.WriteAsync("Short URL not found");
        return;
    }

    // Redirect to the original URL
    context.Response.Redirect(urlMatch.Url);
});

app.Run();


app.Run();

class ApiDbContext : DbContext
{
    public virtual DbSet<UrlSchema> Urls { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options):base(options)
    {
        
    }
}