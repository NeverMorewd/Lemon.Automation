/**
 * @param {*} src 
 * @returns 
 */
function getByte(src) {
    return fetch(src).then(function (res) {
        return res.blob()
    }).then(function (data) {
        return (data.size / (1024)).toFixed(2) + 'kB'
    })
}

/**
 * @param {*} el 
 * @param {number} byte zijie
 */
function showInfo(el, byte) {
    var html = `naturalregion:${el.naturalWidth}*${el.naturalHeight}\n region:${el.width}*${el.height}\n size:${byte}`;
    el.title = html
}

function download(url) {
    var options = {
        url: url
    }
    chrome.downloads.download(options)
}

//
document.addEventListener('mouseover', function (e) {
    if (e.target.tagName == 'IMG') {
        getByte(e.target.src).then(byte => {
            showInfo(e.target, byte)
        })
    }
}, true)

//
document.addEventListener('dragend', async function (e) {
    if (e.target.tagName == 'IMG') {
        await chrome.runtime.sendMessage({ type: 'down', data: e.target.src });
    }
})


chrome.runtime.onMessage.addListener(function (message, sender, sendResponse) {
    if (message.type == 'images') {
        var imgs = document.querySelectorAll('img');
        var srcs = Array.from(imgs).map(img => img.src)
        sendResponse(srcs);
    }
});


window.addEventListener('load', async function (e) {
    var imgs = document.querySelectorAll('img');
    await chrome.runtime.sendMessage({ type: 'badge', data: imgs.length + '' });
})

