const ERRORS = {
    UNKNOWN: -1,  // 未知异常    
    PERMISSION: 2,// 权限问题
    NO_RESOURCE: 3,  // 无请求资源
    COMMON: 100,  // 通用异常
}

class HandlerError extends Error {
    constructor(message, code) {
        super(message || "")
        this.code = code || ERRORS.COMMON
    }
}

class ContentUnknownError extends Error {
    constructor(message) {
        super(message || "")
    }
}

const WRAPPER_API = {
    'debugger': [
        'attach',
        'detach',
        'sendCommand',
        'getTargets'
    ]
}

const wrapperChromeApi = (moduleName, methodName, checkError = true) => {
    let apiRoot = chrome
    if (!apiRoot || !apiRoot[moduleName] || !apiRoot[moduleName][methodName]) {
        return () => Promise.reject(new HandlerError('No permissions.', ERRORS.PERMISSION))
    }
    if (apiRoot) {
        return apiRoot[moduleName][methodName].bind(apiRoot[moduleName])
    }
    return function () {
        return new Promise((res, rej) => {
            chrome[moduleName][methodName].apply(chrome[moduleName], argToArray(arguments).concat([function () {
                if (checkError && chrome.runtime.lastError) {
                    rej.apply(null, [chrome.runtime.lastError].concat(argToArray(arguments)))
                }
                res.apply(null, argToArray(arguments))
            }]))
        })
    }
}

