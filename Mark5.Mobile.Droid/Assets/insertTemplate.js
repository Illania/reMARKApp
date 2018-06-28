var templateType = '%%0%%';
var templateNode = document.createElement('div');
templateNode.setAttribute('id', 'template_%%1%%');
var templateContent = `%%2%%`;

var editor = document.getElementById('editor');
var selection = window.getSelection();
var range = selection.getRangeAt(0);
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