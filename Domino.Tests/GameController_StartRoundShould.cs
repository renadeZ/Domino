using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

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

    [Test]
    public void StartRound_OnBoardReset_ShouldClearBoardChain()
    {
        IDominoTile tile1 = new DominoTile(6, 4);
        IDominoTile tile2 = new DominoTile(4, 2);

        _board.Chain.Add(tile1);
        _board.Chain.Add(tile2);
        _board.LeftEnd = tile1.Top;
        _board.RightEnd = tile2.Bottom;

        if (_deck.Tiles.Count == _deck.TotalTiles)
        {
            _deck.Tiles.RemoveAt(0);
        }

        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        Assert.AreEqual(0, dto.Board.Chain.Count, "Board chain should be cleared when deck reset occurs during StartRound");
        Assert.AreEqual(0, dto.Board.LeftEnd, "Board left end should be reset to 0");
        Assert.AreEqual(0, dto.Board.RightEnd, "Board right end should be reset to 0");
    }

    [Test]
    public void StartRound_NextTurn_ShouldNextPlayer()
    {
        _gameController.StartRound();
        IGameDTO before = _gameController.UpdateDto();
        int beforeIndex = before.CurrentPlayerIndex;
        IPlayer current = before.Players[beforeIndex];

        bool isTurnCompleted = false;
        _gameController.TurnCompleted += (s, e) => isTurnCompleted = true;

        _gameController.ApplyTimeOut(current);

        // Assert
        IGameDTO after = _gameController.UpdateDto();
        int expectedNextFirst = (beforeIndex + 1) % _players.Count;
        Assert.AreEqual(expectedNextFirst, after.CurrentPlayerIndex, "CurrentPlayerIndex should advance by one (wrap around)");
        Assert.IsTrue(isTurnCompleted, "TurnCompleted event should be fired when advancing to next player");
    }

    [Test]
    public void StartRound_NextTurn_ShouldBackToFirst()
    {
        // Arrange
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        int startIndex = dto.CurrentPlayerIndex;
        int stepsToLast = (_players.Count - 1 - startIndex + _players.Count) % _players.Count;

        // Advance to absolute last index
        for (int i = 0; i < stepsToLast; i++)
        {
            dto = _gameController.UpdateDto();
            _gameController.ApplyTimeOut(dto.Players[dto.CurrentPlayerIndex]);
        }

        dto = _gameController.UpdateDto();
        Assert.AreEqual(_players.Count - 1, dto.CurrentPlayerIndex, "Precondition: should be at last player index");

        // Act: advance once more and expect wrap to 0
        _gameController.ApplyTimeOut(dto.Players[dto.CurrentPlayerIndex]);

        // Assert
        IGameDTO after = _gameController.UpdateDto();
        Assert.AreEqual(0, after.CurrentPlayerIndex, "After advancing from last player, index should wrap to 0");
    }

    [Test]
    public void StartRound_InstantWin_ShouldRaiseRoundEndedWithInstantWin()
    {
        IDominoTile balak = new DominoTile(6, 6);
        IDominoTile other = new DominoTile(1, 2);

        List<IDominoTile> desiredDeckOrder = new List<IDominoTile> { other, balak };

        IDeck detDeck = new DesiredDeck(desiredDeckOrder, desiredDeckOrder.Count, 6)
        {
            DesiredOrder = desiredDeckOrder
        };

        IGameRules oneTileInstantRules = new GameRules(
            151,
            30,
            -1,
            1,
            2,
            20,
            -40,
            1, // TilesPerPlayer
            1, // InstantWinBalakCount
            5  // ReshuffleMinBalak
        );

        _gameController = new GameController(_players, _board, detDeck, oneTileInstantRules);
        _gameController.StartGame();

        bool raised = false;
        GameEventArgs? args = null;
        _gameController.RoundEnded += (s, e) => { raised = true; args = e; };

        _gameController.StartRound();

        Assert.IsTrue(raised, "RoundEnded should be raised when instant winner occurs");
        Assert.IsNotNull(args);
        Assert.AreEqual(_players[0], args!.Player);
        Assert.AreEqual(RoundResult.InstantWin, args.Result);
        Assert.AreEqual(oneTileInstantRules.WinScore, args.ScoreChange);
    }

    [Test]
    public void StartRound_ReShuffle_ShouldRaiseRoundEndedWithReShuffle()
    {
        IDominoTile p1Tile1 = new DominoTile(6,6);
        IDominoTile p1Tile2 = new DominoTile(5,5);
        IDominoTile p2Tile1 = new DominoTile(1,2);
        IDominoTile p2Tile2 = new DominoTile(3,4);

        List<IDominoTile> desiredDeckOrder = new List<IDominoTile> { p2Tile2, p1Tile2, p2Tile1, p1Tile1 };

        IDeck detDeck = new DesiredDeck(desiredDeckOrder, desiredDeckOrder.Count, 6)
        {
            DesiredOrder = desiredDeckOrder
        };

        IGameRules twoTileRules = new GameRules(
            151,
            30,
            -1,
            1,
            2,
            20,
            -40,
            2, // TilesPerPlayer
            3, // InstantWinBalakCount (higher than player's balaks)
            2  // ReshuffleMinBalak
        );

        _gameController = new GameController(_players, _board, detDeck, twoTileRules);
        _gameController.StartGame();

        bool raised = false;
        GameEventArgs? args = null;
        _gameController.RoundEnded += (s, e) => { raised = true; args = e; };

        _gameController.StartRound();

        Assert.IsTrue(raised, "RoundEnded should be raised when reshuffle condition occurs");
        Assert.IsNotNull(args);
        Assert.AreEqual(_players[0], args!.Player);
        Assert.AreEqual(RoundResult.ReShuffle, args.Result);
    }

}