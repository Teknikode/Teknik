$(document).ready(function () {
    $("[name='update_upload_saveKey']").bootstrapSwitch();
    $("[name='update_upload_serverSideEncrypt']").bootstrapSwitch();
    $("[name='update_security_two_factor']").bootstrapSwitch();
    $("[name='update_security_allow_trusted']").bootstrapSwitch();

    $('#ResendVerification').click(function () {
        $.ajax({
            type: "POST",
            url: resendVerifyURL,
            data: AddAntiForgeryToken({}),
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

    $('#authenticatorSetup').on('shown.bs.modal', function (e) {
        $('#auth_setup_code').focus();
    });

    $('#authenticatorSetup').on('hide.bs.modal', function (e) {
        $("#authSetupStatus").css('display', 'none', 'important');
        $("#authSetupStatus").html('');
        $('#auth_setup_code').val('');
    });

    $('#auth_setup_verify').click(function () {
        setCode = $("#auth_setup_code").val();
        $.ajax({
            type: "POST",
            url: confirmAuthSetupURL,
            data: AddAntiForgeryToken({
                code: setCode
            }),
            success: function (html) {
                if (html.result) {
                    $("#authSetupStatus").css('display', 'inline', 'important');
                    $("#authSetupStatus").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Success!</div>');
                }
                else {
                    errorMsg = html;
                    if (html.error) {
                        errorMsg = html.error;
                        if (html.error.message) {
                            errorMsg = html.error.message;
                        }
                    }
                    $("#authSetupStatus").css('display', 'inline', 'important');
                    $("#authSetupStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorMsg + '</div>');
                }
            }
        });
    });

    $('#ClearDevices').click(function () {
        $.ajax({
            type: "POST",
            url: clearTrustedDevicesURL,
            data: AddAntiForgeryToken({}),
            success: function (html) {
                if (html.result) {
                    $('#ClearDevices').html('Clear Trusted Devices (0)');
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Successfully Cleared Trusted Devices</div>');
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

    $('#generate_token').click(function () {
        bootbox.prompt("Specify a name for this Auth Token", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: generateTokenURL,
                    data: AddAntiForgeryToken({ name: result }),
                    success: function (response) {
                        if (response.result) {
                            bootbox.dialog({
                                title: "Authentication Token",
                                message: '<label for="authToken">Make sure to copy your new personal access token now.<br />You won\'t be able to see it again!</label><input type="text" class="form-control" id="authToken" onClick="this.select();" value="' + response.result + '">',
                                callback: function () {
                                    window.location.reload();
                                }
                            });
                        }
                        else {
                            errorMsg = response;
                            if (response.error) {
                                errorMsg = response.error;
                                if (response.error.message) {
                                    errorMsg = response.error.message;
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

    $('#revoke_all_tokens').click(function () {
        bootbox.confirm("Are you sure you want to revoke all your auth tokens?<br /><br />This is <b>irreversable</b> and all applications using a token will stop working.", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: revokeAllTokensURL,
                    data: AddAntiForgeryToken({}),
                    success: function (response) {
                        if (response.result) {
                            window.location.reload();
                        }
                        else {
                            errorMsg = response;
                            if (response.error) {
                                errorMsg = response.error;
                                if (response.error.message) {
                                    errorMsg = response.error.message;
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

    $('#delete_account').click(function () {
        bootbox.confirm("Are you sure you want to delete your account?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteUserURL,
                    data: AddAntiForgeryToken({}),
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
        update_security_allow_trusted = $("#update_security_allow_trusted").is(":checked");
        update_security_two_factor = $("#update_security_two_factor").is(":checked");
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
            data: AddAntiForgeryToken({
                curPass: current_password,
                newPass: password,
                newPassConfirm: password_confirm,
                pgpPublicKey: update_pgp_public_key,
                allowTrustedDevices: update_security_allow_trusted,
                twoFactorEnabled: update_security_two_factor,
                recoveryEmail: recovery,
                website: website,
                quote: quote,
                about: about,
                blogTitle: blog_title,
                blogDesc: blog_desc,
                saveKey: upload_saveKey,
                serverSideEncrypt: upload_serverSideEncrypt
            }),
            success: function (html) {
                $.unblockUI();
                if (html.result) {
                    if (html.result.checkAuth)
                    {
                        $('#setupAuthenticatorLink').removeClass('hide');
                        $('#authSetupSecretKey').text(html.result.key);
                        $('#authQRCode').attr("src", html.result.qrUrl);
                        $('#authenticatorSetup').modal('show');
                    }
                    else
                    {
                        window.location.reload();
                    }
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
            data: AddAntiForgeryToken({
                username: username
            }),
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
            data: AddAntiForgeryToken({
                password: password,
                confirmPassword: confirmPassword
            }),
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