using Domino.Backend.Interfaces;
namespace Domino.Backend.Models;

public class Deck : IDeck
{
    public List<IDominoTile> Tiles { get; set; }
    public int TotalTiles { get; set; }
    public int MaxPipValue { get; set; }

    public Deck(List<IDominoTile> tiles, int totalTiles, int maxPipValue)
    {
        Tiles = tiles;
        TotalTiles = totalTiles;
        MaxPipValue = maxPipValue;
    }
}