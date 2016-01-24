onmessage = function (event) {
    importScripts(event.data.script);
    var result = self.hljs.highlightAuto(event.data.code);
    postMessage(result.value);
}