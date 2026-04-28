using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;

namespace Domino.Test;

[TestFixture]
public class GameController_GetValidPlacementsShould
{
    private GameController _gameController;
    private IBoard _board;
    private IDeck _deck;
    private IGameRules _rules;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        _board = new Board(new List<IDominoTile>(), 0, 0);

        List<IDominoTile> tiles = new List<IDominoTile>();
        _deck = new Deck(tiles, 0, 6);
        for (int i = 0; i < _deck.MaxPipValue + 1; i++)
        {
            for (int j = i; j < _deck.MaxPipValue + 1; j++)
            {
                _deck.Tiles.Add(new DominoTile(i, j));
                _deck.TotalTiles++;
            }
        }

        _rules = new GameRules(
            151,
            30,
            -1,
            1,
            2,
            20,
            -40,
            7,
            7,
            5
        );

        _players = new List<IPlayer>
        {
            new Player("Player 1"),
            new Player("Player 2")
        };

        _gameController = new GameController(_players, _board, _deck, _rules);
        _gameController.StartGame();
    }

    [Test]
    public void GetValidPlacements_EmptyBoard_ReturnsBothSides()
    {
        var dto = _gameController.UpdateDto();
        dto.Board.Chain.Clear();

        var tile = new DominoTile(1, 2);
        var sides = _gameController.GetValidPlacements(tile);

        Assert.Contains(PlacementSide.Left, sides);
        Assert.Contains(PlacementSide.Right, sides);
    }

    [TestCase(3,5,3,6,true,false)]
    [TestCase(2,4,1,4,false,true)]
    [TestCase(3,4,3,4,true,true)]
    public void GetValidPlacements_BoardMatches_ReturnsExpected(int top, int bottom, int left, int right, bool expectLeft, bool expectRight)
    {
        var dto = _gameController.UpdateDto();
        dto.Board.Chain.Clear();
        var boardTile = new DominoTile(left, right);
        dto.Board.Chain.Add(boardTile);
        dto.Board.LeftEnd = left;
        dto.Board.RightEnd = right;

        var tile = new DominoTile(top, bottom);
        var sides = _gameController.GetValidPlacements(tile);

        Assert.AreEqual(expectLeft, sides.Contains(PlacementSide.Left));
        Assert.AreEqual(expectRight, sides.Contains(PlacementSide.Right));
    }

    [Test]
    public void GetValidPlacements_TileMatchesBothSides_WhenTileIsDoubleMatchingBoth()
    {
        var dto = _gameController.UpdateDto();
        dto.Board.Chain.Clear();
        var boardTile = new DominoTile(2, 2);
        dto.Board.Chain.Add(boardTile);
        dto.Board.LeftEnd = boardTile.Top;
        dto.Board.RightEnd = boardTile.Bottom;

        var tile = new DominoTile(2, 2);
        var sides = _gameController.GetValidPlacements(tile);

        Assert.Contains(PlacementSide.Left, sides);
        Assert.Contains(PlacementSide.Right, sides);
    }
}
