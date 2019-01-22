/* globals createClientURL, getClientURL, editClientURL, deleteClientURL */
$(document).ready(function () {

    $('#clientModal').on('shown.bs.modal', function () {
        $("#clientStatus").css('display', 'none', 'important');
        $("#clientStatus").html('');

        $('#clientName').focus();
    });

    $('#clientModal').on('hide.bs.modal', function () {
        $("#clientStatus").css('display', 'none', 'important');
        $("#clientStatus").html('');

        $(this).find('#clientCreateSubmit').addClass('hidden');
        $(this).find('#clientEditSubmit').addClass('hidden');

        clearInputs('#clientModal');
    });


    $('#clientModal').find('#clientCreateSubmit').click(createClient);
    $('#clientModal').find('#clientEditSubmit').click(editClientSave);

    $("#createClient").click(function () {
        $('#clientModal').find('#clientCreateSubmit').removeClass('hidden');
        $('#clientModal').find('#clientCreateSubmit').text('Create Client');

        $('#clientModal').modal('show');
    });

    $(".editClient").click(function () {
        var clientId = $(this).attr("data-clientId");
        editClient(clientId);
    });

    $(".deleteClient").click(function () {
        var clientId = $(this).attr("data-clientId");
        deleteClient(clientId);
    });
});

function createClient() {
    saveClientInfo(createClientURL, 'Create Client', 'Creating Client...', function (response) {
        $('#clientModal').modal('hide');

        var dialog = bootbox.dialog({
            closeButton: false,
            buttons: {
                close: {
                    label: 'Close',
                    className: 'btn-primary',
                    callback: function () {
                        if ($('#noClients')) {
                            $('#noClients').remove();
                        }

                        var item = $(response);

                        processClientItem(item);

                        $('#clientList').append(item);
                    }
                }
            },
            title: "Client Secret",
            message: '<label for="clientSecret">Make sure to copy your client secret now.<br />You won\'t be able to see it again!</label><input type="text" class="form-control" id="clientSecret" value="' + response.secret + '">',
        });

        dialog.init(function () {
            dialog.find('#clientSecret').click(function () {
                $(this).select();
            });
        });
    });
}

function processClientItem(item) {
    item.find('.editClient').click(function () {
        var clientId = $(this).attr("data-clientId");
        editClient(clientId);
    });

    item.find('.deleteClient').click(function () {
        var clientId = $(this).attr("data-clientId");
        deleteClient(clientId);
    });
}

function editClient(clientId) {
    disableButton('.editClient[data-clientId="' + clientId + '"]', 'Loading...');

    $.ajax({
        type: "POST",
        url: getClientURL,
        data: AddAntiForgeryToken({ clientId: clientId }),
        success: function (data) {
            if (data.result) {
                $('#clientModal').find('#clientId').val(data.client.id);
                $('#clientModal').find('#clientName').val(data.client.name);
                $('#clientModal').find('#clientHomepageUrl').val(data.client.homepageUrl);
                $('#clientModal').find('#clientLogoUrl').val(data.client.logoUrl);
                $('#clientModal').find('#clientCallbackUrl').val(data.client.callbackUrl);

                $('#clientModal').find('#clientEditSubmit').removeClass('hidden');
                $('#clientModal').find('#clientEditSubmit').text('Save Client');

                $('#clientModal').modal('show');

                enableButton('.editClient[data-clientId="' + clientId + '"]', 'Edit');
            }
        }
    });
}

function editClientSave() {
    saveClientInfo(editClientURL, 'Save Client', 'Saving Client...', function (response) {
        $('#client_' + response.clientId).replaceWith(response.html);
        processClientItem($('#client_' + response.clientId));

        $('#clientModal').modal('hide');

        $("#top_msg").css('display', 'inline', 'important');
        $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Successfully Saved Client</div>');
    });
}

function deleteClient(clientId) {
    disableButton('.deleteClient[data-clientId="' + clientId + '"]', 'Deleting...');
    bootbox.confirm("<h2>Are you sure you want to delete this client?</h2><br /><br />This is <b>irreversable</b> and all applications using these client credentials will stop working.", function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: deleteClientURL,
                data: AddAntiForgeryToken({ clientId: clientId }),
                success: function (response) {
                    if (response.result) {
                        $('#client_' + clientId).remove();
                        if ($('#clientList li').length <= 0) {
                            $('#clientList').html('<li class="list-group-item text-center" id="noClients">No Clients</li>');
                        }
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                    }
                }
            }).always(function () {
                enableButton('.deleteClient[data-clientId="' + clientId + '"]', 'Delete');
            });
        } else {
            enableButton('.deleteClient[data-clientId="' + clientId + '"]', 'Delete');
        }
    });
}

function saveClientInfo(url, submitText, submitActionText, callback) {
    var clientId, name, homepageUrl, logoUrl, callbackUrl;
    disableButton('.clientSubmit', submitActionText);

    clientId = $('#clientModal').find('#clientId').val();
    name = $('#clientModal').find('#clientName').val();
    homepageUrl = $('#clientModal').find('#clientHomepageUrl').val();
    logoUrl = $('#clientModal').find('#clientLogoUrl').val();
    callbackUrl = $('#clientModal').find('#clientCallbackUrl').val();

    $.ajax({
        type: "POST",
        url: url,
        data: AddAntiForgeryToken({ clientId: clientId, name: name, homepageUrl: homepageUrl, logoUrl: logoUrl, callbackUrl: callbackUrl }),
        success: function (response) {
            if (response.result) {
                if (callback) {
                    callback(response);
                }
            }
            else {
                $("#clientStatus").css('display', 'inline', 'important');
                $("#clientStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
            }
        },
        error: function (response) {
            $("#clientStatus").css('display', 'inline', 'important');
            $("#clientStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response.responseText) + '</div>');
        }
    }).always(function () {
        enableButton('.clientSubmit', submitText);
    });
}
