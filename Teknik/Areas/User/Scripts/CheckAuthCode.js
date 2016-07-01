$(document).ready(function () {
    $("#authCheckStatus").css('display', 'none', 'important');
    $('#Code').focus();

    $("#verifyCodeSubmit").click(function () {
        setCode = $("#Code").val();
        returnUrl = $("#ReturnUrl").val();
        rememberMe = ($("#RememberMe").val() == 'True');
        rememberDevice = $("#RememberDevice").is(":checked");
        $.ajax({
            type: "POST",
            url: confirmAuthCodeURL,
            data: {
                code: setCode,
                returnUrl: returnUrl,
                rememberMe: rememberMe,
                rememberDevice: rememberDevice
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