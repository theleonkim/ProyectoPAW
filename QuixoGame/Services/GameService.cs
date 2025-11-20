using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using System.Text.Json;

namespace Server.Services;

public class GameService
{
    private readonly QuixoDbContext _context;
    private readonly GameLogicService _gameLogic;
    
    public GameService(QuixoDbContext context, GameLogicService gameLogic)
    {
        _context = context;
        _gameLogic = gameLogic;
    }
    
    public async Task<Game> CreateNewGame(GameMode mode)
    {
        var board = _gameLogic.InitializeBoard();
        var boardState = _gameLogic.SerializeBoard(board);
        
        var game = new Game
        {
            Mode = mode,
            CreatedAt = DateTime.Now,
            Status = GameStatus.InProgress,
            BoardState = boardState,
            CurrentPlayer = 1,
            IsFirstRound = true
        };
        
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        
        return game;
    }
    
    public async Task<Game?> GetGameById(int gameId)
    {
        return await _context.Games
            .Include(g => g.Moves.OrderBy(m => m.MoveNumber))
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }
    
    public async Task<GameViewModel> GetGameViewModel(int gameId)
    {
        var game = await GetGameById(gameId);
        if (game == null)
            throw new Exception("Game not found");
        
        var board = _gameLogic.DeserializeBoard(game.BoardState);
        var timeElapsed = game.FinishedAt.HasValue 
            ? game.Duration ?? TimeSpan.Zero
            : DateTime.Now - game.CreatedAt;
        
        string? winner = null;
        if (game.Status == GameStatus.WonByPlayer1)
            winner = "Jugador 1";
        else if (game.Status == GameStatus.WonByPlayer2)
            winner = "Jugador 2";
        else if (game.Status == GameStatus.WonByTeamA)
            winner = "Equipo A";
        else if (game.Status == GameStatus.WonByTeamB)
            winner = "Equipo B";
        
        return new GameViewModel
        {
            GameId = game.Id,
            Mode = game.Mode,
            Board = board,
            CurrentPlayer = game.CurrentPlayer,
            IsFirstRound = game.IsFirstRound,
            Status = game.Status,
            TimeElapsed = timeElapsed,
            Winner = winner,
            CreatedAt = game.CreatedAt,
            FinishedAt = game.FinishedAt
        };
    }
    
    public async Task<(bool success, string? error, GameResponseDto? game)> MakeMove(MoveRequest request)
    {
        var game = await GetGameById(request.GameId);
        if (game == null)
            return (false, "Partida no encontrada", null);
        
        if (game.Status != GameStatus.InProgress)
            return (false, "La partida ya ha finalizado", null);
        
        var board = _gameLogic.DeserializeBoard(game.BoardState);
        
        // Se valida que la posición de origen sea válida
        if (!_gameLogic.IsPeripheral(request.FromRow, request.FromCol))
            return (false, "Solo se pueden retirar cubos de la periferia", null);
        
        var cube = board[request.FromRow, request.FromCol];
        
        // Se valida que el jugador pueda retirar el cubo
        if (!_gameLogic.CanPickCube(cube, game.CurrentPlayer, game.Mode, game.IsFirstRound, 
                                    request.FromRow, request.FromCol))
        {
            return (false, "No puedes retirar este cubo", null);
        }
        
        // Se valida la posición de destino
        var validPositions = _gameLogic.GetValidPlacementPositions(request.FromRow, request.FromCol);
        if (!validPositions.Contains((request.ToRow, request.ToCol)))
        {
            return (false, "Posición de destino inválida", null);
        }
        
        // Se determina el símbolo del jugador
        CubeSymbol playerSymbol;
        if (game.Mode == GameMode.TwoPlayers)
        {
            playerSymbol = game.CurrentPlayer == 1 ? CubeSymbol.Circle : CubeSymbol.Cross;
        }
        else // Cuatro jugadores
        {
            playerSymbol = (game.CurrentPlayer == 1 || game.CurrentPlayer == 3) 
                ? CubeSymbol.Circle 
                : CubeSymbol.Cross;
        }
        
        // Se hace el movimiento
        var newBoard = _gameLogic.MakeMove(board, request.FromRow, request.FromCol, 
                                         request.ToRow, request.ToCol, 
                                         playerSymbol, request.PointDirection);
        
        var newBoardState = _gameLogic.SerializeBoard(newBoard);
        var timeElapsed = DateTime.Now - game.CreatedAt;
        
        // Revisar ganador
        var winnerStatus = _gameLogic.CheckWinner(newBoard, game.Mode);
        
        // Si ambos ganan, se determina quién pierde (el que creó la línea del contrario)
        if (winnerStatus == GameStatus.Finished)
        {
            // Se verifica qué líneas se crearon
            var circleWins = CheckLine(newBoard, CubeSymbol.Circle);
            var crossWins = CheckLine(newBoard, CubeSymbol.Cross);
            
            if (circleWins && crossWins)
            {
                // El jugador actual pierde porque creó la línea del contrario
                if (game.Mode == GameMode.TwoPlayers)
                {
                    winnerStatus = game.CurrentPlayer == 1 
                        ? GameStatus.WonByPlayer2 
                        : GameStatus.WonByPlayer1;
                }
                else
                {
                    winnerStatus = (game.CurrentPlayer == 1 || game.CurrentPlayer == 3)
                        ? GameStatus.WonByTeamB
                        : GameStatus.WonByTeamA;
                }
            }
            else if (circleWins)
            {
                winnerStatus = game.Mode == GameMode.TwoPlayers 
                    ? GameStatus.WonByPlayer1 
                    : GameStatus.WonByTeamA;
            }
            else if (crossWins)
            {
                winnerStatus = game.Mode == GameMode.TwoPlayers 
                    ? GameStatus.WonByPlayer2 
                    : GameStatus.WonByTeamB;
            }
        }
        
        // Aca se guarda  el movimiento
        var moveNumber = game.Moves.Count + 1;
        var move = new Move
        {
            GameId = game.Id,
            MoveNumber = moveNumber,
            Player = game.CurrentPlayer,
            FromRow = request.FromRow,
            FromCol = request.FromCol,
            ToRow = request.ToRow,
            ToCol = request.ToCol,
            Symbol = playerSymbol,
            PointDirection = request.PointDirection,
            BoardStateAfter = newBoardState,
            TimeElapsed = timeElapsed
        };
        
        _context.Moves.Add(move);
        
        // Y se actualiza el juego
        game.BoardState = newBoardState;
        game.IsFirstRound = false; // Después del primer movimiento de cada jugador
        
        if (winnerStatus != GameStatus.InProgress)
        {
            game.Status = winnerStatus;
            game.FinishedAt = DateTime.Now;
            game.Duration = timeElapsed;
        }
        else
        {
            // Se cambia al siguiente jugador
            if (game.Mode == GameMode.TwoPlayers)
            {
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
            }
            else // Cuatro jugadores
            {
                game.CurrentPlayer = game.CurrentPlayer == 4 ? 1 : game.CurrentPlayer + 1;
            }
        }
        
        await _context.SaveChangesAsync();
        
        var viewModel = await GetGameViewModel(game.Id);
        var dto = ConvertToDto(viewModel);
        return (true, null, dto);
    }
    
