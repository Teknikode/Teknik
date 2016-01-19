self.addEventListener('message', function (e) {
    var data = e.data;
    importScripts(data.script);

    switch (data.cmd) {
        case 'encrypt':
            //var startByte = 0;
            //var endByte = 0;
            //var prog = [];

            //var bytes = new Uint8Array(data.file);

            //// Create aes encryptor object
            //var aesEncryptor = CryptoJS.algo.AES.createEncryptor(data.key, { iv: data.iv });

            //while (startByte <= (bytes.length - 1)) {
            //    // Set the end byte
            //    endByte = startByte + data.chunkSize;
            //    if (endByte > bytes.length - 1)
            //    {
            //        endByte = bytes.length - 1;
            //    }

            //    // Grab current set of bytes
            //    var curBytes = bytes.subarray(startByte, endByte);
            //    var wordArray = CryptoJS.lib.WordArray.create(curBytes)

            //    // encrypt the passed in file data and add it to bits[]
            //    prog.push(aesEncryptor.process(wordArray));

            //    // Set the next start as the current end
            //    startByte = endByte + 1;
            //}//then finalize
            //prog.push(aesEncryptor.finalize());

            //throw JSON.stringify({ data: prog, start: startByte, end: endByte, len: bytes.length })
            var wordArray = CryptoJS.lib.WordArray.create(new Uint8Array(data.file));

            var encWords = CryptoJS.AES.encrypt(wordArray, data.key, { iv: data.iv, mode: CryptoJS.mode.CBC });

            //throw JSON.stringify({ data: wordArray });

            var dcBase64String = encWords.toString(); // to Base64-String
            //var encByteArray = wordToByteArray(encWords.words);
            // patch it all back together for the trip home
            var objData =
                {
                    encrypted: str2ab(dcBase64String)
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
    var bufView = new Uint16Array(buf);
    for (var i = 0, strLen = str.length; i < strLen; i++) {
        bufView[i] = str.charCodeAt(i);
    }
    return buf;
}