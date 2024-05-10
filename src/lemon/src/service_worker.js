
function download(url) {
    var options = {
        url: url
    }
    chrome.downloads.download(options)
}

//
async function onMenuClick() {
    //
    const [tab] = await chrome.tabs.query({ active: true, lastFocusedWindow: true });
    //
    var response = await chrome.tabs.sendMessage(tab.id, { type: 'images' });
    var data = response || []
    //
    chrome.storage.local.get(['filterUrl']).then((result) => {
        var value = result['filterUrl'];
        console.log(value)
        if (value) {
            data.filter(src => src.indexOf(value) != -1).map(download)
        } else {
            data.map(download)
        }
    });
}



chrome.contextMenus.create({
    type: 'normal',
    title: '下载图片',
    contexts: ['all'],
    id: 'menu-1'
});


chrome.contextMenus.onClicked.addListener(function (data) {
    if (data.menuItemId == 'menu-1') {
        console.log(data)

        if (data != null && data != undefined)
        {
            if (data['srcUrl'] != null && data['srcUrl'] != undefined)
            {
                download(data['srcUrl'])
                return
            }
        }
        console.warn('This is not an image!');
    }
})


chrome.runtime.onMessage.addListener(function (message, sender, sendResponse) {
    if (message.type == 'down') {
        download(message.data)
    } else if (message.type == 'badge') {
        chrome.action.setBadgeBackgroundColor({ color: '#f00' })
        chrome.action.setBadgeText({
            text: message.data
        })
    }
});