
$(document).ready(function () {
    $('#subscribeModal').on('shown.bs.modal', function () {
        $("#subscribeStatus").css('display', 'none', 'important');
        $("#subscribeStatus").html('');
    });

    $("#subscribeSubmit").click(function () {
        // Reset register status messages
        $("#subscribeStatus").css('display', 'none', 'important');
        $("#subscribeStatus").html('');

        // Disable the register button
        disableButton('#subscribeSubmit', 'Subscribing...');

        var form = $('#registrationForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.success) {
                    $('#registerModal').modal('hide');

                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Registration Successful.  Redirecting...</div>');

                    window.location = html.redirectUrl;
                }
                else {
                    $("#registerStatus").css('display', 'inline', 'important');
                    $("#registerStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }
            },
            error: function (response) {
                $("#registerStatus").css('display', 'inline', 'important');
                $("#registerStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
            }
        }).always(function () {
            enableButton('#registerSubmit', 'Sign Up');
        });
        return false;
    });

});