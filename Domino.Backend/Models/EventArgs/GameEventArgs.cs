using Domino.Backend.Models.Enums;

namespace Domino.Backend.Models.EventArgs;

public class GameEventArgs : System.EventArgs
{
    public Player Player { get; set; }
    public RoundResult Result{ get; set; }
    public int ScoreChange { get; set;  }
    public string Message { get; set; }

    public GameEventArgs(Player player, RoundResult result, int scoreChange, string message)
    {
        Player = player;
        Result = result;
        ScoreChange = scoreChange;
        Message = message;
    }
}