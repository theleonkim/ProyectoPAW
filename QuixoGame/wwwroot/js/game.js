// Lógica
let selectedCube = null;
let gameMode = '2';
let currentPlayer = 1;
let isFirstRound = true;
let gameStatus = '0';

document.addEventListener('DOMContentLoaded', function() {
    // Se espera un momento para asegurar que todo esté cargado
    setTimeout(function() {
        gameMode = document.getElementById('gameMode')?.value || '2';
        currentPlayer = parseInt(document.getElementById('currentPlayer')?.value || '1');
        isFirstRound = document.getElementById('isFirstRound')?.value === 'true';
        gameStatus = document.getElementById('gameStatus')?.value || '0';
        
        console.log('Game initialized:', { gameMode, currentPlayer, isFirstRound, gameStatus });
        
        const cubes = document.querySelectorAll('.cube-cell');
        console.log('Found cubes:', cubes.length);
        cubes.forEach(cube => {
            // Agregar evento directamente al td
            cube.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                handleCubeClick({ currentTarget: cube });
            });
        });
        
        // Configurar botones de dirección del punto (modo 4 jugadores)
        if (gameMode === '4') {
            const pointButtons = document.querySelectorAll('#pointDirectionSelector button');
            pointButtons.forEach(btn => {
                btn.addEventListener('click', function() {
                    const direction = this.getAttribute('data-direction');
                    completeMove(direction);
                });
            });
        }
    }, 100);
});

function handleCubeClick(event) {
    console.log('Cube clicked, gameStatus:', gameStatus);
    // GameStatus.InProgress = 0
    if (gameStatus !== '0' && gameStatus !== 0) {
        console.log('Game finished, ignoring click');
        return; // Juego finalizado
    }
    
    const cell = event.currentTarget;
    const row = parseInt(cell.getAttribute('data-row'));
    const col = parseInt(cell.getAttribute('data-col'));
    const symbol = cell.getAttribute('data-symbol');
    
    console.log('Click on cube:', { row, col, symbol, selectedCube: selectedCube ? 'yes' : 'no' });
    
    if (!selectedCube) {
        // Seleccionar cubo
        if (canPickCube(cell, row, col)) {
            selectedCube = { row, col, cell };
            cell.classList.add('selected');
            highlightValidPositions(row, col);
            console.log('Cube selected:', { row, col });
        } else {
            console.log('Cannot pick this cube');
            showMessage('No puedes retirar este cubo. Debe ser de la periferia y cumplir las reglas del juego.', 'danger');
        }
    } else {
        // Colocar cubo
        if (selectedCube.row === row && selectedCube.col === col) {
            // Deseleccionar
            console.log('Deselecting cube');
            clearSelection();
        } else {
            // Verificar si es una posición válida
            const isValid = isValidPlacement(selectedCube.row, selectedCube.col, row, col);
            console.log('Placement validation:', {
                from: { row: selectedCube.row, col: selectedCube.col },
                to: { row, col },
                isValid: isValid
            });
            
            if (isValid) {
                // Hacer el movimiento
                console.log('Making move:', { from: { row: selectedCube.row, col: selectedCube.col }, to: { row, col } });
                if (gameMode === '4') {
                    // Mostrar selector de dirección del punto
                    showPointDirectionSelector();
                    window.pendingMove = {
                        fromRow: selectedCube.row,
                        fromCol: selectedCube.col,
                        toRow: row,
                        toCol: col
                    };
                } else {
                    makeMove(selectedCube.row, selectedCube.col, row, col, null);
                }
            } else {
                console.log('Invalid placement - clearing selection');
                showMessage('Posición de destino inválida. Debe estar en la misma fila o columna y en la periferia.', 'danger');
                clearSelection();
            }
        }
    }
}

function canPickCube(cell, row, col) {
    // Debe ser de la periferia
    if (row !== 0 && row !== 4 && col !== 0 && col !== 4) {
        return false;
    }
    
    const symbol = cell.getAttribute('data-symbol');
    
    // Primera vuelta: solo neutros
    if (isFirstRound) {
        return symbol === 'Neutral';
    }
    
    // No puede retirar cubo del contrario
    if (gameMode === '2') {
        if (currentPlayer === 1 && symbol === 'Cross') return false;
        if (currentPlayer === 2 && symbol === 'Circle') return false;
    } else {
        // Modo 4 jugadores: verificar orientación del punto
        const pointDir = cell.getAttribute('data-point');
        if (symbol !== 'Neutral' && pointDir) {
            if (currentPlayer === 1 && pointDir !== 'Top') return false;
            if (currentPlayer === 2 && pointDir !== 'Right') return false;
            if (currentPlayer === 3 && pointDir !== 'Bottom') return false;
            if (currentPlayer === 4 && pointDir !== 'Left') return false;
        }
        
        // Verificar símbolo del equipo
        if (symbol === 'Circle' && (currentPlayer === 2 || currentPlayer === 4)) return false;
        if (symbol === 'Cross' && (currentPlayer === 1 || currentPlayer === 3)) return false;
    }
    
    return true;
}

