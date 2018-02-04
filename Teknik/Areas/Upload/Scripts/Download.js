$(document).ready(downloadFile);

function downloadFile() {
    var key = window.location.hash.substring(1);
    if (decrypt && (key == null || key == '')) {
        bootbox.prompt("Enter the file's private key", function (result) {
            if (result) {
                key = result;
            }
            processDownload(key, iv, decrypt);
        });
    }
    else {
        processDownload(key, iv, decrypt);
    }
}

function processDownload(key, iv, decrypt) {
    // speed info
    var startTime = (new Date()).getTime();

    var fd = new FormData();
    fd.append('file', fileName);
    fd.append('decrypt', !decrypt);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.open('POST', downloadDataUrl, true);
    xhr.responseType = 'arraybuffer';

    xhr.onload = function(e) {
        if (this.status === 200) {
            decryptDownload(this.response, key, iv, decrypt);
        }
    };

    xhr.onprogress = function (e) {
        if (e.lengthComputable) {
            var curTime = (new Date()).getTime();
            var elapsedTime = (curTime - startTime) / 1000;
            var speed = (e.loaded / elapsedTime);
            var percentComplete = Math.round(e.loaded * 100 / e.total);
            setProgress(percentComplete,
                'progress-bar-success progress-bar-striped active',
                percentComplete + '%',
                'Downloading File [' +
                getReadableFileSizeString(e.loaded) +
                ' / ' +
                getReadableFileSizeString(e.total) +
                ' @ ' +
                getReadableBandwidthString(speed * 8) +
                ']');
        }
    }

    xhr.onerror = function(e) {
        setProgress(100, 'progress-bar-danger', '', 'Download Failed');
    };

    xhr.onabort = function(e) {
        setProgress(100, 'progress-bar-warning', '', 'Download Aborted');
    };

    xhr.send(fd);
}

function decryptDownload(fileData, key, iv, decrypt) {
    // speed info
    var lastTime = (new Date()).getTime();
    var lastData = 0;

    // Do we need to decrypt the download?
    if (decrypt) {
        if (key !== null && key !== '' && iv !== null && iv !== '') {
            var worker = new Worker(GenerateBlobURL(encScriptSrc));

            worker.addEventListener('message',
                function (e) {
                    switch (e.data.cmd) {
                        case 'progress':
                            var curTime = (new Date()).getTime();
                            var elapsedTime = (curTime - lastTime) / 1000;
                            if (elapsedTime >= 0.1) {
                                var speed = ((e.data.processed - lastData) / elapsedTime);
                                lastTime = curTime;
                                lastData = e.data.processed;
                                var percentComplete = Math.round(e.data.processed * 100 / e.data.total);
                                setProgress(percentComplete,
                                    'progress-bar-success progress-bar-striped active',
                                    percentComplete + '%',
                                    'Decrypting [' +
                                    getReadableFileSizeString(e.data.processed) +
                                    ' / ' +
                                    getReadableFileSizeString(e.data.total) +
                                    ' @ ' +
                                    getReadableBandwidthString(speed * 8) +
                                    ']');
                            }
                            break;
                        case 'finish':
                            setProgress(100, 'progress-bar-success', 'Complete', '');
                            if (fileType == null || fileType == '') {
                                fileType = "application/octet-stream";
                            }
                            var blob = new Blob([e.data.buffer], { type: fileType });

                            // prompt save-as
                            saveAs(blob, fileName);
                            break;
                    }
                });

            worker.onerror = function (err) {
                // An error occured
                setProgress(100, 'progress-bar-danger', '', 'Error Occured');
            }

            // Create a blob for the aes script
            var scriptBlob = GenerateBlobURL(aesScriptSrc);

            // Execute worker with data
            var objData =
                {
                    cmd: 'decrypt',
                    script: scriptBlob,
                    key: key,
                    iv: iv,
                    chunkSize: chunkSize,
                    file: fileData
                };
            worker.postMessage(objData, [objData.file]);
        } else {
            setProgress(100, 'progress-bar-danger', '', 'Private Key Needed');
        }
    } else {
        // We want to just prompt the file for DL
        setProgress(100, 'progress-bar-success', 'Complete', '');

        // Convert file to blob
        if (fileType == null || fileType == '') {
            fileType = "application/octet-stream";
        }
        var blob = new Blob([fileData], { type: fileType });

        // Add the file download link
        addDownloadLink(fileData, key, iv, decrypt);

        saveAs(blob, fileName);
    }
}

function addDownloadLink(fileData, key, iv, decrypt) {

    var newItem = $('<button type="button" class="btn btn-default" id="reDownloadFile" >Download</button>');
    newItem.click(function() {
        decryptDownload(fileData, key, iv, decrypt);
    });
    $('#progress-panel').find('#progress-info').append(newItem);
}

function setProgress(percentage, classes, barMessage, title) {
    var progress = $('#progress-panel');
    if (progress !== null) {
        progress.find('#progress-bar').css('width', percentage + '%');
        progress.find('#progress-bar').removeClass();
        progress.find('#progress-bar').addClass('progress-bar');
        progress.find('#progress-bar').addClass(classes);
        progress.find('#progress-bar').html(barMessage);
        progress.find('#progress-info').html(title);
    }
}
