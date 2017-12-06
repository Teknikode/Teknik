$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');

    $("[name='encrypt']").bootstrapSwitch();

    linkCopyAll($('#copy-all'));
    linkCreateVault($('#create-vault'));

    $('#add-to-vault-menu').find('.add-to-vault').each(function () {
        linkAddToVault($(this));
    });
});

function linkUploadDelete(element, deleteUrl) {
    element.click(function () {
        bootbox.dialog({
            title: "Direct Deletion URL",
            message: '<input type="text" class="form-control" id="deletionLink" onClick="this.select();" value="' + deleteUrl + '">'
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
            $('#upload-action-buttons').hide();
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
                allUploads.unshift(url);
            }
        });
        if (allUploads.length > 0) {
            var urlList = allUploads.join();
            copyTextToClipboard(urlList);
        }
    });
}

function linkCreateVault(element) {
    element.click(function () {
        var allUploads = [];
        $("div[id^='upload-panel-']").each(function () {
            var url = $(this).find('#upload-url').val();
            if (url !== '') {
                var origFile = $(this).find('#upload-title').text();
                if (origFile !== null && origFile !== '') {
                    url += ':' + origFile;
                }
                allUploads.unshift(url);
            }
        });
        if (allUploads.length > 0) {
            var urlList = allUploads.join();
            var finalUrl = addParamsToUrl(createVaultURL, { items: encodeURIComponent(urlList) });
            window.open(finalUrl, '_blank');
        }
        else {
            window.open(createVaultURL, '_blank');
        }
    });
}

function linkAddToVault(element) {
    element.click(function () {
        var addToVaultURL = $(this).data('add-to-vault-url');
        var allUploads = [];
        $("div[id^='upload-panel-']").each(function () {
            var url = $(this).find('#upload-url').val();
            if (url !== '') {
                var origFile = $(this).find('#upload-title').text();
                if (origFile !== null && origFile !== '') {
                    url += ':' + origFile;
                }
                allUploads.unshift(url);
            }
        });
        if (allUploads.length > 0) {
            var urlList = allUploads.join();
            var finalUrl = addParamsToUrl(addToVaultURL, { items: encodeURIComponent(urlList) });
            window.open(finalUrl, '_blank');
        }
        else {
            window.open(addToVaultURL, '_blank');
        }
    });
}

var dropZone = new Dropzone(document.body, {
    url: uploadFileURL, 
    maxFilesize: maxUploadSize, // MB
    addRemoveLinks: true,
    autoProcessQueue: false,
    clickable: "#uploadButton",
    previewTemplate: function () { },
    addedfile: function (file) {

        // Convert file to blob
        var blob = file.slice(0, file.size);

        // Create the Upload
        var fileID = createUpload(file.name);

        // Process the file
        processFile(blob, file.name, file.type, file.size, fileID);

        // Remove this file from the dropzone set
        this.removeFile(file);
    }
});

var fileCount = 0;

function createUpload(fileName) {
    // Create the UI element for the new item
    var fileID = fileCount;
    fileCount++;

    $("#upload-links").css('display', 'inline', 'important');

    var itemDiv = $('#upload-template').clone();
    itemDiv.attr('id', 'upload-panel-' + fileID);

    // Hide Upload Details 
    itemDiv.find('#upload-link-panel').hide();

    // Assign buttons
    linkRemove(itemDiv.find('#upload-close'), fileID);

    // Set the info
    itemDiv.find('#upload-title').html(fileName);

    // Add the upload panel to the list
    $("#upload-links").prepend(itemDiv);

    return fileID;
}

function processFile(fileBlob, fileName, contentType, contentSize, fileID) {
    // Check the file size
    if (contentSize <= maxUploadSize) {

        var fileExt = getFileExtension(fileName);

        // Get session settings
        var encrypt = $('#encrypt').is(':checked');

        if (encrypt) {
            // Encrypt the file and upload it
            encryptFile(fileBlob, fileName, contentType, fileID, uploadFile);
        } else {
            // pass it along
            uploadFile(fileBlob, null, null, contentType, fileExt, fileID, encrypt);
        }
    }
    else {
        // An error occured
        setProgress(fileID, 100, 'progress-bar-danger', '', 'File Too Large');
    }
}

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(blob, fileName, contentType, ID, callback) {
    var fileExt = getFileExtension(fileName);

    // Get session settings
    var encrypt = $('#encrypt').is(':checked');

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {

            // Just send straight to server if they don't want to encrypt it
            if (!encrypt) {
                callback(e.target.result, null, null, contentType, fileExt, ID, encrypt);
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
                                setProgress(ID, percentComplete, 'progress-bar-success progress-bar-striped active', percentComplete + '%', 'Encrypting [' + getReadableFileSizeString(e.data.processed) + ' / ' + getReadableFileSizeString(e.data.total) + ' @ ' + getReadableBandwidthString(speed * 8) + ']');
                            }
                            break;
                        case 'finish':
                            if (callback != null) {
                                // Finish 
                                callback(e.data.buffer, keyStr, ivStr, contentType, fileExt, ID, encrypt);
                            }
                            break;
                    }
                });

                worker.onerror = function (err) {
                    // An error occured
                    setProgress(ID, 100, 'progress-bar-danger', '', 'Error Occured');
                    $('#upload-panel-' + ID).find('.panel').addClass('panel-danger');
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
            setProgress(ID, progress, 'progress-bar-success progress-bar-striped active', progress + '%', 'Loading');
        }
    }

    // Start async read
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
    if (iv != null)
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
            setProgress(fileID, percentComplete, 'progress-bar-success progress-bar-striped active', percentComplete + '%', 'Uploading to Server [' + getReadableFileSizeString(evt.loaded) + ' / ' + getReadableFileSizeString(evt.total) + ' @ ' + getReadableBandwidthString(speed * 8) + ']');
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
            var deleteUrl = obj.result.deleteUrl;

            // Set progress bar
            setProgress(fileID, 100, 'progress-bar-success', '', 'Complete');

            // Set the panel to success
            itemDiv.find('.panel').addClass('panel-success');

            // Add the upload details
            itemDiv.find('#upload-url').val(name);
            itemDiv.find('#upload-link').attr('href', fullName);
            itemDiv.find('#upload-link').text(fullName);
            itemDiv.find('#upload-contentType').html(contentType);
            itemDiv.find('#upload-contentLength').html(contentLength);

            // Setup the buttons
            linkUploadDelete(itemDiv.find('#delete-link'), deleteUrl);
            linkShortenUrl(itemDiv.find('#shortenUrl'), fileID, fullName);

            // Hide the progress bar
            itemDiv.find('#upload-progress-panel').hide();

            // Show the details
            itemDiv.find('#upload-link-panel').show();

            // Allow actions for all uploads
            $('#upload-action-buttons').show();
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
