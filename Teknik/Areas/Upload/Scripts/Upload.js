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
    clickable: true,
    init: function() {
        this.on("addedfile", function(file, responseText) {
            $("#upload_message").css('display', 'none', 'important');
        });
        this.on("sending", function (file, xhrObject, formData) {

            // We need to encrypt the file before upload
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
            if (progress != 100)
            {
                $(".progress").children('.progress-bar').html(progress.toFixed(2)+'%');
            }
            else
            {
                $(".progress").children('.progress-bar').html('Encrypting');
            }
        });
        this.on("queuecomplete", function() {
            $(".progress").children('.progress-bar').html('Complete');
        });
    }
};

function encryptFile(file)
{
    var key = CryptoJS.enc.Utf8.parse('8080808080808080');
    var iv = CryptoJS.enc.Utf8.parse('8080808080808080');
    var reader = new FileReader();
    reader.onload = function (e) {
        var encrypted = CryptoJS.AES.encrypt(e.target.result, key);


    }
}

function readBlob(opt_startByte, opt_stopByte) {

    var files = document.getElementById('fileinput').files;
    if (!files.length) {
        alert('Please select a file!');
        return;
    }

    var file = files[0];
    var start = parseInt(opt_startByte) || 0;
    var stop = parseInt(opt_stopByte) || file.size - 1;

    var reader = new FileReader();

    // If we use onloadend, we need to check the readyState.
    reader.onloadend = function (evt) {
        if (evt.target.readyState == FileReader.DONE) { // DONE == 2
            window.bits.push(aesEncryptor.process(evt.target.result));
        }
    };

    var blob = file.slice(start, stop + 1);
    reader.readAsBinaryString(blob);
}

function handling(evt) {

    // INITIALIZE PROGRESSIVE ENCRYPTION
    var key = CryptoJS.enc.Hex.parse(document.getElementById('pass').value);
    var iv = CryptoJS.lib.WordArray.random(128 / 8);
    window.bits = [];
    window.aesEncryptor = CryptoJS.algo.AES.createEncryptor(key, { iv: iv });

    // LOOP THROUGH BYTES AND PROGRESSIVELY ENCRYPT
    var startByte = 0;
    var endByte = 0;
    while (startByte < document.querySelector('input[type=file]').files[0].size - 1) {
        endByte = startByte + 1000000;
        readBlob(startByte, endByte);
        startByte = endByte;
    }

    // FINALIZE ENCRYPTION AND UPLOAD
    var encrypted = aesEncryptor.finalize();
    encrypted = encodeURIComponent(encrypted);
    var filename = document.getElementById('fileinput').value;
    var file_type = document.getElementById('fileinput').files[0].type;
    var url = 'data=' + encrypted + '&filename=' + filename + '&filetype=' + file_type;
    $.ajax({
        url: 'myphpscript.php',
        type: 'POST',
        data: url
    }).success(function (data) {
        // Display encrypted data
        document.getElementById('status').innerHTML = 'Upload Complete.';
    });
    alert(encrypted);

}
