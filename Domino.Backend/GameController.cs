using System.Diagnostics;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

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

    public event EventHandler? TurnCompleted;
    public event EventHandler<GameEventArgs>? RoundEnded; 
    public event EventHandler<GameEventArgs>? GameOver;

    public GameController(List<IPlayer> players, IBoard board, IDeck deck, IGameRules rules)
    {
        _players = players;
        _board = board;
        _deck = deck;
        _rules = rules;
        
        _playerHand = new Dictionary<IPlayer, List<IDominoTile>>();
        _scores = new Dictionary<IPlayer, int>();
    }
    
    
    public bool StartGame()
    {
        bool isStarted = _players.Count > 1;
        //Setup skor
        foreach (IPlayer player in _players)
        {
            _scores[player] = 0;
        }
        
        //Mengatur round
        _roundNumber = 0;
        
        //Setup Hand
        foreach (IPlayer player in _players)
        {
            _playerHand[player] = new List<IDominoTile>();
        }
        return isStarted;
    }

    public bool StartRound()
    {
        bool started = false;
        _roundNumber++;
        
        //Setup deck
        ResetDeck();
        
        //Setup hands
        foreach (IPlayer player in _players)
        {
            _playerHand[player].Clear();
        }
        ShuffleAndDeal();

        //Cek Instant Winner
        IPlayer? instantWinner = FindInstantWinner();
        if (instantWinner != null)
        {
            OnRoundEnded(instantWinner, RoundResult.InstantWin);
            return true;
        }
        
        //Check Reshuffle
        CheckReShuffle();
        
        //Mencari Pemain Pertama
        _currentPlayerIndex = _players.IndexOf(FindFirstPlayer(_roundNumber==1));

        started = true;
        return started;
    }

    private bool ResetDeck()
    {
        bool isDone = false;
        if (_deck.Tiles.Count != _deck.TotalTiles)
        {
            // Take from board
            if (_board.Chain.Count != 0)
            {
                _deck.Tiles.AddRange(_board.Chain);
                _board.Chain.Clear();

                if (_board.Chain.Count == 0)
                {
                    _board.RightEnd = 0;
                    _board.LeftEnd = 0;
                }
            }
            // Take from player
            foreach (IPlayer player in _players)
            {
                _deck.Tiles.AddRange(_playerHand[player]);
                _playerHand[player].Clear();
            }

            isDone = true;
        }

        return isDone;
    }
    

    public bool MakeMove(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        bool isValid = false;
        if (!IsGameOver())
        {
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

            
            if (isPlaced)
            {
                _playerHand[player].Remove(tile);
                if (_playerHand[player].Count == 0)
                {
                    if (tile.Top == 6 && tile.Bottom == 6)
                        OnRoundEnded(player, RoundResult.WinBalak6);
                    else
                        OnRoundEnded(player, RoundResult.Win);
                    isValid = true;
                }
                else
                {
                    OnTurnCompleted();
                    isValid = true;
                }
            }
        }

        return isValid;
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
    
    private IPlayer NextTurn()
    {
        if (_currentPlayerIndex == _players.Count - 1) 
        {
            _currentPlayerIndex = 0;
        }
        else 
        {
            _currentPlayerIndex++;
        }
        return _players[_currentPlayerIndex];
    }

    public void Pass(IPlayer player)
    {
        Debug.WriteLine($"{player.Name} passed");
        if (GetPlayableTiles(player).Count == 0)
        {
            OnTurnCompleted();
        }
        
    }

    public void ApplyTimeOut(IPlayer player)
    {
        Debug.WriteLine($"{player.Name} timed out");
        _scores[player] += _rules.PenaltyPoints;
        
        OnTurnCompleted();
    }

    //DONE
    private bool MatchesSide(IDominoTile tile, int value)
    {
        bool match = tile.Top == value || tile.Bottom == value;
        return match;
    }
    
    //DONE
    private void ShuffleAndDeal()
    {
        //Shuffle
        IDominoTile[] tiles = _deck.Tiles.ToArray();
        Random.Shared.Shuffle(tiles);
        _deck.Tiles = tiles.ToList();
        
        //Bagi kartu
        for (int i = 0; i < _rules.TilesPerPlayer; i++)
        {
            foreach (IPlayer player in _players)
            {
                _playerHand[player].Add(_deck.Tiles[_deck.Tiles.Count-1]);
                _deck.Tiles.RemoveAt(_deck.Tiles.Count-1);
            }
        }
    }

    //DONE
    public List<IDominoTile> GetPlayableTiles(IPlayer player)
    {
        if (_board.Chain.Count == 0)
        {
            return _playerHand[player];
        }
        
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
        List<IDominoTile> playable = GetPlayableTiles(player);
        List<IDominoTile> unplayable = _playerHand[player].Except(playable).ToList();
        return unplayable;
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

    private int GetPlayerTotalPips(IPlayer player)
    {
        int totalPips = _playerHand[player].Sum(tile => tile.Top + tile.Bottom);
        return totalPips;
    }

    private int GetPlayerBalakCount(IPlayer player)
    {
        int totalBalak = _playerHand[player].Count(tile => tile.Top == tile.Bottom);
        return totalBalak;
    }

    private IDominoTile? GetSmallestBalak(IPlayer player)
    {
        int minIndex = -1;
        for (int i = 0; i < _playerHand[player].Count; i++)
        {
            if (_playerHand[player][i].Top == _playerHand[player][i].Bottom)
            {
                if (minIndex == -1 || _playerHand[player][i].Top < _playerHand[player][minIndex].Top)
                {
                    minIndex = i;
                }
            }
        }

        return minIndex == -1 ? null : _playerHand[player][minIndex];
    }

    private IDominoTile? GetHighestBalak(IPlayer player)
    {
        int maxIndex = -1;
        for (int i = 0; i < _playerHand[player].Count; i++)
        {
            if (_playerHand[player][i].Top == _playerHand[player][i].Bottom)
            {
                if (maxIndex == -1 || _playerHand[player][i].Top > _playerHand[player][maxIndex].Top)
                {
                    maxIndex = i;
                }
            }
        }

        return maxIndex == -1 ? null : _playerHand[player][maxIndex];
    }

    private IPlayer FindFirstPlayer(bool isFirstRound)
    {
        if (isFirstRound)
        {
            //Balak Tertinggi
            IPlayer? selectedPlayer = null;
            foreach (IPlayer player in _players)
            {
                IDominoTile? highestBalak = GetHighestBalak(player);
                if (highestBalak != null)
                {
                    if (selectedPlayer == null)
                    {
                        selectedPlayer = player;
                    }
                    else
                    {
                        IDominoTile? currentBestBalak = GetHighestBalak(selectedPlayer);
                        if (currentBestBalak != null && highestBalak.Top > currentBestBalak.Top)
                        {
                            selectedPlayer = player;
                        }
                    }
                }
            }

            if (selectedPlayer != null)
            {
                return selectedPlayer;
            }
            
            //Pips tertinggi
            int HighestPip(IPlayer player)
            {
                int highest = 0;
                foreach (IDominoTile tile in _playerHand[player])
                {
                    int currentTotal = tile.Bottom + tile.Top;
                    if (currentTotal > highest) 
                    {
                        highest = currentTotal;
                    }
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
            {
                return selectedPlayer;
            }
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

                if (counter >= _rules.ReshuffleMinBalak)
                {
                    OnRoundEnded(player, RoundResult.ReShuffle);
                    return;
                }
                
            }
        }
    }

    private IPlayer? FindInstantWinner()
    {
        IPlayer? winner = null;
        foreach (IPlayer player in _players)
        {
            int counter = 0;
            foreach (IDominoTile tile in _playerHand[player])
            {
                if (tile.Top == tile.Bottom)
                {
                    counter++;
                }
            }
            if (counter == _rules.InstantWinBalakCount)
            {
                winner = player;
            }
        }

        return winner;
    }

    private void HandleGaple()
    {
        IPlayer? winner = null;
        
        //Titik tersedikit
        List<int> playerTotalPips = new List<int>();
        foreach (IPlayer player in _players)
        {
            playerTotalPips.Add(GetPlayerTotalPips(player));   
        }
        int minPip = playerTotalPips.Min();
        int minCount = playerTotalPips.FindAll(p => p == minPip).Count;

        if ( minCount == 1)
        {
            winner = _players[playerTotalPips.FindIndex(p => p == minPip)];
        }
        
        //Tie Breaker
        if (winner == null)
        {
            //A. Balak tersedikit
            List<int> playerTotalBalak = new List<int>();
            foreach (IPlayer player in _players)
            {
                playerTotalBalak.Add(GetPlayerBalakCount(player));
            }
            int minBalak = playerTotalBalak.Min();
            minCount = playerTotalBalak.FindAll(p => p == minBalak).Count;

            if (minCount == 1)
            {
                winner = _players[playerTotalBalak.FindIndex(p => p == minBalak)];
            }
        }

        if (winner == null)
        {
            //B. Balak terkecil 
            foreach (IPlayer player in _players)
            {
                if (winner == null)
                {
                    winner = player;
                }
                else
                {
                    IDominoTile? currentSmallest = GetSmallestBalak(player);
                    IDominoTile? winnerSmallest = GetSmallestBalak(winner);
                    if (currentSmallest != null && winnerSmallest != null && currentSmallest.Top < winnerSmallest.Top)
                    {
                        winner = player;
                    }
                }
            }
        }
        
        // Check Balak 0 Winner and Balak 0 Loser
        if (winner != null)
        {
            bool isThereBalak0 = false;
            
            // Balak 0 
            foreach (IPlayer player in _players)
            {
                if (player != winner)
                {
                    IDominoTile? smallest = GetSmallestBalak(player);
                    if (smallest != null && smallest.Top == 0)
                    {
                        _scores[player] += _rules.LoseBalak0Penalty;
                        isThereBalak0 = true;
                        break;
                    }
                }
            }
            
            // Balak 0 Winner
            if (!isThereBalak0)
            {
                IDominoTile? smallestBalak = GetSmallestBalak(winner);
                if (smallestBalak != null && smallestBalak.Top == 0)
                {
                    OnRoundEnded(winner, RoundResult.DrawWinBalak0);
                }
                else
                {
                    OnRoundEnded(winner, RoundResult.DrawWinNormal);
                }
            }
            else 
                OnRoundEnded(winner, RoundResult.DrawWinNormal);
        }


    }

    public bool IsGameOver()
    {
        bool isGameOver = false;
        foreach (IPlayer player in _players)
        {
            if (_scores[player] >= _rules.WinningScore)
            {
                OnGameOver(player);
                isGameOver = true;
                break;
            }
        }

        return isGameOver;
    }

    public IGameDTO UpdateDTO()
    {
        GameDto dto = new GameDto(_board, _deck, _rules, _playerHand, _scores, _players, _currentPlayerIndex,
            _roundNumber);
        return dto;
    }
    
    private void OnTurnCompleted()
    {
        //If stuck
        bool stuck = true;
        foreach (IPlayer player in _players)
        {
            if (GetPlayableTiles(player).Count > 0)
            {
                stuck = false;
            }
        }
        if(stuck)
        {
            Debug.WriteLine("Stuck Unplayable");
            HandleGaple();
            return;
        }
        Debug.WriteLine($"Turn Completed");
        NextTurn();
        TurnCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnGameOver(IPlayer winner)
    {
        GameOver?.Invoke(this, new GameEventArgs(winner, RoundResult.Win, _scores[winner], ""));
    }

    private void OnRoundEnded(IPlayer winner, RoundResult result)
    {
        int score = 0;
        string msg = "";
        switch (result)
        {
            case RoundResult.Win:
                score += _rules.WinScore;
                msg = "Normal Win";
                break;
            case RoundResult.InstantWin:
                score += _rules.WinScore;
                msg = "Instant Win (7 Balaks)";
                break;
            case RoundResult.WinBalak6:
                score += _rules.WinBalak6Score;
                msg = "Win with Balak 6!";
                break;
            case RoundResult.DrawWinBalak0:
                score += _rules.WinBalak0Score;
                msg = "Gaple Win with Balak 0 remaining!";
                break;
            case RoundResult.DrawWinNormal:
                score += _rules.WinScore;
                msg = "Gaple Win (Lowest Pips or Balak)";
                break;
            case RoundResult.DrawLoseBalak0:
                score += _rules.LoseBalak0Penalty;
                msg = "Gaple Lose with Balak 0";
                break;
            case RoundResult.ReShuffle:
                msg = "Reshuffle due to >= 5 balaks";
                break;
            
        }

        _scores[winner] += score;
        
        RoundEnded?.Invoke(this, new GameEventArgs(winner, result, score, msg));
    }
}