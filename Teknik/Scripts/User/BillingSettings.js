/* globals cancelSubscriptionURL */
$(document).ready(function () {
    $('.cancel-subscription-button').click(function () {
        disableButton('#cancel_subscription', 'Canceling Subscription...');

        var subscriptionId = $(this).data('subscription-id');
        var productId = $(this).data('product-id');
        var element = $('#activeSubscriptionList [id="' + subscriptionId + '"');

        confirmDialog('Confirm', 'Back', 'Are you sure you want to cancel your subscription?', function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: cancelSubscriptionURL,
                    data: AddAntiForgeryToken({ subscriptionId: subscriptionId, productId: productId }),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            element.remove();
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Subscription successfully canceled.</div>');
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
            } else {
                enableButton('#cancel_subscription', 'Cancel Subscription');
            }
        });
    });
});