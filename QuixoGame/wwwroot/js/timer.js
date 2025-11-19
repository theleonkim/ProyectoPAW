// Timer con display de 7 segmentos
class SevenSegmentTimer {
    constructor(containerId, initialSeconds = 0) {
        this.container = document.getElementById(containerId);
        this.totalSeconds = initialSeconds;
        this.intervalId = null;
        this.isRunning = false;
        
        if (this.container) {
            this.updateDisplay();
        }
    }
    
    start() {
        if (this.isRunning) return;
        this.isRunning = true;
        this.intervalId = setInterval(() => {
            this.totalSeconds++;
            this.updateDisplay();
        }, 1000);
    }
    
    stop() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
            this.isRunning = false;
        }
    }
    
    reset(seconds = 0) {
        this.totalSeconds = seconds;
        this.updateDisplay();
    }
    
    setTime(seconds) {
        this.totalSeconds = seconds;
        this.updateDisplay();
    }
    
    updateDisplay() {
        if (!this.container) return;
        
        const hours = Math.floor(this.totalSeconds / 3600);
        const minutes = Math.floor((this.totalSeconds % 3600) / 60);
        const seconds = this.totalSeconds % 60;
        
        const digits = this.container.querySelectorAll('.digit');
        if (digits.length >= 3) {
            this.updateDigit(digits[0], hours);
            this.updateDigit(digits[1], minutes);
            this.updateDigit(digits[2], seconds);
        }
    }
    
    updateDigit(element, value) {
        const formatted = value.toString().padStart(2, '0');
        element.textContent = formatted;
    }
}

// Inicializar timer si existe el contenedor
document.addEventListener('DOMContentLoaded', function() {
    // Esperar un momento para asegurarse de que los campos hidden estén disponibles
    setTimeout(function() {
        const timerContainer = document.getElementById('timer');
        if (timerContainer) {
            const initialTimeInput = document.getElementById('initialTime');
            const initialSeconds = initialTimeInput ? Math.floor(parseFloat(initialTimeInput.value) || 0) : 0;
            
            console.log('Initializing timer with seconds:', initialSeconds);
            
            window.gameTimer = new SevenSegmentTimer('timer', initialSeconds);
            
            // Iniciar el timer si el juego está en progreso
            const gameStatus = document.getElementById('gameStatus');
            const statusValue = gameStatus ? gameStatus.value : '';
            console.log('Game status:', statusValue);
            
            // GameStatus.InProgress = 0, así que comparamos con '0'
            if (statusValue === '0' || statusValue === 0 || statusValue === '') {
                console.log('Starting timer');
                window.gameTimer.start();
            } else {
                console.log('Game not in progress, timer not started. Status:', statusValue);
            }
        } else {
            console.log('Timer container not found');
        }
    }, 200);
});