const crx = Object.assign({
    webNavigation:
    {
        getAllFrames: (tabId) => {
            return new Promise((resolve, reject) => {
                chrome.webNavigation.getAllFrames({
                    tabId: tabId
                }, (frames) => {
                    resolve(frames)
                })
            })
        }
    },
    windows: {
        getAll: (queryInfo) => {
            return new Promise((resolve, reject) => {
                chrome.windows.getAll(queryInfo, (wnd) => {
                    resolve(wnd)
                })
            })
        },
        get: (windowId) => {
            return new Promise((resolve, reject) => {
                chrome.windows.get(windowId, null, (wnd) => {
                    resolve(wnd)
                })
            })
        },
        getLastFocused: () => {
            return new Promise((resolve, reject) => {
                chrome.windows.getLastFocused(null, (wnd) => {
                    resolve(wnd)
                })
            })
        },
        update: (windowId, updateInfo) => {
            return new Promise((resolve, reject) => {
                chrome.windows.update(windowId, updateInfo, (wnd) => {
                    resolve(wnd)
                })
            })
        },
        getCurrent: () => {
            return new Promise((resolve, reject) => {
                chrome.windows.getCurrent(null, (wnd) => {
                    resolve(wnd)
                })
            })
        }
    },
    cookies: {
        getAll: (details) => {
            return new Promise((resolve, reject) => {
                chrome.cookies.getAll(details, (cookies) => { resolve(cookies) })
            })
        },
        remove: (details) => {
            return new Promise((resolve, reject) => {
                chrome.cookies.remove(details, () => { resolve() })
            })
        },
        set: (details) => {
            return new Promise((resolve, reject) => {
                chrome.cookies.set(details, () => { resolve() })
            })
        }
    },
    downloads: {
        addCreatedEvent: () => {
            chrome.downloads.onCreated.removeListener(utils.record);
            chrome.downloads.onCreated.addListener(utils.record);
        },
        removeCreatedEvent: () => {
            downloadData = null;
            chrome.downloads.onCreated.removeListener(utils.record);
        },
        search: (queryInfo) => {
            return new Promise((resolve, reject) => {
                chrome.downloads.search(queryInfo, (items) => {
                    resolve(items)
                })
            })
        },
        searchV1: (queryInfo) => {
            return new Promise((resolve, reject) => {
                chrome.downloads.search(queryInfo, (items) => {
                    if (downloadData)
                        items = [downloadData]
                    resolve(items)
                    downloadData = null
                })
            })
        },
        download: (options) => {
            return new Promise((resolve, reject) => {
                chrome.downloads.download(options, (downloadId) => {
                    resolve(downloadId)
                })
            })
        }
    },
    tabs: {
        query: (queryInfo) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.query(queryInfo, (tabs) => {
                    resolve(tabs)
                })
            })
        },
        create: (createInfo) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.create(createInfo, (tab) => {
                    resolve(tab)
                })
            })
        },
        update: (tabId, updateInfo) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.update(tabId, updateInfo, (tab) => {
                    resolve(tab)
                })
            })
        },
        get: (tabId) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.get(tabId, (tab) => { resolve(tab) })
            })
        },
        reload: (tabId, bypassCache) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.reload(tabId, {
                    bypassCache: bypassCache
                }, () => {
                    resolve()
                })
            })
        },
        goForward: (tabId) => {
            return new Promise((resolve, reject) => { chrome.tabs.goForward(tabId, () => { resolve() }) })
        },
        goBack: (tabId) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.goBack(tabId, () => { resolve() })
            })
        },
        remove: (tabIds) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.remove(tabIds, () => { resolve() })
            })
        },
        getZoom: (tabId) => {
            return new Promise((resolve, reject) => {
                chrome.tabs.getZoom(tabId, (factor) => { resolve(factor) })
            })
        },
        executeScriptOnFrame: (tabId, frameId, code) => {
            return new Promise((resolve, reject) => {
                const details = {
                    frameId: frameId,
                    code: code,
                    matchAboutBlank: true
                }
                chrome.tabs.executeScript(tabId, details, (result) => {
                    // result存在以下三种情况:                         
                    // 1. 正常执行返回数组(因为指定了frameId，所以数组中只有一项)                         
                    // 1.1 有返回值即是数组中的第一项                         
                    // 1.2 无返回值数组的第一项为null                         
                    // 2. Javascript执行失败，返回数组，且第一项为null，同1.2                         
                    // 3. 发生跨域错误，这时需要对chrome.runtime.lastError添加判断，否则会在控制台打印错误信息                         
                    if (chrome.runtime.lastError) {
                        reject(new CrossOriginError(chrome.runtime.lastError.message))
                    } else {
                        if (Array.isArray(result) && result.length == 1) {
                            resolve(result[0])
                        } else {
                            reject(new Error(`unknown error: fail to execute script on ${frameId}`))
                        }
                    }
                })
            })
        },
        requestWait: async (actionPromise, timeout) => {
            if (!timeout) {
                return await Promise.race([actionPromise]);
            }
            const timeoutPromise = new Promise(function (resolve) {
                setTimeout(function () {
                    resolve(null);
                }, timeout);
            });
            return await Promise.race([actionPromise, timeoutPromise])
        },
        requestOnFrame: async (tabId, frameId, method, params, timeout) => {

            // 从最外层Iframe遍历到最内层Iframe                 
            // response存在以下四种情况                 
            // 0: 成功 { code: 0, result: null }                 
            // -1: 继续下次Frame请求(跨域) { code: -1, next: { frameIndex: 111, params: params } }                 
            //     next.params可能存储了上一次请求的结构                 
            // -2: content需要初始化 { code: -2, message: 'need init' }                 
            // 1: 请求失败 { code: 1, error: {code: -1, message: ''} }                 
            // undefined/null: content script注入失败(出现在某些about:blank页面)                  
            if (!params) {
                params = {}
            }
            if (!params.codeId) {
                params.codeId = CODE_ID
            }
            params.frameId = frameId
            const code = `handle('${method}', ${JSON.stringify(params)})`
            const response = await crx.tabs.executeScriptOnFrame(tabId, frameId, code)
            if (!response) {
                return null
            } else if (response.code == 0) {
                return response.result
            } else if (response.code == -1) {
                // continue to next frame                     
                let frames = await crx.webNavigation.getAllFrames(tabId)
                for (const frame of frames) {
                    if (frame.parentFrameId === frameId) {
                        try {
                            const getFrameIndex = crx.tabs.requestOnFrame(tabId, frame.frameId, 'getFrameIndex');
                            let frameIndex = await crx.tabs.requestWait(getFrameIndex, 200);
                            if (frameIndex == null) {
                                continue;
                            }
                            /*let frameIndex = await crx.tabs.requestOnFrame(tabId, frame.frameId, 'getFrameIndex')*/
                            if (frameIndex == response.next.frameIndex) {
                                const request = crx.tabs.requestOnFrame(tabId, frame.frameId, method, response.next.params, timeout)
                                return await crx.tabs.requestWait(request, timeout);
                            }
                        } catch (e) {
                            if (e instanceof CrossOriginError) {
                                continue
                                // 忽略跨域异常                                 
                            } else { throw e }
                        }
                    }
                }
                throw new Error(`fail to request on frame, cannot call next frame`)
            } else if (response.code == -2) {
                // need init content                     
                const requestInit = crx.tabs.requestOnFrame(tabId, frameId, 'init', { code: contentScript }, timeout)
                await crx.tabs.requestWait(requestInit, timeout);
                // retry                     
                const request = crx.tabs.requestOnFrame(tabId, frameId, method, params, timeout)
                return crx.tabs.requestWait(request, timeout);
            } else {
                // code === 1                     
                const error = response.error
                if (error.code == ERRORS.UNKNOWN) {
                    throw new ContentUnknownError(error.message)
                } else {
                    throw new HandlerError(error.message, error.code)
                }
            }
        },
        requestOnFrameReverse: async (tabId, frameId, method, params) => {
            // 从最内层Iframe遍历到最外层Iframe
            // response存在以下四种情况
            // 0: 成功 { code: 0, result: null }                 
            // -1: 继续下次Frame请求(跨域) { code: -1, next: { params: params } }                 
            //     next.params可能存储了上一次请求的结构                 
            // -2: content需要初始化 { code: -2, message: 'need init' }                 
            // 1: 请求失败 { code: 1, error: {code: -1, message: ''} }                 
            if (!params) {
                params = {}
            } if (!params.codeId) {
                params.codeId = CODE_ID
            } params.frameId = frameId
            const code = `handle('${method}', ${JSON.stringify(params)})`
            const response = await crx.tabs.executeScriptOnFrame(tabId, frameId, code)
            if (!response) {
                return null
            } else if (response.code == 0) {
                return response.result
            } else if (response.code == -1) {
                // continue to next frame                     
                let frames = await crx.webNavigation.getAllFrames(tabId)
                for (const frame of frames) {
                    if (frame.frameId === frameId) {
                        try {
                            return await crx.tabs.requestOnFrameReverse(tabId, frame.parentFrameId, method, response.next.params)
                        } catch (e) {
                            if (e instanceof CrossOriginError) {
                                continue
                                // 忽略跨域异常                                 
                            } else { throw e }
                        }
                    }
                }
                throw new Error(`fail to request on frame, cannot call next frame`)
            } else if (response.code == -2) {
                // need init content 
                await crx.tabs.requestOnFrame(tabId, frameId, 'init', { code: contentScript })
                // retry                     
                return await crx.tabs.requestOnFrameReverse(tabId, frameId, method, params)
            } else {
                // code === 1                     
                const error = response.error
                if (error.code == ERRORS.UNKNOWN) {
                    throw new ContentUnknownError(error.message)
                } else {
                    throw new HandlerError(error.message, error.code)
                }
            }
        },
        captureVisibleTab: wrapperChromeApi('tabs', 'captureVisibleTab'),
        captureTab: wrapperChromeApi('tabs', 'captureTab')
    }
}, (() => {
    const wrapper = {}
    for (const [key, value] of Object.entries(WRAPPER_API)) {
        wrapper[key] = {}
        value.forEach(v => wrapper[key][v] = wrapperChromeApi(key, v))
    }
    return wrapper
})())

class Debugger {
    constructor(tabId) {
        this.tabId = tabId
        this._ready = undefined
        this._caches = new Map()
    }

    addCache(method, filter) {
        if (!this._caches.get(method)) {
            this._caches.set(method, [])
        }
        const caches = this.getCache(method)
        caches.splice(0, caches.length)
        const factory = EVENT_CACHE_FACTORY[method] || EventCache
        const cache = new factory(200, filter)
        caches.push(cache)
        return cache.id
    }

