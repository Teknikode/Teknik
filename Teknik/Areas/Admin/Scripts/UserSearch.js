$(document).ready(function () {
    $('#Query').on('input', function (e) {
        query = $(this).val();
        $.ajax({
            type: "POST",
            url: userSearchResultsURL,
            data: { query: query },
            success: function (response) {
                if (response.result) {
                    $("#results").html(response.result.html);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
});
