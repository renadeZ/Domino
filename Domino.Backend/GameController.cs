using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

namespace Domino.Backend;

public class GameController
{
    private IBoard _board;
    private IDeck _deck;
    private IGameRules _rules;

    private Dictionary<IPlayer, List<IDominoTile>> _playerHand;
    private Dictionary<IPlayer, int> _scores;
    private List<IPlayer> _players;

    private int _currentPlayerIndex;
    private int _roundNumber;
    private int _consecutivePasses;

    public event EventHandler<GameEventArgs>? OnTurnCompleted;
    public event EventHandler<GameEventArgs>? OnRoundEnded;
    public event EventHandler<GameEventArgs>? OnScoreUpdated;
    public event EventHandler<GameEventArgs>? OnPenaltyApplied;
    public event EventHandler<GameEventArgs>? OnGameOver;

    public GameController(List<IPlayer> players, IBoard board, IDeck deck, IGameRules rules)
    {
        _board = board;
        _deck = deck;
        _rules = rules;
    }

    public void StartGame()
    {
        
    }

    public void StartRound()
    {
        
    }

    public void PlayTile(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        
    }

    public void Pass(IPlayer player)
    {
        
    }

    public void ApplyTimeout(IPlayer player)
    {
        
    }

    private void AdvanceTurn()
    {
        
    }

    private bool MatchesSide(IDominoTile tile, int value)
    {
        
    }
    
    private void PlaceTile(IDominoTile tile, PlacementSide side)
    {
        
    }

    private void PrepareRound()
    {
        
    }

    public List<DominoTile> GetPlayable()
    {
        
    }

    private int GetPlayerTotalPips(IPlayer player)
    {
        
    }

    private int GetPlayerBalakCount(IPlayer player)
    {
        return _playerHand[player].Count(tile => tile.Top == tile.Bottom);
    }

    private IDominoTile GetSmallestBalak(IPlayer player)
    {
        return _playerHand[player].MinBy(tile => tile.Top == tile.Bottom);
    }

    private IDominoTile GetHighestBalak(IPlayer player)
    {
        return _playerHand[player].MaxBy(tile => tile.Top == tile.Bottom);
    }

    private IPlayer FindFirstPlayer(bool isFirstRound)
    {
        
    }

    private void CheckReShuffle()
    {
        
    }

    private IPlayer? FindInstantWinner()
    {
        
    }

    private void HandleGaple()
    {
        
    }

    private int GetRoundScore(RoundResult result)
    {
        
    }

    private int GetGaplePenalty(IPlayer loser)
    {
        
    }

    public bool IsGameOver()
    {
        
    }
}