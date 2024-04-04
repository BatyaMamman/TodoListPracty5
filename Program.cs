using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
//גישה למסד
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
        new MySqlServerVersion(new Version(8, 0, 26))));

builder.Services.AddScoped<Item>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//swagger -בניית ה
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});
//הוספת הרשאת גישה לכולם
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }));
var app = builder.Build();//משתנה לפעולות
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("corsapp");
        app.UseAuthorization();
//הצגה
app.MapGet("/items",  (ToDoDbContext db) =>
{
    return  db.Items;
});
//הוספה
app.MapPost("/newItem", async (Item[] items, ToDoDbContext db) =>
{
    await db.Items.AddRangeAsync(items);
    await db.SaveChangesAsync();

    return Results.Ok(items);
});
//עדכון
app.MapPatch("/items/{id}", async (int Id, bool IsComplete, Item item, ToDoDbContext Db) =>
{
    var item1 = await Db.Items.FindAsync(Id);

    if (item1 is null) return Results.NotFound();

    item1.IsComplete = IsComplete;

    await Db.SaveChangesAsync();

    return Results.NoContent();
});
//מחיקה
app.MapDelete("/todoitems/{id}", async (int Id, ToDoDbContext Db) =>
{
    if (await Db.Items.FindAsync(Id) is Item item)
    {
        Db.Items.Remove(item);
        await Db.SaveChangesAsync();
        return Results.Ok(item);
    }

    return Results.NotFound();
});

app.Run();
