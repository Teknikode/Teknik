$(document).ready(function () {
    $("#reset_pass_send_submit").click(function () {
        var form = $('#reset_pass_send');
        username = $("#reset_username").val();
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: AddAntiForgeryToken({
                username: username
            }),
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>The Password Reset Link has been sent to your recovery email.</div>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }
            }
        });
        return false;
    });

    $("#setNewPass_submit").click(function () {
        var form = $('#setNewPass');
        password = $("#setNewPass_Password").val();
        confirmPassword = $("#setNewPass_ConfirmPassword").val();
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: AddAntiForgeryToken({
                Password: password,
                PasswordConfirm: confirmPassword
            }),
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Password has successfully been reset.</div>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }
            }
        });
        return false;
    });
});
