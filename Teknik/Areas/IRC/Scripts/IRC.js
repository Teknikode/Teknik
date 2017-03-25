$(document).ready(function () {
    /* ----------------------------------------
       Websocket for the irc client                   
    -----------------------------------------*/
    var irc = $.connection.ircClient;

    irc.client.rawMessageReceived = function (message) {
        var txt = $('#clientOutput');
        txt.val(txt.val() + '\n' + message);
    };

    $.connection.hub.start();

    $('#sendMessage').click(function () {
        irc.server.sendRawMessage($('#message').val());
        $('#message').val('');
    });
});