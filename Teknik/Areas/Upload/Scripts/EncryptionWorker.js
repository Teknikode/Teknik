self.addEventListener('message', function (e) {
    var data = e.data;
    importScripts(data.script);

    switch (data.cmd) {
        case 'encrypt':
            // encrypt the passed in file data
            var encrypted = CryptoJS.AES.encrypt(data.file, data.key, { iv: data.iv });

            var cipherText = encrypted.toString(); 

            self.postMessage(cipherText);
            break;
        case 'decrypt':
            // decrypt the passed in file data
            var decrypted = CryptoJS.AES.decrypt(data.file, data.key, { iv: data.iv });

            var fileText = decrypted.toString();

            self.postMessage(fileText);
            break;
    }
}, false);