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
