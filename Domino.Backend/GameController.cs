using System.Diagnostics;
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

    public IGameDTO DominoGameDto { get; private set; }
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
    
    //DONE
    public void StartGame()
    {
        //Setup skor
        foreach (IPlayer player in _players)
        {
            _scores.Add(player, 0);
        }
        
        //Mengatur round
        _roundNumber = 0;
        
        //Setup Hand
        foreach (IPlayer player in _players)
        {
            _playerHand.Add(player, new List<IDominoTile>());
        }
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
        {
            _playerHand[player].Clear();
        }
        ShuffleAndDeal();
        
        //[TEST]
        // TestHand(7);

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
        DominoGameDto = UpdateDto();
    }

    private void TestHand(int situation)
    {
        switch (situation)
        {
            case 1: //Normal Win : Run out card
                _board.Chain.Add(new DominoTile(3, 4));
                _board.LeftEnd = 3;
                _board.RightEnd = 4;

                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(3, 3));
                _playerHand[_players[0]].Add(new DominoTile(4, 4));
        
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(2, 2));
                _playerHand[_players[1]].Add(new DominoTile(1, 1));
                break;
        
            case 2: //End game with Balak 6
                _board.Chain.Add(new DominoTile(6, 4));
                _board.LeftEnd = 6;
                _board.RightEnd = 4;
                
                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(6, 6)); 
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(2, 2));
                _playerHand[_players[1]].Add(new DominoTile(3, 3));
                break;
            
            case 3: //Gaple least Pips
                _board.Chain.Add(new DominoTile(3, 4));
                _board.LeftEnd = 3;
                _board.RightEnd = 4;

                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(1, 2));
                _playerHand[_players[0]].Add(new DominoTile(1, 5));
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(5, 6));
                _playerHand[_players[1]].Add(new DominoTile(1, 6));
                break;
            
            case 4 : //Gaple win with Balak 0
                _board.Chain.Add(new DominoTile(3, 4));
                _board.LeftEnd = 3;
                _board.RightEnd = 4;

                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(0, 0));
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(5, 6));
                break;
            
            case 5: //Gaple lose with Balak 0 and least balak
                _board.Chain.Add(new DominoTile(3, 4));
                _board.LeftEnd = 3;
                _board.RightEnd = 4;
                
                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(1, 1));
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(0, 0));
                _playerHand[_players[1]].Add(new DominoTile(2, 2));
                break;
            
            case 6: //Smallest balak
                _board.Chain.Add(new DominoTile(5, 5));
                _board.LeftEnd = 5;
                _board.RightEnd = 5;
                
                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(1, 1)); 
                _playerHand[_players[0]].Add(new DominoTile(2, 2)); 
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(3, 3)); 
                _playerHand[_players[1]].Add(new DominoTile(4, 4)); 
                break;

            case 7: //Instant Win with 7 Balaks
                
                _playerHand[_players[0]].Clear();
                _playerHand[_players[0]].Add(new DominoTile(0, 0)); 
                _playerHand[_players[0]].Add(new DominoTile(1, 1)); 
                _playerHand[_players[0]].Add(new DominoTile(2, 2)); 
                _playerHand[_players[0]].Add(new DominoTile(3, 3)); 
                _playerHand[_players[0]].Add(new DominoTile(4, 4)); 
                _playerHand[_players[0]].Add(new DominoTile(5, 5)); 
                _playerHand[_players[0]].Add(new DominoTile(6, 6)); 
                
                _playerHand[_players[1]].Clear();
                _playerHand[_players[1]].Add(new DominoTile(3, 4)); 
                _playerHand[_players[1]].Add(new DominoTile(4, 5)); 
                break;
        }
    }
    
    public bool MakeMove(IPlayer player, IDominoTile tile, PlacementSide side)
    {
        if (IsGameOver())
        {
            return false;
        }

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
                return true;
            }

            OnTurnCompleted();
            return true;
        }

        return false;
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
        
        
        if (_currentPlayerIndex == _players.Count - 1) 
        {
            _currentPlayerIndex = 0;
            }
        else 
        {
            _currentPlayerIndex++;
        }
    }

    public void Pass(IPlayer player)
    {
        Debug.WriteLine($"{player.Name} passed");
        if (GetPlayableTiles(player).Count == 0)
        {
            _passCount++;
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
        bool match = false;
        if (tile.Top == value || tile.Bottom == value) 
        {
            match = true;
        }
        return match;
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

    //DONE
    private int GetPlayerTotalPips(IPlayer player)
    {
        int totalPips = _playerHand[player].Sum(tile => tile.Top + tile.Bottom);
        return totalPips;
    }

    //DONE
    private int GetPlayerBalakCount(IPlayer player)
    {
        int totalBalak = _playerHand[player].Count(tile => tile.Top == tile.Bottom);
        return totalBalak;
    }

    //DONE
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

    //DONE
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
                    OnRoundEnded(player, RoundResult.ReShuffle);
                    
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
                    var currentSmallest = GetSmallestBalak(player);
                    var winnerSmallest = GetSmallestBalak(winner);
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
            
            // Balak 0 Loser
            foreach (var player in _players)
            {
                if (player != winner)
                {
                    var smallest = GetSmallestBalak(player);
                    if (smallest != null && smallest.Top == 0)
                    {
                        _scores[player] += _rules.LoseBalak0Penalty;
                        isThereBalak0 = true;
                        break;
                    }
                }
            }
            
            // Balak 0 Winner
            if ( !isThereBalak0 )
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

    private IGameDTO UpdateDto()
    {
        var newDto = new GameDto(_board, _deck, _rules, _playerHand, _scores, _players, _currentPlayerIndex,
            _roundNumber, _passCount);
        return newDto;
    }
    
    public void OnTurnCompleted()
    {
        //If stuck
        bool stuck = true;
        foreach (var player in _players)
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
        DominoGameDto = UpdateDto();
        TurnCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void OnGameOver(IPlayer winner)
    {
        DominoGameDto = UpdateDto();
        GameOver?.Invoke(this, new GameEventArgs(winner, RoundResult.Win, _scores[winner], ""));
    }

    public void OnRoundEnded(IPlayer winner, RoundResult result)
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
        
        DominoGameDto = UpdateDto(); 
        RoundEnded?.Invoke(this, new GameEventArgs(winner, result, score, msg));
    }
}