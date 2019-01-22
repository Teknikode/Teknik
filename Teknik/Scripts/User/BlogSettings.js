/* globals editURL */
$(document).ready(function () {
    $("#update_submit").click(function () {
        // Start Updating Animation
        disableButton('#update_submit', 'Saving...');
        
        var blog_title = $("#update_blog_title").val();
        var blog_desc = $("#update_blog_description").val();
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                Title: blog_title,
                Description: blog_desc,
            }),
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Settings Saved!</div>');
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
