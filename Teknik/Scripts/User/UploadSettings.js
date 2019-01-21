$(document).ready(function () {
    $("[name='update_upload_encrypt']").bootstrapSwitch();

    $("[name='expireunit']").change(function () {
        setExpireWidth($(this).val());
    });

    $("#update_submit").click(function () {
        // Start Updating Animation
        disableButton('#update_submit', 'Saving...');
        
        upload_encrypt = $("#update_upload_encrypt").is(":checked");
        upload_expireLength = $("#expirelength").val();
        upload_expireUnit = $("#expireunit").val();
        $.ajax({
            type: "POST",
            url: editURL,
            data: AddAntiForgeryToken({
                Encrypt: upload_encrypt,
                ExpirationLength: upload_expireLength,
                ExpirationUnit: upload_expireUnit
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

    // Initialize the widths
    setExpireWidth($("[name='expireunit']").val());
});

function setExpireWidth(unit) {
    if (unit === "Never") {
        $('#length-div').addClass("hidden");
        $('#unit-div').removeClass("col-sm-7");
        $('#unit-div').addClass("col-sm-9");
    }
    else {
        $('#length-div').removeClass("hidden");
        $('#unit-div').removeClass("col-sm-9");
        $('#unit-div').addClass("col-sm-7");
    }
}