using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers;

public class HistoryController : Controller
{
    private readonly GameService _gameService;
    private readonly ExportService _exportService;
    private readonly GameLogicService _gameLogic;
    private readonly ILogger<HistoryController> _logger;
    
    public HistoryController(GameService gameService, ExportService exportService, 
                            GameLogicService gameLogic, ILogger<HistoryController> logger)
    {
        _gameService = gameService;
        _exportService = exportService;
        _gameLogic = gameLogic;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var games = await _gameService.GetFinishedGames();
        return View(games);
    }
    
    [HttpGet]
    public async Task<IActionResult> View(int id)
    {
        try
        {
            var game = await _gameService.GetGameViewModel(id);
            var gameEntity = await _gameService.GetGameById(id);
            
            if (gameEntity == null)
                return NotFound();
            
            ViewBag.Moves = gameEntity.Moves.OrderBy(m => m.MoveNumber).ToList();
            ViewBag.GameId = id;
            
            return View(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing game {GameId}", id);
            return RedirectToAction("Index");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetMoveState(int gameId, int moveNumber)
    {
        try
        {
            var game = await _gameService.GetGameById(gameId);
            if (game == null)
                return Json(new { success = false, error = "Game not found" });
            
            Move? move = null;
            if (moveNumber == 0)
            {
                // Estado inicial - todos los cubos en neutro
                var initialBoard = _gameLogic.InitializeBoard();
                return Json(new { success = true, board = SerializeBoard(initialBoard), timeElapsed = TimeSpan.Zero });
            }
            else
            {
                move = game.Moves.FirstOrDefault(m => m.MoveNumber == moveNumber);
                if (move == null)
                    return Json(new { success = false, error = "Move not found" });
                
                var board = _gameLogic.DeserializeBoard(move.BoardStateAfter);
                return Json(new { success = true, board = SerializeBoard(board), timeElapsed = move.TimeElapsed });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting move state");
            return Json(new { success = false, error = "Error al obtener el estado" });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Export(int id, string filePath)
    {
        try
        {
            var xml = await _exportService.ExportGameToXml(id);
            
            if (string.IsNullOrEmpty(filePath))
                filePath = $"quixo_game_{id}_{DateTime.Now:yyyyMMddHHmmss}.xml";
            
            if (!filePath.EndsWith(".xml"))
                filePath += ".xml";
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            return File(bytes, "application/xml", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting game {GameId}", id);
            return RedirectToAction("View", new { id });
        }
    }
    
    private object SerializeBoard(Models.Cube[,] board)
    {
        var result = new List<List<object>>();
        for (int i = 0; i < 5; i++)
        {
            var row = new List<object>();
            for (int j = 0; j < 5; j++)
            {
                row.Add(new
                {
                    symbol = board[i, j].Symbol.ToString(),
                    pointDirection = board[i, j].PointDirection?.ToString()
                });
            }
            result.Add(row);
        }
        return result;
    }
}

