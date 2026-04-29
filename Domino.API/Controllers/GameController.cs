using Microsoft.AspNetCore.Mvc;
using Domino.Backend.Enums;
using Domino.API.Services;
using Domino.API.DTOs;
using Serilog;


namespace Domino.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IGameService _gameService;

    public GameController( ILogger<GameController> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost("start")]
    public IActionResult StartGame()
    {
        _logger.LogInformation("Received request to start game.");

        try
        {
            ServiceResult<string> result = _gameService.StartGame();
            
            if (result.IsSuccess) 
            {
                _logger.LogInformation("Game Service : {Message}", result.Data);
                return Ok(new { Message = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to start game: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the game.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }

    }

    [HttpPost("round/start")]
    public IActionResult StartRound()
    {
        _logger.LogInformation("Received request to start round.");

        try
        {
            ServiceResult<string> result = _gameService.StartRound();
            if (result.IsSuccess)
            {
                _logger.LogInformation("Round started successfully.");
                return Ok(new { Message = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to start round: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting the round.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        _logger.LogInformation("Received request to get game state.");

        try
        {
            ServiceResult<GameStateResponse> result = _gameService.GetState();
            if (result.IsSuccess)
            {
                _logger.LogInformation("Game state retrieved successfully.");
                return Ok(result.Data);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve game state: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving game state.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpPost("move")]
    public IActionResult MakeMove([FromBody] MoveRequest request)
    {
        _logger.LogInformation(
            "Received request to make move for player {PlayerName}. Tile: {TileTop}-{TileBottom}, Side: {Side}", 
            request.PlayerName, request.TileTop, request.TileBottom, request.Side);

        try
        {
            ServiceResult<string> result = _gameService.MakeMove(request.PlayerName, request.TileTop, request.TileBottom, request.Side);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Move made successfully for player {PlayerName}.", request.PlayerName);
                return Ok(new { Message = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to make move for player {PlayerName}: {ErrorMessage}", request.PlayerName, result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while making a move for player {PlayerName}.", request.PlayerName);
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpPost("pass")]
    public IActionResult Pass([FromBody] PassRequest request)
    {
        _logger.LogInformation("Received request to pass turn for player {PlayerName}.", request.PlayerName);

        try
        {
            ServiceResult<string> result = _gameService.Pass(request.PlayerName);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Player {PlayerName} passed successfully.", request.PlayerName);
                return Ok(new { Message = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to pass for player {PlayerName}: {ErrorMessage}", request.PlayerName, result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while passing turn for player {PlayerName}.", request.PlayerName);
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpPost("timeout")]
    public IActionResult TimeOut([FromBody] PassRequest request)
    {
        _logger.LogInformation("Received timeout request for player {PlayerName}.", request.PlayerName);

        try
        {
            ServiceResult<string> result = _gameService.TimeOut(request.PlayerName);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Timeout applied successfully for player {PlayerName}.", request.PlayerName);
                return Ok(new { Message = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to apply timeout for player {PlayerName}: {ErrorMessage}", request.PlayerName, result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying timeout for player {PlayerName}.", request.PlayerName);
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpGet("playable/{playerName}")]
    public IActionResult GetPlayableTiles(string playerName)
    {
        _logger.LogInformation("Received request to get playable tiles for player {PlayerName}.", playerName);

        try
        {
            ServiceResult<List<TileResponse>> result = _gameService.GetPlayableTiles(playerName);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Playable tiles retrieved successfully for player {PlayerName}.", playerName);
                return Ok(result.Data);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve playable tiles for player {PlayerName}: {ErrorMessage}", playerName, result.ErrorMessage);
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving playable tiles for player {PlayerName}.", playerName);
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }
}