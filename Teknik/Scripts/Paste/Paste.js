$(document).ready(function () {
    $('#content').focus();

    $("select[name='ExpireUnit']").change(function () {
        if ($(this).val() == "Never") {
            $('#length-div').addClass("hidden");
            $('#unit-div').removeClass("col-sm-2");
            $('#unit-div').addClass("col-sm-4");
        }
        else {
            $('#length-div').removeClass("hidden");
            $('#unit-div').removeClass("col-sm-4");
            $('#unit-div').addClass("col-sm-2");
        }
    });
});