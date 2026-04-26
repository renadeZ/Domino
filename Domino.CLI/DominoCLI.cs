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
    private bool _isFirstTurn;
    private bool _hasTimedOut;
    private IGameDTO? _dto;
    
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
        finally
        {
            _gameController.RoundEnded -= OnRoundEnded;
            _gameController.TurnCompleted -= OnTurnCompleted;
            _gameController.GameOver -= OnGameOver;
        }
    }
    private void RoundStart()
    {
        _isRoundActive = true;
        _isFirstTurn = true;
        _gameController.StartRound();
        RefreshDto();
        
        while (_isRoundActive)
        {
            _hasTimedOut = false;
            DrawArena();
            PlayerMove();
            _isFirstTurn = false;
        }
        
    }

    private void ConsoleSetup()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.CursorVisible = false;
    }

    private void RefreshDto()
    {
        _dto = _gameController.UpdateDTO();
    }



    private void DrawArena()
    {
        ConsoleSetup();
        var currentPlayer = _dto.Players[_dto.CurrentPlayerIndex];
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                                                             DOMINO                                                                                                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Current Round : {_dto.RoundNumber}");
        Console.WriteLine($"Current Turn  : {currentPlayer.Name}\n");
        Console.WriteLine("Player list :");
        foreach (var player in _dto.PlayerHands)
            Console.WriteLine($"{player.Key.Name} : {_dto.Scores[player.Key]} Point with {player.Value.Count()} Cards Remaining");
        
        Console.WriteLine("\n========================================================================================================== BOARD ================================================================================================================");
        DrawBoardCard();
        Console.WriteLine(  "======================================================================================================== YOUR CARD ==============================================================================================================");
        Console.WriteLine("Playable Card : ");
        DrawPlayerCard(_gameController.GetPlayableTiles(currentPlayer), true);
        Console.WriteLine("Unplayable Card : ");
        List<IDominoTile> unplayable = _gameController.GetUnplayableTiles(currentPlayer);
        DrawPlayerCard(unplayable, false);
        
        Console.WriteLine($"Press 1-{_gameController.GetPlayableTiles(currentPlayer).Count} To Choose Tiles, Press 0 to Pass\n\n");
    }

    private void DrawBoardCard()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        StringBuilder[] line = new StringBuilder[5];
        for (int i = 0; i < line.Length; i++)
        {
            line[i] = new StringBuilder();
        }

        if (_dto.Board.Chain.Count > 0)
        {
            foreach (var tile in _dto.Board.Chain)
            {
                if (tile.Top == tile.Bottom)
                {
                    line[0].Append("┌────┐ ");
                    line[1].Append($"│ {tile.Top}  │ ");
                    line[2].Append($"├────┤ ");
                    line[3].Append($"│ {tile.Bottom}  │ ");
                    line[4].Append("└────┘ ");
                }
                else
                {
                    line[0].Append("          ");
                    line[1].Append($"┌───────┐ ");
                    line[2].Append($"│ {tile.Top} | {tile.Bottom} │ ");
                    line[3].Append("└───────┘ ");
                    line[4].Append("          ");
                }
            }

            foreach(var str in line)
                Console.WriteLine(str.ToString());
        }
        Debug.WriteLine(_dto.Board.Chain.ToString());
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
        IPlayer currentPlayer = _dto.Players[_dto.CurrentPlayerIndex];
        List<IDominoTile> playable = _gameController.GetPlayableTiles(currentPlayer);
        
        ConsoleKeyInfo? playerInput = PlayerInput();
        if (playerInput != null && playerInput.Value.KeyChar == '0')
        {
            Console.WriteLine("You Passed Turn");
            _gameController.Pass(currentPlayer);
        }
        else if (playerInput != null)
        {
            IDominoTile selected = playable[playerInput.Value.KeyChar - '1']; 

            var validSides = _gameController.GetValidPlacements(selected);

            if ( _isFirstTurn || validSides.Count == 1 )
            {
                _gameController.MakeMove(currentPlayer, selected, validSides[0]);
            }
            else if (validSides.Count == 2) 
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
        int playableCount = _gameController.GetPlayableTiles(_dto.Players[_dto.CurrentPlayerIndex]).Count();
        ConsoleKeyInfo? input = WaitInput(playableCount);
        
        return input;
    }

    private ConsoleKeyInfo? WaitInput(int validPlayCount)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        while (timer.Elapsed.TotalSeconds < _dto.Rules.TurnTimeLimit)
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
            
            int timeLeft = _dto.Rules.TurnTimeLimit - (int)timer.Elapsed.TotalSeconds;
            if (timeLeft <= 5)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Yellow;

            // Bikin progress bar sederhana
            string bar = new string('█', timeLeft) + new string('-', _dto.Rules.TurnTimeLimit - timeLeft);


            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Time Limit : [{bar}] {timeLeft}s remaining   \r");
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
        RefreshDto();
        Console.WriteLine($"Next turn : {_dto.Players[_dto.CurrentPlayerIndex].Name}");
        Console.WriteLine("Press ENTER to continue...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
    }

    private void OnRoundEnded(object? sender, GameEventArgs e)
    {
        ConsoleSetup();
        _isRoundActive = false;
        RefreshDto();
        Console.WriteLine("============ ROUND ENDED ===============");
        if (e.Result == RoundResult.ReShuffle)
        {
            Console.WriteLine($"Reshuffle, {e.Player.Name} has {_dto.PlayerHands[e.Player].Count()} cards");
        }
        else
        {
            Console.WriteLine("Board :");
            DrawBoardCard();
            foreach (IPlayer player in _dto.Players)
            {
                Console.WriteLine($"{player.Name}'s Card :");
                DrawPlayerCard(_dto.PlayerHands[player], true);
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Round Winner : {e.Player.Name}");
            Console.WriteLine($"Win By       : {e.Message} (+{e.ScoreChange} point)");
        }
        // Console.WriteLine($"Player List  :");
        // foreach (var player in _dto.Scores)
        // {
        //     Console.WriteLine($"  {player.Key.Name} : {player.Value} Point");
        // }

        Console.WriteLine("┌─────────────────┬───────┬────────────┐");
        Console.WriteLine("│ Player name     │ Score │  Card Left │");
        Console.WriteLine("├─────────────────┼───────┼────────────┤");

        foreach (var player in _dto.Players)
        {
            // -15 berarti rata kiri dengan lebar 15 karakter. 5 berarti rata kanan lebar 5.
            Console.WriteLine($"│ {player.Name,-15} │ {_dto.Scores[player],5} │ {_dto.PlayerHands[player].Count,10} │");
        }
        Console.WriteLine("└─────────────────┴───────┴────────────┘");
        Console.WriteLine("Press ENTER to continue...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

    }

    public void OnGameOver(object? sender, GameEventArgs e)
    {
        ConsoleSetup();
        RefreshDto();
        Console.WriteLine("\n========================= Game Over =========================");
        Console.WriteLine($"Game Winner : {e.Player.Name}");
        Console.WriteLine("Final Score  :");
        var sortedScores = _dto.Scores.OrderByDescending(x => x.Value);
        foreach (var score in sortedScores)
        {
            Console.WriteLine($"  {score.Key.Name} : {score.Value} Point");
        }
    }

}