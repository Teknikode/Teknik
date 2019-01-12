$(document).ready(function () {

    $('#create_client').click(function () {
        var name, redirectUri, logoutUri;
        bootbox.prompt("Specify a name for this Client", function (result) {
            if (result) {
                name = result;
                bootbox.prompt("Specify the Redirect URI for this Client", function (result) {
                    if (result) {
                        redirectUri = result;
                        bootbox.prompt("Specify the Logout URI for this Client", function (result) {
                            if (result) {
                                logoutUri = result;
                                $.ajax({
                                    type: "POST",
                                    url: createClientURL,
                                    data: AddAntiForgeryToken({ name: name, redirectUri: redirectUri, logoutUri: logoutUri }),
                                    success: function (response) {
                                        if (response.result) {
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
                                                            var item = $(response.result.html);

                                                            var deleteBtn = item.find('.deleteClient');

                                                            deleteBtn.click(function () {
                                                                var clientId = $(this).attr("data-clientId");
                                                                deleteClient(clientId);
                                                            });

                                                            $('#clientList').append(item);
                                                        }
                                                    }
                                                },
                                                title: "Client Secret",
                                                message: '<label for="clientSecret">Make sure to copy your client secret now.<br />You won\'t be able to see it again!</label><input type="text" class="form-control" id="clientSecret" value="' + response.result.secret + '">',
                                            });

                                            dialog.init(function () {
                                                dialog.find('#authToken').click(function () {
                                                    $(this).select();
                                                });
                                            });
                                        }
                                        else {
                                            $("#top_msg").css('display', 'inline', 'important');
                                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                                        }
                                    }
                                });
                            }
                        });
                    }
                });
            }
        });
    });

    $(".deleteClient").click(function () {
        var clientId = $(this).attr("data-clientId");
        deleteClient(clientId);
    });

    $('#create_authToken').click(function () {
        bootbox.prompt("Specify a name for this Auth Token", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: generateTokenURL,
                    data: AddAntiForgeryToken({ name: result }),
                    success: function (response) {
                        if (response.result) {
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
                                            var item = $(response.result.html);

                                            var deleteBtn = item.find('.deleteAuthToken');
                                            var editBtn = item.find('.editAuthToken');

                                            deleteBtn.click(function () {
                                                var authTokenId = $(this).attr("data-authid");
                                                deleteAuthToken(authTokenId);
                                            });

                                            editBtn.click(function () {
                                                var authTokenId = $(this).attr("data-authid");
                                                editAuthToken(authTokenId);
                                            });

                                            $('#authTokenList').append(item);
                                        }
                                    }
                                },
                                title: "Authentication Token",
                                message: '<label for="authToken">Make sure to copy your new personal access token now.<br />You won\'t be able to see it again!</label><input type="text" class="form-control" id="authToken" value="' + response.result.token + '">',
                            });

                            dialog.init(function () {
                                dialog.find('#authToken').click(function () {
                                    $(this).select();
                                });
                            });
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

    $('#revoke_all_authTokens').click(function () {
        bootbox.confirm("Are you sure you want to revoke all your clients?<br /><br />This is <b>irreversable</b> and all applications using these clients will stop working.", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: revokeAllClientsURL,
                    data: AddAntiForgeryToken({}),
                    success: function (response) {
                        if (response.result) {
                            $('#authTokenList').html('<li class="list-group-item text-center" id="noAuthTokens">No Authentication Tokens</li>');
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

    $(".deleteAuthToken").click(function () {
        var authTokenId = $(this).attr("data-authid");
        deleteAuthToken(authTokenId);
    });

    $(".editAuthToken").click(function () {
        var authTokenId = $(this).attr("data-authid");
        editAuthToken(authTokenId);
    });
});

function editAuthToken(authTokenId) {
    bootbox.prompt("Specify a new name for this Auth Token", function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: editTokenNameURL,
                data: AddAntiForgeryToken({ tokenId: authTokenId, name: result }),
                success: function (response) {
                    if (response.result) {
                        $('#authTokenName_' + authTokenId).html(response.result.name);
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                    }
                }
            });
        }
    });
}

function deleteAuthToken(authTokenId) {
    bootbox.confirm("Are you sure you want to revoke this auth token?<br /><br />This is <b>irreversable</b> and all applications using this token will stop working.", function (result) {
        if (result) {
            $.ajax({
                type: "POST",
                url: deleteTokenURL,
                data: AddAntiForgeryToken({ tokenId: authTokenId }),
                success: function (response) {
                    if (response.result) {
                        $('#authToken_' + authTokenId).remove();
                        if ($('#authTokenList li').length <= 0) {
                            $('#authTokenList').html('<li class="list-group-item text-center" id="noAuthTokens">No Authentication Tokens</li>');
                        }
                    }
                    else {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                    }
                }
            });
        }
    });
}

function deleteClient(clientId) {
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
            });
        }
    });
}
