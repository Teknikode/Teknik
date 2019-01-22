$(document).ready(function () {
    $("#reset_pass_send_submit").click(function () {
        disableButton('#reset_pass_send_submit', 'Generating Link...');
        var form = $('#reset_pass_send');
        var username = $("#reset_username").val();
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: AddAntiForgeryToken({
                username: username
            }),
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>A Password Reset Link has been sent to your recovery email.</div>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            },
            error: function (response) {
                $("#top_msg").css('display', 'inline', 'important');
                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
            }
        }).always(function () {
            enableButton('#reset_pass_send_submit', 'Send Reset Link');
        });
        return false;
    });

    $("#setNewPass_submit").click(function () {
        disableButton('#setNewPass_submit', 'Resetting...');
        var form = $('#setNewPass');
        var password = $("#setNewPass_Password").val();
        var confirmPassword = $("#setNewPass_ConfirmPassword").val();
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: AddAntiForgeryToken({
                Password: password,
                PasswordConfirm: confirmPassword
            }),
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Password has successfully been reset.</div>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            },
            error: function (response) {
                $("#top_msg").css('display', 'inline', 'important');
                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
            }
        }).always(function () {
            enableButton('#setNewPass_submit', 'Reset Password');
        });
        return false;
    });
});
