
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
});


// chrome.runtime.onMessage.addListener(function (message, sender, sendResponse) {
//     if (message.type == 'down') {
//         download(message.data)
//     } else if (message.type == 'badge') {
//         chrome.action.setBadgeBackgroundColor({ color: '#f00' })
//         chrome.action.setBadgeText({
//             text: message.data
//         })
//     }
// });
let message;
let port = null;
var responseMessage = "";
const hostName = 'lemon.automation.nativehost';
chrome.runtime.onMessage.addListener(
  function(request, sender, sendResponse) 
  {
      //console.log(request.phonenumber);
    //   chrome.runtime.sendNativeMessage(
    //     hostName,
    //     {text: 'Hello'},
    //     function (response) {
    //       console.log('Received ' + response);
    //     }
    //   );
        if(port)
        {
            port.postMessage('{"text":"test!"}');
            sendResponse(responseMessage);
        }
  });

chrome.runtime.onInstalled.addListener(function (details) {
    try {
        console.log('Z-RPA Extension: ' + details.reason)

        chrome.tabs.query({}, (tabsList) => {
            for (tab of tabsList) {
                if (tab.url && tab.url.indexOf("chrome://") != 0) {
                    chrome.tabs.reload(tab.id)
                }
            }
        })
    } catch (e) {
        console.error("unable to installed: " + e)
    }
})

chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    connect();
})

self.addEventListener('activate', (event) => {
    connect();
  });

// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.



function appendMessage(text) {
  //document.getElementById('response').innerHTML += '<p>' + text + '</p>';
  console.log(text);
}


function sendNativeMessage() {
  message = 'hello';
  port.postMessage(message);
  appendMessage('Sent message: <b>' + JSON.stringify(message) + '</b>');
}

function onNativeMessage(message) {
  appendMessage('Received message: <b>' + JSON.stringify(message) + '</b>');
}

//使用 chrome.runtime.connectNative() 连接到原生消息传递主机会使 Service Worker 保持活跃状态。
//如果主机进程崩溃或关闭，端口会关闭，Service Worker 将在计时器完成后终止。
//可通过在端口的 onDisconnect 事件处理脚本中调用 chrome.runtime.connectNative() 来防范此问题。
function onDisconnected() {
  if (chrome.runtime.lastError) {
    console.error(chrome.runtime.lastError);
  }
  port = null;
  //port = chrome.runtime.connectNative(hostName);
  //updateUiState();
}

///https://github.com/GoogleChrome/developer.chrome.com/issues/2688
function connect() {

  if(!port)
  {
    appendMessage('Connecting to native messaging host <b>' + hostName + '</b>');
    port = chrome.runtime.connectNative(hostName);
    port.onMessage.addListener(onNativeMessage);
    port.onDisconnect.addListener(onDisconnected);
  }

  //updateUiState();
}

// document.addEventListener('DOMContentLoaded', function () {
//   //document.getElementById('connect-button').addEventListener('click', connect);
//   connect();
//   sendNativeMessage();
//   //document.getElementById('send-message-button').addEventListener('click', sendNativeMessage);
//   //updateUiState();
// });
