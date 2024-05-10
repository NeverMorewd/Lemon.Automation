window.onload = function () {
    const FILTER_KEY = 'filterUrl';

    function saveOptions(value) {
        chrome.storage.local.set(value)
    }

    document.getElementById('filter-url').addEventListener('change', function (e) {
        saveOptions({ [FILTER_KEY]: e.target.value || '' })
    })

    chrome.storage.local.get([FILTER_KEY]).then((result) => {
        var value = result[FILTER_KEY];
        document.getElementById('filter-url').value = value || ''
    });

}