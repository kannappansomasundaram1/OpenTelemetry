using System.Text.Json;
using Microsoft.Extensions.Options;

public interface ITodoService
{
    Task<TodoItem> GetTodoItem(int id);
}

public class TodoService : ITodoService
{
    private readonly HttpClient _httpClient;
    private readonly TodoServiceConfig _config;

    public TodoService(HttpClient httpClient, IOptionsSnapshot<TodoServiceConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }
    
    public async Task<TodoItem> GetTodoItem(int id)
    {
        var requestUri = new Uri($"{_config.BaseUri}/todos/{id}");
        try
        {
            var todoItems = await _httpClient.GetFromJsonAsync<TodoItem>(requestUri, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            }, CancellationToken.None);
            return todoItems;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}