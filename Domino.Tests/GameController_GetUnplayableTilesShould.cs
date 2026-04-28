using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_GetUnplayableTilesShould
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
    public void GetUnplayableTiles_EmptyBoard_ReturnsEmpty()
    {
        IGameDTO dto = _gameController.UpdateDto();
        IPlayer player = _players[0];

        List<IDominoTile> unplayable = _gameController.GetUnplayableTiles(player);

        Assert.IsEmpty(unplayable, "Unplayable tiles should be empty when board is empty (all tiles playable)");
    }

    [TestCase(2,5)]
    [TestCase(3,4)]
    public void GetUnplayableTiles_NonEmptyBoard_ReturnsOnlyNonMatchingTiles(int leftEnd, int rightEnd)
    {
        IGameDTO dto = _gameController.UpdateDto();
        IPlayer player = _players[0];

        IDominoTile tileOnBoard = new DominoTile(leftEnd, rightEnd);
        dto.Board.Chain.Add(tileOnBoard);
        dto.Board.LeftEnd = tileOnBoard.Top;
        dto.Board.RightEnd = tileOnBoard.Bottom;

        IDominoTile matching1 = new DominoTile(leftEnd, 1);
        IDominoTile matching2 = new DominoTile(rightEnd, 1);

        IDominoTile non1 = new DominoTile(0, 0);
        IDominoTile non2 = new DominoTile(1, 1);


        dto.PlayerHands[player].Clear();
        dto.PlayerHands[player].Add(matching1);
        dto.PlayerHands[player].Add(matching2);
        dto.PlayerHands[player].Add(non1);
        dto.PlayerHands[player].Add(non2);

        List<IDominoTile> unplayable = _gameController.GetUnplayableTiles(player);

        Assert.IsFalse(unplayable.Contains(matching1), "Matching tile should not be in unplayable list");
        Assert.IsFalse(unplayable.Contains(matching2), "Matching tile should not be in unplayable list");
        Assert.Contains(non1, unplayable, "Non-matching tile should be in unplayable list");
        Assert.Contains(non2, unplayable, "Non-matching tile should be in unplayable list");
        Assert.AreEqual(2, unplayable.Count, "Should have exactly 2 unplayable tiles");
    }

    [Test]
    public void GetUnplayableTiles_WithUnknownPlayer_ThrowsKeyNotFoundException()
    {
        var unknown = new Player("Unknown");
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.GetUnplayableTiles(unknown));
    }

    [Test]
    public void GetUnplayableTiles_WithNullPlayer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _gameController.GetUnplayableTiles(null!));
    }
}
