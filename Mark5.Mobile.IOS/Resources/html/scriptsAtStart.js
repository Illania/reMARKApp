document.addEventListener("DOMContentLoaded", function() {
    window.webkit.messageHandlers.domloaded.postMessage({});
});

document.addEventListener("input", function() {
    window.webkit.messageHandlers.input.postMessage(getCaretYCoordinate());
});

getCaretYCoordinate = function() {
    var y = 0;
    var selection = window.getSelection();
    if (selection.rangeCount) 
    {
        var range = selection.getRangeAt(0);
        if (range.startOffset == 0 && range.startContainer.offsetTop != undefined) 
        {
            y = range.startContainer.offsetTop - window.pageYOffset;
        } 
        else if (range.getClientRects) 
        {
            var rects = range.getClientRects();
            if (rects.length > 0)
            {
                y = rects[0].top;
            }
        }
    }
    return y;
};
