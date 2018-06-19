$(document).ready(function () {
    linkCreateVault($('#create-vault'));

    $('#add-to-vault-menu').find('.add-to-vault').each(function () {
        linkAddToVault($(this));
    });

    if (useFormat) {
        var code = document.querySelector('#code');
        var worker = new Worker(GenerateBlobURL(highlightWorkerSrc));
        var scriptBlob = GenerateBlobURL(highlightSrc);
        worker.onmessage = function (event) {
            code.innerHTML = event.data.value;
            if (autoDetect) {
                $('#syntaxLanguage').html('Auto-Detect (' + event.data.language + ')');
            }
        }
        worker.postMessage({
            content: code.textContent,
            script: scriptBlob,
            format: format,
            autoDetect: autoDetect
        });
    }
});

function linkCreateVault(element) {
    element.click(function () {
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(addParamsToUrl(createVaultURL, { items: encodeURIComponent(pasteUrl + ':' + pasteTitle) }), '_blank');
    });
}

function linkAddToVault(element) {
    element.click(function () {
        var addToVaultURL = $(this).data('add-to-vault-url');
        var pasteUrl = $(this).data('paste-url');
        var pasteTitle = $(this).data('paste-title');
        window.open(addParamsToUrl(addToVaultURL, { items: encodeURIComponent(pasteUrl + ':' + pasteTitle) }), '_blank');
    });
}