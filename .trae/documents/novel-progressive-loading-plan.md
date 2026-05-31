# 小说阅读渐进加载方案

## 目标

用户在小说阅读器中逐页翻看时，自动从原始 `小说/` 文件夹加载后续内容，实现"可读完整本小说"的体验。加载策略：始终保持当前位置前后共 ~6 页缓冲（72 段），当前页接近边界时自动触发预加载。

---

## 现状分析

### 当前架构

```
build.js 构建时:
  小说/*.txt (350-530MB)
    → decodeToUTF8() 解码
    → extractContent(text, 80) 截取前 80 段
    → JSON.stringify → base64
    → 嵌入 C# NovelsData 字符串
    → C# 编译为 EXE

EXE 运行时:
  C# AppForm() 
    → Convert.FromBase64String(NovelsData)
    → File.WriteAllText("novels_data.js")
    → WebBrowser 加载 index.html
      → <script src="novels_data.js"> 执行
        → window.__NV = {"nv0":"80段内容","nv1":"80段内容",...}
      → JS novelLoadText(id) 读 window.__NV[id]
        → renderNovelReader() 每页12段 → 最多 80/12≈7页
```

### 核心限制

- `MAX_PARAS = 80`：每本小说只有 ~7 页可读
- 完整小说 350-530MB，无法嵌入 EXE
- `window.__NV` 是静态注入的全局变量

### 关键文件

