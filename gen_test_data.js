var fs = require('fs');
var path = require('path');
var execSync = require('child_process').execSync;

var NOVEL_DIR = path.join(__dirname, '小说');
var WWW = path.join(__dirname, 'www');
var MAX_PARAS = 30; // shorter for fast test

function decodeToUTF8(filePath) {
    var buf = fs.readFileSync(filePath);
    var text = buf.toString('utf-8');
    var invalid = 0;
    for (var i = 0; i < Math.min(text.length, 1000); i++) {
        if (text.charCodeAt(i) === 0xFFFD) invalid++;
    }
    if (invalid < 3) return text;
    try {
        return execSync(
            'powershell -NoProfile -Command "[Console]::OutputEncoding=[Text.Encoding]::UTF8;$c=[System.Text.Encoding]::Default.GetString([System.IO.File]::ReadAllBytes(\\"' + filePath + '\\"));Write-Output $c"',
            { encoding: 'utf-8', maxBuffer: 10 * 1024 * 1024 }
        );
    } catch (e) { return text; }
}

function extractContent(text, maxParas) {
    var lines = text.split(/\r?\n/);
    var paras = [], foundChapter = false, buf = '';
    for (var i = 0; i < lines.length; i++) {
        var l = lines[i].replace(/^\s+|\s+$/g, '');
        if (l.indexOf('www.') !== -1 && l.length < 80) continue;
        if (/^(声明|本站|本书|下载|更多|百度|手机|网址|用户上传|存储空间|版权|八零电子书|轻小说文库|WenKu|txt80)/i.test(l)) continue;
        if (/^[-=★☆◆◇*]{5,}/.test(l)) continue;
        if (/^(扫图|录入|修图|台版|转自|轻之|仅供|请勿).*[:：]/i.test(l)) continue;
        var isChapter = /^(第[一二三四五六七八九十百千\d]+[章节卷集部]|序章|楔子|序幕|终章|尾声|后记|番外)/i.test(l);
        if (isChapter) {
            if (buf.trim().length > 15) { paras.push(buf.trim()); if (paras.length >= maxParas) break; }
            buf = ''; foundChapter = true; continue;
        }
        if (!foundChapter && l.length < 6) continue;
        if (l.length === 0) {
            if (buf.trim().length > 10) { paras.push(buf.trim()); if (paras.length >= maxParas) break; buf = ''; }
            continue;
        }
        if (buf.length > 0) buf += '\n'; buf += l;
    }
    if (buf.trim().length > 10 && paras.length < maxParas) paras.push(buf.trim());
    return paras.join('\n\n');
}

// Process first 3 novels for quick test
var novelFiles = fs.readdirSync(NOVEL_DIR).filter(function(f) { return f.toLowerCase().endsWith('.txt'); }).slice(0, 3);
var novelDataObj = {};
var novelMetas = [];

for (var i = 0; i < novelFiles.length; i++) {
    var filePath = path.join(NOVEL_DIR, novelFiles[i]);
    var name = novelFiles[i].replace('.txt', '').replace(/_\w+\.(com|cn)/, '').substring(0, 15);
    var text = decodeToUTF8(filePath);
    var content = extractContent(text, MAX_PARAS);
    var id = 'nv' + i;
    var pCount = content ? content.split('\n\n').length : 0;
    novelDataObj[id] = content;
    novelMetas.push({ id: id, title: name + '...', author: '佚名', cat: '玄幻', intro: name + ' - 点击阅读' });
    console.log('[' + (i + 1) + '/3] ' + name + ': ' + pCount + ' paras, ' + (content.length / 1024).toFixed(0) + ' KB');
}

var novelDataJs = 'window.__NV=' + JSON.stringify(novelDataObj) + ';';
fs.writeFileSync(path.join(WWW, 'novels_data.js'), novelDataJs, 'utf-8');
console.log('Wrote novels_data.js: ' + (novelDataJs.length / 1024).toFixed(0) + ' KB');

// Write metadata
var metaJs = 'var NOVELS = ' + JSON.stringify(novelMetas) + ';';
fs.writeFileSync(path.join(WWW, 'test_meta.js'), metaJs, 'utf-8');
console.log('Wrote test_meta.js');
console.log('Done - ready for testing');
