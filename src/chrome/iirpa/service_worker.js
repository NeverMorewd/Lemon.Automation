// var url1 = registration.scope + 'jquery.js';
// importScripts(url1);

class BackgroundPage {
    static extensionUrl = chrome.runtime.getURL('iirpa/ServiceWorkerPage.html')
    static extensionWndData = {
        state: "minimized",
        type: "popup",
        url: BackgroundPage.extensionUrl
    }
    static async getExtensionTab() {
        const [tab] = await chrome.tabs.query({url: BackgroundPage.extensionUrl, windowType: 'popup'})
        return tab
    }
    static async createExtensionTab() {
        if (!(await BackgroundPage.getExtensionTab())) {
            let tab = chrome.windows.create(BackgroundPage.extensionWndData)
        }
    }
    static {
        BackgroundPage.createExtensionTab()
    }
}


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

        BackgroundPage.createExtensionTab();
        //debuggerManager.init()
    } catch (e) {
        console.error("unable to installed: " + e)
    }
})

// chrome.runtime.onConnect.addListener(port => {
//     port.onMessage.addListener(msg => 
//     {
//       console.log("onMessage");
//       console.log(msg);
//     });
//   });
  
//chrome.runtime.onMessage.addListener((request, sender, sendResponse) => sendResponse('pong'));
// const nativeMsgHost = function () {
//     /*  https://developers.chrome.com/extensions/nativeMessaging*/
//     const clientFlag = "Chrome";
//     const hostName = "ii.rpa.chromenativemsg";
//     const version = "2.7"
//     var traceEnabled = false;
//     var rpa_conn_service;
//     var tryingToReconnect = false;
//     var rpa_conn_native = chrome.runtime.connectNative(hostName);  
//     rpa_conn_native.onDisconnect.addListener(onDisconnected);
//     rpa_conn_native.onMessage.addListener((message) => {
//         try {
//             var data = {};
//             if (message.Action && message.Action == "SendTransferMessage") {
//                 data = message;
//                 data.connType = "nativemsg_tMsg";
//             }
//             else {
//                 data.SendData = message;
//                 data.connType = "nativemsg";
//             }
//             received(data);
//         }
//         catch (e) {
//             traceLog("nativemsghost message exception: " + e)
//             responseFail(-1000101, "nativemsghost message exception!", data)
//         }
//     })
//     function traceLog(message) {
//         if (traceEnabled) {
//             try {
//                 console.log(new Date().toLocaleString() + ":" + message);
//             } catch (e) { }
//         }
//     }
//     function received(data) {
//         var message = data.SendData;
//         if (message) {
//             traceLog(message.method);
//             console.log(message.method);
//             chrome.windows.getCurrent(function (currentWindow) 
//             {
//                 chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
//                 {
//                     //debuggerManager.sendCommand(activeTabs[0].id,'Runtime.evaluate','alert("hello")')
//                 });
//             });
//             switch (message.method) {
//                 case "getVersion": 
//                     responseOK(version, data); 
//                     break;
//                 case "ping": 
//                     responseOK('pong', data); 
//                     break;
//                 case "init":
//                     try {
//                         //注入
//                         traceLog("init code");
//                         //eval.call(this, message.params.code)
//                         // self.clients.matchAll().then(function(clients) {
//                         //     clients.forEach(function(client) {
//                         //       console.log("client:");
//                         //       console.log(client);
//                         //       console.log("message.params.code");
//                         //       client.postMessage(message.params.code);
//                         //       console.log(message.params.code);
//                         //     });
//                         //   });

//                           chrome.windows.getCurrent(function (currentWindow) 
//                           {
//                             chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
//                             {
//                                 //if (activeTabs[0].url?.startsWith("chrome://")) return undefined;
//                                 chrome.tabs.sendMessage(activeTabs[0].id, { type: 'init', code: message.params.code });
//                                 // fetch(chrome.runtime.getURL('iirpa/utils.js'))
//                                 //     .then(response => response.text())
//                                 //     .then(scriptContent => {
//                                 //         // 使用 chrome.scripting.executeScript 执行外部 JavaScript 文件的内容
//                                 //         chrome.scripting.executeScript({
//                                 //         target: { tabId: activeTabs[0].id },
//                                 //         function: (scriptContent) => {
//                                 //             // 在标签页中执行外部 JavaScript 文件的内容
//                                 //             eval(scriptContent);
//                                 //         },
//                                 //         args: [scriptContent] // 将外部文件内容作为参数传递给执行函数
//                                 //         });
//                                 //     })
//                                 //     .catch(error => {
//                                 //         console.error('Error fetching or executing script:', error);
//                                 //     });

