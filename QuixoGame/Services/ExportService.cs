using System.Xml.Linq;
using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Services;

public class ExportService
{
    private readonly QuixoDbContext _context;
    private readonly GameLogicService _gameLogic;
    
    public ExportService(QuixoDbContext context, GameLogicService gameLogic)
    {
        _context = context;
        _gameLogic = gameLogic;
    }
    
    public async Task<string> ExportGameToXml(int gameId)
    {
        var game = await _context.Games
            .Include(g => g.Moves.OrderBy(m => m.MoveNumber))
            .FirstOrDefaultAsync(g => g.Id == gameId);
        
        if (game == null)
            throw new Exception("Game not found");
        
        var root = new XElement("Game",
            new XElement("Id", game.Id),
            new XElement("Mode", game.Mode == GameMode.TwoPlayers ? "TwoPlayers" : "FourPlayers"),
            new XElement("CreatedAt", game.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
            new XElement("FinishedAt", game.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
            new XElement("Duration", game.Duration?.ToString(@"hh\:mm\:ss") ?? ""),
            new XElement("Status", game.Status.ToString()),
            new XElement("CurrentPlayer", game.CurrentPlayer),
            new XElement("IsFirstRound", game.IsFirstRound),
            new XElement("BoardState", game.BoardState),
            new XElement("Moves",
                game.Moves.Select(move => new XElement("Move",
                    new XElement("MoveNumber", move.MoveNumber),
                    new XElement("Player", move.Player),
                    new XElement("FromRow", move.FromRow),
                    new XElement("FromCol", move.FromCol),
                    new XElement("ToRow", move.ToRow),
                    new XElement("ToCol", move.ToCol),
                    new XElement("Symbol", move.Symbol.ToString()),
                    new XElement("PointDirection", move.PointDirection?.ToString() ?? ""),
                    new XElement("TimeElapsed", move.TimeElapsed.ToString(@"hh\:mm\:ss")),
                    new XElement("BoardStateAfter", move.BoardStateAfter)
                ))
            )
        );
        
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        return doc.ToString();
    }
}

