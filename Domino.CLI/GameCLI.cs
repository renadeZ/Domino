using System.Text;
using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

public class GameCLI
{
    private GameController _gameController;
    private const int BOARD_WIDTH = 40;
    private const int BOARD_HEIGHT = 70;
    
    private List<IDominoTile> discardedCard = new List<IDominoTile>();
    //Colors


    public GameCLI(GameController gameController)
    {
        _gameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
        Console.CursorVisible = false;
    }


    public void GameMain()
    {
        try
        {
            ConsoleSetup();
            
            _gameController.StartGame();

            int numOfPlayers = _gameController.DominoGameDto.Players.Count();
            
            _gameController.StartGame();
            while (!_gameController.IsGameOver())
            {
                RoundStart();
            }
            
        }
        catch
        {
            
        }
    }

    public void ConsoleSetup()
    {
        Console.Clear();
        Console.CursorVisible = false;
    }

    private void RoundStart()
    {
        _gameController.StartRound();
    }


    public void DrawArena()
    {
        var currentPlayer = _gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex];
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                           DOMINO                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
        Console.WriteLine($"Current Turn : {currentPlayer.Name}\n");
        Console.WriteLine("Player list :");
        foreach (var player in _gameController.DominoGameDto.PlayerHands)
            Console.WriteLine($"{player.Key.Name} : {_gameController.DominoGameDto.Scores[player.Key]} Point with {player.Value.Count()} Cards Remaining");
        
        Console.WriteLine("\n========================= BOARD =============================\n");
        DrawBoardCard();
        Console.WriteLine("\n======================= YOUR CARD ============================\n");
        Console.WriteLine("Playable Card : ");
        DrawPlayerCard(_gameController.GetPlayableTiles(currentPlayer), true);
        Console.WriteLine("\nUnplayable Card : ");
        List<IDominoTile> unplayable = _gameController.DominoGameDto.PlayerHands[currentPlayer].Except(_gameController.GetPlayableTiles(currentPlayer)).ToList();
        DrawPlayerCard(unplayable, false);
        
        Console.WriteLine("Press 1-7 To Choose Tiles, Press 0 to Pass");
    }

    private void DrawBoardCard()
    {
        StringBuilder[] line = new StringBuilder[3];
        
        for (int i = 0; i < discardedCard.Count(); i++)
        {
            line[1].Append($"===== ");
            line[2].Append(($"|{discardedCard[i].Top}|{discardedCard[i].Bottom}| "));
            line[3].Append("====== ");
        }

        foreach(var str in line)
            Console.WriteLine(str.ToString());
    }

    private void DrawPlayerCard(List<IDominoTile> playerHand, bool isPlayable)
    {
        if(isPlayable)
            Console.ForegroundColor = ConsoleColor.Green;
        else
            Console.ForegroundColor = ConsoleColor.Red;

        StringBuilder[] line = new StringBuilder[6];
        int i = 0;
        foreach (var card in playerHand)
        {
            line[0].Append($" {++i}  ");
            line[1].Append("=== ");
            line[2].Append($"|{card.Top}| ");
            line[3].Append($"|=| ");
            line[4].Append($"|{card.Bottom}| ");
            line[5].Append("=== ");
        }

        foreach(var str in line)
            Console.WriteLine(str.ToString());
        
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void OnRoundEnded()
    {
        
    }
}

