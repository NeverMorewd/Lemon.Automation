function handle(method, params) {
    try {
        if (method === 'init') {
            //const result = eval.call(window, params.code)
            return { code: 0, result: 'ok' }
        }
        if(method === 'getCodeId')
        {
            if (typeof iirpa === 'undefined') 
            {
                return { code: -2, message: 'need init' }
            }
            else
            {
                result = iirpa.codeId;
                return {code: 0, result: result}
            }
        }
        else {
            if (typeof iirpa === 'undefined') {
                return { code: -2, message: 'need init' }
            }
            else {
                return iirpa.handle(method, params)
            }
        }
    }
    catch (error) {
        return {
            code: 1,
            error: {
                code: -1, // ERRORS.UNKNOWN
                message: error.stack
            }
        }
    }
}

// function ping() {
//     chrome.runtime.sendMessage('ping', response => {
//       if(chrome.runtime.lastError) {
//         setTimeout(ping, 1000);
//       } else {
//         // Do whatever you want, background script is ready now
//       }
//     });
//   }
  
// ping();

// 监听来自插件的消息
chrome.runtime.onMessage.addListener((message, sender, sendResponse) => 
{
    console.log(message);
    console.log(sender);
    console.log(sendResponse);
    if(typeof(message.type) == "string")
    {
        var result = handle(message.type);
        sendResponse({ message: result });
    }
    else
    {
        iirpanew.handle(message.type, function (response)
        {
            //setTimeout(ping, 1000);
            sendResponse({ message: response })
        })
    }
    return true;
});

//   window.addEventListener('message', (event) => {
//     console.log('EVAL output', event.data);
//     });
// if ("serviceWorker" in navigator) {
//     navigator.serviceWorker.register('/service_worker.js', {scope: '/'}).then(function(registration) {
//       console.log("Service Worker registered with scope: ", registration.scope);
//     }).catch(function(err) {
//       console.log("Service Worker registered failed:", err);
//     });
//   }
  