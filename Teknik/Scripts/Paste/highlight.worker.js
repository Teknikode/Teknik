onmessage = function (event) {
    importScripts(event.data.script);
    var result;
    if (event.data.autoDetect) {
        result = self.hljs.highlightAuto(event.data.content);
    }
    else {
        result = self.hljs.highlight(event.data.format, event.data.content);
    }
    postMessage(result);
}