    getCache(method) {
        return this._caches.get(method) || []
    }

    async _attach() {
        try {
            try {
                await crx.debugger.attach({ tabId: this.tabId }, '1.3');
            }
            catch (e) {
                if (!(e.message && e.message.startsWith('Another debugger is already attached'))) {
                    throw e
                }
            }
            this._sendCommand('Page.enable', {})
            this._sendCommand('Runtime.enable', {})
        } catch (e) {
            this._ready = undefined
            throw e
        }
    }

    _sendCommand(method, commandParam) {
        return crx.debugger.sendCommand({ tabId: this.tabId }, method, commandParam)
    }

    async attach() {
        if (this._ready) {
            return await this._ready
        }
        this._ready = this._attach()
        return await this._ready
    }

    async detach() {
        if (this._ready) {
            await this._ready
            await crx.debugger.detach({ tabId: this.tabId })
            this.markDetached()
            // 仅在手动调用detach时清理数据
            this.clear()
        }
    }

    triggerEvent(method, params) {
        this.getCache(method).forEach(c => c.push(params, this.tabId))
    }

    markDetached() {
        this._ready = undefined
    }

    clear() {
        this._caches.clear()
    }

    async sendCommand(method, commandParam) {
        await this.attach()
        return await this._sendCommand(method, commandParam)
    }
}

class DebuggerManager {
    constructor() {
        this._debuggers = new Map()
    }

    init() {
        this.onEvent()
        this.onDetach()
    }

    onEvent() {
        if (chrome.debugger) {
            chrome.debugger.onEvent.addListener(({ tabId }, method, params) => {
                const debuggee = this.get(tabId)
                if (debuggee) {
                    debuggee.triggerEvent(method, params)
                }
            })
        }
    }

    onDetach() {
        if (chrome.debugger) {
            // 仅标记断开状态
            chrome.debugger.onDetach.addListener(({ tabId }, reason) => {
                const debuggee = this.get(tabId)
                if (debuggee) {
                    switch (reason) {
                        case 'target_closed':
                        case 'canceled_by_user':
                        default:
                            debuggee.markDetached()
                    }
                }
            })
        }
    }

    check(tabId) {
        return !!this._debuggers.get(tabId)
    }

    get(tabId) {
        let debuggee = this._debuggers.get(tabId)
        if (!debuggee) {
            debuggee = new Debugger(tabId)
            this._debuggers.set(tabId, debuggee);
        }
        return debuggee
    }

    addCache(tabId, method, params) {
        return this.get(tabId).addCache(method, params)
    }

    getCache(tabId, method) {
        return this.get(tabId).getCache(method).reduce((all, cache) => all.concat(cache.get()), [])
    }

    getCacheBody(tabId, requestId) {
        const cache = this.get(tabId).getCache('Network.responseReceived')[0]
        return cache ? cache.getCacheBody(requestId) : null
    }

    sendCommand(tabId, method, commandParam) {
        return this.get(tabId).sendCommand(method, commandParam)
    }

    async getResponseBody(tabId, requestId) {
        try {
            return await this.sendCommand(tabId, 'Network.getResponseBody', {
                requestId
            })
        } catch (error) {
            const parseError = tryParseJSON(error.message)
            if ((parseError || {}).code === -32000) {
                return {}
            } else {
                throw error
            }
        }
    }

    detach(tabId) {
        return this.get(tabId).detach()
    }
}

const debuggerManager = new DebuggerManager()

const response = {
    ok: (content) => {
        return {
            code: 200,
            status: 'OK',
            result: {
                content: content, error: null
            }
        }
    },
    fail: (code, message) => {
        return {
            code: 200,
            status: 'OK',
            result: {
                content: null,
                error:
                {
                    code: code,
                    message: message
                }
            }
        }
    }
}

