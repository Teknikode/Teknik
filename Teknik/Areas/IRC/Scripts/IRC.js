var clientViews = [];
var serverView = '--Server--';
var currentLocation = serverView;
var currentNick;
var connected = false;

$(document).ready(function () {
    // UI interaction
    $('#message').bind("enterKey", function (e) {
        sendMessage($('#message').val());
        $('#message').val('');
    });

    $('#sendMessage').click(function () {
        sendMessage($('#message').val());
        $('#message').val('');
    });

    $('#guestSignIn').click(function () {
        var result = irc.server.connect();
        if (result && result != '') {
            $('#clientLogin').hide();
            $('#client').show();
        }
        else {
            $("#verifyStatus").css('display', 'inline', 'important');
            $("#verifyStatus").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Unable to Connect</div>');
        }
    });

    $('#verifyModal').on('shown.bs.modal', function (e) {
        $('#verifyPassword').focus();
    });

    $('#verifyModal').on('hide.bs.modal', function (e) {
        $("#verifyStatus").css('display', 'none', 'important');
        $("#verifyStatus").html('');
        $('#verifyPassword').val('');
    });

    $('#verifySubmit').click(function () {
        var username = $('#verifyUsername').val();
        var password = $('#verifyPassword').val();
        var result = irc.server.connect(username, password);
        if (result) {
            $('#clientLogin').hide();
            $('#client').show();
            $('#verifyModal').modal('hide');
        }
        else {
            $("#verifyStatus").css('display', 'inline', 'important');
            $("#verifyStatus").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Unable to Login</div>');
        }
    });

    function sendMessage(message) {
        irc.server.sendMessage(currentLocation, message);
    }

    $('#message').keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).trigger("enterKey");
        }
    });

    /* ----------------------------------------
       Websocket for the irc client                   
    -----------------------------------------*/
    var irc = $.connection.ircClient;

    irc.client.privateMessageCommand = function (message) {
        var msg = formatMessage(message.TimeStamp, currentNick + ': ' + message.Message);
        addOutput(message.Recipient, msg);
    };

    irc.client.privateNoticeCommand = function (message) {
        var msg = formatMessage(message.TimeStamp, currentNick + ' -NOTICE-: ' + message.Message);
        addOutput(currentLocation, msg);
    };

    irc.client.channelMessage = function (message) {
        var msg = formatMessage(message.TimeStamp, message.Sender.Nickname + ': ' + message.Message);
        addOutput(message.Channel, msg);
    };

    irc.client.channelNotice = function (message) {
        var msg = formatMessage(message.TimeStamp, message.Sender.Nickname + ': ' + message.Message);
        addOutput(currentLocation, msg);
    };

    irc.client.channelModeChange = function (message, modes) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.Nick.Nickname + ' sets mode ' + modes);
        addOutput(message.Channel, msg);
    };

    irc.client.privateMessage = function (message) {
        var location = message.Sender.Nickname;
        if (location.toLowerCase() == 'nickserv' || location.toLowerCase() == 'chanserv') {
            location = serverView;
        }
        var msg = formatMessage(message.TimeStamp, message.Sender.Nickname + ': ' + message.Message);
        addOutput(location, msg);
    };

    irc.client.privateNotice = function (message) {
        var msg = formatMessage(message.TimeStamp, message.Sender.Nickname + ' -NOTICE-: ' + message.Message);
        addOutput(currentLocation, msg);
    };

    irc.client.ctcpMessage = function (message) {
        var msg = formatMessage(message.TimeStamp, '[CTCP] [' + message.Command + '] ' + message.Sender.Nickname + ': ' + message.Arguments);
        addOutput(message.Location, msg);
    };

    irc.client.joinChannel = function (message) {
        var msg = formatMessage(message.TimeStamp, message.Nick.Nickname + ' has joined ' + message.Channel);
        addOutput(message.Channel, msg);
    };

    irc.client.inviteChannel = function (message) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.Requester.Nickname + ' invited ' + message.Recipient.Nickname);
        addOutput(message.Channel, msg);
    };

    irc.client.partChannel = function (message) {
        var msg = formatMessage(message.TimeStamp, message.Nick.Nickname + ' has left ' + message.Channel);
        addOutput(message.Channel, msg);
    };

    irc.client.quit = function (message) {
        var quitMsg = message.Nick.Nickname;
        if (message.Message != '') {
            quitMsg = message.Message;
        }
        var msg = formatMessage(message.TimeStamp, message.Nick.Nickname + ' has quit: (' + quitMsg + ')');
        addOutput(message.Channel, msg);
    };

    irc.client.kick = function (message) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.Nick.Nickname + ' has kicked ' + message.KickedNick.Nickname + ' (' + message.Reason + ')');
        addOutput(message.Channel, msg);
    };

    irc.client.topicChange = function (message) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.Nick.Nickname + ' has changed the topic to: ' + message.Topic);
        addOutput(message.Channel, msg);
    };

    irc.client.userModeChange = function (message, modes) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.Nick.Nickname + ' sets mode ' + modes);
        addOutput(currentLocation, msg);
    };

    irc.client.nickChange = function (message) {
        var msg = formatMessage(message.TimeStamp, ' * ' + message.OldNick.Nickname + ' is now known as ' + message.NewNick.Nickname);
        addOutput(currentLocation, msg);
    };

    irc.client.serverReply = function (message) {
        var msg = formatMessage(message.TimeStamp, '*: ' + message.Message);
        addOutput(serverView, msg);
    };

    irc.client.serverError = function (message) {
        var msg = formatMessage(message.TimeStamp, '*: ' + message.Message);
        addOutput(serverView, msg);
    };

    irc.client.connected = function () {
        connected = true;
    };

    irc.client.disconnected = function () {
        connected = false;
    };

    irc.client.exception = function (exception) {
        $("#top_msg").css('display', 'inline', 'important');
        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + exception + '</div>');
    };

    irc.client.nickChanged = function (nickname) {
        currentNick = nickname;
        $('#clientTitle').html('<p>Logged in as: <strong>' + nickname + '</strong></p>');
    };

    $.connection.hub.start();
});

