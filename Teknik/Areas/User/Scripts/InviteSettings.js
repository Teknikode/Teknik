$(document).ready(function () {
    $('[data-toggle="popover"]').popover();

    $('[data-toggle="popover"]').on('shown.bs.popover', function () {
        var $this = $(this);
        setTimeout(function() {
            $this.popover('hide');
        }, 3000);
    });
});

function copyCode(inviteCodeId, inviteCode) {
    copyTextToClipboard(inviteCode);
    $('#copyCode_' + inviteCodeId + '').popover('show');
}

function createExternalLink(inviteCodeId) {
    $.ajax({
        type: "POST",
        url: createExternalLinkURL,
        data: AddAntiForgeryToken({ inviteCodeId: inviteCodeId }),
        success: function (response) {
            if (response.result) {
                bootbox.dialog({
                    title: "Send this link to someone to claim this invite code.",
                    message: '<input type="text" class="form-control" id="inviteCodeLink" onClick="this.select();" value="' + response.result + '">'
                });
            }
            else {
                $("#top_msg").css('display', 'inline', 'important');
                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
            }
        }
    });
}
