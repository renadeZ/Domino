using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_ApplyTimeoutShould
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
	public void ApplyTimeOut_AddsPenaltyAndTriggersTurnCompleted()
	{
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
		var unknown = new Player("Unknown");
		Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _gameController.ApplyTimeOut(unknown));
	}

	[Test]
	public void ApplyTimeOut_WithNullPlayer_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _gameController.ApplyTimeOut(null!));
	}
}

