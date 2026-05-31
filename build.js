var fs = require('fs');
var path = require('path');
var execSync = require('child_process').execSync;

var ROOT = __dirname;
var WWW = path.join(ROOT, 'www');
var OUT_EXE = path.join(ROOT, '神奇的小玩意.exe');
var SRC_CS = path.join(ROOT, 'build_temp.cs');

function readText(fp) { return fs.readFileSync(fp, 'utf-8'); }

var cssContent = readText(path.join(WWW, 'css', 'style.css'));
var jsContent = readText(path.join(WWW, 'js', 'app.js'));
var html = readText(path.join(WWW, 'index.html'));

// ==================== Novel Processing ====================
var NOVEL_DIR = path.join(ROOT, '小说');
var MAX_PARAS = 24;

function decodeToUTF8(filePath) {
    var buf = fs.readFileSync(filePath);
    var text = buf.toString('utf-8');
    var invalid = 0;
    for (var i = 0; i < Math.min(text.length, 1000); i++) {
        if (text.charCodeAt(i) === 0xFFFD) invalid++;
    }
    if (invalid < 3) return text;
    // UTF-16LE detection (check BOM or null byte pattern)
    var isUtf16LE = (buf[0] === 0xFF && buf[1] === 0xFE) || (buf[0] === 0xFE && buf[1] === 0xFF);
    if (!isUtf16LE) {
        // Heuristic: if many even-positioned bytes are 0x00, it's likely UTF-16LE without BOM
        var nullCount = 0;
        for (var j = 1; j < Math.min(buf.length, 2000); j += 2) {
            if (buf[j] === 0x00) nullCount++;
        }
        if (nullCount > 10) isUtf16LE = true;
    }
    if (isUtf16LE) {
        try {
            var encoding = (buf[0] === 0xFE && buf[1] === 0xFF) ? 'utf-16be' : 'utf-16le';
            return buf.toString(encoding);
        } catch (e) {}
    }
    // GBK fallback
    var esc = filePath.replace(/\\/g, '\\\\');
    try {
        return execSync(
            'powershell -NoProfile -Command "[Console]::OutputEncoding=[Text.Encoding]::UTF8;$c=[System.Text.Encoding]::Default.GetString([System.IO.File]::ReadAllBytes(\'' + esc + '\'));Write-Output $c"',
            { encoding: 'utf-8', maxBuffer: 50 * 1024 * 1024 }
        );
    } catch (e) { return text; }
}

function cleanTitle(name) {
    var t = name.replace(/\.txt$/i, '');
    t = t.replace(/[（(].*?(txt80|wenku8|www\.|手机).*?[）)]/gi, '');
    t = t.replace(/_[_\w]*\.(com|cn|net|org)/gi, '');
    t = t.replace(/[（(][^）)]*$/, '');
    t = t.trim();
    if (/^[（(][^）)]+[）)]$/.test(t)) t = t.replace(/^[（(]|[）)]$/g, '');
    t = t.replace(/[（(]全[本册卷]?[）)]/g, '');
    if (!t || t.length < 1) t = name.replace(/\.txt$/i, '').substring(0, 20);
    return t.replace(/^\s+|\s+$/g, '');
}

function extractMeta(text) {
    var title = '', author = '', intro = '';
    var lines = text.split(/\r?\n/);
    var cleanLines = [];
    for (var i = 0; i < lines.length; i++) {
        cleanLines.push(lines[i].replace(/^\s+|\s+$/g, ''));
    }
    for (var i = 0; i < Math.min(cleanLines.length, 30); i++) {
        var l = cleanLines[i];
        if (!title && /^书名[：:]\s*(.+)/.test(l)) title = RegExp.$1.trim();
        if (!title && /^《(.+)》/.test(l)) title = RegExp.$1.trim();
        if (!author && /^作者[：:]\s*(.+)/.test(l)) author = RegExp.$1.trim();
    }
    var headerEnd = 0;
    for (var i = 0; i < Math.min(cleanLines.length, 40); i++) {
        var l = cleanLines[i];
        if (/^(第[一二三四五六七八九十百千\d]+[章节卷]|序章|楔子|序幕|引子|前言)/.test(l)) { headerEnd = i + 1; break; }
        if (/^[-=★☆◆◇]{3,}/.test(l)) headerEnd = i + 1;
    }
    for (var i = headerEnd; i < Math.min(cleanLines.length, 100); i++) {
        var l = cleanLines[i];
        if (l.length > 20 && !/^(第[一二三四五六七八九十百千\d]+[章节卷]|作者|书名|文案|简介|扫图|录入|修图|台版|转自|轻之|本文|声明|更多|本书|下载|百度|手机|网址|http)/i.test(l) && !/^[-=★☆◆◇*]{3,}/.test(l) && l.indexOf('www.') === -1 && l.indexOf('txt') === -1) {
            intro = l.substring(0, 100);
            break;
        }
    }
    return { title: title, author: author, intro: intro };
}

