using Domino.Backend.Interfaces;
namespace Domino.Backend.Models;

public class Board : IBoard
{
    public List<IDominoTile> Chain { get; set; }
    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }

    public Board(List<IDominoTile> chain, int leftEnd, int rightEnd)
    {
        Chain = chain;
        LeftEnd = leftEnd;
        RightEnd = rightEnd;
    }
}