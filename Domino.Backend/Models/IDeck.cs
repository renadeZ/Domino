namespace Domino.Backend.Models;

public interface IDeck
{
    public List<DominoTile> Tiles { get; set; }
    public int TotalTiles { get; set; }
    public int MaxPipValue { get; set; }
}