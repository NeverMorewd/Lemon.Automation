﻿(function (IIRPA) {
    //if (!window.IIRPA)
    //    IIRPA = {};
    IIRPA.utils = new function () {
        this.guid = function () {
            return 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                    v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            })
        }
        this.clone = function (obj) {
            var copy
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
                    copy[i] = IIRPA.utils.clone(obj[i])
                }
                return copy
            }
            if (obj instanceof Object) {
                copy = {}
                for (var attr in obj) {
                    if (obj.hasOwnProperty(attr))
                        copy[attr] = IIRPA.utils.clone(obj[attr])
                }
                return copy
            }
            throw new Error("Unable to copy obj! Its type isn't supported.")
        }
        this.compareArray = function (arr1, arr2) {
            if (!arr2 || !arr1)
                return false;
            if (arr1.length != arr2.length)
                return false;
            for (var i = 0, l = arr1.length; i < l; i++) {
                if (arr1[i] instanceof Array && arr2[i] instanceof Array) {
                    if (!IIRPA.utils.compareArray(arr1[i], arr2[i]))
                        return false;
                } else if (arr1[i] != arr2[i]) {
                    return false;
                }
            }
            return true;
        }
        this.emptyArray = function (len) {
            var arr = [];
            for (var i = 0; i < len; i++) {
                arr.push("");
            }
            return arr;
        }
        this.indexOfArray = function (arr, val) {
            for (var i = 0; i < arr.length; i++) {
                if (val == arr[i]) {
                    return i;
                }
            }
            return -1;
        }
    }
    IIRPA.CollectionUtils = new function () {
        this.eleInfoTableCache = [];
        this.ExecElementsFilter = function (collectElementsObj, collectionConfig) {
            //collectElementsObj 原数据以列为组，过滤后，以行为组
            var dataAttrNameList = [];
            var filterConfigList = [];
            for (var i = 0; i < collectionConfig.columnConfigArray.length; i++) {
                dataAttrNameList.push(collectionConfig.columnConfigArray[i].dataAttrName);
                filterConfigList.push(collectionConfig.columnConfigArray[i].filterConfig)
            }
            var eleInfoTable = [];
            for (var i = 0; i < collectElementsObj[0].length; i++) {
                var eleInfoRow = [];
                for (var j = 0; j < dataAttrNameList.length; j++) {
                    var content = this.GetCollectContent(collectElementsObj[j][i], dataAttrNameList[j]);
                    eleInfoRow.push({
                        element: collectElementsObj[j][i],
                        content: content
                    });
                }
                if (eleInfoRow.length > 0) {
                    eleInfoTable.push(eleInfoRow);
                }
            }
            return eleInfoTable;
        }
        this.GetCollectContent = function (ele, collectAttrName) {
            if (!ele) {
                return "";
            }
            var resultVal;
            if (collectAttrName == "innerText" && ele.tagName.toLowerCase() == "input") {
                resultVal = ele.value;
            }
            else {
                resultVal = ele[collectAttrName];
            }
            return resultVal ? resultVal : "";
        }
        this.getElementsFromeSelector = function (pathTree, parent, selectorVersion) {
            var elements = [];
            var parentNodePath = [];
            if (parent) {
                var nodeGuiId = IIRPA.elementUtils.getOrSetElementGUID(parent);
                var prop = IIRPA.buildProp(IIRPA.dom.Attr.RPAGUID, nodeGuiId, IIRPA.PropPattern.EQUAL, true)
                parentNodePath.push(IIRPA.buildPath(IIRPA.dom.lowerCaseNodeName(parent), [], [prop]));
            }
            elements = IIRPA.locale.findElements(parentNodePath.concat(pathTree), null, selectorVersion);
            if (elements == null) {
                elements = [];
            }
            return elements;
        }
        this.getSameParentsElements = function (pathTreeList, parents, selectorVersion) {
            var sameParentElements = [];
            var nodeTag = "";
            var nodesElements = {};
            for (var i = 0; i < pathTreeList.length; i++) {
                if (pathTreeList[i].length == 0) {
                    return parents;
                }
                if (nodeTag == "") {
                    nodeTag = pathTreeList[i][0].name;
                }
                else if (nodeTag != pathTreeList[i][0].name) {
                    return parents;
                }
                if (!nodesElements[i]) {
                    nodesElements[i] = [];
                }
            }
            for (var j = 0; j < parents.length; j++) {
                nodesElements[i] = [];
                var parent = parents[j];
                //以每一个公共父级元素为parent,获取级节点匹配元素 
                for (var i = 0; i < pathTreeList.length; i++) {
                    var collectDataSelector = pathTreeList[i];
                    var eles = this.getElementsFromeSelector([collectDataSelector[0]], parent, selectorVersion);//只查一个节点
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
                    var isSame = IIRPA.utils.compareArray(nodesElements[i], nodesElements[i + 1]);
                    if (!isSame) {
                        return parents;
                    }
                }
                if (nodesElements[0]) {
                }
            }
            for (var i = 0; i < pathTreeList.length; i++) {
                pathTreeList[i].shift();
            }
            return this.getSameParentsElements(pathTreeList, sameParentElements, selectorVersion);
        }
        this.getElementsFromeSelectorNew = function (pathTreeList, selectorVersion) {
            var eles = this.getElementsFromeSelector(pathTreeList, null, selectorVersion);
            if (eles.length <= 1) {
                return null;
            }
            var lastPublicParent = null;
            var parentPath = [];
            var level = pathTreeList.length;
            while (!lastPublicParent) {
                level--;
                for (var i = 0; i < eles.length; i++) {
                    parentPath.push(eles[i].parentNode);
                }
                for (var i = 1; i < parentPath.length; i++) {
                    if (parentPath[0] != parentPath[i]) {
                        eles = parentPath;
                        parentPath = [];
                        break;
                    }
                }
                if (parentPath.length > 0) {
                    lastPublicParent = parentPath[0];
                }
            }
            if (!lastPublicParent) {
                return null;
            }
            return {
                baseLeval: level + 1,
                baseParents: this.getElementsFromeSelector([pathTreeList[level]], lastPublicParent, selectorVersion)
            }
        }
        this.queryCollectionColumnElements = function (collectionConfig) {
            // collectionConfig.TableSelector,
            // collectionConfig.commonSelector
            // collectionConfig.columnSelectorArray   列配置数据集合
            // collectionConfig.columnConfigArray
            var collectElementsObj = {};
            var eleInfoTable = [];
            var sameParentElements = [];
            var selectorVersion = collectionConfig.selectorVersion;
            if (collectionConfig.columnSelectorArray.length == 0) {
                return eleInfoTable;
            }
            if (collectionConfig.columnSelectorArray.length != collectionConfig.columnConfigArray.length) {
                return eleInfoTable;
            }
            var baseInfo = this.getElementsFromeSelectorNew(collectionConfig.columnSelectorArray[0], selectorVersion);
            if (!baseInfo) {
                sameParentElements.push(null);
            }
            else {
                for (var i = 0; i < collectionConfig.columnSelectorArray.length; i++) {
                    collectionConfig.columnSelectorArray[i].splice(0, baseInfo.baseLeval);
                }
                sameParentElements = baseInfo.baseParents;
            }
            for (var j = 0; j < collectionConfig.columnSelectorArray.length; j++) {
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
                    var nodeGuiId = IIRPA.elementUtils.getOrSetElementGUID(parentNode);
                    var prop = IIRPA.buildProp(IIRPA.dom.Attr.RPAGUID, nodeGuiId, IIRPA.PropPattern.EQUAL, true)
                    parentNodePath.push(IIRPA.buildPath(IIRPA.dom.lowerCaseNodeName(parentNode), [], [prop]));
                }
                for (var j = 0; j < collectionConfig.columnSelectorArray.length; j++) {
                    var collectDataSelector = collectionConfig.columnSelectorArray[j];
                    var collectElements = [];
                    if (collectDataSelector.length == 0) {
                        collectElements.push(sameParentElements[i]);
                    }
                    else {
                        collectElements = this.getElementsFromeSelector(parentNodePath.concat(collectDataSelector), null, selectorVersion);
                    }
                    if (collectElements && collectElements.length > maxCount) {
                        maxCount = collectElements.length;
                    }
                    for (var k = 0; k < collectElements.length; k++) {
                        if (IIRPA.utils.indexOfArray(collectElementsObj[j], collectElements[k]) < 0) {
                            collectElementsObj[j].push(collectElements[k]);
                        }
                    }
                }
                for (var j = 0; j < collectionConfig.columnSelectorArray.length; j++) {
                    var eleCountDiffNum = (collectedDataCount + maxCount) - collectElementsObj[j].length;
                    if (eleCountDiffNum > 0) {
                        Array.prototype.push.apply(collectElementsObj[j], IIRPA.utils.emptyArray(eleCountDiffNum));
                    }
                }
                IIRPA.elementUtils.removeElementGUID(parentNode);
            }
            eleInfoTable = this.ExecElementsFilter(collectElementsObj, collectionConfig);
            return eleInfoTable;
        }

        this.queryCollectionTableContent = function (collectionConfig, colorGuidanceConfig) {
            var eleInfoTable = [];
            var selectorVersion = collectionConfig.selectorVersion;
            var tables = this.getElementsFromeSelector(collectionConfig.tableSelector, null, selectorVersion);
            if (tables.length == 0) {
                return []
            }
            var tableCells = IIRPA.elementUtils.getTableCells(tables[0]);
            if (!tableCells) {
                return [];
            }
            for (var trIdx = 0; trIdx < tableCells.length; trIdx++) {
                var eleInfoRow = [];
                for (var tdIdx = 0; tdIdx < tableCells[trIdx].length; tdIdx++) {
                    var content = this.GetCollectContent(tableCells[trIdx][tdIdx], "innerText");
                    eleInfoRow.push({
                        element: tableCells[trIdx][tdIdx],
                        content: this.GetCollectContent(tableCells[trIdx][tdIdx], "innerText")
                    });
                }
                eleInfoTable.push(eleInfoRow);
            }
            if (tableCells.length <= 1 || collectionConfig.columnConfigArray.length == 0) {//首次预览，无配置返回完整数据
                return eleInfoTable;
            }
            var isPreview = colorGuidanceConfig && colorGuidanceConfig.actionCmd === "preview";
            if (!isPreview && collectionConfig.columnConfigArray[0].firstRowTitle === 1) {
                eleInfoTable.splice(1, 1);//第一行为标题，首行为第二行
            }
            var eleInfoTableNew = [];
            for (var i = 0; i < eleInfoTable.length; i++) {
                var eleInfoRow = [];
                var flag = true;
                for (var j = 0; j < collectionConfig.columnConfigArray.length; j++) {
                    if (eleInfoTable[i].length <= collectionConfig.columnConfigArray[j].defaultColumnIndex) {
                        flag = false;
                        break;
                    }
                    eleInfoRow.push(eleInfoTable[i][collectionConfig.columnConfigArray[j].defaultColumnIndex]);
                }
                if (flag) {
                    eleInfoTableNew.push(eleInfoRow);
                }
            }
            return eleInfoTableNew;
        }
        this.highlightCollectionElement = function (colorGuidanceConfig, eleInfoTable) {
            try {
                IIRPA.elementUtils.removeHighlight();
                if (!eleInfoTable) {
                    eleInfoTable = IIRPA.CollectionUtils.eleInfoTableCache;
                }
                if (colorGuidanceConfig.invalidRowIdxs.length > 0) {
                    for (var i = 0; i < colorGuidanceConfig.invalidRowIdxs.length; i++) {
                        if (eleInfoTable.length > colorGuidanceConfig.invalidRowIdxs[i]) {
                            eleInfoTable.splice(colorGuidanceConfig.invalidRowIdxs[i], 1);
                        }
                    }
                    IIRPA.CollectionUtils.eleInfoTableCache = eleInfoTable;
                }
                if (colorGuidanceConfig.rowIndex == -1 && colorGuidanceConfig.columnIndex == -1) {
                    for (var i = 0; i < eleInfoTable.length; i++) {
                        for (var j = 0; j < eleInfoTable[i].length; j++) {
                            IIRPA.elementUtils.highlightEle(eleInfoTable[i][j].element, j);
                        }
                    }
                }
                else if (colorGuidanceConfig.rowIndex > -1 && colorGuidanceConfig.columnIndex == -1) {
                    var isScroll = false;
                    for (var j = 0; j < eleInfoTable[colorGuidanceConfig.rowIndex].length; j++) {
                        IIRPA.elementUtils.highlightEle(eleInfoTable[colorGuidanceConfig.rowIndex][j].element, j);
                        if (!isScroll && eleInfoTable[colorGuidanceConfig.rowIndex][j].element) {
                            IIRPA.elementUtils.scrollIntoViewIfNeeded(eleInfoTable[colorGuidanceConfig.rowIndex][j].element);
                            isScroll = true;
                        }
                    }
                }
                else if (colorGuidanceConfig.rowIndex == -1 && colorGuidanceConfig.columnIndex > -1) {
                    for (var i = 0; i < eleInfoTable.length; i++) {
                        IIRPA.elementUtils.highlightEle(eleInfoTable[i][colorGuidanceConfig.columnIndex].element, colorGuidanceConfig.columnIndex);
                    }
                }
                else if (colorGuidanceConfig.rowIndex > -1 && colorGuidanceConfig.columnIndex > -1) {
                    IIRPA.elementUtils.highlightEle(eleInfoTable[colorGuidanceConfig.rowIndex][colorGuidanceConfig.columnIndex].element, colorGuidanceConfig.columnIndex);
                    IIRPA.elementUtils.scrollIntoViewIfNeeded(eleInfoTable[colorGuidanceConfig.rowIndex][colorGuidanceConfig.columnIndex].element);
                }
            } catch (e) {

            }
        }
        this.queryCollectionData = function (params) {
            if (params.colorGuidanceConfig && params.colorGuidanceConfig.actionCmd === "clear") {
                IIRPA.elementUtils.removeHighlight();
                this.eleInfoTable = [];
                return [];
            }
            if (params.colorGuidanceConfig && params.colorGuidanceConfig.actionCmd === "set") {
                this.highlightCollectionElement(params.colorGuidanceConfig);
                return [];
            }
            var eleInfoTable;
            if (params.collectionConfig.tableSelector && params.collectionConfig.tableSelector.length > 0) {
                eleInfoTable = this.queryCollectionTableContent(params.collectionConfig, params.colorGuidanceConfig);
            }
            else {
                eleInfoTable = this.queryCollectionColumnElements(params.collectionConfig);
            }
            if (params.colorGuidanceConfig && params.colorGuidanceConfig.actionCmd === "preview") {
                IIRPA.CollectionUtils.eleInfoTableCache = eleInfoTable;
            }
            var dataTable = [];
            var dataRow;
            for (var i = 0; i < eleInfoTable.length; i++) {
                dataRow = [];
                for (var j = 0; j < eleInfoTable[i].length; j++) {
                    dataRow.push(eleInfoTable[i][j].content);
                }
                dataTable.push(dataRow);
            }
            return dataTable;
        }

        this.getTurningPageElement = function (cssSelectors) {
            if (cssSelectors) {
                for (var i = 0; i < cssSelectors.length; i++) {
                    var ele = IIRPA.dom.querySelector(cssSelectors[i]);
                    if (ele) return ele;
                }
            }
            var pageTexts = [/^下一页/, /^next/]
            //const unQualifyTags = ['script', 'header', 'head', 'html', 'meta', 'style', 'title', 'body']
            var elements = IIRPA.dom.querySelectorAll("body *");
            for (var i = 0; i < elements.length; i++) {
                var ele = elements[i];
                for (var j = 0; j < pageTexts.length; j++) {
                    if (ele.innerHTML && ele.innerHTML.replace(/\s+/g, '').match(pageTexts[j])) {
                        return ele
                    }
                }
            }
            return null;
        }
        this.getAndScrollLastElement = function (selector, selectorVersion, scroll) {
            var eles = this.getElementsFromeSelector(selector, null, selectorVersion);
            if (!eles || eles.length == 0) {
                return null;
            }
            if (scroll != true) {
                return eles[eles.length - 1];
            }
            //var isVisible = IIRPA.elementUtils.eleIsInVisible(eles[eles.length - 1], true, 100);
            //if (!isVisible) {
            //    var eleClientRect = eles[eles.length - 1].getBoundingClientRect()
            //    if (eleClientRect.width != 0 && eleClientRect.height != 0) {
            //        var scrollTop = IIRPA.dom.getScrollTop();
            //        var viewHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
            //        var y1 = eleClientRect.y ? eleClientRect.y : eleClientRect.top;
            //        var y2 = y1 + eleClientRect.height + scrollTop - viewHeight + 200;
            //        IIRPA.dom.scrollTop(null, y2);
            //    }
            //}
            return eles[eles.length - 1];
        }
        this.getUsableAttributes = function (ele) {
            if (!ele) {
                return [];
            }
            return IIRPA.elementUtils.getAttributes(ele);
        }
        this.getSimilarElement = function (ele, selectorVersion) {
            if (!ele) {
                return null;
            }
            var diffGroupData = {};
            var elePath = IIRPA.dom.truncPathTree(ele, true);
            var pathTreeInfo = GetPathTreeInfo(elePath);
            var eleIdxPath = pathTreeInfo.idxPath;
            var disableIdxPathTree = pathTreeInfo.disableIdxPathTree;
            var enableIdxPathTree = pathTreeInfo.enableIdxPathTree;

            var elements = this.getElementsFromeSelector(disableIdxPathTree, null, selectorVersion);
            for (var i = 0; i < elements.length; i++) {
                if (elements[i] == ele) {
                    continue;
                }
                var diffIdxs = [];
                var node = elements[i];
                for (var j = eleIdxPath.length - 1; j >= 0; j--) {
                    if (!node) {
                        diffIdxs = [];
                        break;
                    }
                    var nthChild = IIRPA.dom.getNthChild(node);
                    if (nthChild != eleIdxPath[j]) {
                        diffIdxs.push(j);
                    }
                    if (j > 0) {
                        node = node.parentNode;
                    }
                }
                if (diffIdxs.length > 0) {
                    addOrUpdateDiffData(diffIdxs);
                }
            }
            var diffData = getDiffData();
            var newElePath = [];
            for (var i = 0; i < diffData.length; i++) {
                newElePath = IIRPA.utils.clone(enableIdxPathTree);
                for (var j = 0; j < diffData[i].diffIdxs.length; j++) {
                    var props = newElePath[diffData[i].diffIdxs[j]].props;
                    for (var k = props.length - 1; k >= 0; k--) {
                        if (props[k].name == IIRPA.dom.Attr.NTH_CHILD) {
                            props[k].accurate = "false";
                            break;
                        }
                    }
                }
                elements = this.getElementsFromeSelector(newElePath, null, selectorVersion);
                for (var k = 0; k < elements.length; k++) {
                    if (elements[k] == ele) {
                        elements.splice(k, 1);
                        if (elements.length > 0) {
                            return elements[0];
                        }
                    }
                }
            }
            return null;
            function GetPathTreeInfo(pathTree) {
                var idxPath = [];
                var disableIdxPathTree = IIRPA.utils.clone(pathTree);
                for (var i = 0; i < pathTree.length; i++) {
                    var idx = -1;
                    for (var j = pathTree[i].props.length - 1; j >= 0; j--) {
                        if (pathTree[i].props[j].name == IIRPA.dom.Attr.NTH_CHILD) {
                            pathTree[i].props[j].accurate = "true";
                            disableIdxPathTree[i].props[j].accurate = "false";
                            idx = pathTree[i].props[j].value;
                            break;
                        }
                    }
                    idxPath.push(idx);
                }
                return {
                    idxPath: idxPath,
                    disableIdxPathTree: disableIdxPathTree,
                    enableIdxPathTree: pathTree
                }
            }
            function addOrUpdateDiffData(diffIdxs) {
                var diffIdxsStr = diffIdxs.join(",");
                if (!diffGroupData[diffIdxsStr]) {
                    diffGroupData[diffIdxsStr] = { diffIdxs: diffIdxs, sameCount: 0 };
                }
                diffGroupData[diffIdxsStr].sameCount += 1;
            }

            function getDiffData() {
                var diffData = [];
                for (var porpName in diffGroupData) {
                    diffData.push(diffGroupData[porpName]);
                }
                //Object.keys(diffGroupData).forEach(function (key) {
                //    diffData.push(diffGroupData[key]);
                //});
                diffData.sort(function (a, b) {
                    if (a.diffIdxs.length == b.diffIdxs.length)
                        return b.sameCount - a.sameCount;
                    else
                        return a.diffIdxs.length - b.diffIdxs.length;
                });
                return diffData;
            }
        }
    }


    IIRPA.elementUtils = new function () {
        this.highlightAttrName = "ii-rpa-highlight";
        this.oldStyleAttrName = "ii-rpa-oldStyle-";
        this.highlightColorArrs = [
            ["rgba(255, 56, 45, 0.10)", "rgba(255, 56, 45, 1)"],
            ["rgba(255, 152, 0, 0.10)", "rgba(255, 152, 0, 1)"],
            ["rgba(255, 193, 8, 0.10)", "rgba(255, 193, 8, 1)"],
            ["rgba(255, 235, 59, 0.10)", "rgba(255, 235, 59, 1)"],
            ["rgba(205, 220, 57, 0.10)", "rgba(205, 220, 57, 1)"],
            ["rgba(139, 195, 74, 0.10)", "rgba(139, 195, 74, 1)"],
            ["rgba(75, 175, 80, 0.10)", "rgba(75, 175, 80, 1)"],
            ["rgba(2, 150, 136, 0.10)", "rgba(2, 150, 136, 1)"],
            ["rgba(4, 187, 212, 0.10)", "rgba(4, 187, 212, 1)"],
            ["rgba(0, 133, 255, 0.10)", "rgba(0, 133, 255, 1)"],
            ["rgba(57, 85, 236, 0.10)", "rgba(57, 85, 236, 1)"],
            ["rgba(107, 45, 218, 0.10)", "rgba(107, 45, 218, 1)"],
            ["rgba(156, 39, 176, 0.10)", "rgba(156, 39, 176, 1)"],
            ["rgba(233, 30, 99, 0.10)", "rgba(233, 30, 99, 1)"],
            ["rgba(141, 98, 82, 0.10)", "rgba(141, 98, 82, 1)"],
            ["rgba(97, 125, 139, 0.10)", "rgba(97, 125, 139, 1)"]
        ];
        this.highlightElements = function (params) {
            //params.elements
            //params.columnIdx
            var colorArr = this.highlightColorArrs[params.columnIdx % this.highlightColorArrs.length];
            for (var i = 0; i < params.elements.length; i++) {
                if (eles[i]) {
                    IIRPA.elementUtils.highlightEle(eles[i], colorArr);
                }
            }
        }
        this.highlightEle = function (ele, colorIndex) {
            if (!ele) {
                return;
            }
            if (colorIndex >= this.highlightColorArrs.length) {
                colorIndex = colorIndex % this.highlightColorArrs.length;
            }
            var colorArr = this.highlightColorArrs[colorIndex];
            this.setElementStyle(ele, {
                "outline": colorArr[1] + " auto 2px",
                "outline-offset": "-1px",
                "backgroundColor": colorArr[0]
            });
        }
        this.removeHighlight = function (eles) {
            if (!eles) {
                eles = IIRPA.dom.querySelectorAll("[" + this.highlightAttrName + "]");
            }
            for (var i = 0; i < eles.length; i++) {
                IIRPA.elementUtils.removeElementStyle(eles[i], {
                    "outline": "",
                    "outline-offset": "",
                    "backgroundColor": ""
                });
            }
        }
        this.setElementStyle = function (ele, styles) {
            try {
                if (!ele) {
                    return;
                }
                if (ele[this.highlightAttrName]) {
                    this.removeElementStyle(ele, styles);
                }
                ele.setAttribute(this.highlightAttrName, '1')
                for (var porpName in styles) {
                    var oldStyleVal = ele.style[this.oldStyleAttrName + porpName];
                    if (oldStyleVal) {
                        ele[this.oldStyleAttrName + porpName] = oldStyleVal;
                    }
                    else {
                        ele.style[porpName] = "";
                    }
                    ele.style[porpName] = styles[porpName];
                }
            } catch (e) {

            }
        }
        this.removeElementStyle = function (ele, styles) {
            if (!ele) {
                return;
            }
            for (var porpName in styles) {
                var oldStyleVal = ele[this.oldStyleAttrName + porpName];
                if (oldStyleVal) {
                    ele.style[porpName] = oldStyleVal;
                }
                else {
                    ele.style[porpName] = "";
                }
                ele.removeAttribute(this.oldStyleAttrName + porpName);
            }
            //Object.keys(styles).forEach(function (key) {
            //    var oldStyleVal = ele[this.oldStyleAttrName + key];
            //    if (oldStyleVal) {
            //        ele.style[key] = oldStyleVal;
            //    }
            //    else {
            //        ele.style[key] = "";
            //    }
            //    ele.removeAttribute(this.oldStyleAttrName + key);
            //});
        }
        this.getOrSetElementGUID = function (ele) {
            if (!ele) {
                return "";
            }
            var nodeGuId = ele.getAttribute(IIRPA.dom.Attr.RPAGUID);
            if (!nodeGuId) {
                var nodeGuId = IIRPA.utils.guid();
                ele.setAttribute(IIRPA.dom.Attr.RPAGUID, nodeGuId)
            }
            return nodeGuId;
        }
        this.removeElementGUID = function (ele, rpaGUID) {
            if (ele) {
                ele.removeAttribute(IIRPA.dom.Attr.RPAGUID);
            }
            else {
                var selector = rpaGUID ? ("[" + IIRPA.dom.Attr.RPAGUID + "='" + rpaGUID + "']") : ("[" + IIRPA.dom.Attr.RPAGUID + "]");
                var eles = IIRPA.dom.querySelectorAll(selector);
                for (var i = 0; i < eles.length; i++) {
                    eles[i].removeAttribute(IIRPA.dom.Attr.RPAGUID);
                }
            }
        }
        this.eleIsInVisible = function (ele, isFully, offsetY) {
            if (!ele) {
                return false;
            }
            var eleClientRect = ele.getBoundingClientRect()
            if (eleClientRect.width == 0 || eleClientRect.height == 0) {
                return false;
            }
            var viewWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
            var viewHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
            var x1 = eleClientRect.x ? eleClientRect.x : eleClientRect.left;
            var y1 = eleClientRect.y ? eleClientRect.y : eleClientRect.top;
            var x2 = x1 + eleClientRect.width;
            var y2 = y1 + eleClientRect.height;
            if (isFully == true) {
                if ((y1 >= 0 && y1 <= viewHeight) && (y2 >= 0 && y2 + offsetY <= viewHeight)) {
                    return (x1 >= 0 && x1 <= viewWidth) && (x2 >= 0 && x2 <= viewWidth);
                }
            }
            else {
                if ((y1 >= 0 && y1 <= viewHeight) || (y2 >= 0 && y2 <= viewHeight)) {
                    return (x1 >= 0 && x1 <= viewWidth) || (x2 >= 0 && x2 <= viewWidth);
                }
            }
            return false;
        }
        this.getAttributes = function (ele) {
            if (!ele) {
                return [];
            }
            var retData = [];
            var attrs = ele.attributes;
            for (var i = 0; i < attrs.length; i++) {
                if ('i-rpa' == attrs[i].name || !attrs[i].value) {
                    continue;
                }
                retData.push(attrs[i].name);
            }
            return retData;
        }
        this.getTableCells = function (tableEle) {
            if (!tableEle) {
                return [];
            }
            var tableData = [];
            var rowData = [];
            var rowSpanDatas = [];
            var hasTbHead = false;
            //rowspan的相关信息         
            var rows = tableEle.rows;
            var _colInx = 0;
            for (var item = 0; item < rows.length; item++) {
                rowData = [];
                var row = rows[item]
                if (!hasTbHead && row.parentNode.nodeName === "THEAD") {
                    hasTbHead = true;
                }
                if (row.cells.length == 0) {
                    continue;
                }
                var tds = row.cells;
                //找到匹配到的rowspan信息           
                var datasFromRowSpan = [];
                if (rowSpanDatas.length > 0) {
                    datasFromRowSpan = tdsFromRowSpan(item, rowSpanDatas)
                }
                _colInx = 0;
                //不算rowspan的列序号          
                var preRowData = rowData;
                rowData = dataFromTdRowSpan(datasFromRowSpan, 1, 0, rowData, rowSpanDatas);
                _colInx += rowData.length - preRowData.length;
                for (var tdItem = 0; tdItem < tds.length; tdItem++) {
                    preRowData = rowData;
                    rowData = dataFromTdRowSpan(datasFromRowSpan, 2, _colInx, preRowData, rowSpanDatas);
                    _colInx += rowData.length - preRowData.length;
                    var e = tds[tdItem];
                    var tdChildren = tds[tdItem].children;
                    if (tdChildren.length == 1 && tdChildren[0].tagName.toLowerCase() == "input") {
                        e = tdChildren[0]
                    }
                    rowData.push(e);
                    if (!hasTbHead && tds[tdItem].nodeName === "TH") {
                        hasTbHead = true;
                    }
                    //处理rowspan:                
                    var rowspan = tds[tdItem].rowSpan;
                    if (rowspan != null && rowspan > 1) {
                        rowSpanDatas.push({ col: _colInx, row: item, rowspan: rowspan, value: e });
                    }
                    //处理colspan               
                    var colspan = tds[tdItem].colSpan;
                    if (colspan != null && colspan > 1) {
                        for (var j = 0; j < colspan - 1; j++) {
                            rowData.push(null);
                            _colInx++;
                        }
                    }
                    _colInx++;
                }
                //有无尾巴要补       
                preRowData = rowData;
                rowData = dataFromTdRowSpan(datasFromRowSpan, 3, _colInx, preRowData, rowSpanDatas);
                _colInx += rowData.length - preRowData.length;
                tableData.push(rowData);
            }
            if (!hasTbHead && tableData.length > 0) {
                var emptyRowData = [];
                for (var i = 0; i < tableData[0].length; i++) {
                    emptyRowData.push(null);
                }
                tableData.unshift(emptyRowData);
            }
            return tableData;

            /**      * 先根据行号计算，此行有几个要处理的rowspan信息      */
            function tdsFromRowSpan(row, rowSpanDatas) {
                var array = [];
                for (var i = 0; i < rowSpanDatas.length; i++) {
                    data = rowSpanDatas[i];
                    if (data.row >= row)
                        continue;
                    if (row - data.row + 1 <= data.rowspan) {
                        array.push({ col: data.col, value: data.value });
                    }
                }
                return array;
            }
            /*  
             * 补充rowspan的数据，有三种规则：     
             * 规则1：表格前面列的rowspan
             * 规则2：插入到已有列中间
             * 规则3：尾部合并的rowspan
             */
            function dataFromTdRowSpan(datasFromRowSpan, type, col, rowData, rowSpanDatas) {
                if (datasFromRowSpan.length == 0)
                    return rowData;
                switch (type) {
                    case 1:
                        var _idx = 0
                        while (true) {
                            var matched = false;
                            for (var i = 0; i < rowSpanDatas.length; i++) {
                                data = rowSpanDatas[i];
                                if (data.col == _idx) {
                                    rowData.push(data.value);
                                    matched = true;
                                    break;
                                }
                            }
                            if (!matched)
                                break;
                            _idx++;
                            //针对本次，处理完前面所有的rowspan行       
                        }
                        break;
                    case 2:
                        for (var i = 0; i < rowSpanDatas.length; i++) {
                            data = rowSpanDatas[i];
                            if (data.col == col && col != 0) {
                                rowData.push(data.value);
                                break;
                            }
                        }
                        break;
                    case 3:
                        for (var i = 0; i < rowSpanDatas.length; i++) {
                            data = rowSpanDatas[i];
                            if (data.col + 1 > col) {
                                //这里为什么要+1，因为data.col取时从0开始的，而type=3在所有列处理完后，是总数量。所以要加1         
                                rowData.push(data.value);
                            }
                        }
                        break;
                }
                return rowData;
            }
        }
        this.scrollIntoViewIfNeeded = function (ele) {
            if (!ele) {
                return;
            }
            if (!IIRPA.dom.elementVisibleLocation(ele)) {
                if (window.mozInnerScreenX !== undefined)
                    ele.scrollIntoView({ behavior: "auto", block: "center", inline: "center" });
                else if (ele.scrollIntoViewIfNeeded)
                    ele.scrollIntoViewIfNeeded() //默认参数为True，让元素在可视区域中居中对齐
                else if (ele.scrollIntoView) {
                    ele.scrollIntoView();
                    document.documentElement.scrollTop -= 200
                }
            }
        }
    }
})(IIRPA);
