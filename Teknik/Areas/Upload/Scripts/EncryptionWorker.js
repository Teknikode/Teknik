self.addEventListener('message', function (e) {
    importScripts(e.data.script);
    var bytes = new Uint8Array(e.data.file);

    var startByte = 0;
    var endByte = 0;
    var prog = [];

    var key = CryptoJS.enc.Utf8.parse(e.data.key);
    var iv = CryptoJS.enc.Utf8.parse(e.data.iv);

    // Create aes encryptor object
    var aesCrypto;
    switch (e.data.cmd) {
        case 'encrypt':
            aesCrypto = CryptoJS.algo.AES.createEncryptor(key, { iv: iv, mode: CryptoJS.mode.CTR, padding: CryptoJS.pad.NoPadding });
            break;
        case 'decrypt':
            aesCrypto = CryptoJS.algo.AES.createDecryptor(key, { iv: iv, mode: CryptoJS.mode.CTR, padding: CryptoJS.pad.NoPadding });
            break;
        default:
            break;
    }    

    while (startByte <= (bytes.length - 1)) {
        // Set the end byte
        endByte = startByte + e.data.chunkSize;
        if (endByte > bytes.length - 1) {
            endByte = bytes.length - 1;
        }

        // Grab current set of bytes
        var curBytes = bytes.subarray(startByte, endByte);
        //var b64encoded = btoa(String.fromCharCode.apply(null, curBytes));
        var wordArray = CryptoJS.lib.WordArray.create(curBytes)

        // encrypt the passed in file data
        var enc = aesCrypto.process(wordArray);

        // Convert and add to current array buffer
        var encStr = enc.toString(CryptoJS.enc.Base64); // to string
        prog.pushArray(_base64ToArray(encStr));

        // Send an update on progress
        var objData =
            {
                cmd: 'progress',
                processed: endByte,
                total: bytes.length - 1
            };

        self.postMessage(objData);

        // Set the next start as the current end
        startByte = endByte + 1;
    }

    //then finalize
    var encFinal = aesCrypto.finalize();
    var finalStr = encFinal.toString(CryptoJS.enc.Base64); // to final string
    prog.pushArray(_base64ToArray(finalStr));

    var objData =
        {
            cmd: 'progress',
            processed: bytes.length - 1,
            total: bytes.length - 1
        };

    // convert array to ArrayBuffer
    var arBuf = _arrayToArrayBuffer(prog);

    //throw JSON.stringify({ dataLength: prog.length, len: bytes.length, finalLength: arBuf.byteLength })

    // Now package it into a mesage to send home
    var objData =
        {
            cmd: 'finish',
            buffer: arBuf
        };

    self.postMessage(objData, [objData.buffer]);
}, false);

function _arrayToArrayBuffer(array) {
    var len = array.length;
    var bytes = new Uint8Array(len);
    bytes.set(array, 0);
    return bytes.buffer;
}

function _base64ToArray(base64) {
    var binary_string = atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes;
}

Array.prototype.pushArray = function (arr) {
    this.push.apply(this, arr);
};