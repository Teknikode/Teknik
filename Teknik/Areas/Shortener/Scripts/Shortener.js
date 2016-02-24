$(document).ready(function () {
    $('#url').focus();

    $("#shortenSubmit").click(function () {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
        url = $("#url").val();
        $.ajax({
            type: "POST",
            url: $("#shortenerForm").attr('action'),
            data: { url: url },
            success: function (html) {
                if (html.result) {
                    $('#url').val(html.result.shortUrl);
                }
                else {
                    var errorMsg = "Invalid Url";
                    if (html.error)
                    {
                        errorMsg = html.error;
                    }
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorMsg + '</div>');
                }
                $('#url').focus();
                $('#url').select();
            }
        });
        return false;
    });

    $('#url').on('input', function (e) {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
    });
});