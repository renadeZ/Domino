using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    // Console sink with structured output - great for development
    .WriteTo.Console(outputTemplate: 
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}|{Level:u3}[{SourceContext}]" +
        "{Message:lj} {Properties:j}{NewLine}{Exception}")
    
    // File sink with JSON format - perfect for log aggregation tools
    .WriteTo.File("logs/application-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] " +
                       "{Message:lj} {Properties:j}{NewLine}{Exception}")
    
    // File sink with JSON format for structured data analysis
    .WriteTo.File(new JsonFormatter(), "logs/application-json-.log",
        rollingInterval: RollingInterval.Day)
    
    // Enrich logs with additional context information
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Configuring web application...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    Log.Information("Serilog configured successfully.");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton<GameController>(provider =>
    {
        IPlayer player1 = new Player("Player 1");
        IPlayer player2 = new Player("Player 2");
        List<IPlayer> players = new List<IPlayer> { player1, player2 };

        IBoard board = new Board(new List<IDominoTile>(), 0, 0);
        List<IDominoTile> tiles = new List<IDominoTile>();
        IDeck deck = new Deck(tiles, 0, 6);
        
        for (int i = 0; i < deck.MaxPipValue + 1; i++)
        {
            for (int j = i; j < deck.MaxPipValue + 1; j++)
            {
                deck.Tiles.Add(new DominoTile(i, j));
                deck.TotalTiles++;
            }
        }

        IGameRules rules = new GameRules(151, 30, -1, 1, 2, 20, -40, 7, 7, 5);
        
        return new GameController(players, board, deck, rules);
    });

    builder.Services.AddSingleton<Domino.API.Services.IGameService, Domino.API.Services.GameService>();
    Log.Information("Services registered successfully.");

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Application configured successfully, starting server...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application unexpectedly terminated.");
}
finally
{
    // Ensure all buffered log events are written out before the application exits
    Log.CloseAndFlush();
}


