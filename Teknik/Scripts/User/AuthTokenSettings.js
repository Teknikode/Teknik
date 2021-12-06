/* globals createAuthTokenURL, getAuthTokenURL, editAuthTokenURL, deleteAuthTokenURL */
$(document).ready(function () {

    $('#AuthTokenModal').on('shown.bs.modal', function () {
        $("#authTokenStatus").css('display', 'none', 'important');
        $("#authTokenStatus").html('');

        $('#authTokenName').focus();
    });

    $('#authTokenModal').on('hide.bs.modal', function () {
        $("#authTokenStatus").css('display', 'none', 'important');
        $("#authTokenStatus").html('');

        $(this).find('#authTokenCreateSubmit').addClass('hidden');
        $(this).find('#authTokenEditSubmit').addClass('hidden');

        clearInputs('#authTokenModal');
    });


    $('#authTokenModal').find('#authTokenCreateSubmit').click(createAuthToken);
    $('#authTokenModal').find('#authTokenEditSubmit').click(editAuthTokenSave);

    $("#createAuthToken").click(function () {
        $('#authTokenModal').find('#authTokenCreateSubmit').removeClass('hidden');
        $('#authTokenModal').find('#authTokenCreateSubmit').text('Create Auth Token');

        $('#authTokenModal').modal('show');
    });

    $(".editAuthToken").click(function () {
        var authTokenId = $(this).attr("data-authTokenId");
        editAuthToken(authTokenId);
    });

    $(".deleteAuthToken").click(function () {
        var authTokenId = $(this).attr("data-authTokenId");
        deleteAuthToken(authTokenId);
    });
});

function createAuthToken() {
    saveAuthTokenInfo(createAuthTokenURL, 'Create Auth Token', 'Creating Auth Token...', function (response) {
        $('#authTokenModal').modal('hide');

        var dialog = bootbox.dialog({
            closeButton: false,
            buttons: {
                close: {
                    label: 'Close',
                    className: 'btn-primary',
                    callback: function () {
                        if ($('#noAuthTokens')) {
                            $('#noAuthTokens').remove();
                        }

                        var item = $(response.html);

                        processAuthTokenItem(item);

                        $('#authTokenList').append(item);
                    }
                }
            },
            title: "Auth Token Secret",
            message: '<label for="authTokenSecret">Make sure to copy your auth token now.<br />You won\'t be able to see it again!</label><input type="text" class="form-control" id="authTokenSecret" value="' + response.token + '">',
        });

        dialog.init(function () {
            dialog.find('#authTokenSecret').click(function () {
                $(this).select();
            });
        });
    });
}

function processAuthTokenItem(item) {
    item.find('.editAuthToken').click(function () {
        var authTokenId = $(this).attr("data-authTokenId");
        editAuthToken(authTokenId);
    });

    item.find('.deleteAuthToken').click(function () {
        var authTokenId = $(this).attr("data-authTokenId");
        deleteAuthToken(authTokenId);
    });
}

function editAuthToken(authTokenId) {
    disableButton('.editAuthToken[data-authTokenId="' + authTokenId + '"]', 'Loading...');

    $.ajax({
        type: "POST",
        url: getAuthTokenURL,
        data: AddAntiForgeryToken({ authTokenId: authTokenId }),
        success: function (data) {
            if (data.result) {
                $('#authTokenModal').find('#authTokenId').val(data.authToken.authTokenId);
                $('#authTokenModal').find('#authTokenName').val(data.authToken.name);

                $('#authTokenModal').find('#authTokenEditSubmit').removeClass('hidden');
                $('#authTokenModal').find('#authTokenEditSubmit').text('Save Auth Token');

                $('#authTokenModal').modal('show');

                enableButton('.editAuthToken[data-authTokenId="' + authTokenId + '"]', 'Edit');
            }
        }
    });
}

function editAuthTokenSave() {
    saveAuthTokenInfo(editAuthTokenURL, 'Save Auth Token', 'Saving Auth Token...', function (response) {
        $('#authToken_' + response.authTokenId).replaceWith(response.html);
        processAuthTokenItem($('#authToken_' + response.authTokenId));

        $('#authTokenModal').modal('hide');

        $("#top_msg").css('display', 'inline', 'important');
        $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Successfully Saved Auth Token</div>');
    });
}

function deleteAuthToken(authTokenId) {
    disableButton('.deleteAuthToken[data-authTokenId="' + authTokenId + '"]', 'Deleting...');
    bootbox.confirm({
        message: "<h2>Are you sure you want to delete this auth token?</h2><br /><br />This is <b>irreversable</b> and all applications using this token will stop working.",
        buttons: {
            confirm: {
                label: 'Delete',
                className: 'btn-danger'
            },
            cancel: {
                label: 'Cancel',
                className: 'btn-default'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteAuthTokenURL,
                    data: AddAntiForgeryToken({ authTokenId: authTokenId }),
                    success: function (response) {
                        if (response.result) {
                            $('#authToken_' + authTokenId).remove();
                            if ($('#authTokenList li').length <= 0) {
                                $('#authTokenList').html('<li class="list-group-item text-center" id="noAuthTokens">No Auth Tokens</li>');
                            }
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                }).always(function () {
                    enableButton('.deleteAuthToken[data-authTokenId="' + authTokenId + '"]', 'Delete');
                });
            } else {
                enableButton('.deleteAuthToken[data-authTokenId="' + authTokenId + '"]', 'Delete');
            }
        }
    });
}

function saveAuthTokenInfo(url, submitText, submitActionText, callback) {
    var authTokenId, name;
    disableButton('.authTokenSubmit', submitActionText);

    authTokenId = $('#authTokenModal').find('#authTokenId').val();
    name = $('#authTokenModal').find('#authTokenName').val();

    $.ajax({
        type: "POST",
        url: url,
        data: AddAntiForgeryToken({ authTokenId: authTokenId, name: name }),
        success: function (response) {
            if (response.result) {
                if (callback) {
                    callback(response);
                }
            }
            else {
                $("#authTokenStatus").css('display', 'inline', 'important');
                $("#authTokenStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
            }
        },
        error: function (response) {
            $("#authTokenStatus").css('display', 'inline', 'important');
            $("#authTokenStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
        }
    }).always(function () {
        enableButton('.authTokenSubmit', submitText);
    });
}
