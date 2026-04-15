namespace Domino.Backend.Models;

public class Board : IBoard
{
    public List<DominoTile> Chain { get; set; }
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }

    public Board(List<DominoTile> chain, int leftEnd, int rightEnd)
    {
        Chain = chain;
        LeftEnd = leftEnd;
        RightEnd = rightEnd;
    }
}