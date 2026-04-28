using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_GetPlayableTilesShould
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
    public void GetPlayableTiles_EmptyBoard_ReturnsAllTiles()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        IPlayer player = _players[0];
        List<IDominoTile> playableTiles = _gameController.GetPlayableTiles(player);

        foreach (IDominoTile tile in dto.PlayerHands[player])
        {
            Assert.Contains(tile, playableTiles, $"Tile {tile} should be playable on an empty board");
        }
    }

    [Test]
    public void GetPlayableTiles_OnEmptyBoard_PlayableEqualsHand()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        IPlayer player = _players[0];
        List<IDominoTile> playableTiles = _gameController.GetPlayableTiles(player);

        Assert.AreEqual(dto.PlayerHands[player].Count, playableTiles.Count, "Playable tiles count should match hand count on empty board"); 
    }


    [TestCase(2, 5)]
    [TestCase(3, 4)]
    public void GetPlayableTiles_NonEmptyBoard_ReturnsOnlyMatchingTiles(int leftEnd, int rightEnd)
    {
        IGameDTO dto = _gameController.UpdateDto();
        IPlayer player = _players[0];

        IDominoTile tileOnBoard = new DominoTile(leftEnd, rightEnd);
        dto.Board.Chain.Add(tileOnBoard);
        dto.Board.LeftEnd = tileOnBoard.Top;
        dto.Board.RightEnd = tileOnBoard.Bottom;

        IDominoTile matchingTile1 = new DominoTile(leftEnd, 1);
        IDominoTile matchingTile2 = new DominoTile(rightEnd, 1);
        IDominoTile matchingTile3 = new DominoTile(1, leftEnd);
        IDominoTile matchingTile4 = new DominoTile(1, rightEnd);
        IDominoTile nonMatchingTile = new DominoTile(1, 1);

        dto.PlayerHands[player].Clear();
        dto.PlayerHands[player].Add(matchingTile1);
        dto.PlayerHands[player].Add(matchingTile2);
        dto.PlayerHands[player].Add(matchingTile3);
        dto.PlayerHands[player].Add(matchingTile4);
        dto.PlayerHands[player].Add(nonMatchingTile);

        List<IDominoTile> playableTiles = _gameController.GetPlayableTiles(player);

        Assert.Contains(matchingTile1, playableTiles, "Matching tile 1 should be playable");
        Assert.Contains(matchingTile2, playableTiles, "Matching tile 2 should be playable");
        Assert.Contains(matchingTile3, playableTiles, "Matching tile 3 should be playable");
        Assert.Contains(matchingTile4, playableTiles, "Matching tile 4 should be playable");
        Assert.IsFalse(playableTiles.Contains(nonMatchingTile), "Non-matching tile should not be playable");
    }

    [Test]
    public void GetPlayableTiles_WithUnknownPlayer_ThrowsKeyNotFoundException()
    {
        var unknown = new Player("Unknown");
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.GetPlayableTiles(unknown));
    }

    [Test]
    public void GetPlayableTiles_WithNullPlayer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _gameController.GetPlayableTiles(null!));
    }
}