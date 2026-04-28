using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_ShuffleAndDealShould
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
	public void ShuffleAndDeal_PlayerHandCount_ShouldBeRuleTilePerPlayer()
	{
		_gameController.StartRound();
		IGameDTO dto = _gameController.UpdateDto();

		foreach (IPlayer player in _players)
		{
			Assert.AreEqual(dto.Rules.TilesPerPlayer, dto.PlayerHands[player].Count, $"{player.Name} should have {dto.Rules.TilesPerPlayer} tiles");
		}
	}

    [Test]
    public void ShuffleAndDeal_DeckCountAfterDeals_ShouldReduced(){
        _gameController.StartRound();
        IGameDTO dto = _gameController.UpdateDto();

        int expectedDeckCount = _deck.TotalTiles - (_players.Count * dto.Rules.TilesPerPlayer);
        Assert.AreEqual(expectedDeckCount, dto.Deck.Tiles.Count, "Deck should be reduced by dealt player hands");
    }

	[Test]
	public void StartRound_WithEmptyDeck_ThrowsArgumentOutOfRangeException()
	{
		// create controller with empty deck
		var emptyDeck = new Deck(new List<IDominoTile>(), 0, 6);
		var controller = new GameController(_players, _board, emptyDeck, _rules);
		controller.StartGame();

		Assert.Throws<ArgumentOutOfRangeException>(() => controller.StartRound());
	}
    [Test]
    public void ShuffleAndDeal_CardDealedToPlayers_ShouldBeRemovedFromDeck(){
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
    public void ShuffleAndDeal_CardEachPlayer_ShouldBeUnique(){
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
}

