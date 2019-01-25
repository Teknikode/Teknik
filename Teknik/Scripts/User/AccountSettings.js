/* globals changePasswordUrl, deleteUserURL, homeUrl */
$(document).ready(function () {
    $("#change_password_submit").click(function () {
        // Start Updating Animation
        disableButton('#change_password_submit', 'Saving...');

        var current_password = $("#update_password_current").val();
        var password = $("#update_password").val();
        var password_confirm = $("#update_password_confirm").val();
        $.ajax({
            type: "POST",
            url: changePasswordUrl,
            data: AddAntiForgeryToken({
                CurrentPassword: current_password,
                NewPassword: password,
                NewPasswordConfirm: password_confirm
            }),
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Password has successfully been changed.</div>');
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
            enableButton('#change_password_submit', 'Change Password');
        });
        return false;
    });

    $('#delete_account').click(function () {
        disableButton('#delete_account', 'Deleting Account...');

        deleteConfirm("Are you sure you want to delete your account?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteUserURL,
                    data: AddAntiForgeryToken({}),
                    success: function (response) {
                        if (response.result) {
                            window.location.replace(homeUrl);
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
                    enableButton('#delete_account', 'Delete Account');
                });
            } else {
                enableButton('#delete_account', 'Delete Account');
            }
        });
    });
});