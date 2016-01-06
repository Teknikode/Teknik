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
    accept: function (file, done) {
        file.done = done;
        encryptFile(file);
    },
    init: function() {
        this.on("addedfile", function(file, responseText) {
            $("#upload_message").css('display', 'none', 'important');
        });
        this.on("success", function(file, response) {
            var name = response.result;
            var short_name = file.name.split(".")[0].hashCode();
            $("#upload-links").css('display', 'inline', 'important');
            $("#upload-links").prepend(' \
                <div class="row link_'+short_name+'"> \
                  <div class="col-sm-6"> \
                    '+file.name+' \
                  </div> \
                  <div class="col-sm-3"> \
                    <a href='+uploadURL+'/'+name+'" target="_blank" class="alert-link">'+uploadURL+'/'+name+'</a> \
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
            $(".progress").children('.progress-bar').css('width', progress.toFixed(2)+'%');
            $(".progress").children('.progress-bar').html(progress.toFixed(2)+'%');
        });
        this.on("queuecomplete", function() {
            $(".progress").children('.progress-bar').html('Complete');
        });
    }
};

function readBlob(file, opt_startByte, opt_stopByte)
{
    var start = parseInt(opt_startByte) || 0;
    var stop = parseInt(opt_stopByte) || file.size - 1;

    var reader = new FileReader();

    reader.onload = (function (theFile) {
        var callback = theFile.callback;
        return function (e) {
            window.bits.push(window.aesEncryptor.process(evt.target.result));
            callback();
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
    reader.readAsArrayBuffer(blob);
}

function encryptFile(file)
{
    // INITIALIZE PROGRESSIVE ENCRYPTION
    window.keyStr = randomString(16, '#aA');
    window.ivStr = randomString(16, '#aA');
    var key = CryptoJS.enc.Utf8.parse(keyStr);
    var iv = CryptoJS.enc.Utf8.parse(ivStr);
    window.aesEncryptor = CryptoJS.algo.AES.createEncryptor(key, { iv: iv });

    // LOOP THROUGH BYTES AND PROGRESSIVELY ENCRYPT

    window.bits = []

    var startByte = 0;
    var endByte = 0;
    while (startByte <= file.size - 1) {
        endByte = startByte + 1024;
        readBlob(file, startByte, endByte);
        startByte = endByte;
    }
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
