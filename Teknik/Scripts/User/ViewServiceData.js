/* globals deleteUploadURL, deletePasteURL, deleteShortenURL, deleteVaultURL */
$(document).ready(function () {
    $('.delete-upload-button').click(function () {
        var id = $(this).data('upload-id');
        var element = $('#uploads [id="' + id + '"');
        deleteItem(deleteUploadURL, id, element, "Are you sure you want to delete this upload?");
    });

    $('.delete-paste-button').click(function () {
        var id = $(this).data('paste-id');
        var element = $('#pastes [id="' + id + '"');
        deleteItem(deletePasteURL, id, element, "Are you sure you want to delete this paste?");
    });

    $('.delete-shorten-button').click(function () {
        var id = $(this).data('shorten-id');
        var element = $('#shortenedUrls [id="' + id + '"');
        deleteItem(deleteShortenURL, id, element, "Are you sure you want to delete this shortened url?");
    });

    $('.delete-vault-button').click(function () {
        var id = $(this).data('vault-id');
        var element = $('#vaults [id="' + id + '"');
        deleteItem(deleteVaultURL, id, element, "Are you sure you want to delete this vault?");
    });

    $('a[data-toggle="tab"]').on('shown.bs.tab', function () {
        // save the latest tab; use cookies if you like 'em better:
        localStorage.setItem('lastTab_serviceData', $(this).attr('href'));
    });

    // go to the latest tab, if it exists:
    var lastTab = localStorage.getItem('lastTab_serviceData');
    if (lastTab) {
        $('a[href="' + lastTab + '"]').tab('show');
    }
});

function deleteItem(url, id, element, confirmationMsg) {
    deleteConfirm(confirmationMsg, function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: url,
                data: { id: id },
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                xhrFields: {
                    withCredentials: true
                },
                success: function (response) {
                    if (response.result) {
                        element.remove();
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