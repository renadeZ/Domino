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

    private int _currentPlayerIndex;
    private int _roundNumber;
    private int _passCount;

    public IGameDTO DominoGameDto;
    public event EventHandler TurnCompleted;
    public event EventHandler RoundEnded;
    public event EventHandler ScoreUpdated;
    public event EventHandler PenaltyApplied;
    public event EventHandler GameOver;
    
    public GameController(List<IPlayer> players, IBoard board, IDeck deck, IGameRules rules)
    {
        _players = players;
        _board = board;
        _deck = deck;
        _rules = rules;
        
        _playerHand = new Dictionary<IPlayer, List<IDominoTile>>();
        _scores = new Dictionary<IPlayer, int>();
    }
    
    //DONE
    public void StartGame()
    {
        //Setup skor
        foreach (IPlayer player in _players)
            _scores.Add(player, 0);
        //Mengatur round
        _roundNumber = 0;
    }

    public void StartRound()
    {
        _roundNumber++;
        _passCount = 0;
        
        //Setup deck
        
        
        //Setup hands
        foreach (IPlayer player in _players)
            _playerHand.Add(player, new List<IDominoTile>());
        ShuffleAndDeal();
        
        //Cek Instan Winner
        IPlayer? instantWinner = FindInstantWinner();
        if(instantWinner != null) 
        {
            
        }
        
        //Check Reshuffle
        CheckReShuffle();
        
        //Mencari Pemain Pertama
        _currentPlayerIndex = _players.IndexOf(FindFirstPlayer(_roundNumber==1));
        
    }

    public void MakeMove(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        if (!IsGameOver())
        {
            //Ambil nilai sisi
            int target = side == PlacementSide.Left ? _board.LeftEnd : _board.RightEnd;
            if (tile.Top == target || tile.Bottom == target) PlaceTile(tile, side);
            //Hapus karena sudah ditaruh
            _playerHand[player].Remove(tile);
        }
    }
    private void PlaceTile(IDominoTile tile, PlacementSide side)
    {
        if (side == PlacementSide.Left)
            _board.LeftEnd = (tile.Top == _board.LeftEnd) ? tile.Bottom : tile.Top;
        else
            _board.RightEnd = (tile.Top == _board.RightEnd) ? tile.Bottom : tile.Top;
    }
    
    private void NextTurn()
    {
        
        
        if (_currentPlayerIndex == _players.Count - 1) _currentPlayerIndex = 0;
        else _currentPlayerIndex++;
    }

    public void Pass(IPlayer player)
    {
        if (GetPlayableTiles(player).Count() == 0)
        {
            _passCount++;
            NextTurn();
        }
        
    }

    public void ApplyTimeOut(IPlayer player)
    {
        _scores[player] += _rules.PenaltyPoints;
        
        NextTurn();
    }

    //DONE
    private bool MatchesSide(IDominoTile tile, int value)
    {
        if (tile.Top == value || tile.Bottom == value) return true;
        return false;
    }
    
    //DONE
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
                _playerHand[player].Add(_deck.Tiles[_deck.Tiles.Count-1]);
                _deck.Tiles.RemoveAt(_deck.Tiles.Count-1);
                _deck.TotalTiles--;
            }
        }
    }

    //DONE
    public List<IDominoTile> GetPlayableTiles(IPlayer player)
    {
        if (_deck.Tiles.Count == 0)
            return _playerHand[player];
        
        List<IDominoTile> playable = new List<IDominoTile>();
        foreach (IDominoTile tile in _playerHand[player])
        {
            if (MatchesSide(tile, _board.LeftEnd) || MatchesSide(tile, _board.RightEnd))
            {
                playable.Add(tile);
            }
        }

        return playable;
    }

    //DONE
    private int GetPlayerTotalPips(IPlayer player)
    {
        return _playerHand[player].Sum(tile => tile.Top + tile.Bottom);
    }

    //DONE
    private int GetPlayerBalakCount(IPlayer player)
    {
        return _playerHand[player].Count(tile => tile.Top == tile.Bottom);
    }

    //DONE
    private IDominoTile? GetSmallestBalak(IPlayer player)
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

    //DONE
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

                if (counter >= _rules.ReshuffleMinBalak) //UNDONE : OnRoundEnded
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
    
    //UNDONE : Belom handling event/menyampaikan pemenang
    private void HandleGaple()
    {
        IPlayer? winner = null;
        
        //Titik terkecil
        List<int> playerTotalPips = new List<int>();
        foreach (IPlayer player in _players)
            playerTotalPips.Add(GetPlayerTotalPips(player));   
        
        int minPip = playerTotalPips.Min();
        int minCount = playerTotalPips.FindAll(p => p == minPip).Count();

        if ( minCount == 1)
            winner = _players[playerTotalPips.Find(p => p == minPip)];
        
        //Tie Breaker
        //A. Balak tersedikit
        List<int> playerTotalBalak = new List<int>();
        foreach (IPlayer player in _players)
            playerTotalBalak.Add(GetPlayerBalakCount(player));

        int minBalak = playerTotalBalak.Min();
        minCount = playerTotalBalak.FindAll(p => p == minBalak).Count();

        if (minCount == 1)
            winner = _players[playerTotalBalak.Find((p => p == minBalak))];

        //B. Balak terkecil
        foreach( IPlayer player in _players)
            if (winner == null)
                winner = player;
            else if (GetSmallestBalak(player).Top < GetSmallestBalak(winner).Top)
                winner = player;

    }

    private int GetRoundScore(RoundResult result)
    {
        
    }

    private int GetGaplePenalty(IPlayer loser)
    {
        
    }

    public bool IsGameOver()
    {
        foreach (IPlayer player in _players)
        {
            if (_scores[player] >= 151)
            {
                return true;
            }
        }

        return false;
    }

    private IGameDTO UpdateDTO(IGameDTO dto)
    {
        dto = new GameDTO(_board, _deck, _rules, _playerHand, _scores, _players, _currentPlayerIndex,
            _roundNumber, _passCount);
        return dto;
    }
    
    public void OnTurnCompleted()
    {
        DominoGameDto = UpdateDTO(DominoGameDto);
        NextTurn();
        TurnCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void OnPenaltyApplied()
    {
        DominoGameDto = UpdateDTO(DominoGameDto);
        NextTurn();
        PenaltyApplied?.Invoke(this, EventArgs.Empty);
    }

    public void OnGameOver()
    {
        DominoGameDto = UpdateDTO(DominoGameDto);
        NextTurn();
        GameOver?.Invoke(this, EventArgs.Empty);
    }
}
