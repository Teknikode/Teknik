$(document).ready(function () {
    $('#submitErrorReport').click(function () {
        bootbox.prompt({
            title: "Please enter any additional information that could help us",
            inputType: 'textarea',
            callback: function (result) {
                if (result) {
                    errorMsg = $("#errorMsg").html();
                    $.ajax({
                        type: "POST",
                        url: submitErrorReportURL,
                        data: AddAntiForgeryToken({
                            Message: result,
                            Exception: errorMsg,
                            CurrentUrl: window.location.href
                        }),
                        success: function (response) {
                            if (response.result) {
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg").html(
                                    '<div class="alert alert-info alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Thank you for your help!  Feedback has been submitted.</div>');
                            } else {
                                $("#top_msg").css('display', 'inline', 'important');
                                $("#top_msg")
                                    .html(
                                        '<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' +
                                        parseErrorMessage(response) +
                                        '</div>');
                            }
                        }
                    });
                }
            }
        });
    });

    $(".view-details-button").on("click",
        function () {
            var link = $(this);
            var linkText = link.text().toUpperCase();

            if (linkText === "SHOW DETAILS") {
                link.text("Hide Details");
            } else {
                link.text("Show Details");
            };
        }
    );
});
