$(document).ready(function () {
    $("#update_submit").click(function () {
        // Start Updating Animation
        $.blockUI({ message: '<div class="text-center"><h3>Updating...</h3></div>' });
        
        website = $("#update_website").val();
        quote = $("#update_quote").val();
        about = $("#update_about").val();
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                Website: website,
                Quote: quote,
                About: about
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
