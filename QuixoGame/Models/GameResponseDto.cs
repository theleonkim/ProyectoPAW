namespace Server.Models;

public class GameResponseDto
{
    public int GameId { get; set; }
    public GameMode Mode { get; set; }
    public List<List<CubeDto>> Board { get; set; } = new();
    public int CurrentPlayer { get; set; }
    public bool IsFirstRound { get; set; }
    public GameStatus Status { get; set; }
    public TimeSpan TimeElapsed { get; set; }
    public string? Winner { get; set; }
}

public class CubeDto
{
    public string Symbol { get; set; } = string.Empty;
    public string? PointDirection { get; set; }
}

