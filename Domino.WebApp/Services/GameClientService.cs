using System.Net.Http.Json;

namespace Domino.WebApp.Services;

public class GameStateResponse
{
    public int RoundNumber { get; set; }
    public string CurrentPlayer { get; set; } = string.Empty;
    public BoardStateResponse Board { get; set; } = new();
    public Dictionary<string, int> Scores { get; set; } = new();
    public List<PlayerStateResponse> Players { get; set; } = new();
    public bool IsGameOver { get; set; }
}

public class BoardStateResponse
{
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }
    public List<TileResponse> Chain { get; set; } = new();
}

public class PlayerStateResponse
{
    public string Name { get; set; } = string.Empty;
    public int HandCount { get; set; }
    public List<TileResponse> Hand { get; set; } = new();
}

public class TileResponse
{
    public int Top { get; set; }
    public int Bottom { get; set; }
}

public class MoveRequest
{
    public string PlayerName { get; set; } = string.Empty;
    public int TileTop { get; set; }
    public int TileBottom { get; set; }
    public int Side { get; set; }
}

public class PassRequest
{
    public string PlayerName { get; set; } = string.Empty;
}

public class GameClientService
{
    private readonly HttpClient _http;

    public GameClientService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> StartGameAsync()
    {
        var res = await _http.PostAsync("/api/game/start", null);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string> StartRoundAsync()
    {
        var res = await _http.PostAsync("/api/game/round/start", null);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<GameStateResponse?> GetStateAsync()
    {
        try
        {
            var res = await _http.GetAsync("/api/game/state");
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<GameStateResponse>();
            }
        }
        catch
        {
        }

        return null;
    }

    public async Task<string> MakeMoveAsync(MoveRequest request)
    {
        var res = await _http.PostAsJsonAsync("/api/game/move", request);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string> PassAsync(PassRequest request)
    {
        var res = await _http.PostAsJsonAsync("/api/game/pass", request);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<List<TileResponse>> GetPlayableTilesAsync(string playerName)
    {
        var res = await _http.GetAsync($"/api/game/playable/{playerName}");
        if (res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<List<TileResponse>>() ?? new();
        }

        return new();
    }
}