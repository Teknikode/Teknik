$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');
});

function linkSaveKey(selector, uploadID, key, fileID) {
    $(selector).click(function () {
        $.ajax({
            type: "POST",
            url: saveKeyToServerURL,
            data: AddAntiForgeryToken({ uploadID: uploadID, key: key }),
            success: function (html) {
                if (html.result) {
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkUploadDelete(selector, uploadID) {
    $(selector).click(function () {
        $.ajax({
            type: "POST",
            url: generateDeleteKeyURL,
            data: AddAntiForgeryToken({ uploadID: uploadID }),
            success: function (html) {
                if (html.result) {
                    bootbox.dialog({
                        title: "Direct Deletion URL",
                        message: '<input type="text" class="form-control" id="deletionLink" onClick="this.select();" value="' + html.result + '">'
                    });

                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkRemove(selector, fileID) {
    $(selector).click(function () {
        $('#link-' + fileID).remove();
        return false;
    });
}

var fileCount = 0;

var dropZone = new Dropzone(document.body, {
    url: uploadFileURL, 
    maxFilesize: maxUploadSize, // MB
    addRemoveLinks: true,
    autoProcessQueue: false,
    clickable: "#uploadButton",
    previewTemplate: function () { },
    addedfile: function (file) {
        // Create the UI element for the new item
        var fileID = fileCount;
        fileCount++;
        // save ID to the file object
        file.ID = fileID;
        $("#upload-links").css('display', 'inline', 'important');
        $("#upload-links").prepend(' \
                <div class="panel panel-default" id="link-' + fileID + '"> \
                    <div class="panel-heading text-center" id="link-header-' + fileID + '">'+ file.name + '</div> \
                    <div class="panel-body" id="link-panel-' + fileID + '"> \
                        <div class="row"> \
                            <div class="col-sm-12 text-center" id="upload-link-' + fileID + '"></div> \
                        </div> \
                        <div class="row"> \
                            <div class="col-sm-12 text-center"> \
                                <div class="progress" id="progress-' + fileID + '"> \
                                    <div class="progress-bar progress-bar-success" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 0%">0%</div> \
                                </div> \
                            </div> \
                        </div> \
                    </div> \
                </div> \
              ');

        // Encrypt the file
        encryptFile(file, uploadFile);
        this.removeFile(file);
    }
});

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(file, callback) {
    var filetype = file.type;
    var fileID = file.ID;

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {
            // Create random key and iv
            var keyStr = randomString(24, '#aA');
            var ivStr = randomString(24, '#aA');

            var worker = new Worker(encScriptSrc);

            worker.addEventListener('message', function (e) {
                switch (e.data.cmd)
                {
                    case 'progress':
                        var percentComplete = Math.round(e.data.processed * 100 / e.data.total);
                        $("#progress-" + fileID).children('.progress-bar').css('width', (percentComplete * (2 / 5)) + 20 + '%');
                        $("#progress-" + fileID).children('.progress-bar').html(percentComplete + '% Encrypted');
                        break;
                    case 'finish':
                        if (callback != null) {
                            // Finish 
                            callback(e.data.encrypted, keyStr, ivStr, filetype, fileID);
                        }
                        break;
                }
            });

            worker.onerror = function (err) {
                // An error occured
                $("#progress-" + fileID).children('.progress-bar').css('width', '100%');
                $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
                $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
                $("#progress-" + fileID).children('.progress-bar').html('Error Occured');
            }

            // Execute worker with data
            var objData =
                {
                    cmd: 'encrypt',
                    script: aesScriptSrc,
                    key: keyStr,
                    iv: ivStr,
                    chunkSize: chunkSize,
                    file: e.target.result
                };
            worker.postMessage(objData, [objData.file]);
        };
    })(callback);

    reader.onprogress = function (data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            $('#progress-' + fileID).children('.progress-bar').css('width', (progress / 5) + '%');
            $('#progress-' + fileID).children('.progress-bar').html(progress + '% Loaded');
        }
    }

    // Start async read
    var blob = file.slice(0, file.size);
    reader.readAsArrayBuffer(blob);
}

function uploadFile(data, key, iv, filetype, fileID)
{
    $('#key-' + fileID).val(key);
    var blob = new Blob([data]);
    // Now we need to upload the file
    var fd = new FormData();
    fd.append('fileType', filetype);
    fd.append('iv', iv);
    fd.append('data', blob);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress.bind(null, fileID), false);
    xhr.addEventListener("load", uploadComplete.bind(null, fileID, key), false);
    xhr.addEventListener("error", uploadFailed.bind(null, fileID), false);
    xhr.addEventListener("abort", uploadCanceled.bind(null, fileID), false);
    xhr.open("POST", uploadFileURL);
    xhr.send(fd);
}

function uploadProgress(fileID, evt) {
    if (evt.lengthComputable) {
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        $('#progress-' + fileID).children('.progress-bar').css('width', (percentComplete * (2 / 5)) + 60 + '%');
        $('#progress-' + fileID).children('.progress-bar').html(percentComplete + '% Uploaded');
    }
}

function uploadComplete(fileID, key, evt) {
    obj = JSON.parse(evt.target.responseText);
    var name = obj.result.name;
    var fullName = decodeURIComponent(obj.result.url);
    var keyVar = decodeURIComponent(obj.result.keyVar);
    fullName = fullName.replace(keyVar, key);
    $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
    $('#progress-' + fileID).children('.progress-bar').html('Complete');
    $('#upload-link-' + fileID).html('<p><a href="' + fullName + '" target="_blank" class="alert-link">' + fullName + '</a></p>');
    $('#link-' + fileID).append(' \
                <div class="panel-footer"> \
                    <div class="row"> \
                        <div class="col-sm-4 text-center"> \
                            <button type="button" class="btn btn-default btn-sm" id="save-key-link-' + fileID + '">Save Key On Server</button> \
                        </div> \
                        <div class="col-sm-4 text-center"> \
                            <button type="button" class="btn btn-default btn-sm" id="generate-delete-link-' + fileID + '">Generate Deletion URL</button> \
                        </div> \
                        <div class="col-sm-4 text-center"> \
                            <button type="button" class="btn btn-default btn-sm" id="remove-link-' + fileID + '">Remove From List</button> \
                        </div> \
                    </div> \
                </div> \
              ');
    linkSaveKey('#save-key-link-' + fileID + '', name, key, fileID);
    linkUploadDelete('#generate-delete-link-' + fileID + '', name);
    linkRemove('#remove-link-' + fileID + '', fileID);
}

function uploadFailed(fileID, evt) {
    $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
    $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
    $('#progress-' + fileID).children('.progress-bar').html('Upload Failed');
}

function uploadCanceled(fileID, evt) {
    $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
    $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-warning');
    $('#progress-' + fileID).children('.progress-bar').html('Upload Canceled');
}
