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
            return "0s";
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

    function getRunningDots(nowMilliseconds) {
        const dotCount = Math.floor(nowMilliseconds / 1000) % 5;
        return ".".repeat(dotCount);
    }

    function clamp(value, min, max) {
        return Math.max(min, Math.min(max, value));
    }

    function updateProgressBar(eventCardElement, now, startTimeMilliseconds) {
        const progressElement = eventCardElement.querySelector(".d4-event-progress");
        if (!progressElement) {
            return;
        }

        const progressFillElement = progressElement.querySelector(".d4-event-progress-fill");
        if (!progressFillElement) {
            return;
        }

        const endTimeMilliseconds = Number(eventCardElement.getAttribute("data-d4-progress-end"));
        if (Number.isNaN(endTimeMilliseconds) || endTimeMilliseconds <= startTimeMilliseconds) {
            progressElement.classList.remove("d4-event-progress-visible");
            progressFillElement.style.width = "0%";
            return;
        }

        const isRunning = now >= startTimeMilliseconds && now <= endTimeMilliseconds;
        progressElement.classList.toggle("d4-event-progress-visible", isRunning);

        if (!isRunning) {
            progressFillElement.style.width = "0%";
            return;
        }

        const elapsedMilliseconds = now - startTimeMilliseconds;
        const totalMilliseconds = endTimeMilliseconds - startTimeMilliseconds;
        const progress = clamp(elapsedMilliseconds / totalMilliseconds, 0, 1) * 100;
        progressFillElement.style.width = `${progress.toFixed(2)}%`;
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
            const startTime = new Date(startTimeMilliseconds);
            const isRunning = millisecondsRemaining <= 0;
            const elapsedMilliseconds = now - startTimeMilliseconds;
            const eventCardElement = timerElement.closest(".d4-event-card");
            const endTimeMilliseconds = eventCardElement
                ? Number(eventCardElement.getAttribute("data-d4-progress-end"))
                : Number.NaN;
            const hasValidEndTime = !Number.isNaN(endTimeMilliseconds) && endTimeMilliseconds > startTimeMilliseconds;

            if (isRunning) {
                const dots = getRunningDots(now);
                if (hasValidEndTime) {
                    const millisecondsUntilEnd = Math.max(0, endTimeMilliseconds - now);
                    const formattedRemaining = formatRemaining(millisecondsUntilEnd);
                    const shouldUseApproximateMinutes = millisecondsUntilEnd >= 600000 && millisecondsUntilEnd < 3600000;
                    const remainingWithApproximation = shouldUseApproximateMinutes
                        ? `~${formattedRemaining}`
                        : formattedRemaining;

                    timerElement.textContent = `Started ${localTimeFormatter.format(startTime)}, ends in ${remainingWithApproximation}${dots}`;
                } else {
                    timerElement.textContent = `Started ${localTimeFormatter.format(startTime)}, running for ${formatRemaining(elapsedMilliseconds)}${dots}`;
                }
            } else {
                const formattedRemaining = formatRemaining(millisecondsRemaining);
                const normalizedRemaining = formattedRemaining.startsWith("~")
                    ? formattedRemaining.substring(1)
                    : formattedRemaining;
                const remainingWithApproximation = millisecondsRemaining >= 600000
                    ? `~${normalizedRemaining}`
                    : normalizedRemaining;

                timerElement.textContent = `Starts ${localTimeFormatter.format(startTime)}, in ${remainingWithApproximation}`;
            }
            timerElement.title = localDateTimeFormatter.format(new Date(startTimeMilliseconds));

            if (eventCardElement) {
                eventCardElement.classList.toggle("d4-event-card-soon", millisecondsRemaining <= 600000);
                updateProgressBar(eventCardElement, now, startTimeMilliseconds);

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