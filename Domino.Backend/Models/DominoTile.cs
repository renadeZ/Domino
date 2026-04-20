namespace Domino.Backend.Models;

public class DominoTile : IDominoTile
{
    public int Top { get; set; }
    public int Bottom { get; set; }

    public DominoTile(int top, int bottom)
    {
        Top = top;
        Bottom = bottom;
    }
}