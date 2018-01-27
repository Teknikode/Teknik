$(function () {

    $('.userAccountType').on('change', function () {
        var selected = $(this).find("option:selected").val();

        $.ajax({
            type: "POST",
            url: editAccountType,
            data: AddAntiForgeryToken({ username: username, accountType: selected }),
            success: function (html) {
                if (html) {
                    if (html.result.success) {
                        $("#top_msg").css('display', 'none');
                        $("#top_msg").html('');
                        alert('Successfully changed the account type for \'' + username + '\' to type: ' + selected);
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                    }
                }
            }
        });
    });

    $('.userAccountStatus').on('change', function () {
        var selected = $(this).find("option:selected").val();

        $.ajax({
            type: "POST",
            url: editAccountStatus,
            data: AddAntiForgeryToken({ username: username, accountStatus: selected }),
            success: function (html) {
                if (html) {
                    if (html.result.success) {
                        $("#top_msg").css('display', 'none');
                        $("#top_msg").html('');
                        alert('Successfully changed the account status for \'' + username + '\' to: ' + selected);
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                    }
                }
            }
        });
    });

    $('#createInviteCode').click(function () {
        $.ajax({
            type: "POST",
            url: createInviteCode,
            data: AddAntiForgeryToken({ username: username }),
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'none');
                    $("#top_msg").html('');
                    alert('Successfully created invite code for \'' + username + '\': ' + html.result.code);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }
            }
        });
    });

    $('#delete_account').click(function () {
        bootbox.confirm("Are you sure you want to delete this account?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteUserURL,
                    data: AddAntiForgeryToken({ username: username }),
                    success: function (response) {
                        if (response.result) {
                            window.location.replace(homeUrl);
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
});
