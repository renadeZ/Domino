using Domino.Backend.Enums;
namespace Domino.API.DTOs;

public class MoveRequest
{
    public string PlayerName { get; set; } = string.Empty;
    public int TileTop { get; set; }
    public int TileBottom { get; set; }
    public PlacementSide Side { get; set; }
}

