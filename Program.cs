using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseMySql("Server=localhost;Database=TodoDb;User=user1;Password=Applecar3!", new MySqlServerVersion(new Version(8, 0, 21))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDo API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1"));
}

app.UseHttpsRedirection();

// CRUD Endpoints
app.MapPost("/todo", async (TodoContext db, TodoItem todoItem) =>
{
    db.TodoItems.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todo/{todoItem.Id}", todoItem);
});

app.MapGet("/todo", async (TodoContext db) =>
    await db.TodoItems.ToListAsync());

app.MapGet("/todo/{id}", async (TodoContext db, int id) =>
    await db.TodoItems.FindAsync(id)
        is TodoItem todoItem
            ? Results.Ok(todoItem)
            : Results.NotFound());

app.MapPut("/todo/{id}", async (TodoContext db, int id, TodoItem updatedTodo) =>
{
    var todo = await db.TodoItems.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = updatedTodo.Name;
    todo.IsComplete = updatedTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todo/{id}", async (TodoContext db, int id) =>
{
    if (await db.TodoItems.FindAsync(id) is TodoItem todo)
    {
        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();

class TodoItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems { get; set; }
}
