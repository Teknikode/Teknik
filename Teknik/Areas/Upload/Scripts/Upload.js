$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');

    $("[name='encrypt']").bootstrapSwitch();

    linkCopyAll($('#copy-all-button'));
});

function linkUploadDelete(element, uploadID) {
    element.click(function () {
        $.ajax({
            type: "POST",
            url: generateDeleteKeyURL,
            data: { file: uploadID },
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

function linkShortenUrl(element, fileID, url) {
    element.click(function () {
        var url = $('#upload-panel-' + fileID).find('#upload-link').text();
        $.ajax({
            type: "POST",
            url: shortenURL,
            data: { url: url },
            success: function (html) {
                if (html.result) {
                    element.prop('disabled', true);
                    $('#upload-panel-' + fileID).find('#upload-link').attr('href', html.result.shortUrl);
                    $('#upload-panel-' + fileID).find('#upload-link').text(html.result.shortUrl);
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

function linkRemove(element, fileID) {
    element.click(function () {
        $('#upload-panel-' + fileID).remove();
        if ($('#upload-links').children().length == 0) {
            $("#upload-links").css('display', 'none', 'important');
            $('#copy-all-button').hide();            
        }
        return false;
    });
}

function linkCancel(element, fileID) {
    element.click(function () {
        $('#upload-panel-' + fileID).remove();
        return false;
    });
}

function linkCopyAll(element) {
    element.click(function () {
        var allUploads = [];
        $("div[id^='upload-panel-']").each(function () {
            var url = $(this).find('#upload-link').text();
            if (url !== '') {
                allUploads.push(url);
            }
        });
        if (allUploads.length > 0) {
            var urlList = allUploads.join();
            copyTextToClipboard(urlList);
        }
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

        $("#upload-links").css('display', 'inline', 'important');
        $('#copy-all-button').show();

        var itemDiv = $('#upload-template').clone();
        itemDiv.attr('id', 'upload-panel-' + fileID);

        // Hide Upload Details 
        itemDiv.find('#upload-link-panel').hide();

        // Assign buttons
        linkRemove(itemDiv.find('#upload-close'), fileID);

        // Set the info
        itemDiv.find('#upload-title').html(file.name);

        // Add the upload panel to the list
        $("#upload-links").prepend(itemDiv);

        // save ID to the file object
        file.ID = fileID;

        // Check the file size
        if (file.size <= maxUploadSize) {
            // Encrypt the file and upload it
            encryptFile(file, uploadFile);
        }
        else
        {
            // An error occured
            setProgress(fileID, 100, 'progress-bar-danger', '', 'File Too Large');
        }
        this.removeFile(file);
    }
});

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(file, callback) {
    var filetype = file.type;
    var fileID = file.ID;
    var fileExt = getFileExtension(file.name);

    // Get session settings
    var encrypt = $('#encrypt').is(':checked');

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {

            // Just send straight to server if they don't want to encrypt it
            if (!encrypt) {
                callback(e.target.result, null, null, filetype, fileExt, fileID, encrypt);
            }
            else {
                // Set variables for tracking
                var lastTime = (new Date()).getTime();
                var lastData = 0;

                // Create random key and iv (divide size by 8 for character length)
                var keyStr = randomString((keySize / 8), '#aA');
                var ivStr = randomString((blockSize / 8), '#aA');

                var worker = new Worker(GenerateBlobURL(encScriptSrc));

                worker.addEventListener('message', function (e) {
                    switch (e.data.cmd) {
                        case 'progress':
                            var curTime = (new Date()).getTime();
                            var elapsedTime = (curTime - lastTime) / 1000;
                            if (elapsedTime >= 0.1) {
                                var speed = ((e.data.processed - lastData) / elapsedTime);
                                lastTime = curTime;
                                lastData = e.data.processed;
                                var percentComplete = Math.round(e.data.processed * 100 / e.data.total);
                                setProgress(fileID, percentComplete, 'progress-bar-success progress-bar-striped active', percentComplete + '%', 'Encrypting [' + getReadableBandwidthString(speed) + ']');
                            }
                            break;
                        case 'finish':
                            if (callback != null) {
                                // Finish 
                                callback(e.data.buffer, keyStr, ivStr, filetype, fileExt, fileID, encrypt);
                            }
                            break;
                    }
                });

                worker.onerror = function (err) {
                    // An error occured
                    setProgress(fileID, 100, 'progress-bar-danger', '', 'Error Occured');
                    $('#upload-panel-' + fileID).find('.panel').addClass('panel-danger');
                }

                // Generate the script to include as a blob
                var scriptBlob = GenerateBlobURL(aesScriptSrc);

                // Execute worker with data
                var objData =
                    {
                        cmd: 'encrypt',
                        script: scriptBlob,
                        key: keyStr,
                        iv: ivStr,
                        chunkSize: chunkSize,
                        file: e.target.result
                    };
                worker.postMessage(objData, [objData.file]);
            }
        };
    })(callback);

    reader.onprogress = function (data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            setProgress(fileID, progress, 'progress-bar-success progress-bar-striped active', progress + '%', 'Loading');
        }
    }

    // Start async read
    var blob = file.slice(0, file.size);
    reader.readAsArrayBuffer(blob);
}

function uploadFile(data, key, iv, filetype, fileExt, fileID, encrypt)
{
    // Set variables for tracking
    var lastTime = (new Date()).getTime();
    var lastData = 0;

    var blob = new Blob([data]);
    // Now we need to upload the file
    var fd = new FormData();
    fd.append('fileType', filetype);
    fd.append('fileExt', fileExt);
    fd.append('iv', iv);
    fd.append('keySize', keySize);
    fd.append('blockSize', blockSize);
    fd.append('data', blob);
    fd.append('encrypt', !encrypt);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress.bind(null, fileID, lastTime, lastData), false);
    xhr.addEventListener("load", uploadComplete.bind(null, fileID, key, encrypt), false);
    xhr.addEventListener("error", uploadFailed.bind(null, fileID), false);
    xhr.addEventListener("abort", uploadCanceled.bind(null, fileID), false);
    xhr.open("POST", uploadFileURL);
    xhr.send(fd);
}



function uploadProgress(fileID, lastTime, lastData, evt) {
    var serverSideEncrypt = $('#serverSideEncrypt').is(':checked');
    if (evt.lengthComputable) {
        var curTime = (new Date()).getTime();
        var elapsedTime = (curTime - lastTime) / 1000;
        var speed = ((evt.loaded - lastData) / elapsedTime);
        lastTime = curTime;
        lastData = evt.loaded;
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        if (percentComplete == 100) {
            setProgress(fileID, 100, 'progress-bar-success progress-bar-striped active', '', 'Processing Upload');
        }
        else {
            setProgress(fileID, percentComplete, 'progress-bar-success progress-bar-striped active', percentComplete + '%', 'Uploading to Server [' + getReadableBandwidthString(speed) + ']');
        }
    }
}

function uploadComplete(fileID, key, encrypt, evt) {
    obj = JSON.parse(evt.target.responseText);
    if (obj.result != null) {
        var itemDiv = $('#upload-panel-' + fileID);
        if (itemDiv) {
            var name = obj.result.name;
            var fullName = obj.result.url;
            if (encrypt) {
                fullName = fullName + '#' + key;
            }
            var contentType = obj.result.contentType;
            var contentLength = obj.result.contentLength;

            // Set progress bar
            setProgress(fileID, 100, 'progress-bar-success', '', 'Complete');

            // Set the panel to success
            itemDiv.find('.panel').addClass('panel-success');

            // Add the upload details
            itemDiv.find('#upload-link').attr('href', fullName);
            itemDiv.find('#upload-link').text(fullName);
            itemDiv.find('#upload-contentType').html(contentType);
            itemDiv.find('#upload-contentLength').html(contentLength);

            // Setup the buttons
            linkUploadDelete(itemDiv.find('#generate-delete-link'), name);
            linkShortenUrl(itemDiv.find('#shortenUrl'), fileID, fullName);

            // Hide the progress bar
            itemDiv.find('#upload-progress-panel').hide();

            // Show the details
            itemDiv.find('#upload-link-panel').show();
        }
    }
    else
    {
        var errorMessage = 'Unable to Upload File';
        if (obj.error != null) {
            errorMessage = obj.error.message;
        }
        setProgress(fileID, 100, 'progress-bar-danger', '', errorMessage);
    }
}

function uploadFailed(fileID, evt) {
    setProgress(fileID, 100, 'progress-bar-danger', '', 'Upload Failed');
    $('#upload-panel-' + fileID).find('.panel').addClass('panel-danger');
}

function uploadCanceled(fileID, evt) {
    setProgress(fileID, 100, 'progress-bar-warning', '', 'Upload Canceled');
    $('#upload-panel-' + fileID).find('.panel').addClass('panel-warning');
}

function setProgress(fileID, percentage, classes, barMessage, title) {
    var progress = $('#upload-panel-' + fileID);
    if (progress !== null) {
        progress.find('#progress-bar').css('width', percentage + '%');
        progress.find('#progress-bar').removeClass();
        progress.find('#progress-bar').addClass('progress-bar');
        progress.find('#progress-bar').addClass(classes);
        progress.find('#progress-bar').html(barMessage);
        progress.find('#progress-info').html(title);
    }
}