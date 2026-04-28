using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_StartRoundShould
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
    public void StartRound_RoundAfterStartGame_ShouldBeOne()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        Assert.AreEqual(dto.RoundNumber, 1, "Starting round should increase to 1 after game start round.");
    }

    [Test]
    public void StartRound_PlayerHandTotal_ShouldBeRuleTilePerPlayer()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        foreach (IPlayer player in _players)
        {
            List<IDominoTile> hand = dto.PlayerHands[player];
            Assert.AreEqual(hand.Count, _rules.TilesPerPlayer,
                $"{player.Name} should have {dto.Rules.TilesPerPlayer} tiles at round start");
        }
    }

    [Test]
    public void StartRound_DeckTileCount_ShouldBeTotalTilesReducedByPlayerHands()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        int expected = _deck.TotalTiles - (_players.Count * _rules.TilesPerPlayer);
        Assert.AreEqual(expected, dto.Deck.Tiles.Count, "Deck should have correct number of tiles after dealing");
    }

    [Test]
    public void StartRound_CurrentPlayerIndex_ShouldBeValid()
    {
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        Assert.IsTrue(dto.CurrentPlayerIndex >= 0 && dto.CurrentPlayerIndex < _players.Count,
            "Current player index should be within valid range");
    }

    [Test]
    public void StartRound_ReturnsTrue()
    {
        bool result = _gameController.StartRound();
        Assert.IsTrue(result, "StartRound should return true when successfully started");
    }

    [Test]
    public void StartRound_PlayerWithHighestPip_ShouldBeFirstPlayer()
    {
        IDominoTile player1Tile = new DominoTile(6, 4);
        IDominoTile player2Tile = new DominoTile(1, 3);

        List<IDominoTile> desiredDeckOrder = new List<IDominoTile> { player2Tile, player1Tile };

        IDeck detDeck = new DesiredDeck(desiredDeckOrder, desiredDeckOrder.Count, 6)
        {
            DesiredOrder = desiredDeckOrder
        };

        IGameRules oneTileRules = new GameRules(
            151,
            30,
            -1,
            1,
            2,
            20,
            -40,
            1, // TilesPerPlayer = 1
            7,
            5
        );

        _gameController = new GameController(_players, _board, detDeck, oneTileRules);
        _gameController.StartGame();
        _gameController.StartRound();

        IGameDTO dto = _gameController.UpdateDto();

        Assert.AreEqual(0, dto.CurrentPlayerIndex, "Player with highest pip should be selected as first player");
    }
}