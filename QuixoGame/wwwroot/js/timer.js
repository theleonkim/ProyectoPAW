class SevenSegmentTimer {
    constructor(containerIdOrElement, initialSeconds = 0) {
        this.container = typeof containerIdOrElement === 'string'
            ? document.getElementById(containerIdOrElement)
            : containerIdOrElement;

        this.totalSeconds = initialSeconds;
        this.intervalId = null;
        this.isRunning = false;

        // Aca quemo qué segmentos se encienden para cada dígito 0–9
        this.DIGIT_SEGMENTS = {
            0: ['a', 'b', 'c', 'd', 'e', 'f'],
            1: ['b', 'c'],
            2: ['a', 'b', 'g', 'e', 'd'],
            3: ['a', 'b', 'g', 'c', 'd'],
            4: ['f', 'g', 'b', 'c'],
            5: ['a', 'f', 'g', 'c', 'd'],
            6: ['a', 'f', 'g', 'e', 'c', 'd'],
            7: ['a', 'b', 'c'],
            8: ['a', 'b', 'c', 'd', 'e', 'f', 'g'],
            9: ['a', 'b', 'c', 'd', 'f', 'g']
        };

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

        const hh = hours.toString().padStart(2, '0');
        const mm = minutes.toString().padStart(2, '0');
        const ss = seconds.toString().padStart(2, '0');
        const timeString = `${hh}${mm}${ss}`;

        const digits = this.container.querySelectorAll('.digit');
        if (digits.length !== 6) { 
            return;
        }

        digits.forEach((digitEl, index) => {
            const char = timeString[index];
            const num = parseInt(char, 10);
            this.updateDigit(digitEl, num);
        });
    }

    updateDigit(digitElement, digit) {
        const segments = digitElement.querySelectorAll('.segment');
        segments.forEach(seg => seg.classList.remove('on'));

        if (isNaN(digit) || digit < 0 || digit > 9) return;

        const activeSegments = this.DIGIT_SEGMENTS[digit] || [];
        activeSegments.forEach(segName => {
            const segEl = digitElement.querySelector(`.seg-${segName}`);
            if (segEl) segEl.classList.add('on');
        });
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const timerElements = document.querySelectorAll('[data-seven-seg-timer="true"]');

    timerElements.forEach((el, index) => {
        const initialSeconds = parseInt(el.dataset.initialSeconds || '0', 10);
        const autoStart = (el.dataset.autoStart || 'false') === 'true';
        const timerName = el.dataset.timerName || (el.id || `timer${index}`);

        const timer = new SevenSegmentTimer(el, initialSeconds);
        
        window[timerName] = timer;

        if (autoStart) {
            timer.start();
        }

        console.log(`Initialized SevenSegmentTimer '${timerName}' with ${initialSeconds}s, autoStart=${autoStart}`);
    });
});