function formatMessage(timeStamp, message) {
    var timeStamp = new Date(timeStamp);
    var msg = '[' + timeStamp.getHours() + ':' + timeStamp.getMinutes() + ':' + timeStamp.getSeconds() + '] : ' + message;
    return msg;
}

function addOutput(location, message) {
    // If the current view is the same location as the output, append it to the view
    if (currentLocation == location) {
        var txt = $('#clientOutput');
        if (txt.text() == '') {
            txt.text(message);
        }
        else {
            txt.text(txt.text() + '\n' + message);
        }
        txt.scrollTop(txt[0].scrollHeight);
    }

    // Add it to the in memory buffer for this location
    var clientView = getClientView(location);
    if (clientView) {
        clientView.output.push(message);
    }
}

function changeClientView(location) {
    var clientView = getClientView(location);
    if (clientView) {
        currentLocation = location;
        $('#clientOutput').text(clientView.output.join('\n'));
        $('#clientOutput').scrollTop($('#clientOutput')[0].scrollHeight);
    }
}

function getClientView(location) {
    // Get the item from the master list
    var foundView = clientViews.find(function (item) {
        return item.location === location;
    });
    if (!foundView) {
        foundView = createClientView(location);
    }
    return foundView;
}

function createClientView(location) {
    // Create new view item
    var item = { location: location, output: [] };
    clientViews.push(item);
    // Add the new location to the UI
    var itemDiv = $('<li class="locationTab" id="' + location + '"><a href="#">' + location + '</a></li>');
    itemDiv.find('a').click(function () {
        changeClientView(location);
    });
    $('#locationList').append(itemDiv);
    return item;
}