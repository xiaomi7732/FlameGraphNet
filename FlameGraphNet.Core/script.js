var details, svg;
const marginValue = 10;
const textMargin = 3;
function init(evt) {
    details = document.getElementById("details").firstChild;
    svg = document.getElementsByTagName("svg")[0];

    var allUnits = getAllStackUnits();
    if (!!allUnits) {
        const length = allUnits.length;
        for (var i = 0; i < length; i++) {
            update_text(allUnits[i]);
        }
    }
}

// mouse-over for info
function s(info) { details.nodeValue = 'Current: ' + info; }
function c() { details.nodeValue = ' '; }

// functions
function find_child(parent, name, attr) {
    var children = parent.childNodes;
    for (var i = 0; i < children.length; i++) {
        if (children[i].tagName === name)
            return (attr != undefined) ? children[i].attributes[attr].value : children[i];
    }
    return;
}
function orig_save(e, attr, val) {
    if (!!e.attributes["_orig_" + attr]) return;
    if (!e.attributes[attr]) return;
    if (val === undefined || val === null) {
        val = e.attributes[attr].value;
    }
    e.setAttribute("_orig_" + attr, val);
}

function orig_load(e, attr) {
    if (!e.attributes["_orig_" + attr]) return;
    e.attributes[attr].value = e.attributes["_orig_" + attr].value;
}

function update_text(e) {
    const rect = find_child(e, "rect");
    const text = find_child(e, "text");
    if (text === null || text === undefined) {
        return;
    }
    const width = parseFloat(rect.attributes["width"].value) - textMargin;
    const txt = find_child(e, "title").textContent.replace(/\([^(]*\)/, "");
    text.attributes["x"].value = parseFloat(rect.attributes["x"].value) + textMargin;

    // Smaller than this size won't fit anything
    if (width < 2 * 12 * 0.59) {
        text.textContent = "";
        return;
    }

    text.textContent = txt;
    // Fit in full text width
    if (/^ *$/.test(txt) || text.getSubStringLength(0, txt.length) < width) {
        return;
    }

    for (var x = txt.length - 2; x > 0; x--) {
        if (text.getSubStringLength(0, x + 2) <= width) {
            text.textContent = txt.substring(0, x) + "..";
            return;
        }
    }
    text.textContent = "";
}

// zoom
function zoom_reset(e) {
    if (!!e.attributes) {
        orig_load(e, "x");
        orig_load(e, "width");
    }
    if (!e.childNodes) return;
    for (var i = 0, c = e.childNodes; i < c.length; i++) {
        zoom_reset(c[i]);
    }
}

function zoom_child(item, left, ratio) {
    if (!!item.attributes) {
        if (!!item.attributes["x"]) {
            orig_save(item, "x");
            item.attributes["x"].value = (parseFloat(item.attributes["x"].value) - left - marginValue) * ratio + marginValue;
            if (item.tagName === "text") {
                item.attributes["x"].value = find_child(item.parentNode, "rect", "x") + textMargin;
            }
        }
        if (!!item.attributes["width"]) {
            orig_save(item, "width");
            item.attributes["width"].value = parseFloat(item.attributes["width"].value) * ratio;
        }
    }

    if (!item.childNodes) return;
    for (var i = 0, c = item.childNodes; i < c.length; i++) {
        zoom_child(c[i], left - marginValue, ratio);
    }
}

function zoom_parent(e) {
    if (!!e.attributes) {
        if (!!e.attributes["x"]) {
            orig_save(e, "x");
            e.attributes["x"].value = marginValue;
        }
        if (!!e.attributes["width"]) {
            orig_save(e, "width");
            e.attributes["width"].value = parseInt(svg.width.baseVal.value) - (marginValue * 2);
        }
    }
    if (!e.childNodes) return;
    for (var i = 0, c = e.childNodes; i < c.length; i++) {
        zoom_parent(c[i]);
    }
}

function zoom(node) {
    const attributes = find_child(node, "rect").attributes;
    const width = parseFloat(attributes["width"].value);
    const left = parseFloat(attributes["x"].value);
    const right = parseFloat(left + width);
    const top = parseFloat(attributes["y"].value);
    const ratio = (svg.width.baseVal.value - 2 * marginValue) / width;

    // XXX: Workaround for JavaScript float issues (fix me)
    var fudge = 0.0001;

    var resetZoomButton = document.getElementById("unzoom");
    resetZoomButton.style["opacity"] = "1.0";

    const stackUnits = getAllStackUnits();
    const stackUnitCount = stackUnits.length;
    for (var i = 0; i < stackUnitCount; i++) {
        const currentUnit = stackUnits[i];
        const currentUnitAttributes = find_child(currentUnit, "rect").attributes;
        const currentUnitLeft = parseFloat(currentUnitAttributes["x"].value);
        const currentUnitWidth = parseFloat(currentUnitAttributes["width"].value);
        const currentUnitRight = currentUnitLeft + currentUnitWidth;

        const isAncestor = parseFloat(currentUnitAttributes["y"].value) > top;

        if (isAncestor) {
            // Direct ancestor
            if (currentUnitLeft <= left && currentUnitRight + fudge >= right) {
                currentUnit.style["opacity"] = "0.5";
                zoom_parent(currentUnit);
                currentUnit.onclick = function (e) {
                    resetZoom();
                    zoom(this);
                };
                update_text(currentUnit);
            } else {
                // not in current path
                currentUnit.style["display"] = "none";
            }
        } else {
            // no common path
            if (currentUnitLeft < left || currentUnitLeft + fudge >= right) {
                currentUnit.style["display"] = "none";
            } else {
                zoom_child(currentUnit, left, ratio);
                currentUnit.onclick = function (e) { zoom(this); };
                update_text(currentUnit);
            }
        }
    }
}

function getAllStackUnits() {
    return document.getElementsByClassName("unit_g");
}

function resetZoom() {
    const unZoomBtn = document.getElementById("unzoom");
    unZoomBtn.style["opacity"] = "0.0";

    const stackUnits = getAllStackUnits();
    const length = stackUnits.length;
    for (i = 0; i < length; i++) {
        stackUnits[i].style["display"] = "block";
        stackUnits[i].style["opacity"] = "1";
        zoom_reset(stackUnits[i]);
        update_text(stackUnits[i]);
    }
}