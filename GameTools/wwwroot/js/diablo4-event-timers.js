window.gameTools = window.gameTools || {};

window.gameTools.diablo4EventTimers = (function() {
    let intervalId = null;
    const localDateTimeFormatter = new Intl.DateTimeFormat(navigator.languages, {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
    const localTimeFormatter = new Intl.DateTimeFormat(navigator.languages, {
        hour: "2-digit",
        minute: "2-digit"
    });

    function formatRemaining(millisecondsRemaining) {
        if (millisecondsRemaining <= 0) {
            return "Now";
        }

        const totalSeconds = Math.floor(millisecondsRemaining / 1000);
        const days = Math.floor(totalSeconds / 86400);
        const hours = Math.floor((totalSeconds % 86400) / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        if (days > 0) {
            return `${days}d ${hours}h ${minutes}m`;
        }

        if (hours >= 2) {
            return `~${hours}h`;
        }

        if (hours > 0) {
            if (totalSeconds >= 600) {
                return `${hours}h ${minutes}m`;
            }

            return `${hours}h ${minutes}m ${seconds}s`;
        }

        if (minutes > 0 && totalSeconds >= 600) {
            return `${minutes}m`;
        }

        if (minutes > 0) {
            return `${minutes}m ${seconds}s`;
        }

        return `${seconds}s`;
    }

    function updateTimerText(container) {
        const now = Date.now();
        const timerElements = container.querySelectorAll("[data-d4-start-time]");

        for (const timerElement of timerElements) {
            const startTimeMilliseconds = Number(timerElement.getAttribute("data-d4-start-time"));
            if (Number.isNaN(startTimeMilliseconds)) {
                continue;
            }

            const millisecondsRemaining = startTimeMilliseconds - now;

            timerElement.textContent = `Starts at ${localTimeFormatter.format(new Date(startTimeMilliseconds))}, in ${formatRemaining(millisecondsRemaining)}`;
            timerElement.title = localDateTimeFormatter.format(new Date(startTimeMilliseconds));

            const eventCardElement = timerElement.closest(".d4-event-card");
            if (eventCardElement) {
                eventCardElement.classList.toggle("d4-event-card-soon", millisecondsRemaining <= 600000);

                const notificationButtonElement = eventCardElement.querySelector(".d4-notification-button");
                if (notificationButtonElement) {
                    notificationButtonElement.classList.toggle("d4-notification-button-hidden", millisecondsRemaining <= 300000);
                }
            }
        }
    }

    function stop() {
        if (intervalId !== null) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }

    function start(containerSelector) {
        stop();

        const container = document.querySelector(containerSelector);
        if (!container) {
            return;
        }

        updateTimerText(container);
        intervalId = window.setInterval(() => updateTimerText(container), 1000);
    }

    return {
        start,
        stop
    };
})();