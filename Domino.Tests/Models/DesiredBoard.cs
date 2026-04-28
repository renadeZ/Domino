using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;
internal class DesiredBoard : IBoard
{
    private List<IDominoTile> _chain;
    public List<IDominoTile> DesiredChain { get; set; }

    public List<IDominoTile> Chain
    {
        get => _chain;
        set
        {
            if (DesiredChain != null && DesiredChain.Count > 0)
            {
                _chain = new List<IDominoTile>(DesiredChain);
            }
            else
            {
                _chain = value;
            }
        }
    }

    public int LeftEnd { get; set; }
    public int RightEnd { get; set; }

    public DesiredBoard(List<IDominoTile> initialChain, int leftEnd, int rightEnd)
    {
        _chain = new List<IDominoTile>(initialChain);
        LeftEnd = leftEnd;
        RightEnd = rightEnd;
    }


}