namespace Server.Models;

public class MoveRequest
{
    public int GameId { get; set; }
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
    public PointDirection? PointDirection { get; set; } // Solo para modo 4 jugadores
}

