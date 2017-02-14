$(document).ready(function () {
    $('#newItem').on('show.bs.modal', function (e) {
        var newDiv = $('#newItem');
        newDiv.find("#item_title").val("");
        newDiv.find("#item_description").val("");
        newDiv.find("#item_type").val("Upload");
        newDiv.find("#item_url").val("");

        newDiv.find('#item_error').hide();
        newDiv.find("#item_error_msg").html('');
    });

    $('#editItem').on('show.bs.modal', function (e) {
        var itemID = $(e.relatedTarget).attr("id");
        var itemDiv = $('#vault-item-' + itemID);
        var title = itemDiv.find("#item-title").text();
        var description = itemDiv.find("#item-description").text();

        var editDiv = $('#editItem');
        editDiv.find('#item_id').val(itemID);
        editDiv.find("#item_title").val(title);
        editDiv.find("#item_description").val(description);

        editDiv.find('#item_error').hide();
        editDiv.find("#item_error_msg").html('');
    });

    $("#new_item_submit").click(function () {
        var newDiv = $('#newItem');
        var title = newDiv.find("#item_title").val();
        var description = newDiv.find("#item_description").val();
        var type = newDiv.find('#item_type').val();
        var url = newDiv.find('#item_url').val();

        // First Validation
        if (title == null || title == '') {
            newDiv.find("#item_error").show();
            newDiv.find("#item_error_msg").html('You must supply a Title');
            return false;
        }
        $.ajax({
            type: "POST",
            url: validateItemURL,
            data: AddAntiForgeryToken({ type: type, url: url }),
            success: function (response) {
                if (response.result) {
                    itemCount++;

                    // Add a new item to the main page
                    var itemDiv = $('#item-template').clone();
                    itemDiv.attr('id', 'vault-item-' + itemCount);
                    itemDiv.addClass('vault-item');
                    itemDiv.find('#item-title').html(title);
                    itemDiv.find('#item-description').html(description);
                    itemDiv.find('#item-type').html(type);
                    itemDiv.find('#item-url').html(url);

                    if (description == null || description == '') {
                        itemDiv.find('.panel-footer').hide();
                    }

                    itemDiv.find('#edit-item').attr('id', itemCount);

                    linkRemove(itemDiv);
                    linkMoveUp(itemDiv);
                    linkMoveDown(itemDiv);

                    $('#vault-items').append(itemDiv);

                    $('#newItem').modal('hide');
                }
                else {
                    newDiv.find("#item_error").show();
                    newDiv.find("#item_error_msg").html(response.error.message);
                }
            }
        });
        return false;
    });

    $("#edit_item_submit").click(function () {
        var itemDiv = $('#editItem');
        var id = itemDiv.find('#item_id').val();
        var title = itemDiv.find("#item_title").val();
        var description = itemDiv.find("#item_description").val();

        // First Validation
        if (title == null || title == '') {
            itemDiv.find("#item_error").show();
            itemDiv.find("#item_error_msg").html('You must supply a Title');
            return false;
        }

        var origDiv = $('#vault-item-' + id);
        origDiv.find('#item-title').html(title);
        origDiv.find('#item-description').html(description);

        if (description == null || description == '') {
            origDiv.find('.panel-footer').hide();
        }
        else {
            origDiv.find('.panel-footer').show();
        }

        $('#editItem').modal('hide');
        return false;
    });

    $("#submit").click(function () {
        var title = $("#title").val();
        var description = $("#description").val();
        var items = [];

        $(".vault-item").each(function () {
            var itemTitle = $(this).find("#item-title").text();
            var itemDescription = $(this).find("#item-description").text();
            var itemType = $(this).find('#item-type').text();
            var itemUrl = $(this).find('#item-url').text();
            var item = { title: itemTitle, description: itemDescription, type: itemType, url: itemUrl };
            items.push(item);
        });

        // Validation
        if (title == null || title == '') {
            $("#top_msg").css('display', 'inline', 'important');
            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>You must supply a Title</div>');
            return false;
        }

        if (items.length == 0) {
            $("#top_msg").css('display', 'inline', 'important');
            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>You must have at least one item in the vault</div>');
            return false;
        }

        // Create the vault
        $.ajax({
            type: "POST",
            url: createVaultURL,
            data: AddAntiForgeryToken({ title: title, description: description, items: items }),
            success: function (response) {
                if (response.result) {
                    window.location = response.result.url;
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + response.error.message + '</div>');
                }
            }
        });
        return false;
    });

    $("#show-more-button").on("click", function () {
        var link = $(this);
        var contentDiv = link.parent().prev("div.paste-content");
        var linkText = link.text().toUpperCase();

        if (linkText === "SHOW MORE") {
            linkText = "Show Less";
            contentDiv.removeClass('hideContent');
            contentDiv.addClass('showContent');
        } else {
            linkText = "Show More";
            contentDiv.removeClass('showContent');
            contentDiv.addClass('hideContent');
        };

        link.text(linkText);
    });
});

function linkRemove(element) {
    element.find('#remove-item').click(function () {
        element.remove();
    });
}
function linkMoveUp(element) {
    element.find('#move-up').click(function () {
        moveUp(element);
    });
}
function linkMoveDown(element) {
    element.find('#move-down').click(function () {
        moveDown(element);
    });
}