// using Microsoft.Extensions.Logging;
using Domino.Backend.Enums;
using Domino.API.DTOs;
using Domino.Backend.Interfaces;

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
    private readonly ILogger<GameService> _logger;
    private readonly Domino.Backend.GameController _gameController;
    private string _lastWinner = "";
    private string _lastMessage = "";
    private int _lastScore = 0;
    private bool _isRoundOver = false;

    public GameService(ILogger<GameService> logger, Domino.Backend.GameController gameController)
    {
        _logger = logger;
        _gameController = gameController;
        _gameController.RoundEnded += (sender, args) => 
        {
            _lastWinner = args.Player.Name;
            _lastMessage = args.Message;
            _lastScore = args.ScoreChange;
            _isRoundOver = true;
            _logger.LogInformation("Round ended. Winner: {Winner}, Message: {Message}, Score Change: {ScoreChange}", _lastWinner, _lastMessage, _lastScore);
        };
    }

    public ServiceResult<string> StartGame()
    {
        _logger.LogInformation("Attempting to start game in GameService.");
        
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
        _logger.LogInformation("Attempting to start round in GameService.");
        try
        {
            bool started = _gameController.StartRound();
            if (started) 
            {
                _isRoundOver = false;
                _logger.LogInformation("Round started successfully in GameService.");
                return ServiceResult<string>.Success("Round started successfully.");
            }
            _logger.LogWarning("Could not start round.");
            return ServiceResult<string>.Failure("Could not start round.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting round in GameService.");
            return ServiceResult<string>.Failure("An error occurred while starting the round.");
        }
    }

    public ServiceResult<GameStateResponse> GetState()
    {
        _logger.LogInformation("Attempting to get game state in GameService.");
        try
        {
            IGameDTO dto = _gameController.UpdateDto();
            
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

            _logger.LogInformation("Game state retrieved successfully in GameService.");
            return ServiceResult<GameStateResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game state in GameService.");
            return ServiceResult<GameStateResponse>.Failure("An error occurred while retrieving game state.");
        }
    }

    public ServiceResult<string> MakeMove(string playerName, int tileTop, int tileBottom, PlacementSide side)
    {
        _logger.LogInformation("Attempting to make move for {PlayerName} with tile {Top}-{Bottom} on side {Side}.", playerName, tileTop, tileBottom, side);
        try
        {
            var dto = _gameController.UpdateDto();
            var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
            if (player == null)
            {
                _logger.LogWarning("Player '{PlayerName}' not found.", playerName);
                return ServiceResult<string>.Failure($"Player '{playerName}' not found.");
            }

            if (dto.Players[dto.CurrentPlayerIndex] != player)
            {
                _logger.LogWarning("It is not {PlayerName}'s turn. Current turn: {CurrentPlayer}", player.Name, dto.Players[dto.CurrentPlayerIndex].Name);
                return ServiceResult<string>.Failure($"It is not {player.Name}'s turn. Current turn: {dto.Players[dto.CurrentPlayerIndex].Name}");
            }

            var tile = dto.PlayerHands[player].FirstOrDefault(t => 
                (t.Top == tileTop && t.Bottom == tileBottom) || 
                (t.Top == tileBottom && t.Bottom == tileTop));
                
            if (tile == null)
            {
                _logger.LogWarning("Tile {Top}-{Bottom} not found in {PlayerName}'s hand.", tileTop, tileBottom, player.Name);
                return ServiceResult<string>.Failure("Tile not found in player's hand.");
            }

            var validSides = _gameController.GetValidPlacements(tile);
            if (!validSides.Contains(side))
            {
                _logger.LogWarning("Invalid placement side {Side} for tile {Top}-{Bottom}. Valid sides: {ValidSides}", side, tileTop, tileBottom, string.Join(", ", validSides));
                return ServiceResult<string>.Failure($"Invalid placement side. Valid sides for this tile: {string.Join(", ", validSides)}");
            }

            bool result = _gameController.MakeMove(player, tile, side);
            if (result)
            {
                _logger.LogInformation("Move successful for {PlayerName}.", playerName);
                return ServiceResult<string>.Success("Move successful.");
            }
            
            _logger.LogWarning("Invalid move for {PlayerName}.", playerName);
            return ServiceResult<string>.Failure("Invalid move.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move for {PlayerName} in GameService.", playerName);
            return ServiceResult<string>.Failure("An error occurred while making the move.");
        }
    }

    public ServiceResult<string> Pass(string playerName)
    {
        _logger.LogInformation("Attempting to pass for {PlayerName}.", playerName);
        try
        {
            var dto = _gameController.UpdateDto();
            var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
            if (player == null)
            {
                _logger.LogWarning("Player '{PlayerName}' not found.", playerName);
                return ServiceResult<string>.Failure($"Player '{playerName}' not found.");
            }

            if (dto.Players[dto.CurrentPlayerIndex] != player)
            {
                _logger.LogWarning("It is not {PlayerName}'s turn.", player.Name);
                return ServiceResult<string>.Failure($"It is not {player.Name}'s turn.");
            }

            var playable = _gameController.GetPlayableTiles(player);
            if (playable.Any())
            {
                _logger.LogWarning("{PlayerName} attempted to pass but has playable tiles.", player.Name);
                return ServiceResult<string>.Failure("Cannot pass. You have playable tiles.");
            }

            _gameController.Pass(player);
            _logger.LogInformation("Turn passed for {PlayerName}.", player.Name);
            return ServiceResult<string>.Success("Turn passed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error passing turn for {PlayerName} in GameService.", playerName);
            return ServiceResult<string>.Failure("An error occurred while passing the turn.");
        }
    }

    public ServiceResult<string> TimeOut(string playerName)
    {
        _logger.LogInformation("Attempting timeout for {PlayerName}.", playerName);
        try
        {
            var dto = _gameController.UpdateDto();
            var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
            if (player == null)
            {
                _logger.LogWarning("Player '{PlayerName}' not found.", playerName);
                return ServiceResult<string>.Failure($"Player '{playerName}' not found.");
            }

            if (dto.Players[dto.CurrentPlayerIndex] != player)
            {
                _logger.LogWarning("It is not {PlayerName}'s turn.", player.Name);
                return ServiceResult<string>.Failure($"It is not {player.Name}'s turn.");
            }

            _gameController.ApplyTimeOut(player);
            _logger.LogInformation("Timeout applied for {PlayerName}.", player.Name);
            return ServiceResult<string>.Success("Time out applied.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying timeout for {PlayerName} in GameService.", playerName);
            return ServiceResult<string>.Failure("An error occurred while applying timeout.");
        }
    }

    public ServiceResult<List<TileResponse>> GetPlayableTiles(string playerName)
    {
        _logger.LogInformation("Attempting to get playable tiles for {PlayerName}.", playerName);
        try
        {
            var dto = _gameController.UpdateDto();
            var player = dto.Players.FirstOrDefault(p => p.Name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));
            if (player == null)
            {
                _logger.LogWarning("Player '{PlayerName}' not found.", playerName);
                return ServiceResult<List<TileResponse>>.Failure($"Player '{playerName}' not found.");
            }

            var playable = _gameController.GetPlayableTiles(player);
            var result = playable.Select(t => new TileResponse { Top = t.Top, Bottom = t.Bottom }).ToList();
            
            _logger.LogInformation("Playable tiles retrieved for {PlayerName}.", player.Name);
            return ServiceResult<List<TileResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving playable tiles for {PlayerName} in GameService.", playerName);
            return ServiceResult<List<TileResponse>>.Failure("An error occurred while retrieving playable tiles.");
        }
    }
}