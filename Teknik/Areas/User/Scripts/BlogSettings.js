$(document).ready(function () {
    $("#update_submit").click(function () {
        // Start Updating Animation
        $.blockUI({ message: '<div class="text-center"><h3>Updating...</h3></div>' });
        
        blog_title = $("#update_blog_title").val();
        blog_desc = $("#update_blog_description").val();
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                Title: blog_title,
                Description: blog_desc,
            }),
            success: function (html) {
                $.unblockUI();
                if (html.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Settings Saved!</div>');
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
