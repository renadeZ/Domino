using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

public class GameCLI
{
    private GameController _gameController;
    private const int BOARD_WIDTH = 40;
    private const int BOARD_HEIGHT = 70;
    
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
        
    }


    public void DrawerArena()
    {
        
    }
}
