/* globals resendVerifyURL, generate2FAURL, disable2FAURL, resetRecoveryCodesURL, confirmAuthSetupURL, editURL */
$(document).ready(function () {
    $('#ResendVerification').click(function () {
        $.ajax({
            type: "POST",
            url: resendVerifyURL,
            data: AddAntiForgeryToken({}),
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-info alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Recovery Email Verification Sent.</div>');
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
        });
    });

    $('#disable_2fa_button').click(function () {
        bootbox.confirm({
            size: "small",
            message: "Are you sure you want to disable Two-Factor Authentication?  This will also invalidate all current recovery codes.",
            buttons: {
                confirm: {
                    className: 'btn-danger',
                    label: 'Disable'
                }
            },
            callback: function (result) {
                if (result) {
                    disableButton('#disable_2fa_button', 'Disabling...');
                    $.ajax({
                        type: "POST",
                        url: disable2FAURL,
                        data: AddAntiForgeryToken({}),
                        success: function (response) {
                            if (response.result) {
                                window.location.reload(true);
                            }
                            else {
                                enableButton('#disable_2fa_button', 'Disable');
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                            }
                        },
                        error: function (response) {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
                        }
                    }).always(function () {
                        enableButton('#disable_2fa_button', 'Disable');
                    });
                }
            }
        });
    });

    $('#enable_2fa_button').click(function () {
        disableButton('#enable_2fa_button', 'Generating Key...');
        $.ajax({
            type: "POST",
            url: generate2FAURL,
            data: AddAntiForgeryToken({}),
            success: function (response) {
                if (response.result) {
                    $('#authQRCode').attr('src', response.qrUrl);
                    $('#authSetupSecretKey').html(response.key);

                    $('#authenticatorSetup').modal('show');
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
            enableButton('#enable_2fa_button', 'Enable');
        });
    });

    $('#resetRecoveryCodes').click(function () {
        bootbox.confirm({
            size: "small",
            message: "Are you sure you want to reset your recovery codes?  This will invalidate all current recovery codes.",
            buttons: {
                confirm: {
                    className: 'btn-danger',
                    label: 'Reset'
                }
            },
            callback: function (result) {
                if (result) {
                    $.ajax({
                        type: "POST",
                        url: resetRecoveryCodesURL,
                        data: AddAntiForgeryToken({}),
                        success: function (response) {
                            if (response.result) {
                                bootbox.dialog({
                                    closeButton: false,
                                    buttons: {
                                        close: {
                                            label: 'Close',
                                            className: 'btn-primary'
                                        }
                                    },
                                    title: "Recovery Codes",
                                    message: '<div class="alert alert-warning">Make sure to copy your recovery codes. You won\'t be able to see them again!</div><div class="text-center">' + response.recoveryCodes.join('<br />') + '</div>'
                                });
                            }
                            else {
                                enableButton('#disable_2fa_button', 'Disable');
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                            }
                        },
                        error: function (response) {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
                        }
                    });
                }
            }
        });
    });

    $('#authenticatorSetup').on('shown.bs.modal', function () {
        $('#auth_setup_code').focus();
    });

    $('#authenticatorSetup').on('hide.bs.modal', function () {
        $("#authSetupStatus").css('display', 'none', 'important');
        $("#authSetupStatus").html('');
        $('#authQRCode').attr('src', '');
        $('#authSetupSecretKey').html('');
        $('#auth_setup_code').val('');
    });

    $('#auth_setup_verify').click(function () {
        disableButton('#auth_setup_verify', 'Verifying...');
        var setCode = $("#auth_setup_code").val();
        $.ajax({
            type: "POST",
            url: confirmAuthSetupURL,
            data: AddAntiForgeryToken({
                code: setCode
            }),
            success: function (response) {
                if (response.result) {
                    $('#authenticatorSetup').modal('hide');
                    bootbox.dialog({
                        closeButton: false,
                        buttons: {
                            close: {
                                label: 'Close',
                                className: 'btn-primary',
                                callback: function () {
                                    window.location.reload(true);
                                }
                            }
                        },
                        title: "Recovery Codes",
                        message: '<div class="alert alert-warning">Make sure to copy your recovery codes. You won\'t be able to see them again!</div><div class="text-center">' + response.recoveryCodes.join('<br />') + '</div>'
                    });
                }
                else {
                    $("#authSetupStatus").css('display', 'inline', 'important');
                    $("#authSetupStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            },
            error: function (response) {
                $("#authSetupStatus").css('display', 'inline', 'important');
                $("#authSetupStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
            }
        }).always(function () {
            enableButton('#auth_setup_verify', 'Verify');
            $('#auth_setup_code').val('');
        });
    });

    $("#update_submit").click(function () {
        // Start Updating Animation
        disableButton('#update_submit', 'Saving...');

        var update_pgp_public_key = $("#update_pgp_public_key").val();
        var recovery = $("#update_recovery_email").val();
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                PgpPublicKey: update_pgp_public_key,
                RecoveryEmail: recovery
            }),
            success: function (response) {
                if (response.result) {
                    if (response.result.checkAuth)
                    {
                        $('#setupAuthenticatorLink').removeClass('hide');
                        $('#authSetupSecretKey').text(response.result.key);
                        $('#authQRCode').attr("src", response.result.qrUrl);
                        $('#authenticatorSetup').modal('show');
                    }
                    else
                    {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Settings Saved!</div>');
                    }
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
            enableButton('#update_submit', 'Save');
        });
        return false;
    });
});