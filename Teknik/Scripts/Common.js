$(document).ready(function () {
    $("#top_msg").css('display', 'none', 'important');

    $("#login_dropdown").click(function () {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
    });

    $("#login_submit").click(function () {
        var form = $('#loginForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    $("#reg_dropdown").click(function () {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
    });

    $("#reg_submit").click(function () {
        var form = $('#registrationForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
});

$(function () {
    // Setup drop down menu
    $('.dropdown-toggle').dropdown();

    $(".alert").alert();

    $("#blocker").hide();

    // Fix input element click problem
    $('.dropdown input, .dropdown label').click(function (e) {
        e.stopPropagation();
    });
});

function removeAmp(code) {
    code = code.replace(/&amp;/g, '&');
    return code;
}

String.prototype.hashCode = function () {
    var hash = 0, i, chr, len;
    if (this.length == 0) return hash;
    for (i = 0, len = this.length; i < len; i++) {
        chr = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + chr;
        hash |= 0; // Convert to 32bit integer
    }
    return hash;
};

function randomString(length, chars) {
    var mask = '';
    if (chars.indexOf('a') > -1) mask += 'abcdefghijklmnopqrstuvwxyz';
    if (chars.indexOf('A') > -1) mask += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (chars.indexOf('#') > -1) mask += '0123456789';
    if (chars.indexOf('!') > -1) mask += '~`!@#$%^&*()_+-={}[]:";\'<>?,./|\\';
    var result = '';
    for (var i = length; i > 0; --i) result += mask[Math.floor(Math.random() * mask.length)];
    return result;
}

function getFileExtension(fileName) {
    var index = fileName.lastIndexOf('.');
    return '.' + fileName.substr(index + 1);
}

function SelectAll(id) {
    document.getElementById(id).focus();
    document.getElementById(id).select();
}

/***************************** TIMER Page Load *******************************/
var loopTime;
var startTime = new Date();
var pageGenerationTime = "0.0";

function pageloadTimerCount() {
    loopTime = setTimeout("pageloadTimerCount()", 100);
}

function pageloadDoTimer() {
    pageloadTimerCount();
}

function pageloadStopTimer() {
    var timeMs = Math.floor((new Date() - startTime));

    $('#loadtime').html(timeMs);
    $('#generatetime').html(pageGenerationTime);
    $('#pagetime').show();

    clearTimeout(loopTime);
}