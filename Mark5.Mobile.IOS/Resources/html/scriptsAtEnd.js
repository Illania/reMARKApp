window.onload = function() {
    window.webkit.messageHandlers.loaded.postMessage({});
};

window.onresize = function() {
    window.webkit.messageHandlers.resized.postMessage({});
};

var observer = new MutationObserver(function(mutations) {
    window.webkit.messageHandlers.mutated.postMessage({});
});
observer.observe(document.querySelector('#editor'), {
    attributes: true,
    childList: true,
    characterData: true,
    subtree: true
});

document.addEventListener("keypress", function(e) {
    if (e.which == 13) {
        window.webkit.messageHandlers.enterpressed.postMessage({});
    }
    else
    {
        window.webkit.messageHandlers.keypressed.postMessage({});
    }
});