using Domino.Backend;
using Domino.Backend.Models;
using Domino.GUI.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register GameController
builder.Services.AddScoped<GameController>(sp => 
{
    var players = new List<IPlayer>
    {
        new Player("Player 1"),
        new Player("Player 2")
    };
    var rules = new GameRules(
        151,
        30,
        -1,
        1,
        2,
        20,
        -40, 
        7,     
        7,
        5
    );
    var board = new Board(new List<IDominoTile>(), 0, 0);
    
    List<IDominoTile> tiles = new List<IDominoTile>();
    var deck = new Deck(tiles, tiles.Count(), 6);
    
    return new GameController(players, board, deck, rules);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();