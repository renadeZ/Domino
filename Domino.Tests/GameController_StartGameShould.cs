using Domino.Backend;
using Domino.Backend.Models;
using Domino.Backend.Interfaces;

namespace Domino.Test;

[TestFixture]
public class GameController_StartGameShould
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
        var singlePlayerList = new List<IPlayer> { new Player("Player 1") };
        _gameController = new GameController(singlePlayerList,  _board, _deck, _rules);
        
        bool isStarted = _gameController.StartGame();

        Assert.IsFalse(isStarted, "StartGame should return false when there is only one player.");
    }
}