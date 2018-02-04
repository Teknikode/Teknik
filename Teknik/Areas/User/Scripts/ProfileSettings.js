$(document).ready(function () {
    $("#update_submit").click(function () {
        // Start Updating Animation
        disableButton('#update_submit', 'Saving...');
        
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
            success: function (response) {
                enableButton('#update_submit', 'Save');
                if (response.result) {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Settings Saved!</div>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });
});
