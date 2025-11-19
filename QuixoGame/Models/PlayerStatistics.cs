namespace Server.Models;

public class PlayerStatistics
{
    public string PlayerName { get; set; } = string.Empty;
    public int GamesWon { get; set; }
    public int TotalGames { get; set; }
    public double Effectiveness => TotalGames > 0 ? (GamesWon * 100.0 / TotalGames) : 0;
}

