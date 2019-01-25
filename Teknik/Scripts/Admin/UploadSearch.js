/* globals searchResultsURL, generateDeleteKeyURL */
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
                    LinkUploadDelete($('.delete-upload-button'));
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
});

function LinkUploadDelete(selector) {
    $(selector).click(function () {
        var deleteUrl = $(this).attr('id');
        var uploadID = $(this).data('upload-id');
        deleteConfirm("Are you sure you want to delete this upload?", function (result) {
            if (result) {
                if (deleteUrl !== '') {
                    window.open(deleteUrl, '_blank');
                    window.location.reload();
                }
                else {
                    $.ajax({
                        type: "POST",
                        url: generateDeleteKeyURL,
                        data: { file: uploadID },
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        xhrFields: {
                            withCredentials: true
                        },
                        success: function (response) {
                            if (response.result) {
                                window.open(response.result.url, '_blank');
                                window.location.reload();
                            }
                            else {
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                            }
                        }
                    });
                }
            }
        });
    });
}
