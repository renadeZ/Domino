using System.Collections.Generic;

namespace Domino.API.DTOs;

public class PlayerStateResponse
{
    public string Name { get; set; } = string.Empty;
    public int HandCount { get; set; }
    public List<TileResponse> PlayableHand { get; set; } = new();
    public List<TileResponse> UnplayableHand { get; set; } = new();
}