$(document).ready(function () {
    $("#contact_submit").click(function () {
        var form = $('#contactForm');
        $.validator.unobtrusive.parse(form);
        if ($(form).valid()) {
            $.ajax({
                type: "POST",
                url: form.attr('action'),
                data: form.serialize(),
                success: function (response) {
                    if (response.result == 'true') {
                        $('#contactForm').each(function(){
                            this.reset();
                        });
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Your message has been sent.</div>');
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                    }
                }
            });
        }
        return false;
    });
});
