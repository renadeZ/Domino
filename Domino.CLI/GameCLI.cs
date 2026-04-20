using System.Diagnostics;
using System.Text;
using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

namespace Domino.CLI;
public class GameCli
{
    private GameController _gameController;
    // private const int BOARD_WIDTH = 40;
    // private const int BOARD_HEIGHT = 70;
    private bool _roundLoop;
    
    public GameCli(GameController gameController)
    {
        _gameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
        Console.CursorVisible = false;
    }


    public void RunGame()
    {
        try
        {
            ConsoleSetup();

            _gameController.RoundEnded += OnRoundEnded;
            _gameController.StartGame();

            while (!_gameController.IsGameOver())
            {
                RoundStart();
            }

        }
        catch
        {

        }
        finally
        {
            _gameController.RoundEnded -= OnRoundEnded;
        }
    }
    private void RoundStart()
    {
        _gameController.StartRound();

        _roundLoop = true;
        while (_roundLoop)
        {
            DrawArena();
            PlayerMove();
        }
        
    }

    private void ConsoleSetup()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.CursorVisible = false;
    }



    private void DrawArena()
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
        
        foreach (var tile in _gameController.DominoGameDto.Board.Chain)
        {
            line[1].Append($"===== ");
            line[2].Append(($"|{tile.Top}|{tile.Bottom}| "));
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
        
        Console.ForegroundColor = ConsoleColor.Yellow;
    }

    private void PlayerMove()
    {
        IPlayer currentPlayer = _gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex];
        List<IDominoTile> playable = _gameController.GetPlayableTiles(currentPlayer);
        
        ConsoleKeyInfo? playerInput = PlayerInput();
        if (playerInput != null && playerInput.Value.KeyChar == '0')
        {
            Console.WriteLine("You Passed Turn");
        }
        else if (playerInput != null)
        {
            IDominoTile selected = playable[playerInput.Value.KeyChar - '0'];
            Console.WriteLine("You Played");
            if (_gameController.DominoGameDto.Board.Chain.Count() == 0)
            {
                _gameController.MakeMove(currentPlayer, selected, PlacementSide.Left);
            }
            else
            {
                if ((selected.Top == _gameController.DominoGameDto.Board.LeftEnd &&
                    selected.Top == _gameController.DominoGameDto.Board.RightEnd) ||
                    (selected.Bottom == _gameController.DominoGameDto.Board.LeftEnd &&
                    selected.Bottom == _gameController.DominoGameDto.Board.RightEnd))
                {
                    Console.WriteLine("Press 1, to play on Left, 2 to play on Right");
                    playerInput = WaitInput(2);
                    if (playerInput != null && playerInput.Value.KeyChar == 1)
                    {
                        _gameController.MakeMove(currentPlayer, selected, PlacementSide.Left);
                    }
                    else if (playerInput != null && playerInput.Value.KeyChar == 1)
                    {
                        _gameController.MakeMove(currentPlayer, selected, PlacementSide.Right);
                    }
                    else
                    {
                        Console.WriteLine("Timeout, 30s Passed");
                    }
                    
                }
            }
        }
        else
        {
            Console.WriteLine("Timeout, 30s Passed");
        }
        
        
    }
    
    private ConsoleKeyInfo? PlayerInput()
    {
        int playableCount = _gameController.GetPlayableTiles(_gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex]).Count();
        ConsoleKeyInfo? input = WaitInput(playableCount);
        
        return input;
    }

    private ConsoleKeyInfo? WaitInput(int validPlay)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        while (timer.Elapsed.TotalSeconds < _gameController.DominoGameDto.Rules.TurnTimeLimit)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (validPlay != 0)
                {
                    for (int i = 1; i <= validPlay; i++)
                        if (keyInfo.KeyChar == (char)(i + '0'))
                            return keyInfo;
                }
                else if (keyInfo.KeyChar == '0')
                    return keyInfo;
            }

            Thread.Sleep(50);
        }

        return null;
    }

    private void OnTurnCompleted()
    {
        
    }

    private void OnRoundEnded(object? sender, GameEventArgs e)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        
        _roundLoop = false;
        Console.WriteLine("============ ROUND ENDED ===============");
        Console.WriteLine($"Round Winner : {e.Player}");
        Console.WriteLine($"Win By       : {e.Result.ToString()} (+{e.ScoreChange} point)");
        Console.WriteLine($"Player List  :");
        foreach (var player in _gameController.DominoGameDto.Scores)
        {
            Console.WriteLine($"  {player.Key.Name} : {player.Value} Point");
        }
    }
}

