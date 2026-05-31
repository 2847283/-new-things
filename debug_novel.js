var fs = require('fs');
var execSync = require('child_process').execSync;

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
            { encoding: 'utf-8', maxBuffer: 50 * 1024 * 1024 }
        );
    } catch (e) { return text; }
}

var text = decodeToUTF8('f:/东西/智能体/神奇小玩意/小说/从野怪开始进化升级1.txt');
var lines = text.split(/\r?\n/);
console.log('Total lines: ' + lines.length);

// Show first 50 lines with char codes to identify formatting
for (var i = 0; i < Math.min(lines.length, 50); i++) {
    var raw = lines[i];
    var trimmed = raw.replace(/^[\s\u3000]+|[\s\u3000]+$/g, '');
    var charCodes = [];
    for (var j = 0; j < Math.min(raw.length, 5); j++) {
        charCodes.push(raw.charCodeAt(j).toString(16));
    }
    console.log(i + ': codes=[' + charCodes.join(',') + '] len=' + raw.length + ' trimmed=[' + trimmed.substring(0, 60) + ']');
}
