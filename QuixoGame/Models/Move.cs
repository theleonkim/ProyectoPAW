using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Move
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Game")]
    public int GameId { get; set; }
    
    public Game Game { get; set; } = null!;
    
    [Required]
    public int MoveNumber { get; set; }
    
    [Required]
    public int Player { get; set; }
    
    [Required]
    public int FromRow { get; set; }
    
    [Required]
    public int FromCol { get; set; }
    
    [Required]
    public int ToRow { get; set; }
    
    [Required]
    public int ToCol { get; set; }
    
    [Required]
    public CubeSymbol Symbol { get; set; }
    
    public PointDirection? PointDirection { get; set; }
    
    [Required]
    public string BoardStateAfter { get; set; } = string.Empty; // Estado del tablero después del movimiento
    
    [Required]
    public TimeSpan TimeElapsed { get; set; }
}

