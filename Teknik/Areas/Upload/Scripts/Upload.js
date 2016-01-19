$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');
});

function linkUploadDelete(selector) {
    $(selector).click(function () {
        ID = encodeURIComponent($(this).attr('id'));
        $.ajax({
            type: "POST",
            url: generateDeleteKeyURL,
            data: AddAntiForgeryToken({ uploadID: ID }),
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

Dropzone.options.TeknikUpload = {
    paramName: "file", // The name that will be used to transfer the file
    maxFilesize: maxUploadSize, // MB
    addRemoveLinks: true,
    autoProcessQueue: false,
    clickable: true,
    previewTemplate: function () { },
    addedfile: function (file) {
        // Create the UI element for the new item
        var short_name = file.name.hashCode();
        $("#upload-links").css('display', 'inline', 'important');
        $("#upload-links").prepend(' \
                <div class="row link-' + short_name + '" id="link-' + short_name + '"> \
                    <div class="col-sm-12 text-center"> \
                        '+ file.name + ' \
                    </div> \
                    <div class="progress-' + short_name + '"> \
                        <div class="progress-bar progress-bar-success" id="progressBar-' + short_name + '" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 0%">0%</div> \
                    </div> \
                </div> \
              ');

        // Encrypt the file
        encryptFile(file, uploadFile);
        $("#upload_message").css('display', 'none', 'important');
    },
    init: function() {
        this.on("removedfile", function(file) {
            var name = file.name.hashCode();
            $('.link-'+name).remove();
        });
        this.on("reset", function(file, responseText) {
            $("#upload_message").css('display', 'inline', 'important');
            $(".progress").children('.progress-bar').css('width', '0%');
            $(".progress").children('.progress-bar').html('0%');
        });
        this.on("error", function(file, errorMessage) {
            this.removeFile(file);
            $("#top_msg").css('display', 'inline', 'important');
            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>'+errorMessage+'</div>');
        });
        this.on("totaluploadprogress", function(progress, totalBytes, totalBytesSent) {
            $(".progress").children('.progress-bar').css('width', (progress.toFixed(2) * (3/5)) + 40 +'%');
            $(".progress").children('.progress-bar').html(progress.toFixed(2)+'% Uploaded');
        });
        this.on("queuecomplete", function() {
            $(".progress").children('.progress-bar').html('Complete');
        });
    }
};

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(file, callback) {
    var filetype = file.type;
    var filename = file.name;
    var shortName = file.name.hashCode();

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {
            // Create random key and iv
            var keyStr = randomString(16, '#aA');
            var ivStr = randomString(16, '#aA');
            var key = CryptoJS.enc.Utf8.parse(keyStr);
            var iv = CryptoJS.enc.Utf8.parse(ivStr);

            // Display encryption message
            $(".progress-" + shortName).children('.progress-bar').css('width', '20%');
            $(".progress-" + shortName).children('.progress-bar').html('Encrypting...');

            var worker = new Worker(encScriptSrc);

            worker.addEventListener('message', function (e) {
                if (callback != null) {
                    // Finish 
                    callback(e.data.encrypted, keyStr, ivStr, filetype, filename);
                }
            });

            worker.onerror = function (err) {
                alert(err);

                // An error occured
                $(".progress-" + shortName).children('.progress-bar').css('width', '100%');
                $(".progress-" + shortName).children('.progress-bar').html('Error Occured');
            }

            // Execute worker with data
            var objData =
                {
                    cmd: 'encrypt',
                    script: aesScriptSrc,
                    key: key,
                    iv: iv,
                    chunkSize: 1024,
                    file: e.target.result
                };
            worker.postMessage(objData, [objData.file]);
        };
    })(callback);

    // Start async read
    var blob = file.slice(0, file.size);
    reader.readAsArrayBuffer(blob);
}

function uploadFile(data, key, iv, filetype, filename)
{
    $("#key").val(key);
    $("#iv").val(iv);
    var blob = new Blob([data]);
    // Now we need to upload the file
    var fd = new FormData();
    fd.append('fileType', filetype);
    fd.append('iv', iv);
    fd.append('data', blob);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress.bind(null, filename), false);
    xhr.addEventListener("load", uploadComplete.bind(null, filename), false);
    xhr.addEventListener("error", uploadFailed, false);
    xhr.addEventListener("abort", uploadCanceled, false);
    xhr.open("POST", uploadFileURL);
    xhr.send(fd);
}

function uploadProgress(filename, evt) {
    var shortName = filename.hashCode();
    if (evt.lengthComputable) {
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        $(".progress-" + shortName).children('.progress-bar').css('width', (percentComplete * (3 / 5)) + 40 + '%');
        $(".progress-" + shortName).children('.progress-bar').html(percentComplete + '% Uploaded');
    }
    else {
        document.getElementById('progressNumber').innerHTML = 'unable to compute';
    }
}

function uploadComplete(filename, evt) {
    obj = JSON.parse(evt.target.responseText);
    var name = obj.result.name;
    var fullName = obj.result.url;
    var shortName = filename.hashCode();
    $('.progress-' + shortName).children('.progress-bar').css('width', '100%');
    $('.progress-' + shortName).children('.progress-bar').html('Complete');
    $('.links-' + shortName).append(' \
                <div class="col-sm-6"> \
                ' + filename + ' \
                </div> \
                <div class="col-sm-3"> \
                <a href="' + fullName + '" target="_blank" class="alert-link">' + fullName + '</a> \
                </div> \
                <div class="col-sm-3"> \
                    <button type="button" class="btn btn-default btn-xs generate-delete-link-' + name + '" id="' + name + '">Generate Deletion URL</button> \
                </div> \
              ');
    linkUploadDelete('.generate-delete-link-' + name + '');
}

function uploadFailed(evt) {
    alert("There was an error attempting to upload the file.");
}

function uploadCanceled(evt) {
    alert("The upload has been canceled by the user or the browser dropped the connection.");
}