function guessCategory(title, text) {
    var combined = title + text.substring(0, 5000);
    var keywords = {
        '玄幻': ['修炼','斗气','魔法','仙','神','魔','妖','魂','大帝','天尊','道','功法','丹药','灵脉','修行','元尊','洪荒','莽荒','吞噬','完美世界','星辰','混沌','造化','圣体','斩神','红月','校花','野怪','进化','大奉打更人','大王饶命','夜的命名术','妖二代','毒奶','第一序列','混血','屠龙','烤鱼','打更','精神','刺客信条'],
        '轻小说': ['异世界','转生','勇者','魔王','精灵','剑','冒险者','公会','迷宫','学园','高中','少女','后宫','恋爱','妹妹','邻座','女主角','路人女主','继母','拖油瓶','前女友','艾莉','俄语','实力至上','游戏人生','约会大作战','零之','Re:','从零开始','为美好的世界','龙王','怕痛','防御力','情色漫画','埃罗芒阿','不起眼','为了女儿','过度谨慎','某科学','超电磁炮','笨蛋','登上舞台','脚灯','灵境','将棋','棋士','小学生','弟子','师傅','Ngnl','NO GAME','No game','充满'],
        '都市': ['都市','校园','总裁','职场','老婆','宠','奶爸','学霸','重生','穿越','系统','签到','低调','追老婆','走向巅峰'],
        '科幻': ['科幻','星际','宇宙','外星','未来','末日','太空','飞船'],
        '悬疑': ['密室','推理','侦探','悬疑','盗墓','失踪'],
        '历史': ['历史','帝国','风云录','王朝','皇上','将军','三国','明朝','清朝','大汉','朱元璋'],
        '游戏': ['网游','游戏','虚拟','玩家','NPC','副本','装备','刺客信条','刺客'],
    };
    var scores = {};
    for (var cat in keywords) {
        scores[cat] = 0;
        var kws = keywords[cat];
        for (var j = 0; j < kws.length; j++) {
            if (combined.indexOf(kws[j]) !== -1) scores[cat] += 1;
        }
    }
    var best = '玄幻', bestScore = 0;
    for (var cat in scores) {
        if (scores[cat] > bestScore) { bestScore = scores[cat]; best = cat; }
    }
    return best;
}

function extractContent(text, maxParas) {
    var lines = text.split(/\r?\n/);
    var paras = [];
    var foundChapter = false;
    var buf = '';

    for (var i = 0; i < lines.length; i++) {
        var l = lines[i].replace(/^[\s\u3000]+|[\s\u3000]+$/g, '');

        if (l.indexOf('www.') !== -1 && l.length < 80) continue;
        if (/^(声明|本站|本书|下载|更多|百度|手机|网址|用户上传|存储空间|版权|八零电子书|轻小说文库|WenKu|txt80)/i.test(l)) continue;
        if (/^[-=★☆◆◇*]{5,}/.test(l)) continue;
        if (/^(扫图|录入|修图|台版|转自|轻之|仅供|请勿).*[:：]/i.test(l)) continue;

        var isChapter = /^(第[一二三四五六七八九十百千\d]+[章节卷集部]|序章|楔子|序幕|终章|尾声|后记|番外|卷\s*[一二三四五六七八九十百千\d]+)/i.test(l);
        var isVolCh = /^[Vv]ol(ume)?[.\s]*\d+/i.test(l) || /^第?[一二三四五六七八九十\d]+[卷册部篇]/i.test(l);

        if (isChapter || isVolCh) {
            if (buf.trim().length > 15) {
                paras.push(buf.trim());
                if (paras.length >= maxParas) break;
            }
            buf = '';
            foundChapter = true;
            continue;
        }

        if (!foundChapter && l.length < 6) continue;

        if (l.length === 0 || /^\s*$/.test(l)) {
            if (buf.trim().length > 10) {
                paras.push(buf.trim());
                if (paras.length >= maxParas) break;
                buf = '';
            }
            continue;
        }

        if (buf.length > 0) buf += '\n';
        buf += l;
    }

    if (buf.trim().length > 10 && paras.length < maxParas) {
        paras.push(buf.trim());
    }

    return paras.join('\n\n');
}

// Process all novels
var novelFiles, novelMetas = [];
try {
    novelFiles = fs.readdirSync(NOVEL_DIR).filter(function(f) { return f.toLowerCase().endsWith('.txt'); });
    console.log('Found ' + novelFiles.length + ' novel files');

    for (var i = 0; i < novelFiles.length; i++) {
        var filePath = path.join(NOVEL_DIR, novelFiles[i]);
        var text = decodeToUTF8(filePath);
        var meta = extractMeta(text);
        var title = meta.title || cleanTitle(novelFiles[i]);
        var author = meta.author || '佚名';
        var intro = meta.intro || title + ' - 点击阅读';
        var category = guessCategory(title, text);
        var id = 'nv' + i;

        novelMetas.push({
            id: id,
            title: title,
            author: author,
            cat: category,
            intro: intro,
            file: novelFiles[i]
        });
        console.log('  [' + (i + 1) + '/' + novelFiles.length + '] ' + title + ' (' + author + ') - ' + category);
    }
} catch (e) {
    console.log('No novel directory or read error, skipping: ' + e.message);
}

// ==================== Generate novel_data.js content ====================
// Only embed empty placeholder - all content comes from NovelBridge at runtime
// novelLoadText will detect empty __NV and trigger bridge loading
var novelDataJs = 'window.__NV={};';
var novelDataBytes = Buffer.from(novelDataJs, 'utf-8');
var novelDataBase64 = novelDataBytes.toString('base64');
console.log('Novel data: embedded placeholder only (' + novelDataBytes.length + ' bytes)');

