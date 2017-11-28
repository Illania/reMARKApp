var paddingElement = document.getElementById('headerpadding');
if (paddingElement == null) {
    paddingElement = document.createElement('div');
    paddingElement.setAttribute('id', 'headerpadding');
    document.body.prepend(paddingElement);
}
paddingElement.setAttribute('style', 'width:100%; height:%%0%%px');