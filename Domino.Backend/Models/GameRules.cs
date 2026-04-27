using Domino.Backend.Interfaces;
namespace Domino.Backend.Models;

public class GameRules : IGameRules
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

    public GameRules(int winningScore, int turnTimeLimit, int penaltyPoints, int winScore, int winBalak6Score,
        int winBalak0Score, int loseBalak0Penalty, int tilesPerPlayer, int instantWinBalakCount, int reshuffleMinBalak)
    {
        WinningScore = winningScore;
        TurnTimeLimit = turnTimeLimit;
        PenaltyPoints = penaltyPoints;
        WinScore = winScore;
        WinBalak6Score = winBalak6Score;
        WinBalak0Score = winBalak0Score;
        LoseBalak0Penalty = loseBalak0Penalty;
        TilesPerPlayer = tilesPerPlayer;
        InstantWinBalakCount = instantWinBalakCount;
        ReshuffleMinBalak = reshuffleMinBalak;
    }
}