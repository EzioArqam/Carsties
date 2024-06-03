using System.Text.Json;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app){
        await DB.InitAsync("SearchDb", MongoClientSettings
        .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();
            
        var count = await DB.CountAsync<Item>();
        //Getting Data from client 
        // if(count == 0){
        //     Console.WriteLine("No data - will attemp to seed");
        //     var itemData = await File.ReadAllTextAsync("Data/auctions.json");
        //     var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
        //     await DB.SaveAsync(items);
        // }
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetService<AuctionServiceHttpClient>();
        var items = await httpClient.GetItemsForDbSearch();
        Console.WriteLine(items.Count + " returned from the auction service");
        if (items.Count > 0) await DB.SaveAsync(items);
    }
}
