using System.Diagnostics;
using System.Text;
using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

namespace Domino.CLI;
public class DominoCli
{
    private GameController _gameController;
    private bool _isRoundActive;
    private bool _hasTimedOut;
    
    public DominoCli(GameController gameController)
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
            _gameController.TurnCompleted += OnTurnCompleted;
            _gameController.GameOver += OnGameOver;
            
            _gameController.StartGame();

            while (!_gameController.IsGameOver())
            {
                RoundStart();
            }

        }
        // catch
        // {
        //
        // }
        finally
        {
            _gameController.RoundEnded -= OnRoundEnded;
        }
    }
    private void RoundStart()
    {
        _isRoundActive = true; 
        _gameController.StartRound();
        
        while (_isRoundActive)
        {
            _hasTimedOut = false;
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
        ConsoleSetup();
        var currentPlayer = _gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex];
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                                                                  DOMINO                                                                                                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Current Round : {_gameController.DominoGameDto.RoundNumber}");
        Console.WriteLine($"Current Turn  : {currentPlayer.Name}\n");
        Console.WriteLine("Player list :");
        foreach (var player in _gameController.DominoGameDto.PlayerHands)
            Console.WriteLine($"{player.Key.Name} : {_gameController.DominoGameDto.Scores[player.Key]} Point with {player.Value.Count()} Cards Remaining");
        
        Console.WriteLine("\n========================================================================================================== BOARD ================================================================================================================");
        DrawBoardCard();
        Console.WriteLine(  "======================================================================================================== YOUR CARD ==============================================================================================================");
        Console.WriteLine("Playable Card : ");
        DrawPlayerCard(_gameController.GetPlayableTiles(currentPlayer), true);
        Console.WriteLine("Unplayable Card : ");
        List<IDominoTile> unplayable = _gameController.GetUnplayableTiles(currentPlayer);
        DrawPlayerCard(unplayable, false);
        
        Console.WriteLine("Press 1-7 To Choose Tiles, Press 0 to Pass\n\n");
        Console.WriteLine("30 Second Remaining");
    }

    private void DrawBoardCard()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        StringBuilder[] line = new StringBuilder[3];
        for (int i = 0; i < line.Length; i++)
        {
            line[i] = new StringBuilder();
        }

        if (_gameController.DominoGameDto.Board.Chain.Count > 0)
        {
            foreach (var tile in _gameController.DominoGameDto.Board.Chain)
            {
                line[0].Append($"┌───────┐ ");
                line[1].Append($"│ {tile.Top} | {tile.Bottom} │ ");
                line[2].Append("└───────┘ ");
            }

            foreach(var str in line)
                Console.WriteLine(str.ToString());
        }
        Debug.WriteLine(_gameController.DominoGameDto.Board.Chain.ToString());
    }

    private void DrawPlayerCard(List<IDominoTile> playerHand, bool isPlayable)
    {
        if(isPlayable)
            Console.ForegroundColor = ConsoleColor.Green;
        else
            Console.ForegroundColor = ConsoleColor.Red;

        StringBuilder[] line = new StringBuilder[6];
        for (int j = 0; j < line.Length; j++)
        {
            line[j] = new StringBuilder();
        }

        int i = 0;
        foreach (var card in playerHand)
        {
            line[0].Append($"  {++i}   ");
            line[1].Append("┌───┐ ");
            line[2].Append($"│ {card.Top} │ ");
            line[3].Append($"├───┤ ");
            line[4].Append($"│ {card.Bottom} │ ");
            line[5].Append("└───┘ ");
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
            _gameController.Pass(currentPlayer);
        }
        else if (playerInput != null)
        {
            IDominoTile selected = playable[playerInput.Value.KeyChar - '1']; // fix index selection 1-7 maps to 0-6

            var validSides = _gameController.GetValidPlacements(selected);

            if (_gameController.DominoGameDto.RoundNumber == 1 || validSides.Count == 1 )
            {
                _gameController.MakeMove(currentPlayer, selected, validSides[0]);
            }
            else if (validSides.Count == 2) // Bisa ditaruh di kiri maupun kanan
            {
                Console.SetCursorPosition(0, Console.CursorTop - 3);
                Console.WriteLine("Press 1, to play on Left, 2 to play on Right\n\n");
                playerInput = WaitInput(2);
                
                if (playerInput != null && playerInput.Value.KeyChar == '1')
                    _gameController.MakeMove(currentPlayer, selected, PlacementSide.Left);
                else if (playerInput != null && playerInput.Value.KeyChar == '2')
                    _gameController.MakeMove(currentPlayer, selected, PlacementSide.Right);
                else
                {
                    Console.WriteLine("Timeout, Time Limit Passed");
                    _gameController.ApplyTimeOut(currentPlayer);
                }
            }
        }
        else
        {
            _hasTimedOut = true;
            _gameController.ApplyTimeOut(currentPlayer);
        }
        
        
    }
    
    private ConsoleKeyInfo? PlayerInput()
    {
        int playableCount = _gameController.GetPlayableTiles(_gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex]).Count();
        ConsoleKeyInfo? input = WaitInput(playableCount);
        
        return input;
    }

    private ConsoleKeyInfo? WaitInput(int validPlayCount)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        while (timer.Elapsed.TotalSeconds < _gameController.DominoGameDto.Rules.TurnTimeLimit)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (validPlayCount != 0)
                {
                    for (int i = 1; i <= validPlayCount; i++)
                        if (keyInfo.KeyChar == (char)(i + '0'))
                            return keyInfo;
                }
                else if (keyInfo.KeyChar == '0')
                    return keyInfo;
            }
            
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("  ");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(_gameController.DominoGameDto.Rules.TurnTimeLimit - (int)timer.Elapsed.TotalSeconds);
            Thread.Sleep(50);
        }

        return null;
    }

    private void OnTurnCompleted(object? sender, EventArgs e)
    {
        ConsoleSetup();
        if (_hasTimedOut)
        {
            Console.WriteLine("Time Limit Passed, You Get Penalty");
        }
        else
        {
            Console.WriteLine("Your turn completed");
        }
        Console.WriteLine($"Next turn : {_gameController.DominoGameDto.Players[_gameController.DominoGameDto.CurrentPlayerIndex].Name}");
        Console.WriteLine("Press ENTER to continue...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
    }

    private void OnRoundEnded(object? sender, GameEventArgs e)
    {
        ConsoleSetup();
        
        _isRoundActive = false;
        Console.WriteLine("============ ROUND ENDED ===============");
        if (e.Result == RoundResult.ReShuffle)
        {
            Console.WriteLine($"Reshuffle, {e.Player.Name} has {_gameController.DominoGameDto.PlayerHands[e.Player].Count()} cards");
        }
        else
        {
            Console.WriteLine("Board :");
            DrawBoardCard();
            foreach (IPlayer player in _gameController.DominoGameDto.Players)
            {
                Console.WriteLine($"{player.Name}'s Card : {_gameController.DominoGameDto.PlayerHands[player].Count()} left");
                DrawPlayerCard(_gameController.DominoGameDto.PlayerHands[player], true);
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Round Winner : {e.Player.Name}");
            Console.WriteLine($"Win By       : {e.Result.ToString()} (+{e.ScoreChange} point)");
        }
        Console.WriteLine($"Player List  :");
        foreach (var player in _gameController.DominoGameDto.Scores)
        {
            Console.WriteLine($"  {player.Key.Name} : {player.Value} Point");
        }
        Console.WriteLine("Press ENTER to continue...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

    }

    public void OnGameOver(object? sender, EventArgs e)
    {
        ConsoleSetup();
        Console.WriteLine("Game Over");
    }
    
}