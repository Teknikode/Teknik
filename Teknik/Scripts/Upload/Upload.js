/* globals shortenURL, createVaultURL, uploadFileURL, maxUploadSize, keySize, blockSize, encScriptSrc, aesScriptSrc, chunkSize */
$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');

    $('[data-toggle="popover"]').popover();

    $('[data-toggle="popover"]').on('shown.bs.popover', function () {
        var $this = $(this);
        setTimeout(function () {
            $this.popover('hide');
        }, 3000);
    });

    $("[name='encrypt']").bootstrapSwitch({ size: "small" });

    // Initialize the widths
    setExpireWidth($("#expireunit").val());

    linkExpireSelect($("#expireunit"));

    linkCopyAll($('#copy-all'));
    linkCreateVault($('#create-vault'));

    $('#add-to-vault-menu').find('.add-to-vault').each(function () {
        linkAddToVault($(this));
    });

    document.onpaste = function (event) {
        var items = (event.clipboardData || event.originalEvent.clipboardData).items;
        for (var index in items) {
            if (!_.isUndefined(index) && !_.isNull(index)) {
                var item = items[index];
                if (!_.isUndefined(item) && !_.isNull(item) && item.kind === 'file') {
                    // Convert file to blob
                    var file = item.getAsFile();
                    if (!_.isUndefined(file) && !_.isNull(file)) {
                        // Upload the file
                        upload(file);
                    }
                }
            }
        }
    }
});

function linkExpireSelect(element) {
    element.change(function () {
        setExpireWidth($(this).val());
    });
}

function linkUploadDelete(element, deleteUrl) {
    element.click(function () {
        var dialog = bootbox.dialog({
            title: "Direct Deletion URL",
            message: '<input type="text" class="form-control" id="deletionLink" value="' + deleteUrl + '">'
        });

        dialog.init(function() {
            dialog.find('#deletionLink').click(function() {
                    $(this).select();
                });
        });
        return false;
    });
}

