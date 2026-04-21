using Domino.CLI;
using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

//Players
IPlayer player1 = new Player("Player 1");
IPlayer player2 = new Player("Player 2");
List<IPlayer> players = [player1, player2];


//Board
IBoard board = new Board(new List<IDominoTile>(), 0, 0);

//Deck
List<IDominoTile> tiles = new List<IDominoTile>();
IDeck deck = new Deck(tiles, tiles.Count(), 6);

//Game Rules
IGameRules rules = new GameRules(
    151,
    30,
    -1,
    1,
    2,
    2,
    -40, 
    7,     
    7,
    5
    );
    
//Game Controller
GameController gameController = new GameController(players, board, deck, rules);

//Start Game UI
DominoCli cli = new DominoCli(gameController);
cli.RunGame();
