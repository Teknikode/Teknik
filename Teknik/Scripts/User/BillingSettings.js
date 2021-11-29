/* globals cancelSubscriptionURL, renewSubscriptionURL */
$(document).ready(function () {
    $('.cancel-subscription-button').click(function () {
        disableButton('#cancel_subscription', 'Canceling Subscription...');

        var subscriptionId = $(this).data('subscription-id');

        confirmDialog('Confirm', 'Back', 'Are you sure you want to cancel your subscription?<br /><br />Your plan will be canceled, but is still available until the end of your billing period.', function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: cancelSubscriptionURL,
                    data: AddAntiForgeryToken({ subscriptionId: subscriptionId }),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Subscription will be canceled at the end of your billing cycle.</div>');
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

    $('.renew-subscription-button').click(function () {
        disableButton('#renew_subscription', 'Renewing Subscription...');

        var subscriptionId = $(this).data('subscription-id');

        confirmDialog('Renew', 'Back', 'Are you sure you want to renew your subscription?', function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: renewSubscriptionURL,
                    data: AddAntiForgeryToken({ subscriptionId: subscriptionId }),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    xhrFields: {
                        withCredentials: true
                    },
                    success: function (response) {
                        if (response.result) {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-success alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>Subscription renewed successfully!</div>');
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
                enableButton('#renew_subscription', 'Renew Subscription');
            }
        });
    });
});