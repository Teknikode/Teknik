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