$(document).ready(function () {
    $("[name='update_upload_encrypt']").bootstrapSwitch();

    $("#update_submit").click(function () {
        // Start Updating Animation
        $.blockUI({ message: '<div class="text-center"><h3>Updating...</h3></div>' });
        
        upload_encrypt = $("#update_upload_encrypt").is(":checked");
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                Encrypt: upload_encrypt
            }),
            success: function (response) {
                $.unblockUI();
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
