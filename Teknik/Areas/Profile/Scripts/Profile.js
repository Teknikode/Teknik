$(document).ready(function () {
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
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Unable to delete your account.  Please contact an Administrator.</div>');
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
        website = $("#update_website").val();
        quote = $("#update_quote").val();
        about = $("#update_about").val();
        blog_title = $("#update_blog_title").val();
        blog_desc = $("#update_blog_description").val();
        $.ajax({
            type: "POST",
            url: editUserURL,
            data: AddAntiForgeryToken({
                curPass: current_password,
                newPass: password,
                newPassConfirm: password_confirm,
                website: website,
                quote: quote,
                about: about,
                blogTitle: blog_title,
                blogDesc: blog_desc
            }),
            success: function (html) {
                if (html.result) {
                    $.unblockUI();
                    window.location.reload();
                }
                else {
                    $.unblockUI();
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
});