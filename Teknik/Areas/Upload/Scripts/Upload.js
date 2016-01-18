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
    autoProcessQueue: true,
    clickable: true,
    accept: function (file, done) {
        encryptFile(file, done);
    },
    init: function() {
        this.on("addedfile", function(file, responseText) {
            $("#upload_message").css('display', 'none', 'important');
        });
        this.on("success", function (file, response) {
            obj = JSON.parse(response);
            var name = obj.result.name;
            var fullName = obj.result.url;
            var short_name = file.name.split(".")[0].hashCode();
            $("#upload-links").css('display', 'inline', 'important');
            $("#upload-links").prepend(' \
                <div class="row link_'+short_name+'"> \
                  <div class="col-sm-6"> \
                    '+file.name+' \
                  </div> \
                  <div class="col-sm-3"> \
                    <a href="' + fullName + '" target="_blank" class="alert-link">' + fullName + '</a> \
                  </div> \
                  <div class="col-sm-3"> \
                    <button type="button" class="btn btn-default btn-xs generate-delete-link-'+short_name+'" id="'+name+'">Generate Deletion URL</button> \
                  </div> \
                </div> \
              ');
            linkUploadDelete('.generate-delete-link-'+short_name+'');
        });
        this.on("removedfile", function(file) {
            var name = file.name.split(".")[0].hashCode();
            $('.link_'+name).remove();
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
    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (theFile, callback) {
        return function (e) {
            // Create random key and iv
            var keyStr = randomString(16, '#aA');
            var ivStr = randomString(16, '#aA');
            var key = CryptoJS.enc.Utf8.parse(keyStr);
            var iv = CryptoJS.enc.Utf8.parse(ivStr);

            // Display encryption message
            $(".progress").children('.progress-bar').css('width', '20%');
            $(".progress").children('.progress-bar').html('Encrypting...');

            var worker = new Worker(encScriptSrc);

            worker.addEventListener('message', function (e) {
                // create the blob from e.data.encrypted
                theFile.data = e.data;
                theFile.key = keyStr;
                theFile.iv = ivStr;

                $("#iv").val(ivStr);

                if (callback != null) {
                    // Finish 
                    callback();
                }
            });

            // Execute worker with data
            worker.postMessage({
                cmd: 'encrypt',
                script: aesScriptSrc,
                key: key,
                iv: iv,
                file: e.target.result,
                chunkSize: 1024
            });
        };
    })(file, callback);

    // While reading, display the current progress
    reader.onprogress = function (data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            $(".progress").children('.progress-bar').css('width', (progress.toFixed(2) / 5) + '%');
            $(".progress").children('.progress-bar').html(progress.toFixed(2) + '% Loaded');
        }
    }

    // Start async read
    reader.readAsDataURL(file);
}

function encryptData(data, file, callback) {

    // 1) create the jQuery Deferred object that will be used
    var deferred = $.Deferred();

    // Create random key and iv
    var keyStr = randomString(16, '#aA');
    var ivStr = randomString(16, '#aA');
    var key = CryptoJS.enc.Utf8.parse(keyStr);
    var iv = CryptoJS.enc.Utf8.parse(ivStr);

    // Ecrypt the file
    var encData = CryptoJS.AES.encrypt(data, key, { iv: iv });

    // Save Data
    file.data = encData;
    file.key = keyStr;
    file.iv = ivStr;

    // Finish 
    callback();

    // 2) return the promise of this deferred
    return deferred.promise();
}

function readBlob(file, opt_startByte, opt_stopByte)
{
    var start = parseInt(opt_startByte) || 0;
    var stop = parseInt(opt_stopByte) || file.size - 1;

    var reader = new FileReader();

    reader.onload = (function (theFile) {
        var callback = theFile.done;
        window.processedSize += theFile.size;
        return function (e) {
            // Ecrypt the blog
            window.bits.push(window.aesEncryptor.process(evt.target.result));
            // Add the current size to the processed variable
            window.processedSize += evt.target.result.size;
            // If we have processed the entire file, let's do a finalize and call the callback
            if (window.processedSize > window.totalSize - 1) {
                window.bits.push(aesEncryptor.finalize());

                callback();
            }
        };
    })(file);

    reader.onprogress = function (data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            $(".progress").children('.progress-bar').css('width', progress.toFixed(2) + '%');
            $(".progress").children('.progress-bar').html(progress.toFixed(2) + '% Encrypted');
        }
    }

    var blob = file.slice(start, stop + 1);
    reader.readAsDataURL(blob);
}

function fileDataEncrypted(done)
{
    bits.push(aesEncryptor.finalize());
    var encrypted = window.bits.join("");
    var filename = file.name;
    var file_type = file.type;
    var fileData =
            {
                data: encrypted,
                filename: filename,
                filetype: file_type,
                iv: ivStr
            };
    return fileData;
}
