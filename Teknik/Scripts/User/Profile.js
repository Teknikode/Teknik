$(document).ready(function () {
    $('.delete-upload-button').click(function () {
        var deleteUrl = $(this).attr('id');
        var uploadID = $(this).data('upload-id');
        bootbox.confirm("Are you sure you want to delete this upload?", function (result) {
            if (result) {
                if (deleteUrl !== '') {
                    window.open(deleteUrl, '_blank');
                    window.location.reload();
                }
                else {
                    $.ajax({
                        type: "POST",
                        url: generateDeleteKeyURL,
                        data: { file: uploadID },
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        xhrFields: {
                            withCredentials: true
                        },
                        success: function (response) {
                            if (response.result) {
                                window.open(response.result.url, '_blank');
                                window.location.reload();
                            }
                            else {
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                            }
                        }
                    });
                }
            }
        });
    });

    $('.delete-vault-button').click(function () {
        var vaultUrl = $(this).data('vault-url');
        bootbox.confirm("Are you sure you want to delete this vault?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteVaultURL,
                    data: { url: vaultUrl },
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            window.location = response.result.url;
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

    $('#pgpKeyBlock').click(function () {
        SelectAll('pgpKeyBlock');
    });
});
