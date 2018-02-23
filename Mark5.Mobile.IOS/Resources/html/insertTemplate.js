var templateType = '%%0%%';
var templateNode = document.createElement('div');
templateNode.setAttribute('id', 'template_%%1%%');
var templateContent = `%%2%%`;
if (templateType == 'text') {
    templateNode.innerText = templateContent;
}
if (templateType == 'html') {
    templateNode.innerHTML = templateContent;
}
var editor = document.getElementById('editor');
editor.prepend(templateNode);