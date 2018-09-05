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

var savedRange = null;

document.addEventListener("selectionchange", HandleSelectionChange, false);

function HandleSelectionChange() {
    var sel = window.getSelection && window.getSelection();
    if (sel && sel.rangeCount > 0) {
        savedRange = sel.getRangeAt(0);
    }
};

function InsertContent(type, id, content) {
    var templateType = type;
    var templateNode = document.createElement('div');
    templateNode.setAttribute('id', 'template_' + id);
    var templateContent = content;

    var selection = window.getSelection();
    if(selection !== undefined || savedRange !== null) {
        var range = savedRange;
        range.deleteContents();

        if (templateType == 'text') {
            templateNode.innerText = templateContent;
        }
        if (templateType == 'html') {
            templateNode.innerHTML = templateContent;
        }

        var fragment = document.createDocumentFragment(), node, lastNode;

        while((node = templateNode.firstChild)) {
            lastNode = fragment.appendChild(node);
        }

        range.insertNode(fragment);

        if (lastNode) {
            range = range.cloneRange();
            range.setStartAfter(lastNode);
            range.collapse(true); 
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }
};

