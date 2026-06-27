# 火柴冠军赛 Fire Champion

Unity 6000.4.10f1 2D 原型，实现了《火柴冠军赛》策划案的核心可玩闭环。

## 打开与运行

1. 用 Unity Hub 添加并打开本目录。
2. 打开 `Assets/Scenes/FireChampion.unity`。
3. 点击 Play。场景会通过运行时 Bootstrap 自动生成游戏。

## 打包 Windows EXE

在 Unity 顶部菜单选择：

`Fire Champion > Build Windows EXE`

输出路径：

`..\outputs\FireChampionWindows\FireChampion.exe`

Windows 构建会默认使用 Direct3D 11、1280x720 窗口模式启动，以避开部分 Unity 6000 + Direct3D 12 + 显卡驱动组合的启动闪退。

当前交付包：

- 玩家包：`..\outputs\FireChampion-Windows-YYYYMMDD-HHMM.zip`
- Unity 源工程包：`..\outputs\FireChampion-UnityProject-YYYYMMDD-HHMM.zip`

## 默认键位

P1:
- A / D：移动
- W：跳跃/上挑
- S：下压/扣杀倾向
- F：挥拍/发球
- G：技能

P2:
- 左 / 右：移动
- 上：跳跃/上挑
- 下：下压/扣杀倾向
- K：挥拍/发球
- L：技能

键位可在游戏内“设置”页面重新绑定。

## 已实现内容

- 快速比赛 vs AI、本地双人、锦标赛、自由沙盒练习、交互教程、好友直连/局域网 IP。
- 7 分制、领先 2 分、11 分封顶、界外、触网、落地判分。
- 方向+时机击球，包含高远球、平抽/挑球、扣杀、技巧型吊球，并带短挥拍命中窗口和输入缓冲。
- 3 段能量：1 段小技能，满 3 段可强化扣杀或释放角色强化技。
- 4 个角色：CORE、DASH、HEAVY、TRICK，主菜单会显示角色说明、打法建议、被动、小技能、满能量技能、能力差异和速度/扣杀/控球/容错能力条。
- 角色定位、优势、短板、适合玩家、核心能力数值，以及规则、AI、联机、球场物理、沙盒、能量、技能、挥拍、发球、主要球路手感、音效音量/兜底合成音提示、震屏/横幅反馈、VFX 时长/半径/事件上限和程序化 2D 视觉比例已放入 `Assets/Resources/FireChampion/Data/fire_champion_balance.json`，方便后续平衡性和表现调整。
- 四个角色现在也有数据驱动的外观轮廓差异：DASH 更轻，HEAVY 更壮，TRICK 更灵巧，CORE 更均衡；球场线条、球拍、挥拍轨迹和技能光环尺寸也可在 `gameplay.visuals` 中调整。
- 素材来源与后续导入规范记录在 `Assets/Resources/FireChampion/ASSET_SOURCES.md`；当前已有项目内自绘透明 PNG VFX 和生成 WAV 音效，后续球场背景、角色参考图和高精度音效 brief 记录在 `docs/ArtDirectionAndAssetBriefs-20260619.md`。
- 3 个球场：道场、屋顶、未来场，支持可预读环境扰动和代码绘制的主题背景。
- 自由沙盒包含落点目标、命中率、连续命中、目标刷新、击球手感评分、甜区率、最近击球时间线和随档案保存的长期练习历史，用于基础手感练习。
- 交互教程包含进度提示、P1 键位实时高亮、场上目标标记、停滞提示和失败后的恢复建议。
- 锦标赛包含三轮具名 AI 对手、完整赛程面板、轮次提示、对手风格说明、逐轮难度、只记录荣誉/外观的奖牌奖励、随档案保存的锦标赛历史、中断后继续当前轮次的入口，以及重开活动锦标赛前的确认提示。
- 外观预留已存在：P1/P2 可选择视觉配色，外观目录来自 `fire_champion_balance.json` 的 `cosmetics`，只改变颜色表现，不修改强度数值。
- AI 长期记录玩家常用球路，并用本地档案保存。
- 本地档案保存昵称、键位、设置、战绩、徽章与 AI 习惯。Windows 构建会把这些记录写到 EXE 同目录下的 `FireChampionRecords`，不使用 C 盘 LocalLow 作为游戏档案目录。
- 主菜单会显示档案目录和最近一次档案读取/保存状态。
- 直连联机：主机监听端口，客机输入 IP 和端口加入；连接前会停在等待界面，主机向客机同步比分、局分、结算状态、角色、外观、球场、规则和球旋转。
- 联机菜单和对局内会显示连接诊断，包括输入/快照收发计数、最近收发时间、RTT 延迟、Ping/Pong 计数和静默超时提示。
- 直连联机运行时代码已拆到 `Assets/Scripts/DirectIpSession.cs`，主游戏脚本保留调用和界面逻辑。
- 运行时枚举、规则配置、角色/球场/外观配置、玩家状态、输入包、比赛统计和网络快照已拆到 `Assets/Scripts/FireChampionRuntimeTypes.cs`。
- 键盘输入读取、教程按键高亮状态和键位重绑定赋值已拆到 `Assets/Scripts/FireChampionInputMapper.cs`。
- 胜局判断、几局几胜和发球犯规判定已拆到 `Assets/Scripts/FireChampionMatchRules.cs`。
- AI 输入决策、目标站位、落点预测、习惯适应提示和挥拍/技能选择已拆到 `Assets/Scripts/FireChampionAiController.cs`。
- AI 脚本化验证已拆到 `Assets/Scripts/FireChampionAiSimulation.cs`，覆盖四角色、三球场和典型球路，用于检查 AI 移动、挥拍、跳跃、技能、习惯适应和目标点边界。
- 主菜单和比赛叠层布局计算已拆到 `Assets/Scripts/FireChampionLayout.cs`，窄窗口会隐藏右侧角色说明卡；主菜单标题/Logo 固定在首屏，左侧内容支持纵向滚动，避免 960x540、1024x576 和 1280x720 等窗口高度下按钮被裁掉或首屏像被卷走。比赛中的练习、教程、锦标赛、联机、暂停和结算面板也使用集中布局；这些布局已通过 Unity batchmode 验证并进入最新正式玩家包。
- IMGUI 基础绘制原语已拆到 `Assets/Scripts/FireChampionGuiDrawing.cs`，主游戏脚本只保留薄包装调用，方便后续继续抽离世界渲染和美术表现。
- 音效资源加载已拆到 `Assets/Scripts/FireChampionAudioAssets.cs`，优先播放 `Assets/Resources/FireChampion/Audio` 下的 WAV；资源缺失时回退到 `ToneAudioPlayer` 的合成音。
- 比赛中的横幅、等待联机、暂停和结算叠层渲染已拆到 `Assets/Scripts/FireChampionOverlayRenderer.cs`，通过 action enum 把按钮点击结果交回主游戏脚本处理，不直接修改比赛状态。
- 比赛世界渲染已开始拆到 `Assets/Scripts/FireChampionMatchRenderer.cs`：球场背景、场地线、球网、练习目标、教程标记、羽毛球、角色绘制和世界坐标换算都通过渲染 context 读取状态，不负责修改比赛流程；球拍命中位置仍由主玩法脚本计算并传入渲染器，避免影响击球判定。
- 比赛 HUD 渲染已拆到 `Assets/Scripts/FireChampionHudRenderer.cs`：比分板、角色名、场地/局分说明、网络状态和能量条都通过 HUD context 读取状态，不直接改动比分、能量或网络流程。
- 击球、扣杀、得分和技能触发的 VFX 已拆到 `Assets/Scripts/FireChampionVfxSystem.cs`，透明 PNG 资源由 `Assets/Scripts/FireChampionVfxAssets.cs` 加载，旧程序化绘制继续作为 fallback；它只读取反馈表现参数，不改变碰撞、得分或能量逻辑。
- 比赛结束后的胜负、战绩、锦标赛奖牌/续赛状态、徽章和结算文案已拆到 `Assets/Scripts/FireChampionMatchFlow.cs`。
- Unity 编辑器菜单 `Fire Champion > Validate Project` 可检查启动场景、平衡数据、UI 资源、角色/球场配置、基础赛制规则、锦标赛续赛档案状态、AI 控制器基础行为、AI 脚本模拟和比赛结算流程；Windows 构建前会自动运行该验证。

