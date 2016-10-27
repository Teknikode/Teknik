$(document).ready(function () {
    $("#top_msg").css('display', 'none', 'important');

    $('#loginButton').removeClass('hide');

    $('#loginModal').on('shown.bs.modal', function (e) {
        $("#loginStatus").css('display', 'none', 'important');
        $("#loginStatus").html('');
        $('#loginUsername').focus();
    });

    $("#loginSubmit").click(function () {
        var form = $('#loginForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            headers: {'X-Requested-With': 'XMLHttpRequest'},
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location = html.result;
                }
                else {
                    $("#loginStatus").css('display', 'inline', 'important');
                    $("#loginStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    $('#registerButton').removeClass('hide');

    $('#registerModal').on('shown.bs.modal', function (e) {
        $("#registerStatus").css('display', 'none', 'important');
        $("#registerStatus").html('');
        $('#registerUsername').focus();
    });

    $("#registerSubmit").click(function () {
        var form = $('#registrationForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            headers: {'X-Requested-With': 'XMLHttpRequest'},
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#registerStatus").css('display', 'inline', 'important');
                    $("#registerStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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

    // for bootstrap 3 use 'shown.bs.tab', for bootstrap 2 use 'shown' in the next line
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        // save the latest tab; use cookies if you like 'em better:
        localStorage.setItem('lastTab', $(this).attr('href'));
    });

    // go to the latest tab, if it exists:
    var lastTab = localStorage.getItem('lastTab');
    if (lastTab) {
        $('[href="' + lastTab + '"]').tab('show');
    }

    // Auo-select bitcoin address
    $('#bitcoin_address_footer').on('click', 'input[type=text]', function () { this.select(); });

    // Setup anti-forgery functions
    $.appendAntiForgeryToken = function (data, token) {
        // Converts data if not already a string.
        if (data && typeof data !== "string") {
            data = $.param(data);
        }

        // Gets token from current window by default.
        token = token ? token : $.getAntiForgeryToken(); // $.getAntiForgeryToken(window).

        data = data ? data + "&" : "";
        // If token exists, appends {token.name}={token.value} to data.
        return token ? data + encodeURIComponent(token.name) + "=" + encodeURIComponent(token.value) : data;
    };

    $.getAntiForgeryToken = function (tokenWindow, appPath) {
        // HtmlHelper.AntiForgeryToken() must be invoked to print the token.
        tokenWindow = tokenWindow && typeof tokenWindow === typeof window ? tokenWindow : window;

        appPath = appPath && typeof appPath === "string" ? "_" + appPath.toString() : "";
        // The name attribute is either __RequestVerificationToken,
        // or __RequestVerificationToken_{appPath}.
        var tokenName = "__RequestVerificationToken" + appPath;

        // Finds the <input type="hidden" name={tokenName} value="..." /> from the specified window.
        // var inputElements = tokenWindow.$("input[type='hidden'][name=' + tokenName + "']");
        var inputElements = tokenWindow.document.getElementsByTagName("input");
        for (var i = 0; i < inputElements.length; i++) {
            var inputElement = inputElements[i];
            if (inputElement.type === "hidden" && inputElement.name === tokenName) {
                return {
                    name: tokenName,
                    value: inputElement.value
                };
            }
        }
    };
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

function getAnchor() {
    var currentUrl = document.URL,
    urlParts = currentUrl.split('#');

    return (urlParts.length > 1) ? urlParts[1] : null;
}

function GenerateBlobURL(url) {
    var cachedBlob = null;
    jQuery.ajax({
        url: url,
        success: function (result) {
            var workerJSBlob = new Blob([result], {
                type: "text/javascript"
            });
            cachedBlob = window.URL.createObjectURL(workerJSBlob);
        },
        async: false
    });
    return cachedBlob;
}

AddAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    return data;
};

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