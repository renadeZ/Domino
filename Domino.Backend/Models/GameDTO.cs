namespace Domino.Backend.Models;

public class GameDto : IGameDTO
{
    public IBoard Board { get; set; }
    public IDeck Deck { get; set; }
    public IGameRules Rules { get; set; }
    public Dictionary<IPlayer, List<IDominoTile>> PlayerHands { get; set; }
    public Dictionary<IPlayer, int> Scores { get; set; }
    public List<IPlayer> Players { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int RoundNumber { get; set; }
    public int PassCount { get; set; }
    public IPlayer winningPlayer { get; set; }

    public GameDto(IBoard board, IDeck deck, IGameRules rules, Dictionary<IPlayer, List<IDominoTile>> playerHands,
        Dictionary<IPlayer, int> scores, List<IPlayer> players, int currentPlayerIndex, int roundNumber, int passCount)
    {
        Board = board;
        Deck = deck;
        Rules = rules;
        PlayerHands = playerHands;
        Scores = scores;
        Players = players;
        CurrentPlayerIndex = currentPlayerIndex;
        RoundNumber = roundNumber;
        PassCount = passCount;
    }
}