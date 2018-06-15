$(document).ready(function () {
    $('#delete_account').click(function () {
        bootbox.confirm("Are you sure you want to delete your account?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteUserURL,
                    data: AddAntiForgeryToken({}),
                    success: function (response) {
                        if (response.result) {
                            window.location.replace(homeUrl);
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
});