function linkShortenUrl(element, fileID) {
    element.click(function () {
        var url = $('#upload-panel-' + fileID).find('#upload-link').text();
        $.ajax({
            type: "POST",
            url: shortenURL,
            data: { url: url },
            success: function (response) {
                if (response.result) {
                    element.prop('disabled', true);
                    $('#upload-panel-' + fileID).find('#upload-link').attr('href', response.result.shortUrl);
                    $('#upload-panel-' + fileID).find('#upload-link').text(response.result.shortUrl);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });
}

function linkRemove(element, fileID, token) {
    element.click(function () {
        if (token.isCancelable()) {
            token.cancel();
        } else {
            $('#upload-panel-' + fileID).remove();
            if ($('#upload-links').children().length == 0) {
                $("#upload-links").css('display', 'none', 'important');
                $('#upload-action-buttons').hide();
            }
        }
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
        element.popover('show');
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

function setExpireWidth(unit) {
    if (unit === "Never") {
        $('#length-div').addClass("hidden");
        $('#unit-div').removeClass("col-sm-5");
        $('#unit-div').addClass("col-sm-9");
    }
    else {
        $('#length-div').removeClass("hidden");
        $('#unit-div').removeClass("col-sm-9");
        $('#unit-div').addClass("col-sm-5");
    }
}

$(document.body).dropzone({
    url: uploadFileURL, 
    maxFilesize: maxUploadSize, // MB
    addRemoveLinks: true,
    autoProcessQueue: false,
    clickable: "#uploadButton",
    previewTemplate: function () { },
    addedfile: function (file) {
        // Upload the file to the server
        upload(file);

        // Remove this file from the dropzone set
        this.removeFile(file);
    }
});

function upload(file) {
    // Convert file to blob
    var blob = file.slice(0, file.size);

    // Create a token for this upload
    var token = {
        callback: null,
        cancel: function () {
            this.callback();
        },
        isCancelable: function () {
            return this.callback !== null;
        }
    };

    // Create the Upload
    var fileID = createUpload(file.name, token);

    // Process the file
    processFile(blob, file.name, file.type, file.size, fileID, token);
}

var fileCount = 0;

function createUpload(fileName, token) {
    // Create the UI element for the new item
    var fileID = fileCount;
    fileCount++;

    $("#upload-links").css('display', 'inline', 'important');

    var itemDiv = $('#upload-template').clone();
    itemDiv.attr('id', 'upload-panel-' + fileID);

    // Hide Upload Details 
    itemDiv.find('#upload-link-panel').hide();

    // Assign buttons
    linkRemove(itemDiv.find('#upload-close'), fileID, token);

    // Set the info
    itemDiv.find('#upload-title').html(fileName);

    // Add the upload panel to the list
    $("#upload-links").prepend(itemDiv);

    return fileID;
}

function processFile(fileBlob, fileName, contentType, contentSize, fileID, token) {
    // Check the file size
    if (contentSize <= maxUploadSize) {

        var fileExt = getFileExtension(fileName);

        // Get session settings
        var encrypt = $('#encrypt').is(':checked');
        var expireUnit = $("#expireunit").val();
        var expireLength = $("#expirelength").val();

        var options = {
            encrypt: encrypt,
            expirationUnit: expireUnit,
            expirationLength: expireLength
        }

        if (options.encrypt) {
            // Encrypt the file and upload it
            encryptFile(fileBlob, fileName, contentType, fileID, uploadFile, options, token);
        } else {
            // pass it along
            uploadFile(fileBlob, null, null, contentType, fileExt, fileID, options, token);
        }
    }
    else {
        // An error occured
        setProgress(fileID, 100, 'progress-bar-danger', '', 'File Too Large');
    }
}

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(blob, fileName, contentType, ID, callback, options, token) {
    var fileExt = getFileExtension(fileName);

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {
            // Just send straight to server if they don't want to encrypt it
            if (!options.encrypt) {
                callback(e.target.result, null, null, contentType, fileExt, ID, options, token);
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
                                callback(e.data.buffer, keyStr, ivStr, contentType, fileExt, ID, options, token);
                            }
                            break;
                    }
                });

                worker.onerror = function (err) {
                    uploadFailed(ID, token, err);
                }

                token.callback = function () {  // SPECIFY CANCELLATION
                    worker.terminate(); // terminate the worker process

                    uploadCanceled(ID, token, null);
                };

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

    reader.onerror = function (err) {
        uploadFailed(ID, token, err);
    }

    reader.onprogress = function(data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            setProgress(ID, progress, 'progress-bar-success progress-bar-striped active', progress + '%', 'Loading');
        }
    };

    reader.onabort = function() {
        uploadCanceled(ID, token, null);
    };

    token.callback = function () {  // SPECIFY CANCELLATION
        reader.abort(); // abort request
    };

    // Start async read
    reader.readAsArrayBuffer(blob);
}

function uploadFile(data, key, iv, filetype, fileExt, fileID, options, token)
{
    // Set variables for tracking
    var startTime = (new Date()).getTime();

    var blob = new Blob([data]);
    // Now we need to upload the file
    var fd = new FormData();
    fd.append('fileType', filetype);
    fd.append('fileExt', fileExt);
    if (iv != null)
        fd.append('iv', iv);
    fd.append('keySize', keySize);
    fd.append('blockSize', blockSize);
    fd.append('options', JSON.stringify(options));
    fd.append('file', blob);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress.bind(null, fileID, startTime), false);
    xhr.addEventListener("load", uploadComplete.bind(null, fileID, key, options, token), false);
    xhr.addEventListener("error", uploadFailed.bind(null, fileID, token), false);
    xhr.addEventListener("abort", uploadCanceled.bind(null, fileID, token), false);

    token.callback = function () {  // SPECIFY CANCELLATION
        xhr.abort(); // abort request
    };

    xhr.open("POST", uploadFileURL);
    xhr.send(fd);
}

function uploadProgress(fileID, startTime, evt) {
    if (evt.lengthComputable) {
        var curTime = (new Date()).getTime();
        var elapsedTime = (curTime - startTime) / 1000;
        var speed = (evt.loaded / elapsedTime);
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        if (percentComplete == 100) {
            setProgress(fileID, 100, 'progress-bar-success progress-bar-striped active', '', 'Processing Upload');
        }
        else {
            setProgress(fileID, percentComplete, 'progress-bar-success progress-bar-striped active', percentComplete + '%', 'Uploading to Server [' + getReadableFileSizeString(evt.loaded) + ' / ' + getReadableFileSizeString(evt.total) + ' @ ' + getReadableBandwidthString(speed * 8) + ']');
        }
    }
}

function uploadComplete(fileID, key, options, token, evt) {
    // Cancel out cancel token
    token.callback = null;

    try {
        var obj = JSON.parse(evt.target.responseText);
        if (obj.result != null) {
            var itemDiv = $('#upload-panel-' + fileID);
            if (itemDiv) {
                var name = obj.result.name;
                var fullName = obj.result.url;
                if (options.encrypt) {
                    fullName = fullName + '#' + key;
                }
                var contentType = obj.result.contentType;
                var contentLength = obj.result.contentLength;
                var deleteUrl = obj.result.deleteUrl;
                var expirationUnit = obj.result.expirationUnit;
                var expirationLength = obj.result.expirationLength;

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

                var expirationMessage = expirationUnit;
                if (expirationUnit !== "Never") {
                    expirationMessage = expirationLength + ' ' + expirationUnit;
                }
                itemDiv.find('#upload-expiration').html(expirationMessage);

                // Setup the buttons
                linkUploadDelete(itemDiv.find('#delete-link'), deleteUrl);
                linkShortenUrl(itemDiv.find('#shortenUrl'), fileID);

                // Hide the progress bar
                itemDiv.find('#upload-progress-panel').hide();

                // Show the details
                itemDiv.find('#upload-link-panel').show();

                // Allow actions for all uploads
                $('#upload-action-buttons').show();
            }
        }
        else {
            var errorMessage = 'Unable to Upload File';
            if (obj.error != null) {
                errorMessage = obj.error.message;
            }
            setProgress(fileID, 100, 'progress-bar-danger', '', errorMessage);
        }
    }
    catch(err) {
        setProgress(fileID, 100, 'progress-bar-danger', '', 'Unable to Upload File: ' + parseErrorMessage(err));
    }
}

function uploadFailed(fileID, token, evt) {
    // Cancel out cancel token
    token.callback = null;

    setProgress(fileID, 100, 'progress-bar-danger', '', 'Upload Failed: ' + parseErrorMessage(evt));
    $('#upload-panel-' + fileID).find('.panel').addClass('panel-danger');
}

function uploadCanceled(fileID, token) {
    // Cancel out cancel token
    token.callback = null;

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
