// Navegación del historial de partidas
let currentMove = 0;
let totalMoves = 0;
let gameId = 0;
let isTwoPlayers = true;

document.addEventListener('DOMContentLoaded', function() {
    gameId = parseInt(document.getElementById('gameId')?.value || '0');
    totalMoves = parseInt(document.getElementById('totalMoves')?.value || '0');
    isTwoPlayers = document.getElementById('isTwoPlayers')?.value === 'true';
    
    // Inicializar con el estado inicial
    loadMoveState(0);
    
    // Configurar botones
    document.getElementById('btnFirst')?.addEventListener('click', () => goToMove(0));
    document.getElementById('btnPrev')?.addEventListener('click', () => goToMove(currentMove - 1));
    document.getElementById('btnNext')?.addEventListener('click', () => goToMove(currentMove + 1));
    document.getElementById('btnLast')?.addEventListener('click', () => goToMove(totalMoves));
    
    // Inicializar timer
    if (window.gameTimer) {
        window.gameTimer.stop();
    }
});

function goToMove(moveNumber) {
    if (moveNumber < 0) moveNumber = 0;
    if (moveNumber > totalMoves) moveNumber = totalMoves;
    
    currentMove = moveNumber;
    loadMoveState(moveNumber);
    updateMoveInfo();
    updateButtons();
}

function loadMoveState(moveNumber) {
    fetch(`/History/GetMoveState?gameId=${gameId}&moveNumber=${moveNumber}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateBoard(data.board);
                if (window.gameTimer) {
                    const totalSeconds = Math.floor(data.timeElapsed);
                    window.gameTimer.setTime(totalSeconds);
                }
            }
        })
        .catch(error => {
            console.error('Error loading move state:', error);
        });
}

function updateBoard(board) {
    const cells = document.querySelectorAll('#historyBoard .cube-cell');
    cells.forEach((cell, index) => {
        const row = Math.floor(index / 5);
        const col = index % 5;
        const cubeData = board[row][col];
        
        cell.className = 'cube-cell';
        if (cubeData.symbol === 'Neutral') {
            cell.classList.add('neutral');
        } else if (cubeData.symbol === 'Circle') {
            cell.classList.add('circle');
        } else {
            cell.classList.add('cross');
        }
        
        const cubeSymbol = cell.querySelector('.cube-symbol');
        if (cubeSymbol) {
            if (cubeData.symbol === 'Neutral') {
                cubeSymbol.textContent = '·';
            } else if (cubeData.symbol === 'Circle') {
                cubeSymbol.textContent = '○';
            } else {
                cubeSymbol.textContent = '×';
            }
        }
        
        // Limpiar indicadores de punto anteriores
        const pointIndicator = cell.querySelector('.point-indicator');
        if (pointIndicator) {
            pointIndicator.remove();
        }
        
        // Agregar indicador de punto si es necesario (modo 4 jugadores)
        if (!isTwoPlayers && cubeData.pointDirection) {
            const pointSpan = document.createElement('span');
            pointSpan.className = `point-indicator point-${cubeData.pointDirection.toLowerCase()}`;
            pointSpan.textContent = '·';
            cell.querySelector('.cube').appendChild(pointSpan);
        }
    });
}

function updateMoveInfo() {
    const moveInfo = document.getElementById('moveInfo');
    if (moveInfo) {
        moveInfo.textContent = `Movimiento ${currentMove} de ${totalMoves}`;
    }
}

function updateButtons() {
    document.getElementById('btnFirst').disabled = currentMove === 0;
    document.getElementById('btnPrev').disabled = currentMove === 0;
    document.getElementById('btnNext').disabled = currentMove >= totalMoves;
    document.getElementById('btnLast').disabled = currentMove >= totalMoves;
}

