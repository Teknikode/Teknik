$(document).ready(downloadFile);

function downloadFile() {
    var key = window.location.hash.substring(1);
    var fd = new FormData();
    fd.append('file', fileName);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.open('POST', downloadDataUrl, true);
    xhr.responseType = 'arraybuffer';

    xhr.onload = function (e) {
        if (this.status == 200) {

            if (iv != '' && key != '') {
                var worker = new Worker(GenerateBlobURL(encScriptSrc));

                worker.addEventListener('message', function (e) {
                    switch (e.data.cmd) {
                        case 'progress':
                            var percentComplete = Math.round(e.data.processed * 100 / e.data.total);
                            $("#progress").children('.progress-bar').css('width', (percentComplete / 2) + 50 + '%');
                            $("#progress").children('.progress-bar').html(percentComplete + '% Decrypted');
                            break;
                        case 'finish':
                            $('#progress').children('.progress-bar').css('width', '100%');
                            $('#progress').children('.progress-bar').html('Complete');
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
                    $("#progress").children('.progress-bar').css('width', '100%');
                    $("#progress").children('.progress-bar').removeClass('progress-bar-success');
                    $("#progress").children('.progress-bar').addClass('progress-bar-danger');
                    $("#progress").children('.progress-bar').html('Error Occured');
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
                        file: this.response
                    };
                worker.postMessage(objData, [objData.file]);
            }
            else {
                $('#progress').children('.progress-bar').css('width', '100%');
                $('#progress').children('.progress-bar').html('Complete');
                if (fileType == null || fileType == '') {
                    fileType = "application/octet-stream";
                }
                var blob = new Blob([this.response], { type: fileType });

                // prompt save-as
                saveAs(blob, fileName);
            }
        }
    };

    xhr.onprogress = function (e) {
        if (e.lengthComputable) {
            var percentComplete = Math.round(e.loaded * 100 / e.total);
            $('#progress').children('.progress-bar').css('width', (percentComplete / 2) + '%');
            $('#progress').children('.progress-bar').html(percentComplete + '% Downloaded');
        }
    };

    xhr.onerror = function (e) {
        $('#progress').children('.progress-bar').css('width', '100%');
        $('#progress').children('.progress-bar').removeClass('progress-bar-success');
        $('#progress').children('.progress-bar').addClass('progress-bar-danger');
        $('#progress').children('.progress-bar').html('Download Failed');
    };

    xhr.onabort = function (e) {
        $('#progress').children('.progress-bar').css('width', '100%');
        $('#progress').children('.progress-bar').removeClass('progress-bar-success');
        $('#progress').children('.progress-bar').addClass('progress-bar-warning');
        $('#progress').children('.progress-bar').html('Download Failed');
    };

    xhr.send(fd);
}