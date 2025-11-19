namespace Server.Models;

public class Cube
{
    public CubeSymbol Symbol { get; set; }
    public PointDirection? PointDirection { get; set; } // Null para modo 2 jugadores
    
    public Cube()
    {
        Symbol = CubeSymbol.Neutral;
        PointDirection = null;
    }
    
    public Cube(CubeSymbol symbol, PointDirection? pointDirection = null)
    {
        Symbol = symbol;
        PointDirection = pointDirection;
    }
}

