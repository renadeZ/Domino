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
    private int _passCount;

    public IGameDTO DominoGameDto;
    public event EventHandler TurnCompleted;
    public event EventHandler<GameEventArgs> RoundEnded; //Winner
    // public event EventHandler ScoreUpdated; //Player, ScoreChange
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
        
        //Setup Hand
        foreach (IPlayer player in _players)
            _playerHand.Add(player, new List<IDominoTile>());
    }

    public void StartRound()
    {
        _roundNumber++;
        _passCount = 0;
        
        //Setup deck
        if (_board.Chain.Count != 0)
        {
            _board.Chain.Clear();
        }
        _deck.Tiles.Clear();
        _deck.TotalTiles = 0;
            
        for (int i = 0; i < _deck.MaxPipValue + 1; i++)
        {
            for (int j = i; j < _deck.MaxPipValue + 1; j++)
            {
                _deck.Tiles.Add(new DominoTile(i, j));
                _deck.TotalTiles++;
            }
        }

        //Setup hands
        foreach (IPlayer player in _players)
            _playerHand[player].Clear();
        ShuffleAndDeal();
        
        //Cek Instant Winner
        IPlayer? instantWinner = FindInstantWinner();
        if(instantWinner != null) 
        {
            OnRoundEnded(instantWinner, RoundResult.InstantWin);
        }
        
        //Check Reshuffle
        CheckReShuffle();
        
        //Mencari Pemain Pertama
        _currentPlayerIndex = _players.IndexOf(FindFirstPlayer(_roundNumber==1));
        DominoGameDto = UpdateDto(DominoGameDto);
    }

    public bool MakeMove(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        if (IsGameOver()) return false;

        bool isPlaced = false;

        if (_board.Chain.Count == 0)
        {
            PlaceTile(tile, side);
            isPlaced = true;
        }
        else
        {
            int target = side == PlacementSide.Left ? _board.LeftEnd : _board.RightEnd;
            if (tile.Top == target || tile.Bottom == target) 
            {
                PlaceTile(tile, side);
                isPlaced = true;
            }
        }

        // Hanya hapus kartu dan ganti turn JIKA kartu BENAR-BENAR diletakkan
        if (isPlaced)
        {
            _playerHand[player].Remove(tile);
            OnTurnCompleted();
            return true;
        }

        return false; // Beritahu pemanggil (UI) bahwa langkah gagal
    }
    private void PlaceTile(IDominoTile tile, PlacementSide side)
    {
        if (_board.Chain.Count == 0)
        {
            _board.LeftEnd = tile.Top;
            _board.RightEnd = tile.Bottom;
            _board.Chain.Add(tile);
        }
        else if (side == PlacementSide.Left)
        {
            if (tile.Top == _board.LeftEnd)
            {
                (tile.Top, tile.Bottom) = (tile.Bottom, tile.Top);
            }
            _board.LeftEnd = tile.Top;
            _board.Chain.Insert(0,tile);
        }
        else
        {
            if (tile.Bottom == _board.RightEnd)
            {
                (tile.Top, tile.Bottom) = (tile.Bottom, tile.Top);
            }
            _board.RightEnd = tile.Bottom;
            _board.Chain.Add(tile);
        }
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
            OnTurnCompleted();
        }
        
    }

    public void ApplyTimeOut(IPlayer player)
    {
        _scores[player] += _rules.PenaltyPoints;
        
        OnTurnCompleted();
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
        if (_board.Chain.Count == 0)
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

    public List<IDominoTile> GetUnplayableTiles(IPlayer player)
    {
        var playable = GetPlayableTiles(player);
        return _playerHand[player].Except(playable).ToList();
    }

    public List<PlacementSide> GetValidPlacements(IDominoTile tile)
    {
        List<PlacementSide> validSides = new List<PlacementSide>();
        
        if (_board.Chain.Count == 0)
        {
            validSides.Add(PlacementSide.Left);
            validSides.Add(PlacementSide.Right);
            return validSides;
        }

        if (tile.Top == _board.LeftEnd || tile.Bottom == _board.LeftEnd)
            validSides.Add(PlacementSide.Left);
            
        if (tile.Top == _board.RightEnd || tile.Bottom == _board.RightEnd)
            validSides.Add(PlacementSide.Right);

        return validSides;
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
        for (int i = 0; i < _playerHand[player].Count; i++)
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
                var currentHighest = GetHighestBalak(player);
                var selectedHighest = selectedPlayer != null ? GetHighestBalak(selectedPlayer) : null;
                if (currentHighest != null && (selectedHighest == null || currentHighest.Top > selectedHighest.Top))
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
                    (selectedPlayer != null && HighestPip(player) > HighestPip(selectedPlayer)))
                {
                    selectedPlayer = player;
                }
            }
            if( selectedPlayer != null)
                return selectedPlayer;
        }
        
        return _players[_currentPlayerIndex];
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
                    OnRoundEnded(player, RoundResult.ReShuffle);
                    
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
        int minCount = playerTotalPips.FindAll(p => p == minPip).Count;

        if ( minCount == 1)
            winner = _players[playerTotalPips.FindIndex(p => p == minPip)];
        if ( winner != null)
        {
            OnRoundEnded(winner, RoundResult.Win);
            return;
        }
        
        //Tie Breaker
        //A. Balak tersedikit
        List<int> playerTotalBalak = new List<int>();
        foreach (IPlayer player in _players)
            playerTotalBalak.Add(GetPlayerBalakCount(player));

        int minBalak = playerTotalBalak.Min();
        minCount = playerTotalBalak.FindAll(p => p == minBalak).Count;

        if (minCount == 1)
            winner = _players[playerTotalBalak.FindIndex(p => p == minBalak)];
        if (winner != null)
        {
            OnRoundEnded(winner, RoundResult.Win);
            return;
        }

        //B. Balak terkecil 
        foreach( IPlayer player in _players)
            if (winner == null)
                winner = player;
            else
            {
                var currentSmallest = GetSmallestBalak(player);
                var winnerSmallest = GetSmallestBalak(winner);
                if (currentSmallest != null && winnerSmallest != null && currentSmallest.Top < winnerSmallest.Top)
                    winner = player;
            }
        
        if (winner != null)
        {
            IDominoTile? smallestBalak = GetSmallestBalak(winner);
            if (smallestBalak != null && smallestBalak.Top == 0)
            {
                foreach (IPlayer player in _players)
                {
                    if (player != winner)
                        OnPenaltyApplied(player, _rules.LoseBalak0Penalty);
                }
                OnRoundEnded(winner, RoundResult.DrawWinBalak0);
            }
            OnRoundEnded(winner, RoundResult.Win);
            return;
        }


    }

    // private int GetRoundScore(RoundResult result)
    // {
    //     
    // }
    //
    // private int GetGaplePenalty(IPlayer loser)
    // {
    //     
    // }

    public bool IsGameOver()
    {
        foreach (IPlayer player in _players)
        {
            if (_scores[player] >= _rules.WinningScore)
            {
                OnGameOver();
                return true;
            }
        }

        return false;
    }

    private IGameDTO UpdateDto(IGameDTO dto)
    {
        // if (dto == null) throw new ArgumentNullException(nameof(dto));
        dto = new GameDto(_board, _deck, _rules, _playerHand, _scores, _players, _currentPlayerIndex,
            _roundNumber, _passCount);
        return dto;
    }
    
    public void OnTurnCompleted()
    {

        //Check 0 Card
        foreach (IPlayer player in _players)
        {
            if (_playerHand[player].Count == 0)
            {
                OnRoundEnded(player, RoundResult.Win);
                return;
            }
        }
        
        //If stuck
        bool stuck = true;
        foreach (var player in _players)
            if (GetPlayableTiles(player).Count > 0)
                stuck = false;
        if(stuck)
        {
            HandleGaple();
            return;
        }
        
        NextTurn();
        DominoGameDto = UpdateDto(DominoGameDto);
        TurnCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void OnPenaltyApplied()
    {
        DominoGameDto = UpdateDto(DominoGameDto);
        NextTurn();
        PenaltyApplied?.Invoke(this, EventArgs.Empty);
    }

    public void OnGameOver()
    {
        DominoGameDto = UpdateDto(DominoGameDto);
        GameOver?.Invoke(this, EventArgs.Empty);
    }

    public void OnRoundEnded(IPlayer winner, RoundResult result)
    {
        int score = 0;
        string msg;
        // int score = 0;
        switch (result)
        {
            case RoundResult.Win:
                score += _rules.WinScore;
                break;
            case RoundResult.InstantWin:
                score += _rules.WinScore;
                break;
            case RoundResult.WinBalak6:
                score += _rules.WinBalak6Score;
                break;
            
        }

        _scores[winner] += score;
        
        DominoGameDto = UpdateDto(DominoGameDto); 
        RoundEnded?.Invoke(this, new GameEventArgs(winner, result, score, ""));
    }

    public void OnPenaltyApplied(IPlayer player, int penalty)
    {
        _scores[player] += penalty;
        DominoGameDto = UpdateDto(DominoGameDto);
        PenaltyApplied?.Invoke(this, new GameEventArgs(player, 0, penalty, ""));
    }
}