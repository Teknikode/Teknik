$(document).ready(function () {
    $('#content').focus();

    linkCreateVault($('#create-vault'));

    $('#add-to-vault-menu').find('.add-to-vault').each(function () {
        linkAddToVault($(this));
    });
});

function linkCreateVault(element) {
    element.click(function () {
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(createVaultURL + '&items=' + encodeURIComponent(pasteUrl + ':' + pasteTitle), '_blank');
    });
}

function linkAddToVault(element) {
    element.click(function () {
        var addToVaultURL = $(this).data('add-to-vault-url');
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(addToVaultURL + '&items=' + encodeURIComponent(pasteUrl + ':' + pasteTitle), '_blank');
    });
}