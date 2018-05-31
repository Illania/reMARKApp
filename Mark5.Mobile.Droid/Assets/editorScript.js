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

document.addEventListener("keypress", function(e) {
    webViewInterface.OnKeyPressed(e.which);
    if (e.which == 13) {
        webViewInterface.OnEnterPressed(getCaretYCoordinate());
    }
    else {
        webViewInterface.OnKeyPressed(getCaretYCoordinate());
    }
});

