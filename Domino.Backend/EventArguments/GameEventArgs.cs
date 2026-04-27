using Domino.Backend.Enums;
using Domino.Backend.Interfaces;

namespace Domino.Backend.EventArguments;

public class GameEventArgs : System.EventArgs
{
    public IPlayer Player { get; set; }
    public RoundResult Result{ get; set; }
    public int ScoreChange { get; set;  }
    public string Message { get; set; }

    public GameEventArgs(IPlayer player, RoundResult result, int scoreChange, string message)
    {
        Player = player;
        Result = result;
        ScoreChange = scoreChange;
        Message = message;
    }
}