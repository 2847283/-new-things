var fs = require('fs');
var path = require('path');

var srcDir = path.join(__dirname, 'www');
var dstDir = path.join(__dirname, 'test_env');

// Copy CSS
var cssDir = path.join(dstDir, 'css');
if (!fs.existsSync(cssDir)) fs.mkdirSync(cssDir, { recursive: true });
fs.copyFileSync(path.join(srcDir, 'css', 'style.css'), path.join(cssDir, 'style.css'));

// Copy JS
var jsDir = path.join(dstDir, 'js');
if (!fs.existsSync(jsDir)) fs.mkdirSync(jsDir, { recursive: true });
fs.copyFileSync(path.join(srcDir, 'js', 'app.js'), path.join(jsDir, 'app.js'));

// Copy novels_data.js
fs.copyFileSync(path.join(srcDir, 'novels_data.js'), path.join(dstDir, 'novels_data.js'));

// Read and modify HTML
var html = fs.readFileSync(path.join(srcDir, 'index.html'), 'utf-8');
// Add novels_data.js before </head>
html = html.replace('</head>', '  <script src="novels_data.js"></script>\n</head>');
// Keep original JS reference (it has the 10-book NOVELS which is fine for UI, and novelLoadText will use window.__NV)
fs.writeFileSync(path.join(dstDir, 'index.html'), html, 'utf-8');

// Verify
var files = fs.readdirSync(dstDir);
console.log('Test env files: ' + files.join(', '));
console.log('novels_data.js exists: ' + fs.existsSync(path.join(dstDir, 'novels_data.js')));
console.log('Done');
