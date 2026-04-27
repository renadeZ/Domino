using Microsoft.AspNetCore.Mvc;
using Domino.Backend.Enums;
using Domino.API.Services;
using Domino.API.DTOs;

namespace Domino.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("start")]
    public IActionResult StartGame()
    {
        ServiceResult<string> result = _gameService.StartGame();
        if (result.IsSuccess) return Ok(new { Message = result.Data });
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpPost("round/start")]
    public IActionResult StartRound()
    {
        ServiceResult<string> result = _gameService.StartRound();
        if (result.IsSuccess) return Ok(new { Message = result.Data });
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        ServiceResult<GameStateResponse> result = _gameService.GetState();
        if (result.IsSuccess) return Ok(result.Data);
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpPost("move")]
    public IActionResult MakeMove([FromBody] MoveRequest request)
    {
        ServiceResult<string> result = _gameService.MakeMove(request.PlayerName, request.TileTop, request.TileBottom, request.Side);
        if (result.IsSuccess) return Ok(new { Message = result.Data });
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpPost("pass")]
    public IActionResult Pass([FromBody] PassRequest request)
    {
        ServiceResult<string> result = _gameService.Pass(request.PlayerName);
        if (result.IsSuccess) return Ok(new { Message = result.Data });
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpPost("timeout")]
    public IActionResult TimeOut([FromBody] PassRequest request)
    {
        ServiceResult<string> result = _gameService.TimeOut(request.PlayerName);
        if (result.IsSuccess) return Ok(new { Message = result.Data });
        return BadRequest(new { Message = result.ErrorMessage });
    }

    [HttpGet("playable/{playerName}")]
    public IActionResult GetPlayableTiles(string playerName)
    {
        ServiceResult<List<TileResponse>> result = _gameService.GetPlayableTiles(playerName);
        if (result.IsSuccess) return Ok(result.Data);
        return BadRequest(new { Message = result.ErrorMessage });
    }
}