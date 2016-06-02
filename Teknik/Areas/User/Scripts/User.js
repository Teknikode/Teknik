$(document).ready(function () {
    $("[name='update_upload_saveKey']").bootstrapSwitch();
    $("[name='update_upload_serverSideEncrypt']").bootstrapSwitch();

    $('#ResendVerification').click(function () {
        $.ajax({
            type: "POST",
            url: resendVerifyURL,
            data: {},
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    errorMsg = html;
                    if (html.error) {
                        errorMsg = html.error;
                        if (html.error.message) {
                            errorMsg = html.error.message;
                        }
                    }
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorMsg + '</div>');
                }
            }
        });
    });

    $('#delete_account').click(function () {
        bootbox.confirm("Are you sure you want to delete your account?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteUserURL,
                    data: {},
                    success: function (html) {
                        if (html.result) {
                            window.location.replace(homeUrl);
                        }
                        else {
                            errorMsg = html;
                            if (html.error) {
                                errorMsg = html.error;
                                if (html.error.message) {
                                    errorMsg = html.error.message;
                                }
                            }
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorMsg + '</div>');
                        }
                    }
                });
            }
        });
    });

    $("#update_submit").click(function () {
        // Start Updating Animation
        $.blockUI({ message: '<div class="text-center"><h3>Updating...</h3></div>' });

        current_password = $("#update_password_current").val();
        password = $("#update_password").val();
        password_confirm = $("#update_password_confirm").val();
        update_pgp_public_key = $("#update_pgp_public_key").val();
        recovery = $("#update_recovery_email").val();
        website = $("#update_website").val();
        quote = $("#update_quote").val();
        about = $("#update_about").val();
        blog_title = $("#update_blog_title").val();
        blog_desc = $("#update_blog_description").val();
        upload_saveKey = $("#update_upload_saveKey").is(":checked");
        upload_serverSideEncrypt = $("#update_upload_serverSideEncrypt").is(":checked");
        $.ajax({
            type: "POST",
            url: editUserURL,
            data: {
                curPass: current_password,
                newPass: password,
                newPassConfirm: password_confirm,
                pgpPublicKey: update_pgp_public_key,
                recoveryEmail: recovery,
                website: website,
                quote: quote,
                about: about,
                blogTitle: blog_title,
                blogDesc: blog_desc,
                saveKey: upload_saveKey,
                serverSideEncrypt: upload_serverSideEncrypt
            },
            success: function (html) {
                $.unblockUI();
                if (html.result) {
                    window.location.reload();
                }
                else {
                    var error = html;
                    if (html.error)
                        error = html.error;
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + error + '</div>');
                }
            }
        });
        return false;
    });

    $("#reset_pass_send_submit").click(function () {
        var form = $('#reset_pass_send');
        username = $("#reset_username").val();
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: {
                username: username
            },
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>The Password Reset Link has been sent to your recovery email.</div>');
                }
                else {
                    var error = html;
                    if (html.error)
                        error = html.error;
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            data: {
                password: password,
                confirmPassword: confirmPassword
            },
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Password has successfully been reset.</div>');
                }
                else {
                    var error = html;
                    if (html.error)
                        error = html.error;
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
});