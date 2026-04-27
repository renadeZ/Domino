namespace Domino.Backend.Interfaces;

public interface IBoard
{
    public List<IDominoTile> Chain { get; set; }
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }
}