using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

namespace Domino.Test;

[TestFixture]
public class GameTests
{
    private Game _gameController;
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
        
        _gameController = new Game(_players, _board, _deck, _rules);
    }

    [Test]
    public void StartGame_WithMoreThanOnePlayer_ReturnTrue()
    {
        bool isStarted = _gameController.StartGame();
        
        Assert.IsTrue(isStarted, "Start game true when more than one player");
    }

    [Test]
    public void StartGame_RoundNumberWhenStart_ShouldBeZero()
    {
        _gameController.StartGame();
        IGameDTO dto = _gameController.UpdateDto();
        Assert.AreEqual(0, dto.RoundNumber, "Round number should be 0 when start game");
    }

    [Test]
    public void StartGame_ScoreAndPlayersHand_ShouldBeAllRegisteredAndZero()
    {
        _gameController.StartGame();
        IGameDTO dto = _gameController.UpdateDto();
        
        foreach (var player in _players)
        {
            Assert.IsTrue(dto.Scores.ContainsKey(player), $"Player {player.Name} should be registered in score dictionary.");
            Assert.AreEqual(0, dto.Scores[player], $"Initial score for {player.Name} should be 0.");
            
            Assert.IsTrue(dto.PlayerHands.ContainsKey(player), $"Player {player.Name} should be registered in hand dictionary.");
            Assert.IsEmpty(dto.PlayerHands[player], $"Hands for {player.Name} should be empty before round start.");
        }
    }
    
    [Test]
    public void StartGame_WithOnlyOnePlayer_ShouldReturnFalse()
    {
        _gameController.StartGame();
        var singlePlayerList = new List<IPlayer> { new Player("Player 1") };
        _gameController = new Game(singlePlayerList,  _board, _deck, _rules);
        
        bool isStarted = _gameController.StartGame();

        Assert.IsFalse(isStarted, "StartGame should return false when there is only one player.");
    }

    [Test]
    public void StartRound_RoundAfterStartGame_ShouldBeOne()
    {
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        Assert.AreEqual(dto.RoundNumber, 1, "Starting round should increase to 1 after game start round.");
    }

    [Test]
    public void StartRound_PlayerHandTotal_ShouldBeRuleTilePerPlayer()
    {
        _gameController.StartGame();
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
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        int expected = _deck.TotalTiles - (_players.Count * _rules.TilesPerPlayer);
        Assert.AreEqual(expected, dto.Deck.Tiles.Count, "Deck should have correct number of tiles after dealing");
    }

    [Test]
    public void StartRound_CurrentPlayerIndex_ShouldBeValid()
    {
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();
        Assert.IsTrue(dto.CurrentPlayerIndex >= 0 && dto.CurrentPlayerIndex < _players.Count,
            "Current player index should be within valid range");
    }

    [Test]
    public void StartRound_ReturnsTrue()
    {
        _gameController.StartGame();
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

        _gameController = new Game(_players, _board, detDeck, oneTileRules);
        _gameController.StartGame();
        _gameController.StartRound();

        IGameDTO dto = _gameController.UpdateDto();

        Assert.AreEqual(0, dto.CurrentPlayerIndex, "Player with highest pip should be selected as first player");
    }

    [Test]
    public void StartRound_OnBoardReset_ShouldClearBoardChain()
    {
        _gameController.StartGame();
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
        _gameController.StartGame();
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
        _gameController.StartGame();
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

        _gameController = new Game(_players, _board, detDeck, oneTileInstantRules);
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

        _gameController = new Game(_players, _board, detDeck, twoTileRules);
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

	[Test]
	public void ShuffleAndDeal_PlayerHandCount_ShouldBeRuleTilePerPlayer()
	{
        _gameController.StartGame();
		_gameController.StartRound();
		IGameDTO dto = _gameController.UpdateDto();

		foreach (IPlayer player in _players)
		{
			Assert.AreEqual(dto.Rules.TilesPerPlayer, dto.PlayerHands[player].Count, $"{player.Name} should have {dto.Rules.TilesPerPlayer} tiles");
		}
	}

    [Test]
    public void ShuffleAndDeal_DeckCountAfterDeals_ShouldReduced()
    {
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        int expectedDeckCount = _deck.TotalTiles - (_players.Count * dto.Rules.TilesPerPlayer);
        Assert.AreEqual(expectedDeckCount, dto.Deck.Tiles.Count, "Deck should be reduced by dealt player hands");
    }

	[Test]
	public void StartRound_WithEmptyDeck_ThrowsArgumentOutOfRangeException()
	{
		var emptyDeck = new Deck(new List<IDominoTile>(), 0, 6);
		var controller = new Game(_players, _board, emptyDeck, _rules);
		controller.StartGame();

		Assert.Throws<ArgumentOutOfRangeException>(() => controller.StartRound());
	}
    [Test]
    public void ShuffleAndDeal_CardDealedToPlayers_ShouldBeRemovedFromDeck()
    {
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        foreach (IPlayer player in _players)
        {
            foreach (IDominoTile tile in dto.PlayerHands[player])
            {
                Assert.IsFalse(dto.Deck.Tiles.Contains(tile), $"Dealt tile {tile} should not be in the deck");
            }
        }
    }

    [Test]
    public void ShuffleAndDeal_CardEachPlayer_ShouldBeUnique()
    {
        _gameController.StartGame();
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        List<IDominoTile> seenTiles = new List<IDominoTile>();
        foreach (IPlayer player in _players)
        {
            foreach (IDominoTile tile in dto.PlayerHands[player])
            {
                Assert.IsFalse(seenTiles.Contains(tile), $"Tile {tile} should be unique across all players");
                seenTiles.Add(tile);
            }
        }
    }

	[Test]
	public void MakeMove_OnEmptyBoard_PlacesTileAndRemovesFromHand_ReturnsTrue()
	{
        _gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];
		IDominoTile tile = new DominoTile(1, 2);

		dto.PlayerHands[player].Clear();
		dto.PlayerHands[player].Add(tile);
		dto.PlayerHands[player].Add(new DominoTile(3, 4));

		bool result = _gameController.MakeMove(player, tile, PlacementSide.Left);

		Assert.IsTrue(result, "MakeMove should return true for a valid placement on empty board");
		Assert.AreEqual(1, dto.Board.Chain.Count, "Board should contain the placed tile");
		Assert.IsFalse(dto.PlayerHands[player].Contains(tile), "Tile should be removed from player's hand after placement");
	}

	[Test]
	public void MakeMove_InvalidPlacement_DoesNotChangeBoard_ReturnsFalse()
	{
        _gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.Board.Chain.Clear();
		IDominoTile boardTile = new DominoTile(5, 6);
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = boardTile.Top;
		dto.Board.RightEnd = boardTile.Bottom;

		IDominoTile nonMatching = new DominoTile(1, 2);
		dto.PlayerHands[player].Clear();
		dto.PlayerHands[player].Add(nonMatching);

		bool result = _gameController.MakeMove(player, nonMatching, PlacementSide.Left);

		Assert.IsFalse(result, "MakeMove should return false for an invalid placement");
		Assert.AreEqual(1, dto.Board.Chain.Count, "Board should remain unchanged after invalid move");
		Assert.IsTrue(dto.PlayerHands[player].Contains(nonMatching), "Player hand should remain unchanged after invalid move");
	}

	[Test]
	public void MakeMove_PlaceLeft_ReturnsTrue()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.Board.Chain.Clear();
		IDominoTile boardTile = new DominoTile(5, 6);
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = boardTile.Top;
		dto.Board.RightEnd = boardTile.Bottom;

		IDominoTile matchingLeft = new DominoTile(5, 2);
		dto.PlayerHands[player].Clear();
		dto.PlayerHands[player].Add(matchingLeft);

		bool result = _gameController.MakeMove(player, matchingLeft, PlacementSide.Left);

		Assert.IsTrue(result, "MakeMove should return true for a valid placement on the left side");
	}
	
	[Test]
	public void MakeMove_PlaceRight_ReturnsTrue()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.Board.Chain.Clear();
		IDominoTile boardTile = new DominoTile(5, 6);
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = boardTile.Top;
		dto.Board.RightEnd = boardTile.Bottom;

		IDominoTile matchingRight = new DominoTile(2, 6);
		dto.PlayerHands[player].Clear();
		dto.PlayerHands[player].Add(matchingRight);

		bool result = _gameController.MakeMove(player, matchingRight, PlacementSide.Right);

		Assert.IsTrue(result, "MakeMove should return true for a valid placement on the right side");
		Assert.AreEqual(2, dto.Board.RightEnd, "Board RightEnd should be updated to 2 after flipping the tile");
	}

	[Test]
	public void MakeMove_LastTile_WinsRoundAndUpdatesScore()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.PlayerHands[player].Clear();
		IDominoTile winningTile = new DominoTile(2, 3);
		dto.PlayerHands[player].Add(winningTile);

		dto.Board.Chain.Clear();

		bool result = _gameController.MakeMove(player, winningTile, PlacementSide.Left);

		Assert.IsTrue(result, "MakeMove should return true for a valid final placement");
		Assert.IsEmpty(dto.PlayerHands[player], "Player hand should be empty after playing last tile");
		Assert.AreEqual(dto.Rules.WinScore, dto.Scores[player], "Player score should be increased by WinScore after winning the round");
	}

	[Test]
	public void MakeMove_LastTileBalak6_ShouldWinBalak6()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.PlayerHands[player].Clear();
		IDominoTile winningTile = new DominoTile(6, 6);
		dto.PlayerHands[player].Add(winningTile);

		dto.Board.Chain.Clear();

		bool result = _gameController.MakeMove(player, winningTile, PlacementSide.Left);

		Assert.IsTrue(result, "MakeMove should return true for a valid final placement");
		Assert.IsEmpty(dto.PlayerHands[player], "Player hand should be empty after playing last tile");
		Assert.AreEqual(dto.Rules.WinBalak6Score, dto.Scores[player], "Player score should be increased by WinBalak6Score after winning the round");
	}

	[Test]
	public void MakeMove_WithNullTile_ThrowsNullReferenceException()
	{
		_gameController.StartGame();
		IPlayer player = _players[0];
		IDominoTile? tile = null;

		Assert.Throws<NullReferenceException>(() => _gameController.MakeMove(player, tile!, PlacementSide.Left));
	}

	[Test]
	public void MakeMove_WithUnknownPlayer_ThrowsKeyNotFoundException()
	{
		_gameController.StartGame();
		var unknown = new Player("Unknown");
		var tile = new DominoTile(1, 2);

		Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.MakeMove(unknown, tile, PlacementSide.Left));
	}

	[Test]
	public void HandleGaple_LowestPips_WinnerDrawWinNormal()
	{
        _gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		// set board so no tiles are playable (ends = 6)
		var boardTile = new DominoTile(6, 6);
		dto.Board.Chain.Clear();
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = 6;
		dto.Board.RightEnd = 6;

		// player0 has lower total pips than player1
		var p0 = _players[0];
		var p1 = _players[1];
		dto.PlayerHands[p0].Clear();
		dto.PlayerHands[p1].Clear();

		dto.PlayerHands[p0].Add(new DominoTile(1, 2)); // total 3
		dto.PlayerHands[p1].Add(new DominoTile(2, 3)); // total 5

		bool raised = false;
		GameEventArgs? args = null;
		_gameController.RoundEnded += (s, e) => { raised = true; args = e; };

		// Passing when no playable tiles should trigger HandleGaple
		_gameController.Pass(p0);

		Assert.IsTrue(raised, "RoundEnded should be raised by HandleGaple");
		Assert.IsNotNull(args);
		Assert.AreEqual(p0, args!.Player);
		Assert.AreEqual(RoundResult.DrawWinNormal, args.Result);
		Assert.AreEqual(dto.Rules.WinScore, args.ScoreChange);
		Assert.AreEqual(dto.Rules.WinScore, dto.Scores[p0]);
	}

	[Test]
	public void HandleGaple_SmallestBalakZero_WinnerDrawWinBalak0()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		var boardTile = new DominoTile(6, 6);
		dto.Board.Chain.Clear();
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = 6;
		dto.Board.RightEnd = 6;

		var p0 = _players[0];
		var p1 = _players[1];
		dto.PlayerHands[p0].Clear();
		dto.PlayerHands[p1].Clear();

		dto.PlayerHands[p0].Add(new DominoTile(4, 4)); 
		dto.PlayerHands[p0].Add(new DominoTile(1, 1)); 
		dto.PlayerHands[p1].Add(new DominoTile(0, 0)); 
		dto.PlayerHands[p1].Add(new DominoTile(5, 5)); 

		bool raised = false;
		GameEventArgs? args = null;
		_gameController.RoundEnded += (s, e) => { raised = true; args = e; };

		_gameController.Pass(p0);

		Assert.IsTrue(raised, "RoundEnded should be raised by HandleGaple");
		Assert.IsNotNull(args);
		Assert.AreEqual(p1, args!.Player);
		Assert.AreEqual(RoundResult.DrawWinBalak0, args.Result);
		Assert.AreEqual(dto.Rules.WinBalak0Score, args.ScoreChange);
		Assert.AreEqual(dto.Rules.WinBalak0Score, dto.Scores[p1]);
	}

	[Test]
	public void HandleGaple_OtherHasBalak0_ApplyLoseBalak0PenaltyAndWinnerDrawWinNormal()
	{
		_gameController.StartGame();
		IGameDTO dto = _gameController.UpdateDto();
		var boardTile = new DominoTile(6, 6);
		dto.Board.Chain.Clear();
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = 6;
		dto.Board.RightEnd = 6;

		var p0 = _players[0];
		var p1 = _players[1];
		dto.PlayerHands[p0].Clear();
		dto.PlayerHands[p1].Clear();

		dto.PlayerHands[p0].Add(new DominoTile(1, 2)); 
		dto.PlayerHands[p1].Add(new DominoTile(0, 0));
		dto.PlayerHands[p1].Add(new DominoTile(3, 3)); 

		bool raised = false;
		GameEventArgs? args = null;
		_gameController.RoundEnded += (s, e) => { raised = true; args = e; };

		_gameController.Pass(p0);

		Assert.IsTrue(raised, "RoundEnded should be raised by HandleGaple");
		Assert.IsNotNull(args);
		Assert.AreEqual(p0, args!.Player);
		Assert.AreEqual(RoundResult.DrawWinNormal, args.Result);
		Assert.AreEqual(dto.Rules.WinScore, args.ScoreChange);
		Assert.AreEqual(dto.Rules.WinScore, dto.Scores[p0]);
		Assert.AreEqual(dto.Rules.LoseBalak0Penalty, dto.Scores[p1], "Other player should receive LoseBalak0Penalty when they have balak 0");
	}

	[Test]
	public void Pass_WithNoPlayableTiles_TriggersTurnCompletedAndAdvances()
	{
        _gameController.StartGame();
		_gameController.StartRound();
		var dto = _gameController.UpdateDto();
		var current = dto.Players[dto.CurrentPlayerIndex];

		var boardTile = new DominoTile(5, 6);
		dto.Board.Chain.Clear();
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = boardTile.Top;
		dto.Board.RightEnd = boardTile.Bottom;

		dto.PlayerHands[current].Clear();
		dto.PlayerHands[current].Add(new DominoTile(1, 2));

		var other = dto.Players.First(p => p != current);
		dto.PlayerHands[other].Clear();
		dto.PlayerHands[other].Add(new DominoTile(6, 0));

		bool eventRaised = false;
		_gameController.TurnCompleted += (s, e) => eventRaised = true;
		int before = dto.CurrentPlayerIndex;

		_gameController.Pass(current);

		dto = _gameController.UpdateDto();
		Assert.IsTrue(eventRaised, "TurnCompleted should be raised when player passes with no playable tiles");
		Assert.AreNotEqual(before, dto.CurrentPlayerIndex, "CurrentPlayerIndex should advance after a pass");
	}

	[Test]
	public void Pass_WithPlayableTiles_DoesNotTriggerTurnCompleted()
	{
        _gameController.StartGame();
		_gameController.StartRound();
		var dto = _gameController.UpdateDto();
		var current = dto.Players[dto.CurrentPlayerIndex];

		dto.Board.Chain.Clear();
		dto.PlayerHands[current].Clear();
		dto.PlayerHands[current].Add(new DominoTile(1, 2));

		bool eventRaised = false;
		_gameController.TurnCompleted += (s, e) => eventRaised = true;

		_gameController.Pass(current);

		Assert.IsFalse(eventRaised, "Pass should not raise TurnCompleted when player still has playable tiles");
	}

	[Test]
	public void Pass_WithUnknownPlayer_ThrowsKeyNotFoundException()
	{
        _gameController.StartGame();
		var unknown = new Player("Unknown");
		Assert.Throws<KeyNotFoundException>(() => _gameController.Pass(unknown));
	}

	[Test]
	public void Pass_WithNullPlayer_ThrowsArgumentNullException()
	{
        _gameController.StartGame();
		Assert.Throws<ArgumentNullException>(() => _gameController.Pass(null!));
	}

    [Test]
    public void GetValidPlacements_EmptyBoard_ReturnsBothSides()
    {
        _gameController.StartGame();
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
        _gameController.StartGame();
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
        _gameController.StartGame();
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

    [Test]
    public void GetPlayableTiles_EmptyBoard_ReturnsAllTiles()
    {
        _gameController.StartGame();
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
        _gameController.StartGame();
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
        _gameController.StartGame();
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
        _gameController.StartGame();
        var unknown = new Player("Unknown");
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.GetPlayableTiles(unknown));
    }

    [Test]
    public void GetPlayableTiles_WithNullPlayer_ThrowsArgumentNullException()
    {
        _gameController.StartGame();
        Assert.Throws<ArgumentNullException>(() => _gameController.GetPlayableTiles(null!));
    }

    [Test]
    public void GetUnplayableTiles_EmptyBoard_ReturnsEmpty()
    {
        _gameController.StartGame();
        IPlayer player = _players[0];

        List<IDominoTile> unplayable = _gameController.GetUnplayableTiles(player);

        Assert.IsEmpty(unplayable, "Unplayable tiles should be empty when board is empty (all tiles playable)");
    }

    [TestCase(2,5)]
    [TestCase(3,4)]
    public void GetUnplayableTiles_NonEmptyBoard_ReturnsOnlyNonMatchingTiles(int leftEnd, int rightEnd)
    {
        _gameController.StartGame();
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
        _gameController.StartGame();
        var unknown = new Player("Unknown");
        Assert.Throws<KeyNotFoundException>(() => _gameController.GetUnplayableTiles(unknown));
    }

    [Test]
    public void GetUnplayableTiles_WithNullPlayer_ThrowsArgumentNullException()
    {
        _gameController.StartGame();
        Assert.Throws<ArgumentNullException>(() => _gameController.GetUnplayableTiles(null!));
    }

	[Test]
	public void ApplyTimeOut_AddsPenaltyAndTriggersTurnCompleted()
	{
        _gameController.StartGame();
		_gameController.StartRound();
		var dto = _gameController.UpdateDto();
		var player = dto.Players[dto.CurrentPlayerIndex];

		int before = dto.Scores[player];
		bool invoked = false;
		_gameController.TurnCompleted += (s, e) => invoked = true;

		_gameController.ApplyTimeOut(player);

		dto = _gameController.UpdateDto();
		Assert.AreEqual(before + dto.Rules.PenaltyPoints, dto.Scores[player], "Player score should be adjusted by penalty points");
		Assert.IsTrue(invoked, "TurnCompleted should be invoked after ApplyTimeOut");
	}

	[Test]
	public void ApplyTimeOut_WithUnknownPlayer_ThrowsKeyNotFoundException()
	{
        _gameController.StartGame();
		var unknown = new Player("Unknown");
		Assert.Throws<KeyNotFoundException>(() => _gameController.ApplyTimeOut(unknown));
	}

	[Test]
	public void ApplyTimeOut_WithNullPlayer_ThrowsArgumentNullException()
	{
        _gameController.StartGame();
		Assert.Throws<ArgumentNullException>(() => _gameController.ApplyTimeOut(null!));
	}

    [Test]
    public void IsGameOver_NoPlayerReachedWinningScore_ReturnsFalse()
    {
        _gameController.StartGame();
        var dto = _gameController.UpdateDto();

        // ensure all scores below winning
        foreach (var p in dto.Players)
            dto.Scores[p] = dto.Rules.WinningScore - 1;

        bool isOver = _gameController.IsGameOver();

        Assert.IsFalse(isOver);
    }

    [Test]
    public void IsGameOver_PlayerReachedWinningScore_ReturnsTrueAndRaisesGameOver()
    {
        _gameController.StartGame();
        var dto = _gameController.UpdateDto();
        var winner = dto.Players[0];

        dto.Scores[winner] = dto.Rules.WinningScore; // reach winning

        bool eventRaised = false;
        GameEventArgs? received = null;
        _gameController.GameOver += (s, e) => { eventRaised = true; received = e; };

        bool isOver = _gameController.IsGameOver();

        Assert.IsTrue(isOver, "IsGameOver should return true when a player reaches WinningScore");
        Assert.IsTrue(eventRaised, "GameOver event should be raised when a player wins");
        Assert.IsNotNull(received);
        Assert.AreEqual(winner, received!.Player);
        Assert.AreEqual(RoundResult.Win, received.Result);
        Assert.AreEqual(dto.Scores[winner], received.ScoreChange);
    }
}