    private bool CheckLine(Cube[,] board, CubeSymbol symbol)
    {
        const int BOARD_SIZE = 5;
        
        // Verificar líneas horizontales
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            bool allMatch = true;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (board[i, j].Symbol != symbol)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }
        
        // Verificar líneas verticales
        for (int j = 0; j < BOARD_SIZE; j++)
        {
            bool allMatch = true;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (board[i, j].Symbol != symbol)
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }
        
        // Diagonal principal
        bool diag1Match = true;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            if (board[i, i].Symbol != symbol)
            {
                diag1Match = false;
                break;
            }
        }
        if (diag1Match) return true;
        
        // Diagonal secundaria
        bool diag2Match = true;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            if (board[i, BOARD_SIZE - 1 - i].Symbol != symbol)
            {
                diag2Match = false;
                break;
            }
        }
        if (diag2Match) return true;
        
        return false;
    }
    
    public async Task<List<Game>> GetFinishedGames()
    {
        return await _context.Games
            .Where(g => g.Status != GameStatus.InProgress)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<PlayerStatistics>> GetPlayerStatistics()
    {
        var games = await _context.Games
            .Where(g => g.Mode == GameMode.TwoPlayers && g.Status != GameStatus.InProgress)
            .ToListAsync();
        
        var stats = new List<PlayerStatistics>
        {
            new PlayerStatistics { PlayerName = "Jugador 1" },
            new PlayerStatistics { PlayerName = "Jugador 2" }
        };
        
        foreach (var game in games)
        {
            stats[0].TotalGames++;
            stats[1].TotalGames++;
            
            if (game.Status == GameStatus.WonByPlayer1)
                stats[0].GamesWon++;
            else if (game.Status == GameStatus.WonByPlayer2)
                stats[1].GamesWon++;
        }
        
        return stats;
    }
    
    public async Task<List<TeamStatistics>> GetTeamStatistics()
    {
        var games = await _context.Games
            .Where(g => g.Mode == GameMode.FourPlayers && g.Status != GameStatus.InProgress)
            .ToListAsync();
        
        var stats = new List<TeamStatistics>
        {
            new TeamStatistics { TeamName = "Equipo A" },
            new TeamStatistics { TeamName = "Equipo B" }
        };
        
        foreach (var game in games)
        {
            stats[0].TotalGames++;
            stats[1].TotalGames++;
            
            if (game.Status == GameStatus.WonByTeamA)
                stats[0].GamesWon++;
            else if (game.Status == GameStatus.WonByTeamB)
                stats[1].GamesWon++;
        }
        
        return stats;
    }
    
    public async Task ResetGame(int gameId)
    {
        var game = await GetGameById(gameId);
        if (game == null)
            return;
        
        // Eliminar movimientos anteriores
        _context.Moves.RemoveRange(game.Moves);
        
        // Reiniciar el juego
        var board = _gameLogic.InitializeBoard();
        game.BoardState = _gameLogic.SerializeBoard(board);
        game.Status = GameStatus.InProgress;
        game.CurrentPlayer = 1;
        game.IsFirstRound = true;
        game.CreatedAt = DateTime.Now;
        game.FinishedAt = null;
        game.Duration = null;
        
        await _context.SaveChangesAsync();
    }
    
    private GameResponseDto ConvertToDto(GameViewModel viewModel)
    {
        var boardDto = new List<List<CubeDto>>();
        for (int i = 0; i < 5; i++)
        {
            var row = new List<CubeDto>();
            for (int j = 0; j < 5; j++)
            {
                var cube = viewModel.Board[i, j];
                row.Add(new CubeDto
                {
                    Symbol = cube.Symbol.ToString(),
                    PointDirection = cube.PointDirection?.ToString()
                });
            }
            boardDto.Add(row);
        }
        
        return new GameResponseDto
        {
            GameId = viewModel.GameId,
            Mode = viewModel.Mode,
            Board = boardDto,
            CurrentPlayer = viewModel.CurrentPlayer,
            IsFirstRound = viewModel.IsFirstRound,
            Status = viewModel.Status,
            TimeElapsed = viewModel.TimeElapsed,
            Winner = viewModel.Winner
        };
    }
}