## 差距分析

当前差距与后续建议记录在：

`docs/GapAnalysis-20260618.md`

当前状态审计和下一步优先级记录在：

`docs/CurrentGapAudit-20260619.md`

## 开发 QA 启动参数

源码中提供了一个仅供开发/截图 QA 使用的启动参数，不会在普通双击启动时生效：

`-firechampion-qa-screen <screen>`

截图自动化还可以叠加：

`-firechampion-qa-capture <png-path> -firechampion-qa-capture-delay <seconds> -firechampion-qa-exit-after-capture`

结算和球场截图 QA 还可以叠加：

`-firechampion-qa-summary <win|loss|tournament-final|network-client> -firechampion-qa-court <dojo|rooftop|future>`

可用值：

- `settings`
- `network`
- `practice`
- `tutorial`
- `tournament`
- `pause`
- `summary`
- `network-waiting`
- `vfx` / `vfx-preview`
- `quick` / `quick-ai`
- `autoplay` / `auto-match` / `long-run`

这个入口用于自动截图和非主菜单界面布局检查，已用于验证设置、联机、练习、教程、锦标赛、暂停、结算、联机等待界面、PNG VFX 预览、快速比赛自动发球烟测和自动对局长运行烟测。QA 启动会阻止测试过程写入正式本地档案；当前最新正式玩家包以 `..\outputs\LATEST_BUILD.txt` 中列出的已验证包为准。

## 联机说明

首版为直接 IP 模式：
- 同一局域网下，客机输入主机局域网 IP。
- 默认端口为 `27777`。
- 主机可在 Windows 终端运行 `ipconfig`，查看当前网络适配器的 IPv4 地址。
- 若跨公网，需要自行处理端口转发或防火墙放行。

当前网络模型是主机模拟并向客机同步快照，优先响应与原型可玩性；后续可替换为中继或权威服务器。