const handlers = {
    dataCapture:
    {
        startConfig: async (params) => {
            if (!isConfig) {
                isConfig = true;
                return await window.startConfig(params.config);
            } else {
                return null
            }
        },
        startCapture: async (params) => {
            if (!isStart) {
                isStart = true;
                return await window.startCapture(params.info);
            } else {
                return null
            }
        },
        closeStart: async (params) => {
            isStart = false;
        },
        closeConfig: async (params) => {
            isConfig = false;
        },
        isExecutorJavaScript: async (params) => {
            // params.tabId
            // params
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, "isExecutorJavaScript", params)
            return result
        },
        initInjectScript: async (params) => {
            //code : "console.log('abc')"
            await crx.tabs.requestOnFrame(params.tabId, 0, 'init', { code: params.code })
        },
    },
    windows:
    {
        active: async (params) => {
            await chrome.windows.update(params.windowId, { focused: true });
        },
        getAll: async (params) => {
            const result = await crx.windows.getAll({ "populate": true, "windowTypes": params.windowTypes });
            return result;
        },
        update: async (params) => {
            // {                    
            //     windowId: 123                     
            //     updateInfo: https://developers.chrome.com/extensions/windows#method-update                     
            // }                     
            await crx.windows.update(params.windowId, params.updateInfo)
        },
        maximize: async (params) => {
            var winId = params.windowId;
            if (!params.windowId || params.windowId < 0) {
                const wnd = await crx.windows.getCurrent()
                winId = wnd.id
            }
            await crx.windows.update(winId, { 'state': 'maximized' })
        },
        minimized: async (params) => {
            const wnd = await crx.windows.getCurrent()
            await crx.windows.update(wnd.id, { 'state': 'minimized' })
        },
    },
    cookies:
    {
        getAll: async (params) => {
            // {                     
            //     string\t(optional) url\t                     
            //     string\t(optional) name\t                     
            //     string\t(optional) domain\t                     
            // }                     
            params = utils.ignoreEmptyProperties(params)
            const cookies = await crx.cookies.getAll(params)
            if (chrome.runtime.lastError) {
                throw new HandlerError(`获取Cookies失败, ${chrome.runtime.lastError.message}`)
            } else {
                for (const cookie of cookies) {
                    if (cookie.expirationDate) {
                        cookie.expirationDate = (new Date(cookie.expirationDate * 1000)).toLocaleString()
                    }
                }
                return cookies
            }
        },
        remove: async (params) => {
            // {                     
            //     string\turl\t                     
            //     string\tname\t                     
            // }                     
            await crx.cookies.remove(params)
            if (chrome.runtime.lastError)
                throw new HandlerError(`删除Cookies失败, ${chrome.runtime.lastError.message}`)
        },
        set: async (params) => {
            // {                    
            //     string\turl\t                     
            //     string\t(optional) name\t                    
            //     string\t(optional) value\t                     
            //     string\t(optional) domain\t                     
            //     string\t(optional) path\t                     
            //     boolean\t(optional) secure\t                     
            //     boolean\t(optional) httpOnly\t                     
            //     string\t(optional) expirationDate                     
            // }                     
            params = utils.ignoreEmptyProperties(params)
            if (params.expirationDate) {
                params.expirationDate = (new Date(params.expirationDate)).getTime() / 1000
            }
            await crx.cookies.set(params)
            if (chrome.runtime.lastError)
                throw new HandlerError(`设置Cookies失败, ${chrome.runtime.lastError.message}`)
        }
    },
    tabs:
    {
        getIntoViewPortRect: async (params) => {
            return await crx.tabs.requestOnFrame(params.tabId, 0, 'getIntoViewPortRect', params)
        },
        create: async (params) => {
            const result = await crx.tabs.create({ "url": params.url })
            return result
        },
        scroll: async (params) => {
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, 'scroll', params)
            return result
        },
        getScroll: async (params) => {
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, 'getScroll', params)
            return result
        },
        scrollTo: async (params) => {
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, 'scrollTo', Object.assign({
                behavior: 'smooth'
            }, params))
            return result
        },
        stop: async (params) => {
            // {                     
            //     tabId: 123
            // }  
            const result = await crx.tabs.executeScriptOnFrame(params.tabId, 0, 'window.stop()')
            return result
        },
        getDocument: async (params) => {
            const result = await crx.tabs.executeScriptOnFrame(params.tabId, 0, 'document.documentElement.innerHTML')
            return result
        },
        executeScript: async (params) => {
            // {                     
            //     tabId: 123,
            //     code: ......
            // }  
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, 'executeScript', params)
            return result
        },
        getDetail: async (params) => {
            // {                     
            //     tabId: 1     
            //     windowId: 1
            // }      
            const tab = await crx.tabs.get(params.tabId)
            if (tab) {
                if (!params.windowId || params.windowId <= 0) {
                    params.windowId = tab.windowId;
                }
                const win = await crx.windows.get(params.windowId)
                if (chrome.runtime.lastError) {
                    throw new HandlerError(`获取tab失败, ${chrome.runtime.lastError.message}`)
                } else {
                    tab["windowdetail"] = win
                    return tab
                }
            }
            return null;
        },
        get: async (params) => {
            // {                     
            //     tabId: 1                     
            // }                     
            const tab = await crx.tabs.get(params.tabId)
            if (chrome.runtime.lastError) {
                throw new HandlerError(`获取tab失败, ${chrome.runtime.lastError.message}`)
            } else {
                return tab
            }
        },
        query: async (params) => {
            // https://developers.chrome.com/extensions/tabs#method-query                     
            const tabIds = []
            const tabs = await crx.tabs.query(params)
            for (const tab of tabs) {
                if (tab.url && tab.url.indexOf("chrome://") !== 0
                    && tab.url.indexOf("chrome-extension://") !== 0
                    && tab.url.indexOf("edge://") !== 0
                    && tab.url.indexOf("se://") !== 0
                    && tab.url.indexOf("about:") !== 0) {
                    tabIds.push(tab.id)
                }
            }
            return tabIds
        },
        active: async (params) => {
            // {                     
            //     tabId = 1                     
            // }
            const tab = await crx.tabs.get(params.tabId)
            if (tab) {
                chrome.windows.update(tab.windowId, { focused: true })
                chrome.tabs.update(tab.id, { active: true })
            }
        },
        reload: async (params) => {
            // {                     
            //     tabId: 1                     
            //     bypassCache: false                     
            // }                     
            await crx.tabs.reload(params.tabId, params.bypassCache)
        },
        navigate: async (params) => {
            // {                     
            //     tabId: 1                     
            //     url: '...'                     
            // }                     
            await crx.tabs.update(params.tabId, { url: params.url })
        },
        goForward: async (params) => {
            // {                    
            //     tabId: 1                     
            // }                     
            await crx.tabs.goForward(params.tabId)
        },
        goBack: async (params) => {
            // {                     
            //     tabId: 1                     
            // }                     
            await crx.tabs.goBack(params.tabId)
        },
        close: async (params) => {
            // {                     
            //     tabId = 123                     
            // }                     
            await crx.tabs.remove([params.tabId])
            if (chrome.runtime.lastError) {
                throw new HandlerError(`关闭tab失败, ${chrome.runtime.lastError.message}`)
            }
        },
        closeAll: async (params) => {
            const tabs = await crx.tabs.query(params)
            const tabIds = tabs.map(m => m.id)
            await crx.tabs.remove(tabIds)
            if (chrome.runtime.lastError) {
                throw new HandlerError(`关闭tab失败, ${chrome.runtime.lastError.message}`)
            }
        },
        querySelectorAll: async (params) => {
            // params.tabId
            // params.selector
            // params.relativeType    元素位置类型
            // params.positionNum     元素定位数字，与relativeType结合使用
            // params.resultAttrName  返回值属性名，对象层面的逻辑属性，不限于element的属性
            // params.version             
            if (!params.index) {
                params.index = -1 // -1返回所有，否则返回指定索引位置的元素Id    
            }
            var result = await crx.tabs.requestOnFrame(params.tabId, 0, 'querySelector', params)
            return result
        },
        querySelector: async (params) => {
            // params.tabId                     
            // params.selector                     
            // params.index                     
            var result = await crx.tabs.requestOnFrame(params.tabId, 0, 'querySelector', params)
            if (result && result.length > 0) {
                return result[0]
            } else {
                return null
            }
        },
        queryElementsAll: async (params) => {
            // params.tabId
            // params.tagNames   
            if (!params.frameId) {
                params.frameId = 0;
            }
            let nextParams = utils.clone(params)
            var result = await crx.tabs.requestOnFrame(nextParams.tabId, 0, 'queryElementsAll', nextParams)
            let elementDatas = JSON.parse(result);
            let allframeElementDatas = [];
            let frames = await crx.webNavigation.getAllFrames(nextParams.tabId)
            for (const frame of frames) {
                try {
                    if (frame.frameId == 0) continue;
                    nextParams = utils.clone(params)
                    result = await crx.tabs.requestOnFrame(nextParams.tabId, frame.frameId, 'queryElementsAll', nextParams)
                    Array.prototype.push.apply(allframeElementDatas, JSON.parse(result));
                }
                catch { }
            }
            return JSON.stringify(elementDatas.concat(allframeElementDatas));
        },
        queryCSSSelectorAll: async (params) => {
            // params.tabId                     
            // params.selector                     
            // params.parent                     
            params.index = -1 // -1返回所有，否则返回指定索引位置的元素Id                     
            let frameId = 0
            if (params.parent) {
                const uid = new UniqueId(params.parent)
                frameId = uid.targetFrame
                params.frameTree = uid.frameIds
            }
            var result = await crx.tabs.requestOnFrame(params.tabId, frameId, 'queryCSSSelector', params)
            return result
        },
        queryCSSSelector: async (params) => {
            // params.tabId                     
            // params.selector                     
            // params.index                     
            // params.parent                     
            let frameId = 0
            if (params.parent) {
                const uid = new UniqueId(params.parent)
                frameId = uid.targetFrame
                params.frameTree = uid.frameIds
            }
            var result = await crx.tabs.requestOnFrame(params.tabId, frameId, 'queryCSSSelector', params)
            if (result && result.length > 0) {
                return result[0]
            } else { return null }
        },
        elementFromPoint: async (params) => {
            // {                     
            //     tabId: 123,                    
            //     x: 1,                     
            //     y: 2                    
            // }                     
            const origial = utils.clone(params)
            const zoomFactor = await crx.tabs.getZoom(params.tabId)
            const scale = await crx.tabs.executeScriptOnFrame(params.tabId, 0, 'window.devicePixelRatio')
            utils.pointClientToPage(params, scale)
            params.containers = []
            params.zoomFactor = zoomFactor
            const result = await crx.tabs.requestOnFrame(params.tabId, 0, 'elementFromPoint', params)
            return result
        },
        executeCommand: async (params) => {
            // {                     
            //     tabId: 123,                     
            //     command: 'copy'                     
            // }                     
            return await crx.tabs.executeScriptOnFrame(params.tabId, 0, `document.execCommand("${params.command}")`)
        },
        getText: async (params) => {
            // {                     
            //     tabId: 123,                     
            // }                     
            const code = 'document.documentElement.outerText'
            return await crx.tabs.executeScriptOnFrame(params.tabId, 0, code)
        },
        getHtml: async (params) => {
            // {                     
            //     tabId: 123,                     
            // }                     
            const code = 'document.documentElement.outerHTML'
            return await crx.tabs.executeScriptOnFrame(params.tabId, 0, code)
        },
        executeScriptOnFrame: async (params) => {
            // {
            //     tabId: 123,
            //     code: console.log("123")
            // }
            const result = await crx.tabs.executeScriptOnFrame(params.tabId, 0, params.code)
            if (result === undefined || result === null) {
                return null
            } else {
                return JSON.stringify(result)
            }
        },
        queryCollectionData: async (params) => {
            // params.tabId                     
            // params.selector                     
            params.index = -1 // -1返回所有，否则返回指定索引位置的元素Id                     
            var result = await crx.tabs.requestOnFrame(params.tabId, 0, 'queryCollectionData', params)
            return result
        },
        queryCollectionDataV2: async (params) => {
            // params.tabId
            // params.collectionConfig
            // params.collectionConfig.scrollVerticalDelay
            // params.collectionConfig.scrollMaxTotalDelay
            let start_timestamp = new Date().getTime();
            let lastScroll_timestamp = start_timestamp;
            let now_timestamp = start_timestamp;
            let frameId = 0;
            let targetFrameId = -1;
            let scrollTop = 0, scrollHeight = 0;
            do {
                if (!params.colorGuidanceConfig) {
                    await utils.delay(100);
                }
                now_timestamp = new Date().getTime();
                if (targetFrameId === -1) {
                    var elementId = await crx.tabs.requestOnFrame(params.tabId, frameId, 'getAndScrollLastElement', params);
                    if (!elementId) {
                        continue;
                    }
                    targetFrameId = new UniqueId(elementId).targetFrame;
                }
                if (params.colorGuidanceConfig) {
                    break;
                }
                let domAreaInfo = await crx.tabs.requestOnFrame(params.tabId, targetFrameId, 'getDomAreaInfo');
                if (scrollHeight != domAreaInfo.scrollHeight && (scrollTop == 0 || scrollTop == domAreaInfo.scrollTop)) {
                    scrollHeight = domAreaInfo.scrollHeight;
                    scrollTop = domAreaInfo.scrollTop;
                    //滚动一次
                    await crx.tabs.requestOnFrame(params.tabId, targetFrameId, 'scrollTo', {
                        behavior: 'smooth', position: "bottom"
                    });
                    lastScroll_timestamp = new Date().getTime();
                }
            } while ((now_timestamp - lastScroll_timestamp) < params.collectionConfig.scrollVerticalDelay)
            if (targetFrameId === -1) {
                console.log("未采集到数据");
                return [];
            }
            var result = await crx.tabs.requestOnFrame(params.tabId, targetFrameId, 'queryCollectionDataV2', params)
            if (!result) {
                console.log("采集数据为空");
                result = [];
            }
            return result;
        },
        getSimilarElement: async (params) => {
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getSimilarElement', params)
        },
        captureVisibleTab: async (params) => {
            // params.tabId
            // params.format
            // params.quality
            const { windowId } = await crx.tabs.get(params.tabId)
            return await crx.tabs.captureVisibleTab(windowId, {
                format: params.format,
                quality: params.quality
            })
        },
        captureTab: async (params) => {
            // params.tabId
            // params.rect
            // params.format
            // params.quality
            const { tabId, rect, format, quality } = params
            const options = {
                format,
                quality
            }
            return await crx.tabs.captureTab(tabId, rect ? Object.assign({ rect }, options) : options)
        }
    },
    downloads:
    {
        addCreatedEvent: async () => {
            await crx.downloads.addCreatedEvent();
        },
        removeCreatedEvent: async () => {
            await crx.downloads.removeCreatedEvent();
        },
        search: async (params) => {
            return await crx.downloads.search(params)
        },
        searchV1: async (params) => {
            return await crx.downloads.searchV1(params)
        },
        downloadUrl: async (params) => {
            // params.url                     
            return await crx.downloads.download({ url: params.url })
        },
    },
    elements:
    {
        getNextElementBuildSelector: async (params) => {
            // params.innerTextFirst
            //const element = new UniqueId(params.elementId);
            var selector = await crx.tabs.requestOnFrame(params.tabId, 0, 'getNextElementBuildSelector', params)
            const tab = await crx.tabs.get(params.tabId)
            selector.unshift({
                name: "tab",
                props: [{
                    name: "title",
                    value: tab.title,
                    pattern: "equal",
                    accurate: "true"
                },
                {
                    name: "url",
                    value: tab.url,
                    pattern: "equal",
                    accurate: "false"
                }]
            })
            return selector;
        },
        getUsableAttributes: async (params) => {
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getUsableAttributes', params)
        },
        getTurningPageElement: async (params) => {
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getTurningPageElement', params)
        },
        getIsTableName: async (params) => {
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getIsTableName', params)
        },
        getTableData: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getTableData', params)
        },
        getAttributes: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getAttributes', params)
        },
        getControlType: async (params) => {
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getControlType', params)
        },
        getProperty: async (params) => {
            //params.tabId
            //params.elementId
            //params.name
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getProperty', params)
        },
        executeScript: async (params) => {
            //params.tabId
            //params.code: 'alert('a')',
            //params.elementId
            // https://developer.chrome.com/extensions/tabs#method-executeScript
            let frameId = 0
            if (params.elementId) {
                //elementId get frameId
                frameId = (new UniqueId(params.elementId)).targetFrame;
            }
            const result = await crx.tabs.executeScriptOnFrame(params.tabId, frameId, params.code)
            if (result === undefined || result === null) {
                return null
            }
            else {
                return JSON.stringify(result)
            }
        },
        scrollIntoViewIfNeeded_V2: async (params) => {
            // params.tabId                     
            // params.elementId               
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'scrollIntoViewIfNeeded_V2', {
                elementId: element.raw,
            })
        },
        scrollIntoViewIfNeeded: async (params) => {
            // params.tabId                     
            // params.elementId               
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'scrollIntoViewIfNeeded', {
                elementId: element.raw,
                location: params.location
            })
        },
        getBounding: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            const scale = await crx.tabs.executeScriptOnFrame(params.tabId, 0, 'window.devicePixelRatio')
            const bounding = await crx.tabs.requestOnFrameReverse(params.tabId, element.targetFrame, 'getElementBounding',
                {
                    elementId: element.raw
                })
            utils.rectPageToClient(bounding, scale)
            return bounding
        },
        buildCssSelector: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            //添加元素全路径
            const cs = await crx.tabs.requestOnFrameReverse(params.tabId, element.targetFrame, 'cssSelectorAll',
                {
                    elementId: element.raw
                })
            return cs
        },
        buildSelector: async (params) => {
            // params.tabId                     
            // params.elementId                        
            // params.innerTextFirst
            const element = new UniqueId(params.elementId);
            const selector = await crx.tabs.requestOnFrameReverse(params.tabId, element.targetFrame, 'buildSelector',
                {
                    elementId: element.raw,
                    innerTextFirst: params.innerTextFirst,
                    excludeInnerText: params.excludeInnerText,
                    rawData: params.rawData
                })
            //添加xpath
            //const xPath = await crx.tabs.requestOnFrameReverse(params.tabId, element.targetFrame, 'xPath',
            //    {
            //        elementId: element.raw
            //    })
            ////添加元素全路径
            //const cs = await crx.tabs.requestOnFrameReverse(params.tabId, element.targetFrame, 'cssSelectorAll',
            //    {
            //        elementId: element.raw
            //    })
            // 添加窗口级Node                    
            const tab = await crx.tabs.get(params.tabId)
            //let path = ""
            //let outertext = ""
            //try {
            //    path = cs.path
            //} catch (e) {
            //    path = cs
            //}
            //try {
            //    outertext = cs.outertext
            //} catch (e) {
            //    console.log(e)
            //}

            selector.unshift({
                name: "tab",
                accurate: "true",
                //xpath: xPath,
                //cssSelector: path,
                //outertext: outertext,
                //fullSelector: "test",
                props: [{
                    name: "title",
                    value: tab.title,
                    pattern: "equal",
                    accurate: "true"
                },
                {
                    name: "url",
                    value: tab.url,
                    pattern: "equal",
                    accurate: "false"
                }]
            })
            return selector
        },
        getTableContent: async (params) => {
            // params.tabId                     
            // params.token                     
            // params.tokenType                     
            // params.returnType                     
            // params.parent                    
            let frameId = 0
            if (params.parent) {
                frameId = (new UniqueId(params.parent)).targetFrame
            }
            var result = await crx.tabs.requestOnFrame(params.tabId, frameId, 'getTableContent', params)
            if (result && result.length > 0) {
                return result
            }
            else {
                return null
            }
        },
        click: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'click', params)
        },
        dblclick: async (params) => {
            // params.tabId                     
            // params.elementId
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'dblclick', params)
        },
        rclick: async (params) => {
            // params.tabId                     
            // params.elementId
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'rclick', params)
        },
        isEditable: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'isEditable', params)
        },
        input: async (params) => {
            // params.tabId                     
            // params.elementId                     
            // params.value                     
            // params.replace                     
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'input', params)
        },
        focus: async (params) => {
            // params.tabId                     
            // params.elementId
            const element = new UniqueId(params.elementId);
            await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'focus', params)
        },
        getText: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getText', params)
        },
        getHtml: async (params) => {
            // params.tabId                     
            // params.elementId                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getHtml', params)
        },
        getValue: async (params) => {
            // params.tabId                     
            // params.elementId                    
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getValue', params)
        },
        getSelectOption: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.selected: true/false 
            // params.optionType: text/value 
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getSelectOption', params)
        },
        getSelectOptions: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.selected: true/false 
            // params.optionType: text/value 
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getSelectOptions', params)
        },
        setSelectOptions: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.options: [0,1]                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'setSelectOptions', params)
        },
        getCheckStatus: async (params) => {
            // params.tabId                     
            // params.elementId                    
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getCheckStatus', params)
        },
        setCheckStatus: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.isChecked: true/false                    
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'setCheckStatus', params)
        },
        setAttribute: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.name: '...'                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'setAttribute', params)
        },
        getAttribute: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.name: '...'                     
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getAttribute', params)
        },
        scrollTopOrLeft: async (params) => {
            // params.tabId                     
            // params.elementId
            // params.scrollTop: 100                    
            // params.scrollLeft: 200                  
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'scrollTopOrLeft', params)
        },
        isDisplayed: async (params) => {
            // params.tabId                   
            // params.elementId                    
            const element = new UniqueId(params.elementId);
            return await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'isDisplayed', params)
        },
        getChildren: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.name: '...'                     
            const element = new UniqueId(params.elementId);
            var arrayElementId = await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getChildren', params)
            return arrayElementId;
        },
        getParent: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.name: '...'                     
            const element = new UniqueId(params.elementId);
            var arrayElementId = await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getParent', params)
            return arrayElementId;
        },
        getSiblings: async (params) => {
            // params.tabId                     
            // params.elementId                    
            // params.name: '...'                     
            const element = new UniqueId(params.elementId);
            var arrayElementId = await crx.tabs.requestOnFrame(params.tabId, element.targetFrame, 'getSiblings', params)
            return arrayElementId;
        }
    },
    debugger: {
        sendCommand: async (params) => {
            // params.tabId                     
            // params.method
            // params.commandParam
            const { tabId, method, commandParam } = params
            try {
                return await debuggerManager.sendCommand(tabId, method, commandParam)
            } catch (error) {
                const parseError = tryParseJSON(error.message)
                if ((parseError || {}).code === -32000) {
                    throw new HandlerError(error.message, ERRORS.NO_RESOURCE)
                }
                throw new HandlerError(error.message, error.code)
            }
        },
        detach: async (params) => {
            // params.tabId
            const { tabId } = params
            try {
                return await debuggerManager.detach(tabId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        check: async (params) => {
            // params.tabId
            const { tabId } = params
            try {
                return await debuggerManager.check(tabId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        addCache: async (params) => {
            // params.tabId
            // params.method
            // params.filter
            const { tabId, method, filter } = params
            try {
                return await debuggerManager.addCache(tabId, method, filter)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        getCache: async (params) => {
            // params.tabId
            // params.method
            const { tabId, method } = params
            try {
                return await debuggerManager.getCache(tabId, method)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        getCacheBody: async (params) => {
            // params.tabId
            // params.requestId
            const { tabId, requestId } = params
            try {
                return await debuggerManager.getCacheBody(tabId, requestId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
    },
    webRequest: {
        addCache: async (params) => {
            // params.tabId
            const { tabId } = params
            try {
                return await webRequestManager.addCache(tabId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        getCache: async (params) => {
            // params.tabId
            const { tabId } = params
            try {
                return await webRequestManager.getCache(tabId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        startListen: async () => {
            try {
                return await webRequestManager.startListen()
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        endListen: async () => {
            try {
                return await webRequestManager.endListen()
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        getBody: async (params) => {
            // params.tabId
            // params.requestId
            // params.contentType
            // params.type
            const { tabId, requestId, type, contentType } = params
            try {
                return await webRequestManager.getBody(tabId, requestId, type, contentType)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        },
        checkCache: async (params) => {
            // params.tabId
            const { tabId } = params
            try {
                return webRequestManager.checkCacheAvailable(tabId)
            } catch (error) {
                throw new HandlerError(error.message, error.code)
            }
        }
    },
    initContentScript: async (params) => {
        //code : "console.log('abc')"
        contentScript = params.code
    },
    initContentCrawlScript: async (params) => {
        //code : "console.log('abc')"
        contentScript += params.code
        window.contentCrawlScript = params.code
    },
}

const handle = function (message, callback) {
    try
    {
        const tokens = message.method.split('.')
        let handler = handlers
        for (const token of tokens) {
            handler = handler[token]
        }
        handler(message.params).then((result) => {
            callback(response.ok(result))
        }).catch((error) => {
            if (error instanceof HandlerError) {
                callback(response.fail(error.code, error.message))
            }
            else if (error instanceof ContentUnknownError) {
                callback(response.fail(ERRORS.UNKNOWN, error.message))
            }
            else {
                callback(response.fail(ERRORS.UNKNOWN, error.stack))
            }
        })
    }
    catch (error) {
        callback(response.fail(ERRORS.UNKNOWN, error.stack))
    }
}

const nativeHost = function () 
{
    const hostName = "ii.rpa.chromenativemsg";
    const version = "2.7"
    var traceEnabled = false;
    var rpa_conn_native = chrome.runtime.connectNative(hostName);
    
    //disconnected
    rpa_conn_native.onDisconnect.addListener(onDisconnected);

    //onmessage
    rpa_conn_native.onMessage.addListener((message) => 
    {
        try 
        {
            var data = {};
            if (message.Action && message.Action == "SendTransferMessage") {
                data = message;
                data.connType = "nativemsg_tMsg";
            }
            else {
                data.SendData = message;
                data.connType = "nativemsg";
            }
            received(data);
        }
        catch (e) {
            traceLog("nativemsghost message exception: " + e)
            responseFail(-1000101, "nativemsghost message exception!", data)
        }
    })

    //log
    function traceLog(message) {
        if (traceEnabled) {
            try {
                console.log(new Date().toLocaleString() + ":" + message);
            } catch (e) { }
        }
    }

    //received
    function received(data) {
        var message = data.SendData;
        if (message) {
            traceLog(message.method);
            console.log(message.method);
            chrome.windows.getCurrent(function (currentWindow) 
            {
                chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
                {
                    //debuggerManager.sendCommand(activeTabs[0].id,'Runtime.evaluate','alert("hello")')
                });
            });
            switch (message.method) {
                case "getVersion": 
                    responseOK(version, data); 
                    break;
                case "ping": 
                    responseOK('pong', data); 
                    break;
                case "init":
                    try {
                        //注入
                        traceLog("init code");
                        //eval.call(this, message.params.code)
                        // self.clients.matchAll().then(function(clients) {
                        //     clients.forEach(function(client) {
                        //       console.log("client:");
                        //       console.log(client);
                        //       console.log("message.params.code");
                        //       client.postMessage(message.params.code);
                        //       console.log(message.params.code);
                        //     });
                        //   });

                          chrome.windows.getCurrent(function (currentWindow) 
                          {
                            chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
                            {
                                //if (activeTabs[0].url?.startsWith("chrome://")) return undefined;
                                chrome.tabs.sendMessage(activeTabs[0].id, { type: 'init', code: message.params.code });
                                // fetch(chrome.runtime.getURL('iirpa/utils.js'))
                                //     .then(response => response.text())
                                //     .then(scriptContent => {
                                //         // 使用 chrome.scripting.executeScript 执行外部 JavaScript 文件的内容
                                //         chrome.scripting.executeScript({
                                //         target: { tabId: activeTabs[0].id },
                                //         function: (scriptContent) => {
                                //             // 在标签页中执行外部 JavaScript 文件的内容
                                //             eval(scriptContent);
                                //         },
                                //         args: [scriptContent] // 将外部文件内容作为参数传递给执行函数
                                //         });
                                //     })
                                //     .catch(error => {
                                //         console.error('Error fetching or executing script:', error);
                                //     });

                                // chrome.scripting.executeScript(
                                // {
                                //     target: {tabId: activeTabs[0].id, allFrames: true}, 
                                //     files: ['./external.js'],
                                //     // func: () => {
                                //     //     document.body.style.border = "5px solid green";
                                //     //   },
                                // });
                            });
                          });
                        console.log("init code");
                        responseOK("init fulfill", data)
                    } catch (error) {
                        responseFail(-1, error.stack, data)
                    }
                    break;
                case "getCodeId": 
                    chrome.windows.getCurrent(function (currentWindow) 
                    {
                        chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
                        {
                            var result = chrome.tabs.sendMessage(activeTabs[0].id, { type: 'getCodeId', code: data } );
                            result.then(function (value)
                            {
                                console.log(value);
                                responseOK(value.message.result,data);
                            })
                        });
                    });
                    break;
                case "restore":
                    responseOK("restore", data);
                    break;
                case "initContentScript":
                    handle(message, (response) => 
                    {
                        data.CallbackReslut = response;                           
                        responseMessage(data);    
                    })
                    break;
                case "initContentCrawlScript":
                    handle(message, (response) => 
                    {
                        data.CallbackReslut = response;                           
                        responseMessage(data);    
                    })
                    break;
                default:
                    handle(message, (response) => 
                    {
                        data.CallbackReslut = response;
                        
                        responseMessage(data);

                        if (message.method == "tabs.closeAll") 
                        {
                            //todo websocket
                            // tryingToReconnect = false;
                            // rpa_conn_service.disconnectTimeout = 500;
                            // rpa_conn_service.stop();
                        }
                    })
                    // chrome.windows.getCurrent(function (currentWindow) 
                    // {
                    //     chrome.tabs.query({active: true, windowId: currentWindow.id}, function(activeTabs) 
                    //     {
                    //         chrome.tabs.sendMessage(activeTabs[0].id, { type: message, code: data }, function (response) 
                    //         {
                    //             data.CallbackReslut = response;
                    //             console.log(data);
                    //             responseMessage(data);
                    //         });
                    //     });
                    // });
                    // if (typeof iirpa == "undefined" || !iirpa.codeId) {
                    //     console.log("iirpa == undefined");
                    //     responseFail(-100099, "script not injected!", data)
                    // }
                    // else if (message.codeId && message.codeId != iirpa.codeId) {
                    //     responseFail(-1000100, "script required injected!", data)
                    // }
                    // else {
                    //     delete message.codeId;
                    //     iirpa.handle(message, (response) => 
                    //     {
                    //         data.CallbackReslut = response;
                            
                    //         responseMessage(data);

                    //         if (message.method == "tabs.closeAll") 
                    //         {
                    //             //todo websocket
                    //             // tryingToReconnect = false;
                    //             // rpa_conn_service.disconnectTimeout = 500;
                    //             // rpa_conn_service.stop();
                    //         }
                    //     })
                    // }
                    break;
            }
        }
        else {
            // console.error('nknown message');
            // console.error(data);
        }

    }
    
    function evalWithDebugger(codeString)
    {

    }

    function onDisconnected()
    {
        appendMessage('Failed to connect: ' + chrome.runtime.lastError.message);
        rpa_conn_native = null;
        rpa_conn_native = chrome.runtime.connectNative(hostName)
    }
  
  

    function responseOK(content, data) {
        data.CallbackReslut = {
            code: 200,
            status: 'OK',
            result: {
                content: content,
                error: null
            }
        };
        responseMessage(data);
    }

    function responseFail(code, message, data) {
        data.CallbackReslut = {
            code: 200,
            status: 'OK',
            result: {
                content: null,
                error: {
                    code: code,
                    message: message
                }
            }
        }
        responseMessage(data);
    }
    function responseMessage(data) {
        if (data.connType == "nativemsg_tMsg") {
            data.SendData = "";
            data.MessageType = 2;//cakkback
            rpa_conn_native.postMessage(data);
        }
        else {
            rpa_conn_native.postMessage(data.CallbackReslut);
        }
    }
}();

