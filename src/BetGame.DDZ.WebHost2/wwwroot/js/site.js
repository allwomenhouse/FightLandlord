var apipath = '';
function ajax(type, url, data, header, dataType) {

    return new Promise((resolve, reject) => {
        if (!header) header = {};
        header.token = localStorage.getItem('token');
        if (!header.token) delete header.token;
        while (url[0] == '/') url = url.substring(1);
        ajaxRequest(type, apipath + '/' + url, data, resolve, function (d) {
            if (d.code === 5009) localStorage.setItem('token', null); /*登陆TOKEN失效_请重新登陆*/
            d.url = type + ' ' + url;
            console.log(d);
            if ((d.code === 5009 || d.code === 5001) && !top.istmpreload) {
                top.istmpreload = true;
                alert('请重新登陆');
                top.location.href = '/';
            }
            resolve(d);
        }, header, dataType);
    });
};
function ajaxRequest(type, url, data, callback, failCallBack, header, dataType) {
    var url_encode = function (str) {
        return encodeURIComponent(str)
            .replace(/ /gi, '+')
            .replace(/~/gi, '%7e')
            .replace(/'/gi, '%26%2339%3b');
    };

    type = String(type || 'GET').toUpperCase();
    if (type == 'GET') {
        var dataStr = typeof (data) === 'string' ? data : '';
        if (typeof (data) === 'object')
            for (var key in data) {
                if (Object.prototype.toString.call((data[key])) == '[object Array]') {
                    for (var a = 0; a < data[key].length; a++)
                        if (data[key][a] !== undefined) dataStr += '&' + key + '=' + url_encode(data[key][a]);
                }
                if (data[key] !== "" && data[key] !== null) dataStr += '&' + key + '=' + url_encode(data[key]);
            }

        if (dataStr !== '') {
            dataStr = dataStr.substr(1);
            url = url + '?' + dataStr;
        }
    }
    var sendData = '';
    var contentType = 'application/x-www-form-urlencoded; charset=utf-8';
    if (type == "FORM") {
        if (typeof (data) === 'string') sendData = data;
        if (typeof (data) === 'object')
            for (var key in data) {
                if (Object.prototype.toString.call((data[key])) == '[object Array]') {
                    for (var a = 0; a < data[key].length; a++)
                        if (data[key] !== undefined) sendData += '&' + key + "=" + url_encode(data[key][a]);
                } else if (data[key] !== "" && data[key] !== null) sendData += '&' + key + "=" + url_encode(data[key]);
            }
        if (sendData !== '') sendData = sendData.substr(1);
    }
    if (type == 'JSON') {
        sendData = JSON.stringify(data);
        contentType = "application/json; charset=utf-8";
    }
    if (!failCallBack) failCallBack = console.log;
    var requestObj = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject;
    requestObj.onreadystatechange = () => {
        if (requestObj.readyState == 4) {
            if (requestObj.status == 200) {
                var obj = requestObj.response
                if (String(dataType).toLowerCase() === 'html') return callback(obj);
                if (typeof obj !== 'object') obj = JSON.parse(obj);
                if (obj.code === 0) return callback(obj);
                failCallBack(obj);
            } else {
                failCallBack(requestObj)
            }
        }
    };
    requestObj.open(type == 'GET' ? type : 'POST', url, true);
    if (type != 'GET') requestObj.setRequestHeader("Content-type", contentType);
    if (typeof (header) === 'object')
        for (var key in header) requestObj.setRequestHeader(key, header[key]);
    requestObj.send(sendData || null);
}

String.prototype.trim = function () {
    return this.ltrim().rtrim();
}
String.prototype.ltrim = function () {
    return this.replace(/^\s+(.*)/g, '$1');
}
String.prototype.rtrim = function () {
    return this.replace(/([^ ]*)\s+$/g, '$1');
}
//中文按2位算
String.prototype.getLength = function () {
    return this.replace(/([\u0391-\uFFE5])/ig, '11').length;
}
String.prototype.left = function (len, endstr) {
    if (len > this.getLength()) return this;
    var ret = this.replace(/([\u0391-\uFFE5])/ig, '$1\0')
        .substr(0, len).replace(/([\u0391-\uFFE5])\0/ig, '$1');
    if (endstr) ret = ret.concat(endstr);
    return ret;
}
String.prototype.format = function () {
    var val = this.toString();
    for (var a = 0; a < arguments.length; a++) val = val.replace(new RegExp("\\{" + a + "\\}", "g"), arguments[a]);
    return val;
}
var __padleftright = function (str, len, padstr, isleft) {
    str = str || ' ';
    padstr = padstr || '';
    var ret = [];
    for (var a = 0; a < len - str.length; a++) ret.push(padstr);
    if (isleft) ret.unshift(this)
    else ret.push(this);
    return ret.join('');
}
// 'a'.padleft(3, '0') => '00a'
String.prototype.padleft = function (len, padstr) {
    return __padleftright(this, len, padstr, 1);
};
// 'a'.padright(3, '0') => 'a00'
String.prototype.padright = function (len, padstr) {
    return __padleftright(this, len, padstr, 0);
};
Function.prototype.toString2 = function () {
    var str = this.toString();
    str = str.substr(str.indexOf('/*') + 2, str.length);
    str = str.substr(0, str.lastIndexOf('*/'));
    return str;
};
Number.prototype.round = function (r) {
    r = typeof (r) == 'undefined' ? 1 : r;
    var rv = String(this);
    var io = rv.indexOf('.');
    var ri = io == -1 ? '' : rv.substr(io + 1, r);
    var le = io == -1 ? (rv + '.') : rv.substr(0, io + 1 + r);
    for (var a = ri.length; a < r; a++) le += '0';
    return le;
};
Date.prototype.toString = function (f) {
    function tempfunc(opo, pos) {
        var val = '';
        opo = String(opo);
        for (var a = 1; a < arguments.length; a++) {
            var chr = opo.charAt(arguments[a] - 1);
            val += chr;
        }
        return val;
    }

    if (!f) f = 'yyyy-MM-dd HH:mm:ss';
    var year = this.getFullYear();
    var h12 = this.getHours() > 12 ? (this.getHours() - 12) : this.getHours()
    var tmp = {
        'yyyy': year,
        'yy': tempfunc(year, 3, 4),
        'MM': (this.getMonth() < 9 ? '0' : '') + (this.getMonth() + 1),
        'M': this.getMonth() + 1,
        'dd': (this.getDate() < 10 ? '0' : '') + this.getDate(),
        'd': this.getDate(),
        'hh': (h12 < 10 ? '0' : '') + h12,
        'h': h12,
        'HH': (this.getHours() < 10 ? '0' : '') + this.getHours(),
        'H': this.getHours(),
        'mm': (this.getMinutes() < 10 ? '0' : '') + this.getMinutes(),
        'm': this.getMinutes(),
        'ss': (this.getSeconds() < 10 ? '0' : '') + this.getSeconds(),
        's': this.getSeconds()
    };

    for (var p in tmp) f = f.replace(new RegExp('\\b' + p + '\\b', 'g'), tmp[p]);
    return f;
}
Date.prototype.timespan = function () {
    var sec = (new Date().getTime() - this.getTime()) / 1000;
    if (sec < 5) return '刚刚';
    if (sec < 60) return Math.floor(sec) + '秒前';
    var min = sec / 60;
    if (min < 60) return Math.floor(min) + '分钟前';
    var hou = min / 60;
    if (hou < 24) return Math.floor(hou) + '小时前';
    var day = hou / 24;
    if (day < 30) return Math.floor(day) + '天前'
    var mon = day / 30;
    if (mon < 12) return Math.floor(mon) + '个月前';
    return Math.floor(day / 365) + '年前';
};