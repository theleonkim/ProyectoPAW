using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

public class StatisticsController : Controller
{
    private readonly GameService _gameService;
    private readonly ILogger<StatisticsController> _logger;
    
    public StatisticsController(GameService gameService, ILogger<StatisticsController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var playerStats = await _gameService.GetPlayerStatistics();
            var teamStats = await _gameService.GetTeamStatistics();
            
            ViewBag.PlayerStatistics = playerStats;
            ViewBag.TeamStatistics = teamStats;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            return View();
        }
    }
}

