using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;
internal class DesiredDeck : IDeck
{
    private List<IDominoTile> _tiles;
    public List<IDominoTile> DesiredOrder { get; set; }

    public List<IDominoTile> Tiles
    {
        get => _tiles;
        set
        {
            if (DesiredOrder != null && DesiredOrder.Count > 0)
            {
                _tiles = new List<IDominoTile>(DesiredOrder);
            }
            else
            {
                _tiles = value;
            }
        }
    }

    public int TotalTiles { get; set; }
    public int MaxPipValue { get; set; }

    public DesiredDeck(List<IDominoTile> initialTiles, int totalTiles, int maxPip)
    {
        _tiles = new List<IDominoTile>(initialTiles);
        TotalTiles = totalTiles;
        MaxPipValue = maxPip;
    }
}