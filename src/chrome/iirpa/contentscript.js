﻿(function () {
    const ERRORS = {
        UNKNOWN: -1, // 未知异常    
        COMMON: 100, // 通用异常    
    }
    const UNIQUE_KEY = "i-rpa"
    const ElementRelativeType = {
        Self: 0,
        Similar: 1,
        Parent: 2,
        Children: 3,
        Siblings: 4
    }
    class HandlerError extends Error {
        constructor(message, code) {
            super(message || "")
            this.code = code || ERRORS.COMMON
        }
    }
    class Rectangle {
        constructor(left, top, width, height) {
            this.left = left
            this.top = top
            this.width = width
            this.height = height
        }
        contains(x, y) {
            return ((x >= this.left && x <= this.left + this.width) && (y >= this.top && y <= this.top + this.height))
        }
        round() {
            return new Rectangle(Math.round(this.left), Math.round(this.top), Math.round(this.width), Math.round(this.height))
        }
        center() {
            return { x: Math.round(this.left + this.width / 2), y: Math.round(this.top + this.height / 2) }
        }
    }
    class Continue {
        constructor(frameIndex, params) {
            this.frameIndex = frameIndex
            this.params = params
        }
    }
    const utils = {
        guid: () => {
            return 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        },
        fromScreenPoint: (x, y) => {
            const scale = window.devicePixelRatio
            return { x: Math.round(x / scale), y: Math.round(y / scale) };
        },
        isFrameElement: (ele) => {
            var tagName = ele && ele.tagName && ele.tagName.toLowerCase();
            return tagName === "frame" || tagName === "iframe"
        },
        getFrameIndex: (wnd) => {
            if (wnd.parent === wnd || wnd.parent === null) {
                return -1
            } else {
                var wnds = wnd.parent.frames;
                for (var i = 0; i < wnds.length; i++) {
                    if (wnds[i] === wnd) {
                        return i;
                    }
                }
                return -1;
            }
        },
        getChildFrameByIndex: (index) => {
            for (const frameElement of document.getElementsByTagName('iframe')) {
                if (frameElement.contentWindow === window.frames[index]) {
                    return frameElement
                }
            }
            for (const frameElement of document.getElementsByTagName('frame')) {
                if (frameElement.contentWindow === window.frames[index]) {
                    return frameElement
                }
            }
            return null
        },
        getBoundingClientRect: (ele) => {
            var rect = ele.getBoundingClientRect();
            var paddingTop = 0;
            var paddingLeft = 0;
            if (ele.tagName.toLowerCase() == 'iframe' || ele.tagName.toLowerCase() == 'frame') {
                paddingTop = parseInt(getComputedStyle(ele)["padding-top"]);
                paddingLeft = parseInt(getComputedStyle(ele)["padding-left"])
            }
            return new Rectangle(rect.left + ele.clientLeft + paddingLeft, rect.top + ele.clientTop + paddingTop, rect.width, rect.height)
        },
        getElementResultValue: function (elements, resultAttrName, params) {
            let resultVals = [];
            var frameTree = params.frameTree;
            for (const element of elements) {
                switch (resultAttrName) {
                    case "innerText":
                        resultVals.push(element.innerText);
                        break;
                    case "uniqueId":
                        resultVals.push(utils.uniqueId(frameTree, element));
                        break;
                    case "bounding":
                        var scale = window.devicePixelRatio;
                        var eleRect = utils.getBoundingClientRect(element);
                        if (params.frameRect) {
                            eleRect.left += params.frameRect.left;
                            eleRect.top += params.frameRect.top;
                        }
                        utils.rectPageToClient(eleRect, scale);
                        resultVals.push(IIRPA.JSON().stringify(eleRect));
                        break;
                    default:
                        resultVals.push(element.getAttribute(resultAttrName));
                        break;
                }
            }
            return resultVals;
        },
        rectPageToClient: (rectLike, scale) => {
            rectLike.left *= scale
            rectLike.top *= scale
            rectLike.width *= scale
            rectLike.height *= scale
        },
        getRelativeElements: (params) => {
            //params.filter  [{"targetType":"child|after|before|parent",index:0},]
            //params.tragetElement  目标元素 是一个对象，
            let elements = [];
            let _filter = utils.clone(params.filter);
            if (!_filter || _filter.length == 0) {
                return elements;
            }
            var tragetEle = params.tragetElement;
            var node = _filter.shift();
            if (node.targetType == "parent") {
                let pNode = tragetEle.parentNode;
                elements.push(pNode);
            }
            else if (node.targetType == "after") {
                let nextNode = tragetEle;
                while ((nextNode = nextNode.nextSibling) && nextNode.nodeType !== 1) { }
                if (nextNode) {
                    elements.push(nextNode);
                }
            }
            else if (node.targetType == "before") {
                let previousNode = tragetEle;
                while ((previousNode = previousNode.previousSibling) && previousNode.nodeType !== 1) { }
                if (previousNode) {
                    elements.push(previousNode);
                }
            }
            else if (node.targetType == "child") {
                let children = tragetEle.children;
                for (let child of children) {
                    //if (!child || child.nodeType != 1) {
                    //    continue;
                    //}
                    var elementRect = child.getBoundingClientRect();
                    if ((elementRect.right - elementRect.left) <= 0 && (elementRect.bottom - elementRect.top <= 0)) {
                        continue;
                    }
                    elements.push(child);
                    //index有效 且已获取到指定元素
                    if (node.index && node.index == elements.length) {
                        break;
                    }
                }
                //index有效 且已获取到指定元素
                if (node.index && node.index == elements.length) {
                    elements = [elements.pop()];
                }
                //index有效 但未获取到指定元素
                else if (node.index && node.index > elements.length) {
                    elements = [];
                }
                //if (node.index && node.index > elements.length) {
                //    elements = [];
                //}
            }
            //是最后一级return，否则遍历所有匹配到的元素，进行下一级匹配
            if (_filter.length == 0) {
                return elements;
            }
            let matchedElements = [];
            for (var i = 0; i < elements.length; i++) {
                let nextLevelNodes = utils.getRelativeElements({
                    filter: _filter,
                    tragetElement: elements[i]
                });
                if (nextLevelNodes.length > 0) {
                    Array.prototype.push.apply(matchedElements, nextLevelNodes);
                }
            }
            return matchedElements;
        },
        clone: (obj) => {
            let copy
            if (null == obj || "object" != typeof obj)
                return obj
            if (obj instanceof Date) {
                copy = new Date()
                copy.setTime(obj.getTime())
                return copy
            }
            if (obj instanceof Array) {
                copy = []
                for (var i = 0, len = obj.length; i < len; i++) {
                    copy[i] = utils.clone(obj[i])
                }
                return copy
            }
            if (obj instanceof Object) {
                copy = {}
                for (var attr in obj) {
                    if (obj.hasOwnProperty(attr))
                        copy[attr] = utils.clone(obj[attr])
                }
                return copy
            }
            throw new Error("Unable to copy obj! Its type isn't supported.")
        },
        compareArray: (arr1, arr2) => {
            if (!arr2 || !arr1)
                return false;
            if (arr1.length != arr2.length)
                return false;
            for (var i = 0, l = arr1.length; i < l; i++) {
                if (arr1[i] instanceof Array && arr2[i] instanceof Array) {
                    if (!utils.compareArray(arr1[i], arr2[i]))
                        return false;
                } else if (arr1[i] != arr2[i]) {
                    return false;
                }
            }
            return true;
        },
        uniqueId: (frameTree, ele) => {
            // uniqueId: {frameId}/{frameId}/{nodeId}/{tagName}, '1/2/66/TD' 
            return IIRPA.dom.uniqueId(frameTree, ele);
        },
        elementFromUniqueId: (uniqueId) => {
            // uniqueId: {frameId}/{frameId}/{nodeId}/{tagName}, '1/2/66/TD'       
            const tokens = uniqueId.split('/')
            if (tokens.length < 3)
                throw new HandlerError(`无效的UniqueId, ${uniqueId}`)
            const tagName = tokens.pop()
            const uniqueValue = tokens.join('/')
            return document.querySelector(`${tagName}[${UNIQUE_KEY}='${uniqueValue}']`)
        },
        raiseMouseEvents: (types, element, x, y, button) => {
            for (const type of types) {
                var evt = document.createEvent("MouseEvents");
                evt.initMouseEvent(type, true, true, window, 1, 0, 0, x, y, false, false, false, false, button, null);
                element.dispatchEvent(evt);
            }
        },
        raiseInputEvents: (element, value) => {
            element.value = value;
            var event = document.createEvent('HTMLEvents');
            event.initEvent('input', true, true);
            event.eventType = 'message';
            event.simulated = true;
            var tracker = element._valueTracker;
            if (tracker) {
                tracker.setValue(value);
            }
            element.dispatchEvent(event);
        }
    }
    window.iirpa = new function () {
        const response = {
            // 返回值存在以下四种情况    
            // 0: 成功 { code: 0, result: null }   
            // -1: 继续下次Frame请求(跨域) { code: -1, next: { frameIndex: 111, params: params } }    
            //     next.params可能存储了上一次请求的结构          
            // -2: content需要初始化 { code: -2, message: 'need init' }        
            // 1: 请求失败 { code: 1, error: {code: -1, message: ''} }           
            ok: (result) => {
                return {
                    code: 0,
                    result: result
                }
            },
            fail: (code, message) => {
                return {
                    code: 1,
                    error: {
                        code: code,
                        message: message
                    }
                }
            },
            continue: (nextFrameIndex, nextParams) => {
                return {
                    code: -1,
                    next: {
                        frameIndex: nextFrameIndex,
                        params: nextParams
                    }
                }
            }
        }
        // handlers中的方法必须全部为同步方法
        const handlers = {
            getNextElementBuildSelector: (params) => {
                var ele = IIRPA.api.getNextElement(params.cssSelector);
                const elePath = JSON.parse(IIRPA.api.inspectPath(ele, {
                    innerTextFirst: params.innerTextFirst
                }).value)
                return elePath;
            },
            getTurningPageElement: (params) => {
                var ele = IIRPA.CollectionUtils.getTurningPageElement(params.cssSelectors);
                if (ele) {
                    if (!params.frameTree) {
                        params.frameTree = []
                    }
                    params.frameTree.push(params.frameId)
                    return IIRPA.dom.uniqueId(params.frameTree, ele);
                }
                return "";
            },
            getIsTableName: (params) => {
                // params.elementId     
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                let tabEle;
                let result = IIRPA.api.getIsTableName(ele).value;
                if (result && result.isTable === true) {
                    if (!params.frameTree) {
                        params.frameTree = []
                    }
                    params.frameTree.push(params.frameId)
                    return utils.uniqueId(params.frameTree, result.element)
                }
                return null
            },
            getTableData: (params) => {
                // params.elementId             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                let result = IIRPA.api.tableData(ele, "text").value
                return result
            },
            getAttributes: (params) => {
                // params.elementId             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                let result = IIRPA.api.getAttributes(ele).value
                return result
            },
            getControlType: (params) => {
                let node = utils.elementFromUniqueId(params.elementId)
                if (!node) {
                    throw new HandlerError('未找到元素')
                }
                if (node && node.nodeType == 1) {
                    return node.tagName;
                }
                return null;
            },
            isExecutorJavaScript: (params) => {
                //params.executor id
                const ele = document.querySelectorAll(params.executor)
                if (ele.length === 1)
                    return true
                else
                    return false
            },
            scroll: (params) => {
                // params.elementId          
                // params.direction
                // params.value
                IIRPA.api.scroll(params.elementId, params.direction, params.value);
            },
            getDomAreaInfo: (params) => {
                const { elementId } = params
                let node
                if (elementId) {
                    node = utils.elementFromUniqueId(elementId);
                    if (!node) {
                        return { 'status': IIRPA.ErrorCode.NO_SUCH_ELEMENT, 'value': '未找到控件' };
                    }
                }
                return JSON.parse(IIRPA.dom.areaInfo(node));
            },
            scrollTo: (params) => {
                // params.elementId
                // params.top
                // params.left
                // params.behavior
                const { elementId } = params
                let node
                if (elementId) {
                    node = utils.elementFromUniqueId(elementId);
                    if (!node) {
                        return { 'status': IIRPA.ErrorCode.NO_SUCH_ELEMENT, 'value': '未找到控件' };
                    }
                }
                IIRPA.api.scrollTo(node, params);
            },
            getScroll: (params) => {
                // params.elementId          
                // params.direction
                const { elementId } = params
                let node
                if (elementId) {
                    node = utils.elementFromUniqueId(elementId);
                    if (!node) {
                        return { 'status': IIRPA.ErrorCode.NO_SUCH_ELEMENT, 'value': '未找到控件' };
                    }
                }
                return IIRPA.api.getScroll(node, params.direction).value
            },
            getProperty: (params) => {
                // params.elementId               
                let node = utils.elementFromUniqueId(params.elementId)
                if (!node) {
                    throw new HandlerError('未找到元素')
                }
                if (node && node.nodeType == 1) {
                    return IIRPA.dom.nodeHasAttribute(node, params.name)
                }
                return false;
            },
            executeScript: (params) => {
                //到 content 执行js
                var script = document.createElement('script');
                var code = document.createTextNode(params.code)
                script.appendChild(code)
                return document.querySelector('body').appendChild(script)
            },
            getFrameIndex: () => {
                return utils.getFrameIndex(window)
            },
            elementFromPoint: (params) => {
                // {               
                //     tabId: 308,
                //     zoomFactor: 1,        
                //     x: 1, // 页面坐标   
                //     y: 2, // 页面坐标           
                //     containers: [ 
                // 上层iframe左上角坐标(页面坐标)         
                //         { left: 111, top: 222 },   
                //         { left: 111, top: 222 }     
                //     ]               
                //     params.frameTree (nextParams)   
                // }               
                // 计算相对于当前Iframe的坐标          
                let ptX = params.x
                let ptY = params.y
                for (const container of params.containers) {
                    ptX -= container.left
                    ptY -= container.top
                }
                var ele = IIRPA.dom.elementFromPoint(ptX, ptY)
                if (ele) {
                    const rect = utils.getBoundingClientRect(ele)
                    if (utils.isFrameElement(ele) && rect.contains(ptX, ptY)) {
                        let nextFrameIndex = utils.getFrameIndex(ele.contentWindow)
                        const nextParams = utils.clone(params)
                        nextParams.containers.push({ left: rect.left, top: rect.top })
                        if (!nextParams.frameTree) {
                            nextParams.frameTree = []
                        }
                        nextParams.frameTree.push(params.frameId)
                        return new Continue(nextFrameIndex, nextParams)
                    } else {
                        if (!params.frameTree) {
                            params.frameTree = []
                        }
                        params.frameTree.push(params.frameId)
                        return utils.uniqueId(params.frameTree, ele)
                    }
                } else { return null }
            },
            querySelector: (params) => {
                // params.tabId,                
                // params.selector           
                // params.parent           
                // params.index, -1返回所有，否则返回指定索引位置的元素Id     
                // params.frameId             
                // params.frameTree (nextParams)   
                //params.relativeType    元素位置类型
                //params.positionNum     元素定位数字，与relativeType结合使用
                //params.resultAttrName  返回值属性名，对象层面的逻辑属性，不限于element的属性
                // params.version             
                const nextParams = utils.clone(params)
                let isFrame = false;
                const selector = []
                const isAllNode = params.version > 1;
                while (nextParams.selector.length > 0) {
                    const node = nextParams.selector.shift()
                    if (node.name == 'tab') { continue }
                    if (isAllNode) {
                        if ((node.name === 'iframe' || node.name === 'frame') && node.accurate != "true") { continue }
                        selector.push(node)
                    } else {
                        if (node.accurate != "true") { continue }
                        selector.push(node)
                    }
                    if (node.name === 'iframe' || node.name === 'frame') {
                        isFrame = true;
                        break;
                    }
                }
                let parent = null
                if (params.parent) {
                    parent = utils.elementFromUniqueId(params.parent)
                }
                var elements = null;
                if (!isFrame) {
                    if (params.mode) {
                        switch (params.mode) {
                            case "xpath":
                                var nodes = [];
                                try {
                                    var context = document;
                                    var doc = (context && context.ownerDocument) || window.document;

                                    var results = doc.evaluate(params.cssSelector, context || doc, null, XPathResult.ANY_TYPE, null);
                                    var node;
                                    while (node = results.iterateNext()) {
                                        nodes.push(node);
                                    }
                                } catch (e) {
                                }
                                elements = nodes;
                                break;
                            case "cssSelector":
                                elements = IIRPA.dom.querySelectorAll(params.cssSelector);
                                break;
                        }
                    } else if (params.cssSelector) {
                        elements = document.querySelectorAll(params.cssSelector)
                    }
                }
                if (elements == null)
                    elements = IIRPA.locale.findElements(selector, parent, params.version);
                let elementIds = []
                if (elements && elements.length > 0) {
                    if (!nextParams.frameTree) {
                        nextParams.frameTree = []
                    }
                    nextParams.frameTree.push(params.frameId)
                    // 这里目前只考虑了一个iframe的情况     
                    const firstElement = elements[0]
                    if (utils.isFrameElement(firstElement) && nextParams.selector.length > 0 && nextParams.selector.filter(s => s.accurate === "true").length > 0) {
                        let nextFrameIndex = utils.getFrameIndex(firstElement.contentWindow)
                        //if (!nextParams.frameTree) {
                        //    nextParams.frameTree = []
                        //}
                        //nextParams.frameTree.push(params.frameId);
                        if (params.resultAttrName == "bounding") {
                            var rect = utils.getBoundingClientRect(firstElement);
                            if (nextParams.frameRect) {
                                rect.left += nextParams.frameRect.left;
                                rect.top += nextParams.frameRect.top;
                            }
                            nextParams.frameRect = rect;
                        }
                        return new Continue(nextFrameIndex, nextParams)
                    }
                }
                if (elements.length == 0) {
                    return elementIds;
                }
                switch (params.relativeType) {
                    case ElementRelativeType.Parent:
                        params.elementId = utils.uniqueId(nextParams.frameTree, elements[0]);
                        params.level = params.positionNum;
                        elementIds = handlers.getParent(params);
                        break;
                    case ElementRelativeType.Siblings:
                        params.elementId = utils.uniqueId(nextParams.frameTree, elements[0]);
                        params.position = params.positionNum;
                        elementIds = handlers.getSiblings(params);
                        break;
                    case ElementRelativeType.Children:
                        params.elementId = utils.uniqueId(nextParams.frameTree, elements[0]);
                        params.index = params.positionNum;
                        elementIds = handlers.getChildren(params).split("|");
                        break;
                    default:
                        elementIds = GetElementIds(elements, params.index);
                        break;
                }
                return elementIds;
                function GetElementIds(elements, index) {
                    let temp_elementIds = [];
                    let resultAttrName = params.resultAttrName;
                    if (!resultAttrName)
                        resultAttrName = "uniqueId";
                    if (index === -1) {
                        temp_elementIds = utils.getElementResultValue(elements, resultAttrName, nextParams);
                    } else {
                        if (index < elements.length) {
                            temp_elementIds = utils.getElementResultValue([elements[index]], resultAttrName, nextParams);
                        }
                    }
                    return temp_elementIds;
                }
            },
            queryCSSSelector: (params) => {
                const nextParams = utils.clone(params)
                const selector = []
                while (nextParams.selector.length > 0) {
                    const node = nextParams.selector.shift()
                    if (node.type === 'wnd' || node.type === 'win') { continue }
                    selector.push(node)
                    if (node.name === 'iframe' || node.name === 'frame') { break }
                }
                let parent = null
                if (params.parent) {
                    parent = utils.elementFromUniqueId(params.parent)
                }
                let elements = null
                if (selector[0]["name"] == "document") {
                    let root = document
                    if (params.parent) {
                        root = utils.elementFromUniqueId(params.parent)
                    }
                    const cssSelector = selector[0]["props"][0]["value"];
                    elements = root.querySelectorAll(cssSelector)
                    let text = ""
                    if (selector[0]["props"].length > 1) {
                        text = selector[0]["props"][1]["value"];
                        for (var i = 0; i < elements.length; i++) {
                            if (text) {
                                if (elements[i].outerText === text || elements[i].innerText === text) {
                                    break;
                                } else if (IIRPA.regMatch(elements[i].outerText, text)) {
                                    break;
                                } else {
                                    elements.splice(i, 1)
                                }
                            }
                        }
                    }
                }
                else {
                    elements = IIRPA.locale.findElements(selector, parent);
                }

                const elementIds = []
                if (elements && elements.length > 0) {
                    // 这里目前只考虑了一个iframe的情况     
                    const firstElement = elements[0]
                    if (utils.isFrameElement(firstElement) && nextParams.selector.length > 0) {
                        let nextFrameIndex = utils.getFrameIndex(firstElement.contentWindow)
                        if (!nextParams.frameTree) {
                            nextParams.frameTree = []
                        }
                        nextParams.frameTree.push(params.frameId)
                        return new Continue(nextFrameIndex, nextParams)
                    } else {
                        if (!nextParams.frameTree) {
                            nextParams.frameTree = []
                        }
                        nextParams.frameTree.push(params.frameId)
                        if (params.index === -1) {
                            for (const element of elements) {
                                elementIds.push(utils.uniqueId(nextParams.frameTree, element))
                            }
                        } else {
                            if (params.index < elements.length) {
                                elementIds.push(utils.uniqueId(nextParams.frameTree, elements[params.index]))
                            }
                        }
                    }
                }
                return elementIds
                //// params.tabId,             
                //// params.selector              
                //// params.parent            
                //// params.frameTree
                //// params.index, -1返回所有，否则返回指定索引位置的元素Id    
                //let root = document
                //if (params.parent) {
                //    root = utils.elementFromUniqueId(params.parent)
                //}
                //const elements = root.querySelectorAll(params.selector)
                //const frameTree = params.frameTree || [params.frameId]
                //const elementIds = []
                //if (elements && elements.length > 0) {
                //    if (params.index === -1) {
                //        for (const element of elements) {
                //            elementIds.push(utils.uniqueId(frameTree, element))
                //        }
                //    } else {
                //        if (params.index < elements.length) {
                //            elementIds.push(utils.uniqueId(frameTree, elements[params.index]))
                //        }
                //    }
                //}
                //return elementIds
            },
            getIntoViewPortRect: (params) => {
                return new Rectangle(0, 0, window.innerWidth, window.innerHeight)
            },
            scrollIntoViewIfNeeded_V2: (params) => {
                // params.elementId                
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                IIRPA.api.scrollIntoViewIfNeededorscrollIntoView(ele);
            },
            scrollIntoViewIfNeeded: (params) => {
                // params.elementId                
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                //if (!IIRPA.api.visibleLocation(ele, params.location, params.x, params.y).value) {
                IIRPA.api.scrollIntoViewIfNeededorscrollIntoView(ele);
                //}
            },
            getElementBounding: (params) => {
                // 应用反向查找requestOnFrameReverse     
                // params.elementId                
                // params.rects (nextParams)            
                // params.frameIndex (nextParams)       
                if (params.elementId) {
                    const ele = utils.elementFromUniqueId(params.elementId)
                    if (!ele) {
                        throw new HandlerError('未找到元素')
                    }
                    const rect = utils.getBoundingClientRect(ele)
                    if (!params.rects) {
                        params.rects = []
                    }
                    params.rects.push(rect)
                    delete params.elementId
                } else {
                    const frameElement = utils.getChildFrameByIndex(params.frameIndex)
                    if (frameElement) {
                        const rect = utils.getBoundingClientRect(frameElement)
                        params.rects.push(rect)
                    }
                }
                const frameIndex = utils.getFrameIndex(window)
                if (frameIndex === -1) {
                    // frameIndex==-1则为最外层Frame，查找结束，拼接结果      
                    const result = params.rects.reduce((n, m) => {
                        return {
                            left: n.left + m.left,
                            top: n.top + m.top,
                            width: n.width,
                            height: n.height
                        }
                    })
                    return result
                } else {
                    // 继续抛向外层Frame处理           
                    const nextParams = utils.clone(params)
                    nextParams.frameIndex = frameIndex
                    return new Continue(null, nextParams)
                }
            },
            cssSelectorAll: (params) => {
                //params.elementId
                //params.frameIndex
                if (params.elementId) {
                    const ele = utils.elementFromUniqueId(params.elementId)
                    if (!ele) {
                        throw new HandlerError("未找到元素")
                    }
                    const elePath = IIRPA.api.inspectcssSelector(ele).value
                    return elePath
                }
                else {
                    const frameElement = utils.getChildFrameByIndex(params.frameIndex)
                    if (frameElement) {
                        const framePath = IIRPA.api.inspectcssSelector(ele).value
                        return framePath
                    }
                }
                const frameIndex = utils.getFrameIndex(window).value
                if (frameIndex === -1) {
                    // frameIndex==-1则为最外层Frame，查找结束，拼接结果         
                    return params.path
                } else {
                    // 继续抛向外层Frame处理        
                    params.frameIndex = frameIndex
                    return new Continue(null, params)
                }
            },
            xPath: (params) => {
                ///html/body/header/div/ol[2]/li[2]/a[1]
                ////*[@id="dologin"]
                //params.elementId
                //params.frameIndex
                if (params.elementId) {
                    const ele = utils.elementFromUniqueId(params.elementId)
                    if (!ele) {
                        throw new HandlerError("未找到元素")
                    }
                    const elePath = IIRPA.api.inspectxPath(ele).value
                    return elePath
                }
                else {
                    const frameElement = utils.getChildFrameByIndex(params.frameIndex)
                    if (frameElement) {
                        const framePath = IIRPA.api.inspectxPath(ele).value
                        return framePath
                    }
                }
                const frameIndex = utils.getFrameIndex(window).value
                if (frameIndex === -1) {
                    // frameIndex==-1则为最外层Frame，查找结束，拼接结果         
                    return params.path
                } else {
                    // 继续抛向外层Frame处理        
                    params.frameIndex = frameIndex
                    return new Continue(null, params)
                }
            },
            buildSelector: (params) => {
                // 应用反向查找requestOnFrameReverse            
                // params.elementId
                // params.frameIndex (nextParams)
                // params.innerTextFirst
                if (params.elementId) {
                    const ele = utils.elementFromUniqueId(params.elementId)
                    if (!ele) {
                        throw new HandlerError('未找到元素')
                    }
                    const elePath = JSON.parse(IIRPA.api.inspectPath(ele, {
                        innerTextFirst: params.innerTextFirst,
                        excludeInnerText: params.excludeInnerText,
                        rawData: params.rawData
                    }).value)
                    params.path = []
                    params.path.push(...elePath)
                    delete params.elementId
                } else {
                    const frameElement = utils.getChildFrameByIndex(params.frameIndex)
                    if (frameElement) {
                        const framePath = JSON.parse(IIRPA.api.inspectPath(frameElement, {
                            innerTextFirst: params.innerTextFirst,
                            excludeInnerText: params.excludeInnerText,
                            rawData: params.rawData
                        }).value)
                        params.path.unshift(...framePath)
                    }
                }
                const frameIndex = utils.getFrameIndex(window)
                if (frameIndex === -1) {
                    // frameIndex==-1则为最外层Frame，查找结束，拼接结果         
                    return params.path
                } else {
                    // 继续抛向外层Frame处理        
                    params.frameIndex = frameIndex
                    return new Continue(null, params)
                }
            },
            getTableContent: (params) => {
                // params.tabId          
                // params.token
                // params.tokenType      
                // params.returnType              
                // params.parent                
                let root = null
                if (params.parent) {
                    root = utils.elementFromUniqueId(params.parent)
                }
                if (!root) { root = document }
                const tables = root.getElementsByTagName('table')
                let targetTable = null
                if (params.tokenType == 'index') {
                    targetTable = tables[parseInt(params.token)]
                } else if (params.tokenType == 'text') {
                    for (const table of tables) {
                        if (table.outerText.indexOf(params.token) > -1) {
                            targetTable = table
                            break;
                        }
                    }
                } else {
                    for (const table of tables) {
                        if (table.outerHTML.indexOf(params.token) > -1) {
                            targetTable = table
                            break;
                        }
                    }
                }
                contents = []
                if (targetTable) {
                    if (params.returnType == 'text') {
                        for (const row of targetTable.rows) {
                            rowContent = []
                            for (const cell of row.cells) {
                                rowContent.push(cell.innerText)
                            }
                            contents.push(rowContent)
                        }
                    } else {
                        for (const row of targetTable.rows) {
                            rowContent = []
                            for (const cell of row.cells) {
                                rowContent.push(cell.outerHTML)
                            }
                            contents.push(rowContent)
                        }
                    }
                }
                return contents
            },
            click: (params) => {
                // params.elementId               
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                const center = utils.getBoundingClientRect(ele).center()
                utils.raiseMouseEvents(["mousedown", "mouseup", "click"],
                    ele, center.x, center.y, 0)
            },
            dblclick: (params) => {
                // params.elementId             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                const center = utils.getBoundingClientRect(ele).center()
                utils.raiseMouseEvents(["mousedown", "mouseup", "click", "mousedown", "mouseup", "click", "dblclick"],
                    ele, center.x, center.y, 0)
            },
            rclick: (params) => {
                // params.elementId               
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                const center = utils.getBoundingClientRect(ele).center()
                utils.raiseMouseEvents(["mousedown", "mouseup", "contextmenu"],
                    ele, center.x, center.y, 2)
            },
            isEditable: (params) => {
                // params.elementId              
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                const nodeName = IIRPA.dom.lowerCaseNodeName(ele)
                if (ele.nodeType == 1
                    && (nodeName == "textarea" || (nodeName.indexOf("input") !== -1 && (/^(?:text|email|number|search|tel|url|password)$/i.test(ele.type)))
                        || ele.isContentEditable)) {
                    return ele.disabled !== true && ele.readOnly !== true
                } else {
                    return false
                }
            },
            input: (params) => {
                // params.elementId              
                // params.value               
                // params.replace            
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                const nodeName = IIRPA.dom.lowerCaseNodeName(ele)
                if (nodeName == 'textarea' || nodeName == 'input') {
                    var _value = '';
                    if (params.replace) {
                        _value = params.value
                    } else {
                        _value = ele.value + params.value
                    }
                    utils.raiseInputEvents(ele, _value);
                } else {
                    if (params.replace) {
                        ele.innerText = params.value
                    } else {
                        ele.innerText = ele.innerText + params.value
                    }
                }
            },
            focus: (params) => {
                // params.elementId         
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') } ele.focus()
            },
            getValue: (params) => {
                // params.elementId             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                return ele.value || ele.outerText || ele.innerText
            },
            getText: (params) => {
                // params.elementId        
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                switch (ele.tagName.toLowerCase()) {
                    case "input":
                    case "textarea":
                        return ele.value;
                    default:
                        return ele.innerText;
                }
            },
            getHtml: (params) => {
                // params.elementId             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                let html = ele.outerHTML
                //for (var i = 0; i < ele.children.length; i++) {
                //    html += ele.children[i].outerHTML
                //}
                return html
            },
            getSelectOption: (params) => {
                // params.elementId                 
                // params.selected: true/false   
                // params.optionType: text/value 
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                if (ele.tagName.toLowerCase() !== 'select') {
                    throw new HandlerError('该元素不支持选择操作')
                }
                const index = ele.selectedIndex
                if (params.optionType === 'text')
                    return ele.options[index].text
                else
                    return ele.options[index].value
            },
            getSelectOptions: (params) => {
                // params.elementId                 
                // params.selected: true/false   
                // params.optionType: text/value 
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                if (ele.tagName.toLowerCase() !== 'select') {
                    throw new HandlerError('该元素不支持选择操作')
                }
                const result = []
                const options = ele.getElementsByTagName('option')
                for (let i = 0; i < options.length; i++) {
                    if (params.selected === true && options[i].selected !== true) {
                        continue
                    }
                    if (params.optionType === 'text')
                        result.push(options[i].text);
                    else
                        result.push(options[i].value);
                }
                return result
            },
            setSelectOptions: (params) => {
                // params.elementId         
                // params.options: [0,1]           
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                if (ele.tagName.toLowerCase() !== 'select') {
                    throw new HandlerError('该元素不支持选择操作')
                }
                let changed = false
                const options = ele.getElementsByTagName('option')
                //多选              
                if (ele.getAttribute('multiple') !== null) {
                    for (let i = 0; i < options.length; i++) {
                        if (params.options.indexOf(i) !== -1) {
                            options[i].selected = true
                            changed = true
                        } else {
                            options[i].selected = false
                        }
                    }
                }
                //单选 选中第一个匹配项   
                else {
                    if (params.options.length !== 0) {
                        options[params.options[0]].selected = true
                        changed = true
                    }
                }
                if (changed) {
                    const event = document.createEvent('Events');
                    event.initEvent('change', true, false);
                    ele.dispatchEvent(event);
                }
            },
            getCheckStatus: (params) => {
                // params.elementId             
                // params.selected: true/false          
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                if (ele.tagName.toLowerCase() !== 'input' || (ele.type !== 'radio' && ele.type !== 'checkbox')) {
                    throw new HandlerError('该元素不支持勾选操作')
                }
                return ele.checked
            },
            setCheckStatus: (params) => {
                // params.elementId                
                // params.isChecked: true/false         
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                if (ele.tagName.toLowerCase() !== 'input') {
                    throw new HandlerError('该元素不支持勾选操作')
                }
                if (ele.type === 'radio') {
                    if (!params.isChecked) {
                        throw new HandlerError('radio元素不支持取消勾选操作')
                    } else {
                        ele.checked || ele.click()
                    }
                }
                else if (ele.type === 'checkbox') {
                    if (params.isChecked) {
                        ele.checked || ele.click()
                    }
                    else {
                        !ele.checked || ele.click()
                    }
                }
                else {
                    throw new HandlerError('该元素不支持勾选操作')
                }
                ele.checked = params.isChecked
            },
            setAttribute: (params) => {
                // params.tabId                     
                // params.elementId                    
                // params.name: '...'
                // params.value: '...'
                const ele = utils.elementFromUniqueId(params.elementId);
                if (!ele) {
                    throw new HandlerError('未找到元素');
                }
                if (params.value) {
                    ele.setAttribute(params.name, params.value);
                }
                else {
                    ele.setAttribute(params.name, "");
                }
            },
            getAttribute: (params) => {
                // params.elementId             
                // params.name: '...'            
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                if (params.name == "src") {
                    return ele.src || "";
                }
                if (params.name == "innerText") {
                    return (ele.innerText || ele.outerText) || "";
                }
                return ele.getAttribute(params.name)
            },
            scrollTopOrLeft: (params) => {
                // params.elementId               
                // params.scrollTop: 100            
                // params.scrollLeft: 200             
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                ele.scrollTop = params.scrollTop
                ele.scrollLeft = params.scrollLeft
                return true
            },
            isDisplayed: (params) => {
                // params.elementId               
                let node = utils.elementFromUniqueId(params.elementId)
                if (!node) {
                    throw new HandlerError('未找到元素')
                }
                while (node && node.nodeType == 1) {
                    let computedNode = getComputedStyle(node);
                    let display = computedNode.display;
                    let visibility = computedNode.visibility;
                    if (display == '' || display == 'none' || visibility == 'hidden')
                        return false;
                    node = node.parentNode;
                }
                return true;
            },
            getParent: (params) => {
                // params.elementId
                // params.relativeType
                // params.level,层级
                // params.filter
                const nextParams = utils.clone(params)
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                if (ele.tagName.toLowerCase() == "iframe" || ele.tagName.toLowerCase() == "frame") {
                    throw new HandlerError('iframe或者frame元素没有子元素')
                }
                if (!nextParams.frameTree) {
                    nextParams.frameTree = []
                }
                nextParams.frameTree.push(params.frameId)

                var elements = [];
                if (!params.filter) {//非高级模式
                    if (!params.level || params.level <= 0)
                        params.level = 1;
                    var parent = ele;
                    for (var i = 0; i < params.level; i++) {
                        parent = parent.parentNode;
                        if (!parent) {
                            return [];
                        }
                    }
                    elements.push(parent);
                }
                else {//高级模式
                    let filterNodes = utils.getRelativeElements({
                        filter: IIRPA.JSON().parse(params.filter),
                        tragetElement: ele
                    });
                    Array.prototype.push.apply(elements, filterNodes);
                }
                let resultAttrName = params.resultAttrName;
                if (!resultAttrName)
                    resultAttrName = "uniqueId";
                var elementIds = [];
                elementIds = utils.getElementResultValue(elements, resultAttrName, nextParams);

                //var elementIds = [];
                //for (const element of elements) {
                //    elementIds.push(utils.uniqueId(nextParams.frameTree, element))
                //}
                return elementIds;
            },
            getSiblings: (params) => {
                // params.elementId
                // params.relativeType
                // params.filter
                // params.position #兄弟元素位置  1 下一个 2 上一个  int
                const nextParams = utils.clone(params)
                const tragetEle = utils.elementFromUniqueId(params.elementId)
                if (!tragetEle) {
                    throw new HandlerError('未找到元素')
                }
                if (tragetEle.tagName.toLowerCase() == "iframe" || tragetEle.tagName.toLowerCase() == "frame") {
                    throw new HandlerError('iframe或者frame元素没有子元素')
                }
                if (!nextParams.frameTree) {
                    nextParams.frameTree = []
                }
                nextParams.frameTree.push(params.frameId)

                var elements = [];
                if (!params.filter) {
                    if (params.position == 1) {
                        let nextNode = tragetEle;
                        while ((nextNode = nextNode.nextSibling) && nextNode.nodeType !== 1) { }

                        if (nextNode) {
                            elements.push(nextNode);
                        }
                    }
                    else if (params.position == 2) {
                        let previousNode = tragetEle;
                        while ((previousNode = previousNode.previousSibling) && previousNode.nodeType !== 1) { }
                        if (previousNode) {
                            elements.push(previousNode);
                        }
                    }
                    else {
                        elements = IIRPA.dom.siblings(tragetEle.parentNode.firstChild, tragetEle);
                    }
                }
                else {//高级模式
                    let filterNodes = utils.getRelativeElements({
                        filter: IIRPA.JSON().parse(params.filter),
                        tragetElement: tragetEle
                    });
                    Array.prototype.push.apply(elements, filterNodes);
                }
                let resultAttrName = params.resultAttrName;
                if (!resultAttrName)
                    resultAttrName = "uniqueId";
                var elementIds = [];
                elementIds = utils.getElementResultValue(elements, resultAttrName, nextParams);
                //for (const element of elements) {
                //    elementIds.push(utils.uniqueId(nextParams.frameTree, element));
                //}
                return elementIds;
            },
            getChildren: (params) => {
                // params.index   第几个子元素，从1开始
                // params.filter
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) {
                    throw new HandlerError('未找到元素')
                }
                if (ele.tagName.toLowerCase() == "iframe" || ele.tagName.toLowerCase() == "frame") {
                    throw new HandlerError('iframe或者frame元素没有子元素')
                }
                const nextParams = utils.clone(params)
                if (!nextParams.frameTree) {
                    nextParams.frameTree = []
                }
                nextParams.frameTree.push(params.frameId)

                var elements = [];
                if (params.filter) {
                    let filterNodes = utils.getRelativeElements({
                        filter: IIRPA.JSON().parse(params.filter),
                        tragetElement: ele
                    });
                    Array.prototype.push.apply(elements, filterNodes);
                }
                else {
                    var siblings = ele.children;
                    if (!siblings || siblings.length == 0)
                        throw new HandlerError('未找到子元素')
                    var arraySelector = "";
                    for (var i = 0; i < siblings.length; i++) {
                        var sibling = siblings[i];
                        if (sibling == null || sibling.nodeType != 1) {
                            continue;
                        }
                        var elementRect = sibling.getBoundingClientRect();
                        if ((elementRect.right - elementRect.left) <= 0 && (elementRect.bottom - elementRect.top <= 0)) {
                            continue;
                        }

                        elements.push(sibling);
                        //index有效 且已获取到指定元素
                        if (params.index && params.index == elements.length) {
                            //elements = [elements.pop()];
                            break;
                        }
                    }
                    //index有效 且已获取到指定元素
                    if (params.index && params.index == elements.length) {
                        elements = [elements.pop()];
                    }
                    //index有效 但未获取到指定元素
                    else if (params.index && params.index > elements.length) {
                        elements = [];
                    }
                }
                let resultAttrName = params.resultAttrName;
                if (!resultAttrName)
                    resultAttrName = "uniqueId";
                var elementIds = utils.getElementResultValue(elements, resultAttrName, nextParams);
                return elementIds.join('|');
                //return arraySelector;
            },
            getAndScrollLastElement: (params) => {
                const nextParams = utils.clone(params)
                let isTable = nextParams.collectionConfig.tableSelector && nextParams.collectionConfig.tableSelector.length > 0;
                if (!nextParams.frameTree) {
                    nextParams.frameTree = []
                }
                nextParams.frameTree.push(nextParams.frameId)
                let selector;
                let scroll = !params.colorGuidanceConfig;
                if (!nextParams.lastElementId) {
                    let isFrame = false;
                    const nodes = []
                    selector = isTable ? nextParams.collectionConfig.tableSelector : nextParams.collectionConfig.commonSelector;
                    while (selector.length > 0) {
                        let node = selector.shift();
                        nodes.push(node)
                        if ((node.name === 'iframe' || node.name === 'frame')) {
                            isFrame = true;
                            break;
                        }
                    }
                    if (isFrame) {
                        var elements = IIRPA.locale.findElements(nodes, null, nextParams.version);
                        if (elements && elements.length > 0) {
                            // 这里目前只考虑了一个iframe的情况     
                            const firstElement = elements[0]
                            if (utils.isFrameElement(firstElement)) {
                                if (scroll)
                                    firstElement.scrollIntoView();
                                let nextFrameIndex = utils.getFrameIndex(firstElement.contentWindow)
                                return new Continue(nextFrameIndex, nextParams)
                            }
                        }
                        else {
                            return "";
                        }
                    }
                }
                if (isTable) {
                    selector = params.collectionConfig.tableSelector;
                }
                else {
                    selector = params.collectionConfig.commonSelector;
                    if (params.collectionConfig.columnSelectorArray.length > 0) {
                        selector = selector.concat(params.collectionConfig.columnSelectorArray[0]);
                    }
                }

                let lastElement = IIRPA.CollectionUtils.getAndScrollLastElement(selector, params.collectionConfig.selectorVersion, scroll);
                if (!lastElement) {
                    return "";
                }
                return utils.uniqueId(nextParams.frameTree, lastElement);
            },
            queryCollectionDataV2: (params) => {
                let isTable = params.collectionConfig.tableSelector && params.collectionConfig.tableSelector.length > 0;
                let selector = isTable ? params.collectionConfig.tableSelector : params.collectionConfig.commonSelector;
                let iframeIndex = selector.findIndex(node => (node.name === 'iframe' || node.name === 'frame'));
                while (iframeIndex > -1) {
                    selector.splice(0, iframeIndex + 1);
                    iframeIndex = selector.findIndex(node => (node.name === 'iframe' || node.name === 'frame'));
                }
                return IIRPA.CollectionUtils.queryCollectionData(params);
            },
            getSimilarElement: (params) => {
                const ele = utils.elementFromUniqueId(params.elementId);
                const similarElement = IIRPA.CollectionUtils.getSimilarElement(ele, "1.1");
                if (similarElement) {
                    if (!params.frameTree) {
                        params.frameTree = []
                    }
                    params.frameTree.push(params.frameId)
                    return IIRPA.dom.uniqueId(params.frameTree, similarElement);
                }
                return "";
            },
            getUsableAttributes: (params) => {
                const ele = utils.elementFromUniqueId(params.elementId)
                if (!ele) { throw new HandlerError('未找到元素') }
                return IIRPA.CollectionUtils.getUsableAttributes(ele);
            },
            queryCollectionData: (params) => {
                // params.tabId,                
                // params.parentSelector 
                // params.selectorDataList   列配置数据集合
                // params.version
                const collectElementsObj = {};
                const collectData = [];
                var sameParentElements = [];
                if (params.selectorDataList.length == 0) {
                    return collectData;
                }
                if (params.parentSelector && params.parentSelector.length > 0) {
                    var selectorVersion = params.selectorDataList[0].Version;
                    sameParentElements = handlers.getElementsFromeSelector(params.parentSelector, null, selectorVersion);
                    if (!sameParentElements || sameParentElements.length == 0) {
                        return collectData;
                    }
                }
                else {
                    sameParentElements.push(null);
                }
                if (sameParentElements.length > 0 && sameParentElements[0] != null) {
                    for (var i = 0; i < sameParentElements.length; i++) {
                        sameParentElements[i].setAttribute("i-rpa-guid", utils.guid());
                    }

                }
                if (params.selectorDataList.length > 1) {
                    sameParentElements = handlers.getSameParentsElements(params.selectorDataList, sameParentElements);
                }
                for (var j = 0; j < params.selectorDataList.length; j++) {
                    if (!collectElementsObj[j]) {
                        collectElementsObj[j] = [];
                    }
                }
                for (var i = 0; i < sameParentElements.length; i++) {
                    var maxCount = 0;
                    var collectedDataCount = collectElementsObj[0].length;
                    var parentNodePath = [];
                    var parentNode = sameParentElements[i];
                    if (parentNode) {
                        parentNode = sameParentElements[i].parentNode;
                        parentNodePath.push(IIRPA.buildPath(IIRPA.dom.lowerCaseNodeName(sameParentElements[i]), [], IIRPA.dom.getNodeOptionProps(sameParentElements[i], false)));
                    }
                    for (var j = 0; j < params.selectorDataList.length; j++) {
                        var collectDataSelector = params.selectorDataList[j];
                        var collectElements = [];
                        if (collectDataSelector.Selector.length == 0) {
                            collectElements.push(sameParentElements[i]);
                        }
                        else {
                            collectElements = handlers.getElementsFromeSelector(parentNodePath.concat(collectDataSelector.Selector), parentNode, collectDataSelector.Version);
                        }
                        if (collectElements && collectElements.length > maxCount) {
                            maxCount = collectElements.length;
                        }
                        for (var k = 0; k < collectElements.length; k++) {
                            if (collectElementsObj[j].indexOf(collectElements[k]) < 0) {
                                collectElementsObj[j].push(collectElements[k]);
                            }
                        }
                    }
                    for (var j = 0; j < params.selectorDataList.length; j++) {
                        const eleCountDiffNum = (collectedDataCount + maxCount) - collectElementsObj[j].length;
                        if (eleCountDiffNum > 0) {
                            //collectElementsObj[j].push("".padStart(eleCountDiffNum - 1, ",").split(","));
                            Array.prototype.push.apply(collectElementsObj[j], "".padStart(eleCountDiffNum - 1, ",").split(","));
                        }
                    }
                }
                for (var i = 0; i < params.selectorDataList.length; i++) {
                    const data = [];
                    const collectAttrName = params.selectorDataList[i].CollectAttrName;
                    for (var j = 0; j < collectElementsObj[i].length; j++) {
                        if (collectElementsObj[i][j]) {
                            //var resultVal = collectAttrName === "innerText" ? collectElementsObj[i][j].innerText : collectElementsObj[i][j].getAttribute(collectAttrName);
                            var resultVal;
                            if (collectAttrName == "innerText" && collectElementsObj[i][j].tagName.toLowerCase() == "input") {
                                resultVal = collectElementsObj[i][j].value;
                            }
                            else {
                                resultVal = collectElementsObj[i][j][collectAttrName];
                            }
                            if (!resultVal) {
                                resultVal = "";
                            }
                            data.push(resultVal);
                        }
                        else {
                            data.push("");
                        }
                    }
                    collectData.push(data);
                }
                return collectData;
            },
            getSameParentsElements: (pathTreeList, parents) => {
                var sameParentElements = [];
                var nodwTagsSet = new Set();
                var nodesElements = {};
                for (var i = 0; i < pathTreeList.length; i++) {
                    if (pathTreeList[i].Selector.length == 0) {
                        return parents;
                    }
                    nodwTagsSet.add(pathTreeList[i].Selector[0].name)
                    if (!nodesElements[i]) {
                        nodesElements[i] = [];
                    }
                }
                if (nodwTagsSet.size > 1) {
                    return parents;
                }
                for (var j = 0; j < parents.length; j++) {
                    nodesElements[i] = [];
                    let parent = parents[j];
                    //以每一个公共父级元素为parent,获取级节点匹配元素 
                    for (var i = 0; i < pathTreeList.length; i++) {
                        let collectDataSelector = pathTreeList[i];
                        var eles = handlers.getElementsFromeSelector([collectDataSelector.Selector[0]], parent, collectDataSelector.Version);//只差一个节点
                        if (eles && eles.length > 0) {
                            for (var k = 0; k < eles.length; k++) {
                                if (eles[k].parentNode == parent) {
                                    nodesElements[i].push(eles[k]);
                                }
                            }
                        }
                        if (!nodesElements[i] || nodesElements[i].length == 0) {
                            return parents;
                        }
                    }
                    //对比全相等，如全相等说明同级
                    for (var i = 0; i < pathTreeList.length - 1; i++) {
                        let isSame = utils.compareArray(nodesElements[i], nodesElements[i + 1]);
                        if (!isSame) {
                            return parents;
                        }
                    }
                    if (nodesElements[0]) {
                        Array.prototype.push.apply(sameParentElements, nodesElements[0]);
                    }
                }
                for (var i = 0; i < pathTreeList.length; i++) {
                    pathTreeList[i].Selector.shift();
                }
                return handlers.getSameParentsElements(pathTreeList, sameParentElements);
            },
            getElementsFromeSelector: (pathTree, parent, selectorVersion) => {
                const selector = [];
                var elements = [];
                const pathTree_clone = utils.clone(pathTree)
                while (pathTree_clone.length > 0) {
                    const node = pathTree_clone.shift()
                    if (node.name == 'tab') { continue }
                    selector.push(node)
                    if (node.name === 'iframe' || node.name === 'frame') { break }
                }
                elements = IIRPA.locale.findElements(selector, parent, selectorVersion);
                if (elements && elements.length > 0) {
                    //const restPathTree = pathTree_clone.slice(selector.length - pathTree_clone.length);
                    const firstElement = elements[0];
                    if (utils.isFrameElement(firstElement)) {
                        elements = [firstElement.contentWindow.document];
                    }
                }
                if (pathTree_clone.length > 0) {
                    const restElements = [];
                    for (var i = 0; i < elements.length; i++) {
                        var eles = handlers.getElementsFromeSelector(pathTree_clone, elements[i], selectorVersion);
                        if (eles) {
                            Array.prototype.push.apply(restElements, eles);
                        }
                    }
                    return restElements;
                }
                if (elements == null) {
                    elements = [];
                }
                return elements;
            },
            queryElementsAll: (params) => {
                const nextParams = utils.clone(params);
                if (!nextParams.frameTree) {
                    nextParams.frameTree = [];
                }
                nextParams.frameTree.push(params.frameId);
                let elementDatas = IIRPA.dom.getElements(nextParams.frameTree, params.tagNames, params.attrKeywords);
                return elementDatas;

                //let elementDatas = JSON.parse(IIRPA.dom.getElements(nextParams.frameTree, params.tagNames, params.attrKeywords));
                //let allframeElementDatas = [];
                //for (var i = 0; i < elementDatas.length; i++) {
                //    let tagName = elementDatas[i].controlType.toLowerCase()
                //    if (tagName === "frame" || tagName === "iframe") {
                //        let nextFrameIndex = utils.getFrameIndex(utils.elementFromUniqueId(elementDatas[i].uniqueId).contentWindow);
                //        let frameElementDatas = new Continue(nextFrameIndex, nextParams);
                //        allframeElementDatas.concat(frameElementDatas);
                //    }
                //}
                //return Json.stringify(elementDatas.concat(allframeElementDatas));
            }
        }
        this.codeId = '5087f47c1da6ef46c35e08c9ec1a9e2db4fc24a4'
        this.handle = function (method, params) {
            try {
                const tokens = method.split('.')
                let handler = handlers
                for (const token of tokens) {
                    handler = handler[token]
                }
                const result = handler.call(window, params)
                if (result instanceof Continue) {
                    return response.continue(result.frameIndex, result.params)
                } else {
                    return response.ok(result === undefined ? null : result)
                }
            } catch (error) {
                if (error instanceof HandlerError) {
                    return response.fail(error.code, error.message)
                } else {
                    return response.fail(ERRORS.UNKNOWN, error.stack || error.message || error)
                }
            }
        }
    }
})();