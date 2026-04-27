using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();