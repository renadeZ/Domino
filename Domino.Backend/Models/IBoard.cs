namespace Domino.Backend.Models;

public interface IBoard
{
    public List<DominoTile> Chain { get; set; }
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }
}