//Aca resalto las posiciones validas para el cubo seleccionado
function highlightValidPositions(fromRow, fromCol) {
    const cells = document.querySelectorAll('.cube-cell');
    cells.forEach(cell => {
        const row = parseInt(cell.getAttribute('data-row'));
        const col = parseInt(cell.getAttribute('data-col'));
        
        // Resaltar todas las posiciones válidas: misma fila O misma columna, y en periferia
        const isSameRow = row === fromRow;
        const isSameCol = col === fromCol;
        const isPeriphery = row === 0 || row === 4 || col === 0 || col === 4;
        const isNotSame = !(row === fromRow && col === fromCol);
        
        if ((isSameRow || isSameCol) && isPeriphery && isNotSame) {
            cell.classList.add('valid-placement');
        }
    });
}

function isValidPlacement(fromRow, fromCol, toRow, toCol) {
    // No puede colocarse en la misma posición
    if (fromRow === toRow && fromCol === toCol) {
        console.log('Invalid: same position');
        return false;
    }
    
    // Debe estar en la misma fila o columna
    if (fromRow !== toRow && fromCol !== toCol) {
        console.log('Invalid: not in same row or column');
        return false;
    }
    
    // Debe estar en la periferia
    if (toRow !== 0 && toRow !== 4 && toCol !== 0 && toCol !== 4) {
        console.log('Invalid: not on periphery');
        return false;
    }
    
    // Si está en la misma fila, debe ser movimiento horizontal válido
    if (fromRow === toRow) {
        // Está bien, es un movimiento horizontal en la periferia
        return true;
    }
    
    // Si está en la misma columna, debe ser movimiento vertical válido
    if (fromCol === toCol) {
        // Está bien, es un movimiento vertical en la periferia
        return true;
    }
    
    return true;
}

function clearSelection() {
    if (selectedCube) {
        selectedCube.cell.classList.remove('selected');
        selectedCube = null;
    }
    
    const cells = document.querySelectorAll('.cube-cell');
    cells.forEach(cell => {
        cell.classList.remove('valid-placement');
    });
}

function showPointDirectionSelector() {
    const selector = document.getElementById('pointDirectionSelector');
    if (selector) {
        selector.style.display = 'block';
    }
}

function hidePointDirectionSelector() {
    const selector = document.getElementById('pointDirectionSelector');
    if (selector) {
        selector.style.display = 'none';
    }
}

function completeMove(pointDirection) {
    console.log('Completing move with point direction:', pointDirection);
    if (window.pendingMove) {
        console.log('Pending move:', window.pendingMove);
        makeMove(window.pendingMove.fromRow, window.pendingMove.fromCol, 
                window.pendingMove.toRow, window.pendingMove.toCol, pointDirection);
        window.pendingMove = null;
        hidePointDirectionSelector();
    } else {
        console.error('No pending move found!');
    }
}

function makeMove(fromRow, fromCol, toRow, toCol, pointDirection) {
    const gameId = document.getElementById('gameId').value;
    
    const moveData = {
        gameId: parseInt(gameId),
        fromRow: fromRow,
        fromCol: fromCol,
        toRow: toRow,
        toCol: toCol,
        pointDirection: pointDirection || null  // Asegurar que sea null si no se proporciona
    };
    
    console.log('Sending move data:', moveData);
    
    // Detener el timer
    if (window.gameTimer) {
        window.gameTimer.stop();
    }
    
    fetch('/Game/MakeMove', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(moveData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Recargar la página para mostrar el nuevo estado
            window.location.reload();
        } else {
            showMessage(data.error || 'Error al realizar el movimiento', 'danger');
            clearSelection();
            
            // Reiniciar el timer
            if (window.gameTimer) {
                window.gameTimer.start();
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showMessage('Error al realizar el movimiento', 'danger');
        clearSelection();
        
        // Reiniciar el timer
        if (window.gameTimer) {
            window.gameTimer.start();
        }
    });
}

function showMessage(message, type) {
    const messageDiv = document.getElementById('gameMessage');
    if (messageDiv) {
        messageDiv.textContent = message;
        messageDiv.className = `alert alert-${type}`;
        messageDiv.style.display = 'block';
        
        setTimeout(() => {
            messageDiv.style.display = 'none';
        }, 3000);
    }
}

