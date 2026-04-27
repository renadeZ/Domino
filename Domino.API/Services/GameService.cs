using Domino.Backend.Enums;
using Domino.API.DTOs;
using System.Linq;
using System.Collections.Generic;

namespace Domino.API.Services;

public interface IGameService
{
    ServiceResult<string> StartGame();
    ServiceResult<string> StartRound();
    ServiceResult<GameStateResponse> GetState();
    ServiceResult<string> MakeMove(string playerName, int tileTop, int tileBottom, PlacementSide side);
    ServiceResult<string> Pass(string playerName);
    ServiceResult<string> TimeOut(string playerName);
    ServiceResult<List<TileResponse>> GetPlayableTiles(string playerName);
}

public class GameService : IGameService
{
    private readonly Domino.Backend.GameController _gameController;
    private string _lastWinner = "";
    private string _lastMessage = "";
    private int _lastScore = 0;
    private bool _isRoundOver = false;

    public GameService(Domino.Backend.GameController gameController)
    {
        _gameController = gameController;
        _gameController.RoundEnded += (sender, args) => 
        {
            _lastWinner = args.Player.Name;
            _lastMessage = args.Message;
            _lastScore = args.ScoreChange;
            _isRoundOver = true;
        };
    }

    public ServiceResult<string> StartGame()
    {
        bool started = _gameController.StartGame();
        if (started) 
        {
            _isRoundOver = false;
            return ServiceResult<string>.Success("Game started successfully.");
        }
        return ServiceResult<string>.Failure("Could not start game. Ensure at least 2 players.");
    }

    public ServiceResult<string> StartRound()
    {
        bool started = _gameController.StartRound();
        if (started) 
        {
            _isRoundOver = false;
            return ServiceResult<string>.Success("Round started successfully.");
        }
        return ServiceResult<string>.Failure("Could not start round.");
    }

    public ServiceResult<GameStateResponse> GetState()
    {
        var dto = _gameController.UpdateDto();
        
        var response = new GameStateResponse
        {
            RoundNumber = dto.RoundNumber,
            CurrentPlayer = dto.Players[dto.CurrentPlayerIndex].Name,
            Board = new BoardStateResponse
            {
                LeftEnd = dto.Board.LeftEnd,
                RightEnd = dto.Board.RightEnd,
                Chain = dto.Board.Chain.Select(tile => new TileResponse { Top = tile.Top, Bottom = tile.Bottom }).ToList()
            },
            Scores = dto.Scores.ToDictionary(score => score.Key.Name, score => score.Value),
            Players = dto.Players.Select(player => new PlayerStateResponse
            {
                Name = player.Name,
                HandCount = dto.PlayerHands[player].Count,
                PlayableHand = _gameController.GetPlayableTiles(player).Select(t => new TileResponse { Top = t.Top, Bottom = t.Bottom }).ToList(),
                UnplayableHand = _gameController.GetUnplayableTiles(player).Select(t => new TileResponse { Top = t.Top, Bottom = t.Bottom }).ToList()
            }).ToList(),
            IsGameOver = _gameController.IsGameOver(),
            IsRoundOver = _isRoundOver,
            LastRoundWinnerName = _lastWinner,
            LastRoundMessage = _lastMessage,
            LastRoundScore = _lastScore
        };

        return ServiceResult<GameStateResponse>.Success(response);
    }

    public ServiceResult<string> MakeMove(string playerName, int tileTop, int tileBottom, PlacementSide side)
    {
        var dto = _gameController.UpdateDto();
        var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
        if (player == null) return ServiceResult<string>.Failure($"Player '{playerName}' not found.");

        if (dto.Players[dto.CurrentPlayerIndex] != player)
        {
            return ServiceResult<string>.Failure($"It is not {player.Name}'s turn. Current turn: {dto.Players[dto.CurrentPlayerIndex].Name}");
        }

        var tile = dto.PlayerHands[player].FirstOrDefault(t => 
            (t.Top == tileTop && t.Bottom == tileBottom) || 
            (t.Top == tileBottom && t.Bottom == tileTop));
            
        if (tile == null) return ServiceResult<string>.Failure("Tile not found in player's hand.");

        var validSides = _gameController.GetValidPlacements(tile);
        if (!validSides.Contains(side))
        {
            return ServiceResult<string>.Failure($"Invalid placement side. Valid sides for this tile: {string.Join(", ", validSides)}");
        }

        bool result = _gameController.MakeMove(player, tile, side);
        if (result)
        {
            return ServiceResult<string>.Success("Move successful.");
        }
        return ServiceResult<string>.Failure("Invalid move.");
    }

    public ServiceResult<string> Pass(string playerName)
    {
        var dto = _gameController.UpdateDto();
        var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
        if (player == null) return ServiceResult<string>.Failure($"Player '{playerName}' not found.");

        if (dto.Players[dto.CurrentPlayerIndex] != player)
        {
            return ServiceResult<string>.Failure($"It is not {player.Name}'s turn.");
        }

        var playable = _gameController.GetPlayableTiles(player);
        if (playable.Any())
        {
            return ServiceResult<string>.Failure("Cannot pass. You have playable tiles.");
        }

        _gameController.Pass(player);
        return ServiceResult<string>.Success("Turn passed.");
    }

    public ServiceResult<string> TimeOut(string playerName)
    {
        var dto = _gameController.UpdateDto();
        var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
        if (player == null) return ServiceResult<string>.Failure($"Player '{playerName}' not found.");

        if (dto.Players[dto.CurrentPlayerIndex] != player)
        {
            return ServiceResult<string>.Failure($"It is not {player.Name}'s turn.");
        }

        _gameController.ApplyTimeOut(player);
        return ServiceResult<string>.Success("Time out applied.");
    }

    public ServiceResult<List<TileResponse>> GetPlayableTiles(string playerName)
    {
        var dto = _gameController.UpdateDto();
        var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
        if (player == null) return ServiceResult<List<TileResponse>>.Failure($"Player '{playerName}' not found.");

        var playable = _gameController.GetPlayableTiles(player);
        var result = playable.Select(t => new TileResponse { Top = t.Top, Bottom = t.Bottom }).ToList();
        return ServiceResult<List<TileResponse>>.Success(result);
    }
}