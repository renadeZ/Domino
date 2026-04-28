using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_PassShould
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
	public void Pass_WithNoPlayableTiles_TriggersTurnCompletedAndAdvances()
	{
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
		var unknown = new Player("Unknown");
		Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.Pass(unknown));
	}

	[Test]
	public void Pass_WithNullPlayer_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _gameController.Pass(null!));
	}
}

