/* globals createVaultURL, deletePasteURL */
$(document).ready(function () {
    linkCreateVault($('#create-vault'));

    $('#add-to-vault-menu').find('.add-to-vault').each(function () {
        linkAddToVault($(this));
    });

    $('#delete-paste').click(function () {
        var id = $(this).data('paste-url');

        bootbox.confirm("Are you sure you want to delete this paste?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deletePasteURL,
                    data: { id: id },
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            window.location = response.redirect;
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
    });
});

function linkCreateVault(element) {
    element.click(function () {
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(addParamsToUrl(createVaultURL, { items: encodeURIComponent(pasteUrl + ':' + pasteTitle) }), '_blank');
    });
}

function linkAddToVault(element) {
    element.click(function () {
        var addToVaultURL = $(this).data('add-to-vault-url');
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(addParamsToUrl(addToVaultURL, { items: encodeURIComponent(pasteUrl + ':' + pasteTitle) }), '_blank');
    });
}