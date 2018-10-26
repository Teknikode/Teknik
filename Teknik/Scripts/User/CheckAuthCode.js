$(document).ready(function () {
    $("#authCheckStatus").css('display', 'none', 'important');
    $('#Code').focus();

    $("#Code").keyup(function (event) {
        if (event.keyCode == 13) {
            $("#verifyCodeSubmit").click();
        }
    });

    $("#verifyCodeSubmit").click(function () {
        setCode = $("#Code").val();
        returnUrl = $("#ReturnUrl").val();
        rememberMe = ($("#RememberMe").val() == 'True');
        rememberDevice = $("#RememberDevice").is(":checked");
        $.ajax({
            type: "POST",
            url: confirmAuthCodeURL,
            data: AddAntiForgeryToken({
                code: setCode,
                returnUrl: returnUrl,
                rememberMe: rememberMe,
                rememberDevice: rememberDevice
            }),
            xhrFields: {
                withCredentials: true
            },
            success: function (response) {
                if (response.result) {
                    window.location = response.result;
                }
                else {
                    $("#authCheckStatus").css('display', 'inline', 'important');
                    $("#authCheckStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            },
            error: function (response) {
                $("#authCheckStatus").css('display', 'inline', 'important');
                $("#authCheckStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
            }
        });
        return false;
    });
});
