using Domino.Backend.Models;
using Domino.Backend.Models.Enums;
using Domino.Backend.Models.EventArgs;

namespace Domino.Backend;

public class GameController : IGameController
{
    private IBoard _board;
    private IDeck _deck;
    private IGameRules _rules;

    private Dictionary<IPlayer, List<IDominoTile>> _playerHand;
    private Dictionary<IPlayer, int> _scores;
    private List<IPlayer> _players;

    public int CurrentPlayerIndex { get; private set; }
    private int _roundNumber;
    private int _consecutivePasses;

    public event EventHandler<GameEventArgs>? OnTurnCompleted;
    public event EventHandler<GameEventArgs>? OnRoundEnded;
    public event EventHandler<GameEventArgs>? OnScoreUpdated;
    public event EventHandler<GameEventArgs>? OnPenaltyApplied;
    public event EventHandler<GameEventArgs> OnGameOver;

    public GameController(List<IPlayer> players, IBoard board, IDeck deck, IGameRules rules)
    {
        _players = players;
        _board = board;
        _deck = deck;
        _rules = rules;
    }

    public void StartGame()
    {
        //Setup skor dan hands
        foreach (IPlayer player in _players)
        {
            _playerHand.Add(player, new List<IDominoTile>());
            _scores.Add(player, 0);
        }
        
        //Mengatur round
        _roundNumber = 1;
        
        //Membagikan tile ke player
        ShuffleAndDeal();
        
        //Cek Instan Winner
        if(FindInstantWinner() != null) 
        {
            
        }
        
        //Check Reshuffle
        CheckReShuffle();
        
        //Mencari Pemain Pertama
        CurrentPlayerIndex = _players.IndexOf(FindFirstPlayer(true));
        
        StartRound();
    }

    public void StartRound() //PlayerTurn
    {
  
    }
    private void NextTurn()
    {
        
    }

    public void MakeMove(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        if (!IsGameOver())
        {
            
        }
    }

    public void Pass(IPlayer player)
    {
        
    }

    public void ApplyTimeOut(IPlayer player)
    {
        
    }


    private bool MatchesSide(IDominoTile tile, int value)
    {
        
    }
    
    private void PlaceTile(IDominoTile tile, PlacementSide side)
    {
        
    }

    private void ShuffleAndDeal()
    {
        //Shuffle
        var tiles = _deck.Tiles.ToArray();
        Random.Shared.Shuffle(tiles);
        _deck.Tiles = tiles.ToList();
        
        //Bagi kartu
        for (int i = 0; i < _rules.TilesPerPlayer; i++)
        {
            foreach (IPlayer player in _players)
            {
                _playerHand[player].Add(_deck.Tiles[_deck.Tiles.Count]);
                _deck.Tiles.RemoveAt(_deck.Tiles.Count);
                _deck.TotalTiles--;
            }
        }
    }

    public List<IDominoTile> GetPlayableTiles(IPlayer player)
    {
        if (_deck.Tiles.Count == 0)
            return _playerHand[player];
        
        List<IDominoTile> playable = new List<IDominoTile>();
        foreach (IDominoTile tile in _playerHand[player])
        {
            if (tile.Top == _board.LeftEnd || tile.Top == _board.RightEnd || 
                tile.Bottom == _board.LeftEnd || tile.Bottom == _board.RightEnd)
            {
                playable.Add(tile);
            }
        }

        return playable;
    }

    private int GetPlayerTotalPips(IPlayer player)
    {
        int counter = 0;
        foreach (IDominoTile tile in _playerHand[player])
        {
            counter += tile.Top + tile.Bottom;
        }

        return counter;
    }

    private int GetPlayerBalakCount(IPlayer player)
    {
        int counter = 0;
        foreach (IDominoTile tile in _playerHand[player])
        {
            if (tile.Top == tile.Bottom)
                counter++;
        }
        
        return counter;
    }

    private IDominoTile GetSmallestBalak(IPlayer player)
    {
        int min = 12;
        for (int i = 0; i < _playerHand[player].Count(); i++)
        {
            if (_playerHand[player][i].Top == _playerHand[player][i].Bottom)
                if (min == 12 || _playerHand[player][i].Top < _playerHand[player][min].Top)
                    min = i;
        }

        if (min == 12)
            return null;
        else
            return _playerHand[player][min];
    }

    private IDominoTile? GetHighestBalak(IPlayer player)
    {
        int max = -1;
        for (int i = 0; i < _playerHand[player].Count(); i++)
        {
            if (_playerHand[player][i].Top == _playerHand[player][i].Bottom)
                if (max == -1 || _playerHand[player][i].Top < _playerHand[player][max].Top)
                    max = i;
        }

        if (max == -1)
            return null;
        else
            return _playerHand[player][max];
    }

    private IPlayer FindFirstPlayer(bool isFirstRound)
    {
        if (isFirstRound == true)
        {
            //Balak Tertinggi
            IPlayer? selectedPlayer = null;
            foreach (IPlayer player in _players)
            {
                if ((selectedPlayer == null && GetHighestBalak(player) != null) ||
                    GetHighestBalak(player).Top > GetHighestBalak(selectedPlayer).Top)
                {
                    selectedPlayer = player;
                }
            }

            if (selectedPlayer != null) return selectedPlayer;
            
            //Pips tertinggi
            int HighestPip(IPlayer player)
            {
                int highest = 0;
                foreach (IDominoTile tile in _playerHand[player])
                {
                    int currentTotal = tile.Bottom + tile.Top;
                    if (currentTotal > highest) highest = currentTotal;
                }
                return highest;
            }

            foreach (IPlayer player in _players)
            {
                if ((selectedPlayer == null && HighestPip(player) != 0) ||
                    HighestPip(player) > HighestPip(selectedPlayer))
                {
                    selectedPlayer = player;
                }
            }

            return selectedPlayer;
        }
    }

    private void CheckReShuffle()
    {
        if (FindInstantWinner() == null)
        {
            foreach (IPlayer player in _players)
            {
                int counter = 0;
                foreach (IDominoTile tile in _playerHand[player])
                {
                    if (tile.Top == tile.Bottom)
                        counter++;
                }

                if (counter >= _rules.ReshuffleMinBalak)
                    OnGameOver?.Invoke(this, new GameEventArgs(player, RoundResult.ReShuffle, 0, "Game Restart"));
            }
        }
    }

    private IPlayer? FindInstantWinner()
    {
        foreach (IPlayer player in _players)
        {
            int counter = 0;
            foreach (IDominoTile tile in _playerHand[player])
            {
                if (tile.Top == tile.Bottom)
                    counter++;
            }
            if (counter == _rules.InstantWinBalakCount)
                return player;
        }

        return null;
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