//                                 // chrome.scripting.executeScript(
//                                 // {
//                                 //     target: {tabId: activeTabs[0].id, allFrames: true}, 
//                                 //     files: ['./external.js'],
//                                 //     // func: () => {
//                                 //     //     document.body.style.border = "5px solid green";
//                                 //     //   },
//                                 // });
//                             });
//                           });
//                         console.log("init code");
//                         responseOK("init fulfill", data)
//                     } catch (error) {
//                         responseFail(-1, error.stack, data)
//                     }
//                     break;
//                 case "getCodeId": 
//                     chrome.windows.getCurrent(function (currentWindow) 
//                     {
//                         chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
//                         {
//                             var result = chrome.tabs.sendMessage(activeTabs[0].id, { type: 'getCodeId', code: data } );
//                             result.then(function (value)
//                             {
//                                 console.log(value);
//                                 responseOK(value.message.result,data);
//                             })
//                         });
//                     });
//                     break;
//                 case "restore":
//                     responseOK("restore", data);
//                     break;
//                 default:
//                     handle(message, (response) => 
//                     {
//                         data.CallbackReslut = response;
                        
//                         responseMessage(data);

//                         if (message.method == "tabs.closeAll") 
//                         {
//                             //todo websocket
//                             // tryingToReconnect = false;
//                             // rpa_conn_service.disconnectTimeout = 500;
//                             // rpa_conn_service.stop();
//                         }
//                     })
//                     // chrome.windows.getCurrent(function (currentWindow) 
//                     // {
//                     //     chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
//                     //     {
//                     //         chrome.tabs.sendMessage(activeTabs[0].id, { type: message, code: data }, function (response) 
//                     //         {
//                     //             data.CallbackReslut = response;
//                     //             console.log(data);
//                     //             responseMessage(data);
//                     //         });
//                     //     });
//                     // });
//                     // if (typeof iirpa == "undefined" || !iirpa.codeId) {
//                     //     console.log("iirpa == undefined");
//                     //     responseFail(-100099, "script not injected!", data)
//                     // }
//                     // else if (message.codeId && message.codeId != iirpa.codeId) {
//                     //     responseFail(-1000100, "script required injected!", data)
//                     // }
//                     // else {
//                     //     delete message.codeId;
//                     //     iirpa.handle(message, (response) => 
//                     //     {
//                     //         data.CallbackReslut = response;
                            
//                     //         responseMessage(data);

//                     //         if (message.method == "tabs.closeAll") 
//                     //         {
//                     //             //todo websocket
//                     //             // tryingToReconnect = false;
//                     //             // rpa_conn_service.disconnectTimeout = 500;
//                     //             // rpa_conn_service.stop();
//                     //         }
//                     //     })
//                     // }
//                     break;
//             }
//         }
//         else {
//             // console.error('nknown message');
//             // console.error(data);
//         }

//     }
    
//     function onDisconnected()
//     {
//         appendMessage('Failed to connect: ' + chrome.runtime.lastError.message);
//         rpa_conn_native = null;
//         rpa_conn_native = chrome.runtime.connectNative(hostName)
//     }

//     // 在扩展中执行动态代码的函数
//     function executeCodeInActiveTab(code) {
//         chrome.tabs.query({ active: true, currentWindow: true }, tabs => {
//         if (tabs && tabs.length > 0) {
//             const activeTabId = tabs[0].id;
//             chrome.tabs.sendMessage(activeTabId, { type: 'executeCode', code }, response => {
//             if (response && response.type === 'executionResult') {
//                 console.log('Execution result:', response.result);
//             } else if (response && response.type === 'executionError') {
//                 console.error('Execution error:', response.error);
//             }
//             });
//         } else {
//             console.error('No active tab found.');
//         }
//         });
//     }
  
  
  

//     function responseOK(content, data) {
//         data.CallbackReslut = {
//             code: 200,
//             status: 'OK',
//             result: {
//                 content: content,
//                 error: null
//             }
//         };
//         responseMessage(data);
//     }

//     function responseFail(code, message, data) {
//         data.CallbackReslut = {
//             code: 200,
//             status: 'OK',
//             result: {
//                 content: null,
//                 error: {
//                     code: code,
//                     message: message
//                 }
//             }
//         }
//         responseMessage(data);
//     }
//     function responseMessage(data) {
//         if (data.connType == "nativemsg_tMsg") {
//             data.SendData = "";
//             data.MessageType = 2;//cakkback
//             rpa_conn_native.postMessage(data);
//         }
//         else {
//             rpa_conn_native.postMessage(data.CallbackReslut);
//         }
//     }
// }();


