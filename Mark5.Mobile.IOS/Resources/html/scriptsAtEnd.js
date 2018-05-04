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
        document.getElementById('editor').style.lineHei
    }
});

getRelativeCaretYPosition = function() {
    var y = 0;
    var sel = window.getSelection();
    if (sel.rangeCount) {
        var range = sel.getRangeAt(0);
        var needsWorkAround = (range.startOffset == 0)
        /* Removing fixes bug when node name other than 'div' */
        /* && range.startContainer.nodeName.toLowerCase() == 'div'); */
        if (needsWorkAround) {
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