using Domino.Backend.Models;
using Domino.Backend.Interfaces;
using Domino.Backend.Enums;
using Domino.Backend.EventArguments;

namespace Domino.Backend;

public interface IGameController
{
    public void StartGame();
    public void StartRound();
    public void MakeMove(IPlayer player, IDominoTile tile, PlacementSide side);
    public void Pass(IPlayer player);
    public void ApplyTimeOut(IPlayer player);
    // private void NextTurn();
    // private bool MatchesSide(IDominoTile tile, int value);
    // private void PlaceTile(IDominoTile tile, PlacementSide side);
    // private void ShuffleAndDeal();
    public List<IDominoTile> GetPlayableTiles(IPlayer player);
    // private int GetPlayerTotalPips(IPlayer player);
    // private int GetPlayerBalakCount(IPlayer player);
    // private IDominoTile GetSmallestBalak(IPlayer player);
    // private IDominoTile GetHighestBalak(IPlayer player);
    // private IPlayer FindFirstPlayer(bool isFirstRound);
    // private void CheckReShuffle();
    // private IPlayer? FindInstantWinner();
    // private void HandleGaple();
    // private int GetRoundScore(RoundResult result);
    // private int GetGaplePenalty(IPlayer loser);
    public bool IsGameOver();
}