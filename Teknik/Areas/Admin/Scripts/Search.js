$(document).ready(function () {
    $('#Query').on('input', function (e) {
        query = $(this).val();
        $.ajax({
            type: "POST",
            url: searchResultsURL,
            data: { query: query },
            success: function (html) {
                if (html) {
                    if (html.error) {
                        $("#top_msg").css('display', 'inline', 'important');
                        $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                    }
                    else {
                        $("#results").html(html);
                    }
                }
            }
        });
    });
});