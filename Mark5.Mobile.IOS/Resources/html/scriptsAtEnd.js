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
    else {
        window.webkit.messageHandlers.keypressed.postMessage({});
    }
});

getCaretYCoordinate = function() {
    var y = 0;
    var selection = window.getSelection();
    if (selection.rangeCount) {
        var range = selection.getRangeAt(0);
        var noStartOffset = (range.startOffset == 0);
        if (noStartOffset) {
            y = range.startContainer.offsetTop - window.pageYOffset;
        } else {
            if (range.getClientRects) {
                var rects = range.getClientRects();
                if (rects.length > 0) {
                    y = rects[0].top;
                }
            }
        }
    }

    return y;
};