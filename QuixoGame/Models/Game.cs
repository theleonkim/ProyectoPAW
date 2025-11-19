using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class Game
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public GameMode Mode { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? FinishedAt { get; set; }
    
    public TimeSpan? Duration { get; set; }
    
    [Required]
    public GameStatus Status { get; set; }
    
    [Required]
    public string BoardState { get; set; } = string.Empty; // JSON serializado del tablero
    
    public int CurrentPlayer { get; set; } // 1-2 para modo 2 jugadores, 1-4 para modo 4 jugadores
    
    public bool IsFirstRound { get; set; } = true;
    
    public ICollection<Move> Moves { get; set; } = new List<Move>();
}

