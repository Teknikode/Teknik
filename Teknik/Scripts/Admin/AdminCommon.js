/* exported deleteItem */
function deleteItem(type, id, deleteUrl, redirectUrl, confirmationMsg) {
    deleteConfirm(confirmationMsg, function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: deleteUrl,
                data: AddAntiForgeryToken({ type: type, id: id }),
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                xhrFields: {
                    withCredentials: true
                },
                success: function (response) {
                    if (response.result) {
                        window.location.replace(redirectUrl);
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
            });
        }
    });
}