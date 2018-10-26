$(document).ready(function () {
    $('[data-toggle="popover"]').popover();

    $('[data-toggle="popover"]').on('shown.bs.popover', function () {
        var $this = $(this);
        setTimeout(function() {
            $this.popover('hide');
        }, 3000);
    });

    $(".copyCodeBtn").click(function () {
        var code = $(this).attr("data-code");
        copyTextToClipboard(code);
        $(this).popover('show');
    });

    $(".extCodeBtn").click(function () {
        var codeId = $(this).attr("data-codeid");
        createExternalLink(codeId);
    });
});

function createExternalLink(inviteCodeId) {
    $.ajax({
        type: "POST",
        url: createExternalLinkURL,
        data: AddAntiForgeryToken({ inviteCodeId: inviteCodeId }),
        success: function (response) {
            if (response.result) {
                var dialog = bootbox.dialog({
                    title: "Send this link to someone to claim this invite code.",
                    message: '<input type="text" class="form-control" id="inviteCodeLink" value="' + response.result + '">'
                });

                dialog.init(function () {
                    dialog.find('#inviteCodeLink').click(function () {
                        $(this).select();
                    });
                });
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
