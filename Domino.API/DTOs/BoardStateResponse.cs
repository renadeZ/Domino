namespace Domino.API.DTOs;

public class BoardStateResponse
{
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }
    public List<TileResponse> Chain { get; set; } = new();
}