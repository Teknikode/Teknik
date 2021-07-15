/* globals createInviteCode, username */
$(function () {
    $('#createInviteCode').click(function () {
        $.ajax({
            type: "POST",
            url: createInviteCode,
            data: AddAntiForgeryToken(),
            success: function (html) {
                if (html.result) {
                    $("#top_msg").css('display', 'none');
                    $("#top_msg").html('');
                    alert('Successfully created invite code for \'' + username + '\': ' + html.result.code);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }
            }
        });
    });
});