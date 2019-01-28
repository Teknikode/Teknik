$(document).ready(function () {
    $('#url').focus();

    $("#shortenSubmit").click(function () {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
        var url = $("#url").val();
        $.ajax({
            type: "POST",
            url: $("#shortenerForm").attr('action'),
            data: { url: url },
            success: function (response) {
                if (response.result) {
                    $('#url').val(response.result.shortUrl);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
                $('#url').focus();
                $('#url').select();
            }
        });
        return false;
    });

    $('#url').on('input', function () {
        $("#top_msg").css('display', 'none', 'important');
        $("#top_msg").html('');
    });
});
