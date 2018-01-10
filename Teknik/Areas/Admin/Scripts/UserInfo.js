$(function () {

    $('.userAccountType').on('change', function () {
        var selected = $(this).find("option:selected").val();

        $.ajax({
            type: "POST",
            url: editAccountType,
            data: AddAntiForgeryToken({ username: username, accountType: selected }),
            success: function (html) {
                if (html) {
                    if (html.error) {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error.message + '</div>');
                    }
                    else {
                        $("#top_msg").css('display', 'none');
                        $("#top_msg").html('');
                        alert('Successfully changed the account type for \'' + username + '\' to type: ' + selected);
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
                    if (html.error) {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error.message + '</div>');
                    }
                    else {
                        $("#top_msg").css('display', 'none');
                        $("#top_msg").html('');
                        alert('Successfully changed the account status for \'' + username + '\' to: ' + selected);
                    }
                }
            }
        });
    });

});