// ==================== Assemble HTML ====================
var catSet = {};
for (var k = 0; k < novelMetas.length; k++) {
    catSet[novelMetas[k].cat] = true;
}
var allCats = ['全部'];
for (var cat in catSet) { allCats.push(cat); }

var novelMetaJs = JSON.stringify(novelMetas);
var novelCatsJs = JSON.stringify(allCats);

// Replace novel section in JS - use inline data load, no XHR needed
var novelStart = jsContent.indexOf('var NOVELS = [');
var novelEnd = jsContent.indexOf('// ==================== 2048 Game ====================');
if (novelEnd < 0) novelEnd = jsContent.length;

var preNovel = jsContent.substring(0, novelStart);
var postNovel = jsContent.substring(novelEnd);

var novelJs = 'var NOVELS_PER_PAGE = 10;\n';
novelJs += 'var novelFilter = \'\', novelCat = \'全部\', novelCurIdx = -1, novelCurPage = 0;\n';
novelJs += 'var novelCats = ' + novelCatsJs + ';\n';
novelJs += 'var novelParasCache = {};\n';
novelJs += 'var novelTotalParas = {};\n';
novelJs += 'var novelBridgeOK = false;\n';
novelJs += 'var novelBridgeChecked = false;\n';
novelJs += 'var novelLoadingId = null;\n';
novelJs += 'var novelLoadingTimer = null;\n';
novelJs += 'var BUFFER_PAGES = 6;\n';
novelJs += 'var novelPageStarts = {};\n';
novelJs += '\n';
novelJs += 'var NOVELS = ' + novelMetaJs + ';\n';
novelJs += '\n';
novelJs += 'function novelLoadText(id) {\n';
novelJs += '    if (novelParasCache[id] && novelParasCache[id].length > 0) {\n';
novelJs += '        return novelParasCache[id].join(\'\\n\\n\');\n';
novelJs += '    }\n';
novelJs += '    var data = window.__NV || {};\n';
novelJs += '    var text = data[id] || \'\';\n';
novelJs += '    if (text) {\n';
novelJs += '        novelParasCache[id] = text.split(\'\\n\\n\');\n';
novelJs += '        return text;\n';
novelJs += '    }\n';
novelJs += '    return \'\';\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelCheckBridge() {\n';
novelJs += '    if (novelBridgeChecked) return;\n';
novelJs += '    novelBridgeChecked = true;\n';
novelJs += '    try {\n';
novelJs += '        if (typeof window.external !== \'undefined\' && window.external) {\n';
novelJs += '            novelBridgeOK = window.external.IsAvailable();\n';
novelJs += '        }\n';
novelJs += '    } catch(e) {}\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelBridgeLoad(id, start, count) {\n';
novelJs += '    if (!novelBridgeOK) return;\n';
novelJs += '    if (novelCurIdx < 0) return;\n';
novelJs += '    var n = NOVELS[novelCurIdx];\n';
novelJs += '    if (!n || n.id !== id) return;\n';
novelJs += '    var file = n.file;\n';
novelJs += '    if (!file) return;\n';
novelJs += '    if (novelLoadingId === id) return;\n';
novelJs += '    novelLoadingId = id;\n';
novelJs += '    novelLoadingTimer = setTimeout(function() {\n';
novelJs += '        novelLoadingId = null;\n';
novelJs += '    }, 120000);\n';
novelJs += '    var status = \'\';\n';
novelJs += '    try { status = window.external.BeginLoad(file); } catch(e) { return; }\n';
novelJs += '    function doRead() {\n';
novelJs += '        var json = \'\';\n';
novelJs += '        try { json = window.external.ReadParagraphs(file, start, count); } catch(e) {}\n';
novelJs += '        clearTimeout(novelLoadingTimer);\n';
novelJs += '        novelLoadingId = null;\n';
novelJs += '        if (json && json.length > 2) {\n';
novelJs += '            var newParas = [];\n';
novelJs += '            try { newParas = JSON.parse(json); } catch(e) {}\n';
novelJs += '            if (!novelParasCache[id]) novelParasCache[id] = [];\n';
novelJs += '            var arr = novelParasCache[id];\n';
novelJs += '            for (var i = 0; i < newParas.length; i++) {\n';
novelJs += '                if (arr.length <= start + i) arr.push(newParas[i]);\n';
novelJs += '                else arr[start + i] = newParas[i];\n';
novelJs += '            }\n';
novelJs += '            try {\n';
novelJs += '                var tmp = parseInt(window.external.GetTotalParagraphs(file), 10);\n';
novelJs += '                if (tmp > 0) novelTotalParas[id] = tmp;\n';
novelJs += '            } catch(e) {}\n';
novelJs += '            delete novelPageStarts[id];\n';
novelJs += '        }\n';
novelJs += '        if (novelCurIdx >= 0 && NOVELS[novelCurIdx] && NOVELS[novelCurIdx].id === id) {\n';
novelJs += '            renderNovelReader();\n';
novelJs += '        }\n';
novelJs += '    }\n';
novelJs += '    if (status === \'ready\') {\n';
novelJs += '        setTimeout(function() { doRead(); }, 30);\n';
novelJs += '    } else {\n';
novelJs += '        var pollCount = 0;\n';
novelJs += '        var pollId = setInterval(function() {\n';
novelJs += '            pollCount++;\n';
novelJs += '            var cnt = 0;\n';
novelJs += '            try { cnt = parseInt(window.external.GetParagraphCount(file), 10); } catch(e) {}\n';
novelJs += '            if (cnt > 0) {\n';
novelJs += '                clearInterval(pollId);\n';
novelJs += '                doRead();\n';
novelJs += '            } else if (pollCount > 600) {\n';
novelJs += '                clearInterval(pollId);\n';
novelJs += '                clearTimeout(novelLoadingTimer);\n';
novelJs += '                novelLoadingId = null;\n';
novelJs += '            }\n';
novelJs += '        }, 200);\n';
novelJs += '    }\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelEnsureBuffer() {\n';
novelJs += '    if (novelCurIdx < 0) return;\n';
novelJs += '    var n = NOVELS[novelCurIdx];\n';
novelJs += '    if (!n) return;\n';
novelJs += '    var id = n.id;\n';
novelJs += '    novelCheckBridge();\n';
novelJs += '    if (!novelBridgeOK) return;\n';
novelJs += '    if (novelLoadingId === id) return;\n';
novelJs += '    var cache = novelParasCache[id];\n';
novelJs += '    var loaded = cache ? cache.length : 0;\n';
novelJs += '    if (loaded === 0) loaded = (novelLoadText(id).split(\'\\n\\n\')).length;\n';
novelJs += '    var pageEstimate = novelPageStarts[id] ? novelPageStarts[id].length : Math.max(1, Math.floor(loaded / 4));\n';
novelJs += '    if (pageEstimate - novelCurPage < BUFFER_PAGES + 2 && loaded > 0) {\n';
novelJs += '        novelBridgeLoad(id, loaded, 72);\n';
novelJs += '    }\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelGetFiltered() {\n';
novelJs += '    var result = [], i, n;\n';
novelJs += '    for (i = 0; i < NOVELS.length; i++) {\n';
novelJs += '        n = NOVELS[i];\n';
novelJs += '        if (novelCat !== \'全部\' && n.cat !== novelCat) continue;\n';
novelJs += '        if (novelFilter && n.title.indexOf(novelFilter) === -1 && n.author.indexOf(novelFilter) === -1) continue;\n';
novelJs += '        result.push(n);\n';
novelJs += '    }\n';
novelJs += '    return result;\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function showNovelList() {\n';
novelJs += '    $(\'novelShelf\').style.display = \'block\';\n';
novelJs += '    $(\'novelReader\').style.display = \'none\';\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function showNovelReader() {\n';
novelJs += '    $(\'novelShelf\').style.display = \'none\';\n';
novelJs += '    $(\'novelReader\').style.display = \'block\';\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function renderNovelList() {\n';
novelJs += '    var list = novelGetFiltered();\n';
novelJs += '    var totalPages = Math.ceil(list.length / NOVELS_PER_PAGE);\n';
novelJs += '    if (totalPages === 0) totalPages = 1;\n';
novelJs += '    if (novelCurPage >= totalPages) novelCurPage = 0;\n';
novelJs += '    var start = novelCurPage * NOVELS_PER_PAGE, end = Math.min(start + NOVELS_PER_PAGE, list.length);\n';
novelJs += '    var html = \'\';\n';
novelJs += '    for (var i = start; i < end; i++) {\n';
novelJs += '        var n = list[i];\n';
novelJs += '        html += \'<div class="novel-card" onclick="novelOpen(\\\'\' + n.id + \'\\\')">\' +\n';
novelJs += '            \'<div class="novel-card-title">\' + n.title + \'</div>\' +\n';
novelJs += '            \'<div class="novel-card-author">\' + n.author + \' \\u00b7 \' + n.cat + \'</div>\' +\n';
novelJs += '            \'<div class="novel-card-intro">\' + n.intro.substring(0, 60) + \'...</div>\' +\n';
novelJs += '            \'</div>\';\n';
novelJs += '    }\n';
novelJs += '    $(\'novelList\').innerHTML = html || \'<div style="color:#666;padding:20px;">\\u6ca1\\u6709\\u627e\\u5230\\u5339\\u914d\\u7684\\u5c0f\\u8bf4</div>\';\n';
novelJs += '    $(\'novelPageInfo\').innerHTML = (list.length > 0 ? (novelCurPage + 1) + \'/\' + totalPages + \' \\u9875 (\' + list.length + \'\\u672c)\' : \'0\\u672c\');\n';
novelJs += '    $(\'novelBtnPrev\').style.visibility = (novelCurPage > 0 ? \'visible\' : \'hidden\');\n';
novelJs += '    $(\'novelBtnNext\').style.visibility = (novelCurPage < totalPages - 1 ? \'visible\' : \'hidden\');\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function renderNovelCat() {\n';
novelJs += '    var html = \'\';\n';
novelJs += '    for (var i = 0; i < novelCats.length; i++) {\n';
novelJs += '        var sel = (novelCats[i] === novelCat) ? \' active\' : \'\';\n';
novelJs += '        html += \'<span class="novel-cat-tag\' + sel + \'" onclick="novelCat=\\\'\' + novelCats[i] + \'\\\';novelCurPage=0;renderNovelCat();renderNovelList();">\' + novelCats[i] + \'</span>\';\n';
novelJs += '    }\n';
novelJs += '    $(\'novelCats\').innerHTML = html;\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelNextPage() {\n';
novelJs += '    var list = novelGetFiltered();\n';
novelJs += '    var totalPages = Math.ceil(list.length / NOVELS_PER_PAGE);\n';
novelJs += '    if (novelCurPage < totalPages - 1) { novelCurPage++; renderNovelList(); }\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelPrevPage() {\n';
novelJs += '    if (novelCurPage > 0) { novelCurPage--; renderNovelList(); }\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelSearch() {\n';
novelJs += '    var input = $(\'novelSearchInput\');\n';
novelJs += '    novelFilter = input ? input.value : \'\';\n';
novelJs += '    novelCurPage = 0;\n';
novelJs += '    renderNovelList();\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelOpen(id) {\n';
novelJs += '    for (var i = 0; i < NOVELS.length; i++) {\n';
novelJs += '        if (NOVELS[i].id === id) { novelCurIdx = i; break; }\n';
novelJs += '    }\n';
novelJs += '    if (novelCurIdx < 0) return;\n';
novelJs += '    novelCurPage = 0;\n';
novelJs += '    showNovelReader();\n';
novelJs += '    renderNovelReader();\n';
novelJs += '    setTimeout(function() { novelEnsureBuffer(); }, 200);\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelBack() {\n';
novelJs += '    showNovelList();\n';
novelJs += '    novelCurIdx = -1;\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function renderNovelReader() {\n';
novelJs += '    if (novelCurIdx < 0) return;\n';
novelJs += '    var n = NOVELS[novelCurIdx];\n';
novelJs += '    var text = novelLoadText(n.id);\n';
novelJs += '    var paras = text ? text.split(\'\\n\\n\') : [];\n';
novelJs += '    $(\'novelReadTitle\').innerHTML = n.title;\n';
novelJs += '    $(\'novelReadAuthor\').innerHTML = n.author + \' \\u00b7 \' + n.cat;\n';
novelJs += '    if (paras.length === 0) {\n';
novelJs += '        if (novelLoadingId === n.id) {\n';
novelJs += '            $(\'novelReadContent\').innerHTML = \'<div style="color:#999;padding:20px;text-align:center;">\\u6b63\\u5728\\u52a0\\u8f7d...</div>\';\n';
novelJs += '        } else {\n';
novelJs += '            $(\'novelReadContent\').innerHTML = \'<div style="color:#999;padding:20px;text-align:center;">\\u6b63\\u5728\\u52a0\\u8f7d\\u5c0f\\u8bf4\\u5185\\u5bb9\\uff0c\\u8bf7\\u7a0d\\u5019...</div>\';\n';
novelJs += '        }\n';
novelJs += '        $(\'novelReadPageInfo\').innerHTML = \'1/1\';\n';
novelJs += '        return;\n';
novelJs += '    }\n';
novelJs += '    var starts = novelPageStarts[n.id];\n';
novelJs += '    if (!starts) {\n';
novelJs += '        var measureDiv = document.createElement(\'div\');\n';
novelJs += '        measureDiv.className = \'novel-read-content\';\n';
novelJs += '        measureDiv.style.position = \'absolute\';\n';
novelJs += '        measureDiv.style.left = \'-9999px\';\n';
novelJs += '        measureDiv.style.top = \'0\';\n';
novelJs += '        measureDiv.style.height = \'auto\';\n';
novelJs += '        measureDiv.style.visibility = \'hidden\';\n';
novelJs += '        document.body.appendChild(measureDiv);\n';
novelJs += '        var maxHeight = 480;\n';
novelJs += '        starts = [0];\n';
novelJs += '        var buildHtml = \'\';\n';
novelJs += '        var pageStartIdx = 0;\n';
novelJs += '        for (var i = 0; i < paras.length; i++) {\n';
novelJs += '            var testHtml = buildHtml;\n';
novelJs += '            if (paras[i]) testHtml += \'<p class="novel-para">\' + paras[i].replace(/\\n/g, \'<br>\') + \'</p>\';\n';
novelJs += '            measureDiv.innerHTML = testHtml;\n';
novelJs += '            if (measureDiv.offsetHeight > maxHeight) {\n';
novelJs += '                if (i === pageStartIdx) {\n';
novelJs += '                    starts.push(i + 1);\n';
novelJs += '                    pageStartIdx = i + 1;\n';
novelJs += '                    buildHtml = \'\';\n';
novelJs += '                } else {\n';
novelJs += '                    starts.push(i);\n';
novelJs += '                    pageStartIdx = i;\n';
novelJs += '                    buildHtml = \'\';\n';
novelJs += '                    if (paras[i]) buildHtml += \'<p class="novel-para">\' + paras[i].replace(/\\n/g, \'<br>\') + \'</p>\';\n';
novelJs += '                }\n';
novelJs += '            } else {\n';
novelJs += '                buildHtml = testHtml;\n';
novelJs += '            }\n';
novelJs += '        }\n';
novelJs += '        document.body.removeChild(measureDiv);\n';
novelJs += '        novelPageStarts[n.id] = starts;\n';
novelJs += '    }\n';
novelJs += '    var totalPages = starts.length;\n';
novelJs += '    if (novelCurPage >= totalPages) novelCurPage = totalPages - 1;\n';
novelJs += '    var start = starts[novelCurPage];\n';
novelJs += '    var end = (novelCurPage + 1 < starts.length) ? starts[novelCurPage + 1] : paras.length;\n';
novelJs += '    var html = \'\';\n';
novelJs += '    for (var k = start; k < end; k++) {\n';
novelJs += '        if (paras[k]) html += \'<p class="novel-para">\' + paras[k].replace(/\\n/g, \'<br>\') + \'</p>\';\n';
novelJs += '    }\n';
novelJs += '    $(\'novelReadContent\').innerHTML = html;\n';
novelJs += '    $(\'novelReadPageInfo\').innerHTML = (novelCurPage + 1) + \'/\' + totalPages;\n';
novelJs += '    $(\'novelReadPrev\').style.visibility = (novelCurPage > 0 ? \'visible\' : \'hidden\');\n';
novelJs += '    $(\'novelReadNext\').style.visibility = (novelCurPage < totalPages - 1 ? \'visible\' : \'hidden\');\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelReadPrev() {\n';
novelJs += '    if (novelCurPage > 0) { novelCurPage--; renderNovelReader(); novelEnsureBuffer(); }\n';
novelJs += '}\n';
novelJs += '\n';
novelJs += 'function novelReadNext() {\n';
novelJs += '    if (novelCurIdx < 0) return;\n';
novelJs += '    var n = NOVELS[novelCurIdx];\n';
novelJs += '    var starts = novelPageStarts[n.id];\n';
novelJs += '    if (!starts) { renderNovelReader(); return; }\n';
novelJs += '    if (novelCurPage < starts.length - 1) { novelCurPage++; renderNovelReader(); novelEnsureBuffer(); }\n';
novelJs += '}\n';

