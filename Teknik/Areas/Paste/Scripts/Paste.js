$(document).ready(function () {
    $('#content').focus();

    linkCreateVault($('#create-vault'));
});

function linkCreateVault(element) {
    element.click(function () {
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(createVaultURL + '&urls=' + encodeURIComponent(pasteUrl + ':' + pasteTitle), '_blank');
    });
}