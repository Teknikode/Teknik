/* globals searchResultsURL, deletePasteURL, homeUrl */
$(document).ready(function () {
    $('#Query').on('input', function () {
        var query = $(this).val();

        // Try to strip out the ID from the url
        var pattern = '(?:(?:.+)\\/)?([^\\?]+)(?:\\?(?:.*))?';
        var reg = new RegExp(pattern);
        var match = reg.exec(query);
        query = match[1];

        $.ajax({
            type: "POST",
            url: searchResultsURL,
            data: { url: query },
            success: function (response) {
                if (response.result) {
                    $("#top_msg").css('display', 'none');
                    $("#top_msg").html('');
                    $("#results").html(response.result.html);
                    LinkPasteDelete($('.delete-paste-button'));
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
});

function LinkPasteDelete(selector) {
    $(selector).click(function () {
        var id = $(this).data('paste-id');

        deleteConfirm("Are you sure you want to delete this paste?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deletePasteURL,
                    data: { id: id },
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            window.location.replace(homeUrl);
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
        });
    });
}
