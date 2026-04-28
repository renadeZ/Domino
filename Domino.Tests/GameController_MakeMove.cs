using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

namespace Domino.Test;

[TestFixture]
public class GameController_MakeMove
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
	public void MakeMove_OnEmptyBoard_PlacesTileAndRemovesFromHand_ReturnsTrue()
	{
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
		IDominoTile boardTile= new DominoTile(1, 2);
		IBoard board = new DesiredBoard(new List<IDominoTile>(), 5, 6){
			DesiredChain = new List<IDominoTile>{ boardTile }
		};
		
	

		IGameDTO dto = _gameController.UpdateDto();
		IPlayer player = _players[0];

		dto.Board.Chain.Clear();
		IDominoTile boardTile = new DominoTile(5, 6);
		dto.Board.Chain.Add(boardTile);
		dto.Board.LeftEnd = boardTile.Top;
		dto.Board.RightEnd = boardTile.Bottom;

		IDominoTile matchingRight = new DominoTile(6, 2);
		dto.PlayerHands[player].Clear();
		dto.PlayerHands[player].Add(matchingRight);

		bool result = _gameController.MakeMove(player, matchingRight, PlacementSide.Right);

		Assert.IsTrue(result, "MakeMove should return true for a valid placement on the right side");
	}

	[Test]
	public void MakeMove_LastTile_WinsRoundAndUpdatesScore()
	{
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
		IPlayer player = _players[0];
		IDominoTile? tile = null;

		Assert.Throws<NullReferenceException>(() => _gameController.MakeMove(player, tile!, PlacementSide.Left));
	}

	[Test]
	public void MakeMove_WithUnknownPlayer_ThrowsKeyNotFoundException()
	{
		var unknown = new Player("Unknown");
		var tile = new DominoTile(1, 2);

		Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.MakeMove(unknown, tile, PlacementSide.Left));
	}

	[Test]
	public void HandleGaple_LowestPips_WinnerDrawWinNormal()
	{
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

		// Make total pips equal but balak values differ so tie-breaker picks smallest balak
		dto.PlayerHands[p0].Add(new DominoTile(4, 4)); // balak 4
		dto.PlayerHands[p0].Add(new DominoTile(1, 1)); // balak 1 -> p0 total = 10
		// p1 has balak 0 and balak 5 -> p1 total = 10
		dto.PlayerHands[p1].Add(new DominoTile(0, 0)); // balak 0
		dto.PlayerHands[p1].Add(new DominoTile(5, 5)); // balak 5

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

		// p0 wins by lowest pips
		dto.PlayerHands[p0].Add(new DominoTile(1, 2)); // total 3
		// p1 has a balak 0 which should trigger penalty
		dto.PlayerHands[p1].Add(new DominoTile(0, 0));
		dto.PlayerHands[p1].Add(new DominoTile(3, 3)); // total 6

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
}