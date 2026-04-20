namespace Domino.Backend.Models;

public interface IGameDTO
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
}