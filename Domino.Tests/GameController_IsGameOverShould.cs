using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.EventArguments;
using Domino.Backend.Enums;

namespace Domino.Test;

[TestFixture]
public class GameController_IsGameOverShould
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
    public void IsGameOver_NoPlayerReachedWinningScore_ReturnsFalse()
    {
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