| 文件 | 角色 |
|------|------|
| [build.js](file:///f:/东西/智能体/神奇小玩意/build.js) | 构建脚本：扫描小说、截取段落、生成 C# 源文件 |
| [www/js/app.js](file:///f:/东西/智能体/神奇小玩意/www/js/app.js) | 前端逻辑（build 时被替换的小说函数在 L923-L1052） |
| [www/index.html](file:///f:/东西/智能体/神奇小玩意/www/index.html) | HTML 结构（小说书架 + 阅读器双视图） |
| [www/css/style.css](file:///f:/东西/智能体/神奇小玩意/www/css/style.css) | 样式（小说相关在 L862-L1025） |

---

## 方案设计

### 核心思路：C# ↔ JS 双向桥梁

利用 WebBrowser 控件的 `ObjectForScripting` 机制，让 JS 能调用 C# 方法实时读取原始小说文件。

```
EXE 运行时:
  C# AppForm()
    → NovelBridge 注册为 ObjectForScripting
    → 检测 EXE 同目录下是否存在「小说/」文件夹
    → 写入 novels_data.js（仅少量预览段落 + 元数据）
    → WebBrowser 加载 index.html

JS 阅读时:
  novelOpen(id)
    → 先用 window.__NV 快速显示前 N 页（嵌入的预览）
    → 同时通过 bridge 读取文件的完整段落列表
    → novelEnsureBuffer() 确保当前位置前后有 6 页缓冲
    → 翻页时检查缓冲 → 不足则自动调用 bridge 加载
```

### 数据流

```
用户翻到第 7 页（接近嵌入内容末尾）
  → novelReadNext() → novelCurPage = 7
  → novelEnsureBuffer()
    → 检测到 page 7+6=13 超出已加载范围
    → novelBridgeLoad(id, 72, 72)  // 请求第72-144段
    → window.external.ReadParagraphs(fileName, 72, 72)
      → C# 读取小说 txt 文件
      → 提取指定范围的段落
      → 返回 JSON 字符串数组
    → 存入 novelParasCache[id].push(...)
  → renderNovelReader() 正常渲染第 7 页
```

---

## 实现计划

### 一、build.js 修改

#### 1.1 NOVELS 元数据增加 `file` 字段

在 `novelMetas` 中记录原始文件名，供 C# 运行时定位文件：

```javascript
novelMetas.push({
    id: id,
    title: title,
    author: author,
    cat: category,
    intro: intro,
    file: novelFiles[i]  // ← 新增
});
```

#### 1.2 MAX_PARAS 调整为 24（2 页预览）

嵌入预览段落从 80 降为 24（约 2 页），减少 EXE 体积（因为完整内容走 bridge）：

```javascript
var MAX_PARAS = 24;
```

> 注意：这会使 novels_data.js 从 ~2.7MB 降至 ~800KB，再经 base64 约为 ~1MB。

#### 1.3 替换的 JS 代码块更新

在 [build.js:L241-L371](file:///f:/东西/智能体/神奇小玩意/build.js#L241-L371) 的 `novelJs` 变量中，**完全重新设计小说加载函数**（详见下文三、JS 代码设计）。

---

### 二、C# 代码修改（build.js 模板中的 C# 源码）

#### 2.1 添加 NovelBridge 类

在生成 C# 源码时，在 `AppForm` 类之后追加一个独立的 `NovelBridge` 类：

```csharp
[ComVisible(true)]
public class NovelBridge
{
    private string novelDir;
    private Dictionary<string, string[]> cache = new Dictionary<string, string[]>();
    private Dictionary<string, int> totalCache = new Dictionary<string, int>();

    public NovelBridge(string exeDir)
    {
        novelDir = Path.Combine(exeDir, "小说");
    }

    public bool IsAvailable()
    {
        return Directory.Exists(novelDir) && Directory.GetFiles(novelDir, "*.txt").Length > 0;
    }

    public string GetTotalParagraphs(string file)
    {
        if (totalCache.ContainsKey(file))
            return totalCache[file].ToString();
        try { EnsureIndexed(file); }
        catch { return "0"; }
        return totalCache.ContainsKey(file) ? totalCache[file].ToString() : "0";
    }

    public string ReadParagraphs(string file, int start, int count)
    {
        try
        {
            EnsureIndexed(file);
            if (!cache.ContainsKey(file)) return "[]";
            var paras = cache[file];
            var result = new List<string>();
            for (int i = start; i < Math.Min(start + count, paras.Length); i++)
                result.Add(paras[i]);
            // Return as JSON array with proper escaping
            return "[" + string.Join(",", result.Select(p => 
                "\"" + p.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "") + "\""
            )) + "]";
        }
        catch { return "[]"; }
    }

    private void EnsureIndexed(string file)
    {
        if (cache.ContainsKey(file)) return;
        string path = Path.Combine(novelDir, file);
        if (!File.Exists(path)) return;

        string text = ReadWithEncodingDetection(path);
        var paras = ExtractParagraphs(text);
        cache[file] = paras;
        totalCache[file] = paras.Length;
    }

    private string ReadWithEncodingDetection(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        // Try UTF-8
        try { return System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        // Try UTF-16LE (BOM FF FE)
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return System.Text.Encoding.Unicode.GetString(bytes);
        // Try UTF-16BE (BOM FE FF)
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return System.Text.Encoding.BigEndianUnicode.GetString(bytes);
        // GBK fallback (Encoding.Default on Chinese Windows = GBK)
        return System.Text.Encoding.Default.GetString(bytes);
    }

    private string[] ExtractParagraphs(string text)
    {
        var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        var paras = new List<string>();
        var buf = "";
        bool foundChapter = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines[i].Trim();
            // Skip website headers, decorations, metadata
            if (l.IndexOf("www.") >= 0 && l.Length < 80) continue;
            if (System.Text.RegularExpressions.Regex.IsMatch(l, 
                @"^(声明|本站|本书|下载|更多|百度|手机|网址|用户上传|存储空间|版权|八零|文库|WenKu|txt80|扫图|录入|修图|台版|转自|轻之|仅供|请勿)")) continue;
            if (System.Text.RegularExpressions.Regex.IsMatch(l, @"^[-=★☆◆◇*]{5,}")) continue;
            
            bool isChapter = System.Text.RegularExpressions.Regex.IsMatch(l,
                @"^(第[一二三四五六七八九十百千\d]+[章节卷集部]|序章|楔子|序幕|终章|尾声|后记|番外)");
            if (isChapter)
            {
                if (buf.Trim().Length > 15) { paras.Add(buf.Trim()); }
                buf = ""; foundChapter = true; continue;
            }
            if (!foundChapter && l.Length < 6) continue;
            if (l.Length == 0)
            {
                if (buf.Trim().Length > 10) { paras.Add(buf.Trim()); buf = ""; }
                continue;
            }
            if (buf.Length > 0) buf += "\n";
            buf += l;
        }
        if (buf.Trim().Length > 10) paras.Add(buf.Trim());
        return paras.ToArray();
    }
}
```

#### 2.2 在 AppForm 构造函数中注册 bridge

在 `wb.Navigate(...)` 之前添加：

```csharp
string exeDir = Path.GetDirectoryName(Application.ExecutablePath);
var novelBridge = new NovelBridge(exeDir);
wb.ObjectForScripting = novelBridge;
```

#### 2.3 添加 System.Core 引用

C# 源码中使用的 `System.Text.RegularExpressions` 和 `System.Linq` 需要 `System.Core.dll`：

在 build.js 的 refs 中添加：
```javascript
'/r:"' + refDir + '\\System.Core.dll"'
```

---

### 三、JS 代码设计（build.js 中的 novelJs 变量替换内容）

#### 3.1 新增状态变量

```javascript
var novelParasCache = {};     // {id: ['段落1','段落2',...]}
var novelTotalParas = {};     // {id: 总段落数}
var novelBridgeOK = false;    // bridge 是否可用
var novelLoadingId = null;    // 正在异步加载的小说ID
var novelLoadingTimer = null; // 加载超时 timer
var BUFFER_PAGES = 6;         // 缓冲页数
var LINES_PER_PAGE = 12;      // 每页段落数
```

#### 3.2 novelLoadText 改造

不再只从 `window.__NV` 读取，改为分层获取：

```javascript
function novelLoadText(id) {
    // 1. 先从缓存合并（window.__NV 初始内容 + bridge 加载的后续段落）
    if (novelParasCache[id] && novelParasCache[id].length > 0) {
        return novelParasCache[id].join('\n\n');
    }
    // 2. 从 window.__NV 初始化缓存
    var data = window.__NV || {};
    var text = data[id] || '';
    if (text) {
        novelParasCache[id] = text.split('\n\n');
        return text;
    }
    return '';
}
```

#### 3.3 novelBridgeLoad — 从 C# bridge 加载段落

```javascript
function novelBridgeLoad(id, start, count, callback) {
    if (!novelBridgeOK) { if (callback) callback(); return; }
    var n = NOVELS[novelCurIdx];
    var file = n.file;
    if (!file) { if (callback) callback(); return; }
    
    novelLoadingId = id;
    novelLoadingTimer = setTimeout(function() {
        novelLoadingId = null;
        if (callback) callback();
    }, 10000);
    
    setTimeout(function() {
        var json = '';
        try {
            json = window.external.ReadParagraphs(file, start, count);
        } catch(e) {}
        clearTimeout(novelLoadingTimer);
        novelLoadingId = null;
        
        if (json && json.length > 2) {
            var newParas = JSON.parse(json);
            if (!novelParasCache[id]) novelParasCache[id] = [];
            // Merge into cache at correct position
            var arr = novelParasCache[id];
            for (var i = 0; i < newParas.length; i++) {
                arr[start + i] = newParas[i];
            }
            // Get total from bridge
            try {
                var total = parseInt(window.external.GetTotalParagraphs(file), 10);
                if (total > 0) novelTotalParas[id] = total;
            } catch(e) {}
        }
        if (callback) callback();
        // Re-render if needed
        if (novelCurIdx >= 0 && NOVELS[novelCurIdx].id === id) {
            renderNovelReader();
        }
    }, 10);
}
```

#### 3.4 novelEnsureBuffer — 自动预加载

```javascript
function novelEnsureBuffer() {
    if (novelCurIdx < 0) return;
    var n = NOVELS[novelCurIdx];
    var id = n.id;
    
    // Check bridge availability once
    if (novelBridgeOK === false && typeof window.external !== 'undefined') {
        try { novelBridgeOK = window.external.IsAvailable(); } catch(e) {}
    }
    if (!novelBridgeOK) return;
    if (novelLoadingId === id) return; // already loading
    
    var cache = novelParasCache[id];
    if (!cache) cache = [];
    var loaded = cache.length;
    if (loaded === 0) loaded = (novelLoadText(id).split('\n\n')).length;
    
    var needStart = novelCurPage * LINES_PER_PAGE - BUFFER_PAGES * LINES_PER_PAGE;
    var needEnd = (novelCurPage + 1 + BUFFER_PAGES) * LINES_PER_PAGE;
    
    // Check if we need to load behind
    if (needEnd > loaded) {
        // Load next batch starting from loaded
        novelBridgeLoad(id, loaded, BUFFER_PAGES * LINES_PER_PAGE);
    }
}
```

#### 3.5 novelOpen 改造

打开小说时立即初始化缓存并触发预加载：

```javascript
function novelOpen(id) {
    for (var i = 0; i < NOVELS.length; i++) {
        if (NOVELS[i].id === id) { novelCurIdx = i; break; }
    }
    if (novelCurIdx < 0) return;
    novelCurPage = 0;
    showNovelReader();
    // 初始化缓存
    novelLoadText(NOVELS[novelCurIdx].id);
    renderNovelReader();
    // 触发预加载
    novelEnsureBuffer();
}
```

#### 3.6 翻页函数改造

```javascript
function novelReadNext() {
    if (novelCurIdx < 0) return;
    var n = NOVELS[novelCurIdx];
    var totalP = novelTotalParas[n.id] || (novelLoadText(n.id).split('\n\n')).length;
    var totalPages = Math.max(1, Math.ceil(totalP / LINES_PER_PAGE));
    if (novelCurPage < totalPages - 1) {
        novelCurPage++;
        renderNovelReader();
        novelEnsureBuffer();
    }
}

function novelReadPrev() {
    if (novelCurPage > 0) {
        novelCurPage--;
        renderNovelReader();
        novelEnsureBuffer();
    }
}
```

#### 3.7 renderNovelReader 改造

- 使用 `novelTotalParas[id]` 计算总页数（bridge 加载后会更精确）
- 如果正在加载且已超出缓存范围，显示"正在加载..."指示器
- 总页数显示变更：已知完整页数时显示 "+" 标记

```javascript
function renderNovelReader() {
    if (novelCurIdx < 0) return;
    var n = NOVELS[novelCurIdx];
    var text = novelLoadText(n.id);
    var paras = text ? text.split('\n\n') : [];
    var totalP = novelTotalParas[n.id] || paras.length;
    var totalPages = Math.max(1, Math.ceil(totalP / LINES_PER_PAGE));
    
    // 标题/作者不变
    $('novelReadTitle').innerHTML = n.title;
    $('novelReadAuthor').innerHTML = n.author + ' · ' + n.cat;
    
    if (paras.length === 0) {
        $('novelReadContent').innerHTML = '<div style="color:#666;padding:20px;text-align:center;">无法加载小说内容</div>';
        return;
    }
    
    // 渲染
    if (novelCurPage >= totalPages) novelCurPage = totalPages - 1;
    var start = novelCurPage * LINES_PER_PAGE;
    var end = Math.min(start + LINES_PER_PAGE, paras.length);
    var html = '';
    for (var i = start; i < end; i++) {
        if (paras[i]) {
            html += '<p class="novel-para">' + paras[i].replace(/\n/g, '<br>') + '</p>';
        }
    }
    $('novelReadContent').innerHTML = html;
    
    // 页码显示
    var pageLabel = (novelCurPage + 1) + '/' + totalPages;
    $('novelReadPageInfo').innerHTML = pageLabel;
    $('novelReadPrev').style.visibility = (novelCurPage > 0 ? 'visible' : 'hidden');
    $('novelReadNext').style.visibility = (novelCurPage < totalPages - 1 ? 'visible' : 'hidden');
}
```

#### 3.8 新增小说文件夹不存在时的处理

```javascript
// 在 novelEnsureBuffer 中：如果 bridge 不可用，
// totalPages 只基于嵌入内容计算（向后兼容）
if (!novelBridgeOK) {
    // use only embedded content, bridge not available
    return;
}
```

---

### 四、CSS 修改

无需额外修改。现有的 `.novel-read-content`、`.novel-para`、`.novel-pager` 样式已经足够。

---

## 验证步骤

1. **EXE 同目录有小说文件夹时**：
   - 打开任意小说 → 应显示前 2 页（嵌入预览）+ 自动加载后续内容
   - 翻到第 3 页 → 自动触发 bridge 加载第 3-8 页
   - 连续翻页 → 始终流畅，无"到底了"的假象
   - 查看总页数 → 应是基于完整文件的实际页数（如 1500 页而非 7 页）

2. **EXE 同目录无小说文件夹时**：
   - bridge 不可用，仅显示嵌入的预览内容（2 页）
   - 不报错，正常降级

3. **编码兼容性**：
   - 测试 UTF-8 小说（如"完美世界"）→ 正常显示中文
   - 测试 UTF-16LE 小说（如"从野怪开始进化升级"）→ 正常显示
   - 测试 GBK 小说 → 正常显示

4. **编译验证**：
   - `node build.js` 成功 → 生成 EXE
   - EXE 大小接近之前（17MB），因为只是换了加载方式

---

## 风险与缓解

| 风险 | 缓解 |
|------|------|
| 大文件段落提取慢（374K行） | 只在首次访问时提取，之后缓存；用 `setTimeout` 异步化避免阻塞 UI |
| bridge 跨线程调用 | `ObjectForScripting` 在 WebBrowser 中天然支持，无需额外处理 |
| JSON 序列化特殊字符 | C# 端对 `\`, `"`, `\n` 做转义，JS 端 `JSON.parse()` 解回 |
| 中文路径/文件名 | 全部使用 .NET 原生路径处理，已验证 Unicode 支持 |
