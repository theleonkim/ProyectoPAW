using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers;

public class GameController : Controller
{
    private readonly GameService _gameService;
    private readonly ILogger<GameController> _logger;
    
    public GameController(GameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult SelectMode()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> StartGame(GameMode mode)
    {
        try
        {
            var game = await _gameService.CreateNewGame(mode);
            return RedirectToAction("Play", new { id = game.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game");
            return RedirectToAction("SelectMode");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Play(int id)
    {
        try
        {
            var game = await _gameService.GetGameViewModel(id);
            return View(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game {GameId}", id);
            return RedirectToAction("Index", "Home");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> MakeMove([FromBody] MoveRequest? request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("MakeMove called with null request, attempting to read body manually");
                
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                using var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                
                _logger.LogWarning("Request body: {Body}", body);
                
                if (string.IsNullOrEmpty(body))
                {
                    return Json(new { success = false, error = "Solicitud inválida: cuerpo vacío" });
                }
                
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };
                    request = System.Text.Json.JsonSerializer.Deserialize<MoveRequest>(body, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing request body: {Message}", ex.Message);
                    return Json(new { success = false, error = $"Error al procesar la solicitud: {ex.Message}" });
                }
                
                if (request == null)
                {
                    return Json(new { success = false, error = "Solicitud inválida: no se pudo deserializar" });
                }
            }
            
            // Validar que los datos básicos estén presentes
            if (request.GameId <= 0)
            {
                _logger.LogWarning("MakeMove called with invalid GameId: {GameId}", request.GameId);
                return Json(new { success = false, error = "ID de partida inválido" });
            }
            
            _logger.LogInformation("MakeMove: GameId={GameId}, From=({FromRow},{FromCol}), To=({ToRow},{ToCol}), PointDirection={PointDirection}", 
                request.GameId, request.FromRow, request.FromCol, request.ToRow, request.ToCol, request.PointDirection);
            
            var (success, error, game) = await _gameService.MakeMove(request);
            
            if (!success)
            {
                _logger.LogWarning("MakeMove failed: {Error}", error);
                return Json(new { success = false, error = error ?? "Error desconocido" });
            }
            
            if (game == null)
            {
                _logger.LogWarning("MakeMove returned null game");
                return Json(new { success = false, error = "No se pudo obtener el estado del juego" });
            }
            
            return Json(new { success = true, game = game });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return Json(new { success = false, error = $"Error al realizar el movimiento: {ex.Message}" });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Reset(int id)
    {
        try
        {
            await _gameService.ResetGame(id);
            return RedirectToAction("Play", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting game {GameId}", id);
            return RedirectToAction("Play", new { id });
        }
    }
}

