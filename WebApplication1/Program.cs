using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ITodoService, TodoService>();
builder.Services.Configure<TodoServiceConfig>(builder.Configuration.GetSection("TodoServiceApi"));
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MyApp"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317");
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/GetTodoListInParallel", async (ITodoService todoService) =>
    {
        var todoItems = Enumerable.Range(1, 5).Select(todoService.GetTodoItem)
            .ToArray();
        return await Task.WhenAll(todoItems);
    })
    .WithName("GetTodoListInParallel")
    .WithOpenApi();


app.MapGet("/GetTodoListInSequence", async (ITodoService todoService) =>
    {
        var todoItems = new List<TodoItem?>();
        var todoItemTasks = Enumerable.Range(1, 5).Select(todoService.GetTodoItem);
        
        foreach (var task in todoItemTasks)
        {
            var todoItem = await task;
            todoItems.Add(todoItem);
        }
        return todoItems;
    })
    .WithName("GetTodoListInSequence")
    .WithOpenApi();

app.Run();