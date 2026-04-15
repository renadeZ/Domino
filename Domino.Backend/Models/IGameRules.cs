namespace Domino.Backend.Models;

public interface IGameRules
{
    public int WinningScore { get; set; }
    public int TurnTimeLimit { get; set; }
    public int PenaltyPoints { get; set; }
    public int WinScore { get; set; }
    public int WinBalak6Score { get; set; }
    public int WinBalak0Score { get; set; }
    public int LoseBalak0Penalty { get; set; }
    public int TilesPerPlayer { get; set; }
    public int InstantWinBalakCount { get; set; }
    public int ReshuffleMinBalak { get; set; }
}