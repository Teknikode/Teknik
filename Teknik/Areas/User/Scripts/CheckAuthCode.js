$(document).ready(function () {
    $("#authCheckStatus").css('display', 'none', 'important');

    $("#verifyCodeSubmit").click(function () {
        setCode = $("#code").val();
        returnUrl = $("#returnUrl").val();
        rememberMe = $("#rememberMe").val();
        if (rememberMe == '') {
            rememberMe = false;
        }
        $.ajax({
            type: "POST",
            url: confirmAuthCodeURL,
            data: {
                code: setCode,
                returnUrl: returnUrl,
                rememberMe: rememberMe
            },
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location = html.result;
                }
                else {
                    $("#authCheckStatus").css('display', 'inline', 'important');
                    $("#authCheckStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
});