jsContent = preNovel + novelJs + postNovel;

// ==================== Build final HTML (add novels_data.js script tag) ====================
html = html.replace('<link rel="stylesheet" href="css/style.css">', '<style>' + cssContent + '</style>');
html = html.replace('</head>', '<script src="novels_data.js"></script>\n</head>');
html = html.replace('<script src="js/app.js"></script>', '<script>' + jsContent + '</script>');

function toByteArray(content) {
    var buf = Buffer.from(content, 'utf-8');
    var arr = [];
    for (var i = 0; i < buf.length; i++) arr.push(buf[i]);
    return arr;
}

var htmlBytes = toByteArray(html);

// ==================== Generate C# Source ====================
var cs = '';
cs += 'using System;\r\n';
cs += 'using System.Collections.Generic;\r\n';
cs += 'using System.ComponentModel;\r\n';
cs += 'using System.IO;\r\n';
cs += 'using System.Linq;\r\n';
cs += 'using System.Runtime.InteropServices;\r\n';
cs += 'using System.Text.RegularExpressions;\r\n';
cs += 'using System.Windows.Forms;\r\n';
cs += 'using Microsoft.Win32;\r\n';
cs += '\r\n';
cs += 'public class AppForm : Form\r\n';
cs += '{\r\n';
cs += '    private WebBrowser wb;\r\n';
cs += '    private string tempDir;\r\n';
cs += '\r\n';
cs += '    public AppForm()\r\n';
cs += '    {\r\n';
cs += '        this.Text = "神奇的小玩意 v2.1";\r\n';
cs += '        this.Width = 1050;\r\n';
cs += '        this.Height = 720;\r\n';
cs += '        this.StartPosition = FormStartPosition.CenterScreen;\r\n';
cs += '        this.MinimumSize = new System.Drawing.Size(800, 600);\r\n';
cs += '\r\n';
cs += '        SetBrowserEmulation();\r\n';
cs += '\r\n';
cs += '        wb = new WebBrowser();\r\n';
cs += '        wb.Dock = DockStyle.Fill;\r\n';
cs += '        wb.ScriptErrorsSuppressed = true;\r\n';
cs += '        wb.ScrollBarsEnabled = true;\r\n';
cs += '        wb.IsWebBrowserContextMenuEnabled = false;\r\n';
cs += '        wb.AllowNavigation = true;\r\n';
cs += '        wb.AllowWebBrowserDrop = false;\r\n';
cs += '        wb.NewWindow += new CancelEventHandler(WbNewWindow);\r\n';
cs += '        wb.Navigating += new WebBrowserNavigatingEventHandler(WbNavigating);\r\n';
cs += '        this.Controls.Add(wb);\r\n';
cs += '\r\n';
cs += '        string exeDir = Path.GetDirectoryName(Application.ExecutablePath);\r\n';
cs += '        wb.ObjectForScripting = new NovelBridge(exeDir);\r\n';
cs += '\r\n';
cs += '        tempDir = Path.Combine(Path.GetTempPath(), "MG" + Guid.NewGuid().ToString("N").Substring(0, 8));\r\n';
cs += '        Directory.CreateDirectory(tempDir);\r\n';
cs += '\r\n';
cs += '        string htmlPath = Path.Combine(tempDir, "index.html");\r\n';
cs += '        File.WriteAllBytes(htmlPath, HtmlBytes);\r\n';
cs += '\r\n';
cs += '        string novelDataPath = Path.Combine(tempDir, "novels_data.js");\r\n';
cs += '        File.WriteAllText(novelDataPath, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(NovelsData)));\r\n';
cs += '\r\n';
cs += '        wb.Navigate("file:///" + htmlPath.Replace("\\\\", "/"));\r\n';
cs += '\r\n';
cs += '        this.FormClosing += FormClosingHandler;\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private void WbNavigating(object sender, WebBrowserNavigatingEventArgs e)\r\n';
cs += '    {\r\n';
cs += '        string url = e.Url.ToString();\r\n';
cs += '        if (url.StartsWith("http://") || url.StartsWith("https://"))\r\n';
cs += '        {\r\n';
cs += '            e.Cancel = true;\r\n';
cs += '            try { System.Diagnostics.Process.Start(url); } catch { }\r\n';
cs += '        }\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private void WbNewWindow(object sender, CancelEventArgs e)\r\n';
cs += '    {\r\n';
cs += '        e.Cancel = true;\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private void FormClosingHandler(object sender, FormClosingEventArgs e)\r\n';
cs += '    {\r\n';
cs += '        try { if (tempDir != null && Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private void SetBrowserEmulation()\r\n';
cs += '    {\r\n';
cs += '        try\r\n';
cs += '        {\r\n';
cs += '            string appName = System.AppDomain.CurrentDomain.FriendlyName;\r\n';
cs += '            using (var key = Registry.CurrentUser.CreateSubKey(\r\n';
cs += '                @"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION"))\r\n';
cs += '            {\r\n';
cs += '                key.SetValue(appName, 11001, RegistryValueKind.DWord);\r\n';
cs += '            }\r\n';
cs += '        }\r\n';
cs += '        catch { }\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private static readonly byte[] HtmlBytes = new byte[] { ' + htmlBytes.join(',') + ' };\r\n';
cs += '\r\n';
cs += '    private static readonly string NovelsData = "' + novelDataBase64 + '";\r\n';
cs += '\r\n';
cs += '    [STAThread]\r\n';
cs += '    static void Main()\r\n';
cs += '    {\r\n';
cs += '        Application.EnableVisualStyles();\r\n';
cs += '        Application.SetCompatibleTextRenderingDefault(false);\r\n';
cs += '        Application.Run(new AppForm());\r\n';
cs += '    }\r\n';
cs += '}\r\n';
cs += '\r\n';
cs += '[ComVisible(true)]\r\n';
cs += 'public class NovelBridge\r\n';
cs += '{\r\n';
cs += '    private string novelDir;\r\n';
cs += '    private Dictionary<string, string[]> cache = new Dictionary<string, string[]>();\r\n';
cs += '    private Dictionary<string, int> totalCache = new Dictionary<string, int>();\r\n';
cs += '    private HashSet<string> loadingSet = new HashSet<string>();\r\n';
cs += '    private object lockObj = new object();\r\n';
cs += '\r\n';
cs += '    public NovelBridge(string exeDir)\r\n';
cs += '    {\r\n';
cs += '        novelDir = Path.Combine(exeDir, "小说");\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    public bool IsAvailable()\r\n';
cs += '    {\r\n';
cs += '        return Directory.Exists(novelDir) && Directory.GetFiles(novelDir, "*.txt").Length > 0;\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    public string BeginLoad(string file)\r\n';
cs += '    {\r\n';
cs += '        if (cache.ContainsKey(file)) return "ready";\r\n';
cs += '        lock (lockObj)\r\n';
cs += '        {\r\n';
cs += '            if (loadingSet.Contains(file)) return "loading";\r\n';
cs += '            loadingSet.Add(file);\r\n';
cs += '        }\r\n';
cs += '        System.Threading.ThreadPool.QueueUserWorkItem(_ =>\r\n';
cs += '        {\r\n';
cs += '            try { EnsureIndexed(file); }\r\n';
cs += '            catch { }\r\n';
cs += '            lock (lockObj) { loadingSet.Remove(file); }\r\n';
cs += '        });\r\n';
cs += '        return "loading";\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    public string GetParagraphCount(string file)\r\n';
cs += '    {\r\n';
cs += '        lock (lockObj)\r\n';
cs += '        {\r\n';
cs += '            if (cache.ContainsKey(file)) return cache[file].Length.ToString();\r\n';
cs += '        }\r\n';
cs += '        return "0";\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    public string GetTotalParagraphs(string file)\r\n';
cs += '    {\r\n';
cs += '        lock (lockObj)\r\n';
cs += '        {\r\n';
cs += '            if (totalCache.ContainsKey(file)) return totalCache[file].ToString();\r\n';
cs += '        }\r\n';
cs += '        return "0";\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    public string ReadParagraphs(string file, int start, int count)\r\n';
cs += '    {\r\n';
cs += '        try\r\n';
cs += '        {\r\n';
cs += '            lock (lockObj)\r\n';
cs += '            {\r\n';
cs += '                if (!cache.ContainsKey(file)) return "[]";\r\n';
cs += '            }\r\n';
cs += '            string[] paras;\r\n';
cs += '            lock (lockObj) { paras = cache[file]; }\r\n';
cs += '            int end = Math.Min(start + count, paras.Length);\r\n';
cs += '            var parts = new List<string>();\r\n';
cs += '            for (int i = start; i < end; i++)\r\n';
cs += '            {\r\n';
cs += '                string esc = paras[i]\r\n';
cs += '                    .Replace("\\\\", "\\\\\\\\")\r\n';
cs += '                    .Replace("\\"", "\\\\\\"")\r\n';
cs += '                    .Replace("\\n", "\\\\n")\r\n';
cs += '                    .Replace("\\r", "");\r\n';
cs += '                parts.Add("\\"" + esc + "\\"");\r\n';
cs += '            }\r\n';
cs += '            return "[" + string.Join(",", parts) + "]";\r\n';
cs += '        }\r\n';
cs += '        catch { return "[]"; }\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private void EnsureIndexed(string file)\r\n';
cs += '    {\r\n';
cs += '        lock (lockObj) { if (cache.ContainsKey(file)) return; }\r\n';
cs += '        string path = Path.Combine(novelDir, file);\r\n';
cs += '        if (!File.Exists(path)) return;\r\n';
cs += '        string text = ReadWithEncodingDetection(path);\r\n';
cs += '        var paras = ExtractParagraphs(text);\r\n';
cs += '        lock (lockObj)\r\n';
cs += '        {\r\n';
cs += '            if (!cache.ContainsKey(file))\r\n';
cs += '            {\r\n';
cs += '                cache[file] = paras;\r\n';
cs += '                totalCache[file] = paras.Length;\r\n';
cs += '            }\r\n';
cs += '        }\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private string ReadWithEncodingDetection(string path)\r\n';
cs += '    {\r\n';
cs += '        byte[] bytes = File.ReadAllBytes(path);\r\n';
cs += '        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)\r\n';
cs += '            return System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);\r\n';
cs += '        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)\r\n';
cs += '            return System.Text.Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);\r\n';
cs += '        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)\r\n';
cs += '            return System.Text.Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);\r\n';
cs += '        try { return System.Text.Encoding.UTF8.GetString(bytes); }\r\n';
cs += '        catch { }\r\n';
cs += '        return System.Text.Encoding.Default.GetString(bytes);\r\n';
cs += '    }\r\n';
cs += '\r\n';
cs += '    private string[] ExtractParagraphs(string text)\r\n';
cs += '    {\r\n';
cs += '        var lines = text.Replace("\\r\\n", "\\n").Replace("\\r", "\\n").Split(\'\\n\');\r\n';
cs += '        var paras = new List<string>();\r\n';
cs += '        var buf = "";\r\n';
cs += '        bool foundChapter = false;\r\n';
cs += '        var skipRe = new Regex(@"^(声明|本站|本书|下载|更多|百度|手机|网址|用户上传|存储空间|版权|八零|文库|WenKu|txt80|扫图|录入|修图|台版|转自|轻之|仅供|请勿)");\r\n';
cs += '        var decorRe = new Regex(@"^[-=★☆◆◇*]{5,}");\r\n';
cs += '        var chRe = new Regex(@"^(第[一二三四五六七八九十百千\\d]+[章节卷集部]|序章|楔子|序幕|终章|尾声|后记|番外)");\r\n';
cs += '\r\n';
cs += '        for (int i = 0; i < lines.Length; i++)\r\n';
cs += '        {\r\n';
cs += '            string l = lines[i].Trim();\r\n';
cs += '            if (l.IndexOf("www.") >= 0 && l.Length < 80) continue;\r\n';
cs += '            if (skipRe.IsMatch(l)) continue;\r\n';
cs += '            if (decorRe.IsMatch(l)) continue;\r\n';
cs += '            if (chRe.IsMatch(l))\r\n';
cs += '            {\r\n';
cs += '                if (buf.Trim().Length > 15) paras.Add(buf.Trim());\r\n';
cs += '                buf = ""; foundChapter = true; continue;\r\n';
cs += '            }\r\n';
cs += '            if (!foundChapter && l.Length < 6) continue;\r\n';
cs += '            if (l.Length == 0)\r\n';
cs += '            {\r\n';
cs += '                if (buf.Trim().Length > 10) { paras.Add(buf.Trim()); buf = ""; }\r\n';
cs += '                continue;\r\n';
cs += '            }\r\n';
cs += '            if (buf.Length > 0) buf += "\\n";\r\n';
cs += '            buf += l;\r\n';
cs += '        }\r\n';
cs += '        if (buf.Trim().Length > 10) paras.Add(buf.Trim());\r\n';
cs += '        return paras.ToArray();\r\n';
cs += '    }\r\n';
cs += '}\r\n';
cs += '\r\n';
fs.writeFileSync(SRC_CS, cs, 'utf-8');
console.log('Generated C# source: ' + (cs.length / 1024).toFixed(0) + ' KB, html: ' + htmlBytes.length + ' bytes');

// ==================== Compile ====================
var CSC = 'C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe';
var refDir = 'C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319';
var refs = [
    '/r:"' + refDir + '\\System.dll"',
    '/r:"' + refDir + '\\System.Core.dll"',
    '/r:"' + refDir + '\\System.Windows.Forms.dll"',
    '/r:"' + refDir + '\\System.Drawing.dll"'
].join(' ');

try {
    var cmd = '"' + CSC + '" /target:winexe /platform:anycpu ' + refs + ' /out:"' + OUT_EXE + '" "' + SRC_CS + '"';
    var result = execSync(cmd, { cwd: ROOT, encoding: 'utf-8', stdio: 'pipe', maxBuffer: 1024 * 1024 });
    if (result.stdout) console.log(result.stdout);
    console.log('Build success! -> ' + OUT_EXE);
} catch (e) {
    console.error('Build failed!');
    if (e.stderr) console.error(e.stderr.toString());
    if (e.stdout) console.error(e.stdout.toString());
    process.exit(1);
} finally {
    try { fs.unlinkSync(SRC_CS); } catch (e2) {}
}
