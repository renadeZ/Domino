using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

public class GameCLI
{
    private IGameController _gameController;
    private const int BOARD_WIDTH = 40;
    private const int BOARD_HEIGHT = 70;
    
    //Colors


    public GameCLI(IGameController gameController)
    {
        _gameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
        Console.CursorVisible = false;
    }


    public void GameMain()
    {
        try
        {
            ConsoleSetup();
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
}
