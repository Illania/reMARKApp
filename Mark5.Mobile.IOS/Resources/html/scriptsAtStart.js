document.addEventListener("DOMContentLoaded", function() {
    window.webkit.messageHandlers.domloaded.postMessage({});
});