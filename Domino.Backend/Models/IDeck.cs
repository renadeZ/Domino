namespace Domino.Backend.Models;

public interface IDeck
{
    public List<IDominoTile> Tiles { get; set; }
    public int TotalTiles { get; set; }
    public int MaxPipValue { get; set; }
}