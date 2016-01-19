self.addEventListener('message', function (e) {
    var data = e.data;
    importScripts(data.script);

    switch (data.cmd) {
        case 'encrypt':
            var wordArray = CryptoJS.lib.WordArray.create(new Uint8Array(data.file))

            //var dataStr = ab2str(data.file);

            // encrypt the passed in file data
            var encrypted = CryptoJS.AES.encrypt(wordArray, data.key, { iv: data.iv });
            var encByteArray = wordToByteArray(encrypted);

            // patch it all back together for the trip home
            var objData =
                {
                    encrypted: encByteArray.buffer
                };

            self.postMessage(objData, [objData.encrypted]);
            break;
        case 'decrypt':
            // decrypt the passed in file data
            var decrypted = CryptoJS.AES.decrypt(data.file, data.key, { iv: data.iv });

            var fileText = decrypted.toString();

            self.postMessage(fileText);
            break;
    }
}, false);

function wordToByteArray(wordArray) {
    var byteArray = [], word, i, j;
    for (i = 0; i < wordArray.length; ++i) {
        word = wordArray[i];
        for (j = 3; j >= 0; --j) {
            byteArray.push((word >> 8 * j) & 0xFF);
        }
    }
    return byteArray;
}

function _base64ToArrayBuffer(base64) {
    var binary_string = atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}

function ab2str(buf) {
    return String.fromCharCode.apply(null, new Uint16Array(buf));
}

function str2ab(str) {
    var buf = new ArrayBuffer(str.length); // 2 bytes for each char
    var bufView = new Uint8Array(buf);
    for (var i = 0, strLen = str.length; i < strLen; i++) {
        bufView[i] = str.charCodeAt(i);
    }
    return buf;
}