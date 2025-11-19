namespace Server.Models;

public class GameViewModel
{
    public int GameId { get; set; }
    public GameMode Mode { get; set; }
    public Cube[,] Board { get; set; } = new Cube[5, 5];
    public int CurrentPlayer { get; set; }
    public bool IsFirstRound { get; set; }
    public GameStatus Status { get; set; }
    public TimeSpan TimeElapsed { get; set; }
    public string? Winner { get; set; }
}

