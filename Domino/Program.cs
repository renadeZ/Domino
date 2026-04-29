using Domino.CLI;
using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

//Players
IPlayer player1 = new Player("Player 1");
IPlayer player2 = new Player("Player 2");

List<IPlayer> players = [player1, player2];


//Board
IBoard board = new Board(new List<IDominoTile>(), 0, 0);

//Deck
List<IDominoTile> tiles = new List<IDominoTile>();
IDeck deck = new Deck(tiles, tiles.Count(), 6);
for (int i = 0; i < deck.MaxPipValue + 1; i++)
{
    for (int j = i; j < deck.MaxPipValue + 1; j++)
    {
        deck.Tiles.Add(new DominoTile(i, j));
        deck.TotalTiles++;
    }
}

//Game Rules
IGameRules rules = new GameRules(
    // 151,
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


//Game Controller
Game gameController = new Game(players, board, deck, rules);

//Start Game UI
DominoCli cli = new DominoCli(gameController);
cli.RunGame();
