using Server.Models;
using System.Text.Json;

namespace Server.Services;

public class GameLogicService
{
    private const int BOARD_SIZE = 5;
    
    public Cube[,] InitializeBoard()
    {
        var board = new Cube[BOARD_SIZE, BOARD_SIZE];
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                board[i, j] = new Cube(CubeSymbol.Neutral);
            }
        }
        return board;
    }
    
    public string SerializeBoard(Cube[,] board)
    {
        var boardData = new List<List<CubeData>>();
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            var row = new List<CubeData>();
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                row.Add(new CubeData
                {
                    Symbol = board[i, j].Symbol,
                    PointDirection = board[i, j].PointDirection
                });
            }
            boardData.Add(row);
        }
        return JsonSerializer.Serialize(boardData);
    }
    
    public Cube[,] DeserializeBoard(string boardState)
    {
        var boardData = JsonSerializer.Deserialize<List<List<CubeData>>>(boardState);
        var board = new Cube[BOARD_SIZE, BOARD_SIZE];
        
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                var data = boardData![i][j];
                board[i, j] = new Cube(data.Symbol, data.PointDirection);
            }
        }
        return board;
    }

    public bool IsPeripheral(int row, int col)
    {
        return row == 0 || row == BOARD_SIZE - 1 || col == 0 || col == BOARD_SIZE - 1;
    }
    
    //Estas son las reglas a revisar antes de cada movimiento
    public bool CanPickCube(Cube cube, int player, GameMode mode, bool isFirstRound, int row, int col)
    {
        // El cubo a mover debe ser de la periferia
        if (!IsPeripheral(row, col))
            return false;
        
        // Primera vuelta: solo neutros
        if (isFirstRound)
            return cube.Symbol == CubeSymbol.Neutral;
        
        // No se puede retirar el cubo del contrario
        if (mode == GameMode.TwoPlayers)
        {
            if (player == 1 && cube.Symbol == CubeSymbol.Cross)
                return false;
            if (player == 2 && cube.Symbol == CubeSymbol.Circle)
                return false;
        }
        else // Cuatro jugadores
        {
            // En modo 4 jugadores, se debe verificar la orientación del punto
            if (cube.Symbol == CubeSymbol.Circle && player != 1 && player != 3)
                return false;
            if (cube.Symbol == CubeSymbol.Cross && player != 2 && player != 4)
                return false;
            
            // Aca verifico la orientación del punto
            if (cube.Symbol != CubeSymbol.Neutral && cube.PointDirection.HasValue)
            {
                var pointDir = cube.PointDirection.Value;
                // El punto debe estar orientado hacia el jugador
                if (player == 1 && pointDir != PointDirection.Top)
                    return false;
                if (player == 2 && pointDir != PointDirection.Right)
                    return false;
                if (player == 3 && pointDir != PointDirection.Bottom)
                    return false;
                if (player == 4 && pointDir != PointDirection.Left)
                    return false;
            }
        }
        
        return true;
    }
    
    public List<(int row, int col)> GetValidPlacementPositions(int fromRow, int fromCol)
    {
        var positions = new List<(int, int)>();
        
        // Si el cubo está en una esquina, puede ir a dos posiciones
        bool isCorner = (fromRow == 0 || fromRow == BOARD_SIZE - 1) && 
                       (fromCol == 0 || fromCol == BOARD_SIZE - 1);
        
        if (isCorner)
        {
            if (fromRow == 0 && fromCol == 0) // Esquina superior izquierda
            {
                positions.Add((0, 0)); // Mismo lugar (pero no permitido)
                // El cubo puede ir al final de la fila o columna
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromCol) positions.Add((fromRow, i));
                    if (i != fromRow) positions.Add((i, fromCol));
                }
            }
            else if (fromRow == 0 && fromCol == BOARD_SIZE - 1) // Esquina superior derecha
            {
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromCol) positions.Add((fromRow, i));
                    if (i != fromRow) positions.Add((i, fromCol));
                }
            }
            else if (fromRow == BOARD_SIZE - 1 && fromCol == 0) // Esquina inferior izquierda
            {
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromCol) positions.Add((fromRow, i));
                    if (i != fromRow) positions.Add((i, fromCol));
                }
            }
            else if (fromRow == BOARD_SIZE - 1 && fromCol == BOARD_SIZE - 1) // Esquina inferior derecha
            {
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromCol) positions.Add((fromRow, i));
                    if (i != fromRow) positions.Add((i, fromCol));
                }
            }
        }
        else
        {
            // Si el cubo está en un borde (excepto esquinas)
            // Puede ir a cualquier posición de la fila O columna (ambas opciones)
            if (fromRow == 0 || fromRow == BOARD_SIZE - 1) // Fila superior o inferior
            {
                // Puede ir a cualquier posición de esa fila
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (j != fromCol) positions.Add((fromRow, j));
                }
                // También puede ir a cualquier posición de esa columna (periferia)
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromRow) positions.Add((i, fromCol));
                }
            }
            else if (fromCol == 0 || fromCol == BOARD_SIZE - 1) // Columna izquierda o derecha
            {
                // El vubo puede ir a cualquier posición de esa columna
                for (int i = 0; i < BOARD_SIZE; i++)
                {
                    if (i != fromRow) positions.Add((i, fromCol));
                }
                // También puede ir a cualquier posición de esa fila (periferia)
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (j != fromCol) positions.Add((fromRow, j));
                }
            }
        }
        
        // Remover la posición original
        positions.RemoveAll(p => p.Item1 == fromRow && p.Item2 == fromCol);
        
        return positions;
    }
    
    public Cube[,] MakeMove(Cube[,] board, int fromRow, int fromCol, int toRow, int toCol, 
                           CubeSymbol symbol, PointDirection? pointDirection)
    {
        var newBoard = new Cube[BOARD_SIZE, BOARD_SIZE];
        
        // Copiar el tablero
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                newBoard[i, j] = new Cube(board[i, j].Symbol, board[i, j].PointDirection);
            }
        }
        
        // Obtener el cubo
        var cube = newBoard[fromRow, fromCol];
        
        // Remover de la posición original
        newBoard[fromRow, fromCol] = new Cube(CubeSymbol.Neutral);
        
        // Desplazar los cubos en la fila o la columna
        if (fromRow == toRow) // Movimiento horizontal
        {
            if (fromCol < toCol) // Mover a la derecha
            {
                for (int j = fromCol; j < toCol; j++)
                {
                    newBoard[fromRow, j] = newBoard[fromRow, j + 1];
                }
            }
            else // Mover a la izquierda
            {
                for (int j = fromCol; j > toCol; j--)
                {
                    newBoard[fromRow, j] = newBoard[fromRow, j - 1];
                }
            }
        }
        else if (fromCol == toCol) // Movimiento vertical
        {
            if (fromRow < toRow) // Mover hacia abajo
            {
                for (int i = fromRow; i < toRow; i++)
                {
                    newBoard[i, fromCol] = newBoard[i + 1, fromCol];
                }
            }
            else // Mover hacia arriba
            {
                for (int i = fromRow; i > toRow; i--)
                {
                    newBoard[i, fromCol] = newBoard[i - 1, fromCol];
                }
            }
        }
        
        // Colocar el cubo con el símbolo del jugador
        newBoard[toRow, toCol] = new Cube(symbol, pointDirection);
        
        return newBoard;
    }
    
    public GameStatus CheckWinner(Cube[,] board, GameMode mode)
    {
        // Verificar líneas horizontales, verticales y diagonales
        var lines = new List<List<(int row, int col)>>();
        
        // Horizontales
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            var line = new List<(int, int)>();
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                line.Add((i, j));
            }
            lines.Add(line);
        }
        
        // Verticales
        for (int j = 0; j < BOARD_SIZE; j++)
        {
            var line = new List<(int, int)>();
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                line.Add((i, j));
            }
            lines.Add(line);
        }
        
        // Diagonal principal
        var diag1 = new List<(int, int)>();
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            diag1.Add((i, i));
        }
        lines.Add(diag1);
        
        // Diagonal secundaria
        var diag2 = new List<(int, int)>();
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            diag2.Add((i, BOARD_SIZE - 1 - i));
        }
        lines.Add(diag2);
        
        bool circleWins = false;
        bool crossWins = false;
        
        foreach (var line in lines)
        {
            var symbols = line.Select(pos => board[pos.row, pos.col].Symbol).ToList();
            
            if (symbols.All(s => s == CubeSymbol.Circle))
            {
                circleWins = true;
            }
            if (symbols.All(s => s == CubeSymbol.Cross))
            {
                crossWins = true;
            }
        }
        
        if (mode == GameMode.TwoPlayers)
        {
            if (circleWins && crossWins)
            {
                // El jugador que creó la línea del contrario pierde
                // Se revisa quién hizo el último movimiento
                return GameStatus.Finished; // Se determina en el servicio
            }
            if (circleWins)
                return GameStatus.WonByPlayer1;
            if (crossWins)
                return GameStatus.WonByPlayer2;
        }
        else // Cuatro Jugadores
        {
            if (circleWins && crossWins)
            {
                return GameStatus.Finished; // Se determina en el servicio
            }
            if (circleWins)
                return GameStatus.WonByTeamA;
            if (crossWins)
                return GameStatus.WonByTeamB;
        }
        
        return GameStatus.InProgress;
    }
    
    private class CubeData
    {
        public CubeSymbol Symbol { get; set; }
        public PointDirection? PointDirection { get; set; }
    }
}

