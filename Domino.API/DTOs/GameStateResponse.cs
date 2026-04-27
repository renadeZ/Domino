using System.Collections.Generic;

namespace Domino.API.DTOs;

public class GameStateResponse
{
    public int RoundNumber { get; set; }
    public string CurrentPlayer { get; set; } = string.Empty;
    public BoardStateResponse Board { get; set; } = new();
    public Dictionary<string, int> Scores { get; set; } = new();
    public List<PlayerStateResponse> Players { get; set; } = new();
    public bool IsGameOver { get; set; }
    public bool IsRoundOver { get; set; }
    public string LastRoundWinnerName { get; set; } = string.Empty;
    public string LastRoundMessage { get; set; } = string.Empty;
    public int LastRoundScore { get; set; }
}