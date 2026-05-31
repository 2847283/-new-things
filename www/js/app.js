// ==================== Utility ====================
function $(id) { return document.getElementById(id); }

function elCls(el, name) { return el.className.indexOf(name) >= 0; }
function addCls(el, name) { if (!elCls(el, name)) el.className += ' ' + name; }
function remCls(el, name) { el.className = el.className.replace(new RegExp(' ?\\b' + name + '\\b', 'g'), ''); }
function setCls(el, name, on) { if (on) addCls(el, name); else remCls(el, name); }

// ==================== Navigation ====================
var PAGE_IDS = ['page_hub','page_div','page_tarot','page_mine','page_roast','page_novel','page_games','page_2048','page_snake','page_tetris','page_doodle','page_c4','page_pacman'];

function hideAll() {
    for (var i = 0; i < PAGE_IDS.length; i++) {
        remCls($(PAGE_IDS[i]), 'active');
    }
}

function goHub() {
    hideAll();
    addCls($('page_hub'), 'active');
    mineStopTimer();
    if (snakeTimer) clearInterval(snakeTimer);
    if (tetrisTimer) clearInterval(tetrisTimer);
    if (doodleAnimId) cancelAnimationFrame(doodleAnimId);
    if (pmAnimId) cancelAnimationFrame(pmAnimId);
    window._keyCB = null;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (e.keyCode === 27) goHub();
    };
}

function goDivination() {
    hideAll();
    addCls($('page_div'), 'active');
    $('resBox').style.display = 'none';
    $('hz').innerHTML = '☯';
    $('hn').innerHTML = '点击下方按钮开始算卦';
    $('hb').innerHTML = '------';
    $('btnGo').innerHTML = '算一卦';
    isBusy = false;
}

function goTarot() {
    hideAll();
    addCls($('page_tarot'), 'active');
    $('tarotDescBox').style.display = 'none';
    $('tarotEmoji').innerHTML = '🃏';
    $('tarotName').innerHTML = '点击下方按钮抽取塔罗牌';
    $('tarotEn').innerHTML = '--------';
    $('tarotPosWrap').innerHTML = '';
    $('btnTarot').innerHTML = '抽一张牌';
    tarotBusy = false;
}

function goMine() {
    hideAll();
    addCls($('page_mine'), 'active');
    $('gridHex').style.display = 'none';
    $('gridClassic').style.display = 'block';
    addCls($('modeBtnClassic'), 'active');
    remCls($('modeBtnHex'), 'active');
    mineMode = 'classic';
    mineStopTimer();
    buildClassic();
}

function goRoast() {
    hideAll();
    addCls($('page_roast'), 'active');
    $('roastText').innerHTML = '准备好了吗？';
    $('btnRoast').innerHTML = '来一段';
}

function goNovelList() {
    hideAll();
    addCls($('page_novel'), 'active');
    novelPage = 0;
    novelFilter = '';
    novelCat = '全部';
    renderNovelList();
    showNovelList();
}

function goNovel() {
    goNovelList();
}

function goGames() {
    hideAll();
    addCls($('page_games'), 'active');
    if (snakeTimer) clearInterval(snakeTimer);
    if (tetrisTimer) clearInterval(tetrisTimer);
    window._keyCB = null;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (e.keyCode === 27) goHub();
    };
    renderGamesHall();
}

function go2048() {
    hideAll();
    addCls($('page_2048'), 'active');
    g2048Init();
}

function goSnake() {
    hideAll();
    addCls($('page_snake'), 'active');
    snakeInit();
}

function goTetris() {
    hideAll();
    addCls($('page_tetris'), 'active');
    tetrisInit();
}

function goDoodle() {
    hideAll();
    addCls($('page_doodle'), 'active');
    doodleInit();
}

function goC4() {
    hideAll();
    addCls($('page_c4'), 'active');
    c4Init();
}

function goPacman() {
    hideAll();
    addCls($('page_pacman'), 'active');
    pmInit();
}

document.onkeydown = function(e) {
    e = e || window.event;
    if (e.keyCode === 27) goHub();
};

// ==================== Games Hall ====================
var localGames = [
    {name:'2048', icon:'🔲', desc:'经典数字合并，方向键操作', fn:'go2048'},
    {name:'贪吃蛇', icon:'🐍', desc:'经典贪吃蛇，方向键操作', fn:'goSnake'},
    {name:'俄罗斯方块', icon:'🧱', desc:'经典俄罗斯方块，方向键操作', fn:'goTetris'},
    {name:'涂鸦跳跃', icon:'🦘', desc:'踩平台向上跳，← → 键移动', fn:'goDoodle'},
    {name:'扫雷', icon:'💣', desc:'经典扫雷 + 六边形扫雷，可自定义难度', fn:'goMine'},
    {name:'四子棋', icon:'🔴', desc:'双人对战，先连四子胜', fn:'goC4'},
    {name:'吃豆人', icon:'👻', desc:'躲避幽灵吃光豆子，← → ↑ ↓ 移动', fn:'goPacman'}
];

function renderGamesHall() {
    var parts = [];
    for (var i = 0; i < localGames.length; i++) {
        var g = localGames[i];
        parts.push('<div class="game-card"><div class="game-card-icon">' + g.icon + '</div><div class="game-card-name">' + g.name + '</div><div class="game-card-desc">' + g.desc + '</div><span class="game-card-btn" onclick="' + g.fn + '()">▶ 开始游戏</span></div>');
    }
    $('gamesGridLocal').innerHTML = parts.join('');
}

// ==================== Safe Storage ====================
function safeGet(key) {
    try { var v = localStorage.getItem(key); return v; } catch(e) { return null; }
}
function safeSet(key, val) {
    try { localStorage.setItem(key, val); } catch(e) {}
}

// ==================== Zhouyi Divination ====================
var hexList = [
    {n:'乾',s:'☰',b:'111111',j:'元亨，利贞。',i:'天行健，君子以自强不息。',l:['初九：潜龙，勿用。','九二：见龙在田，利见大人。','九三：君子终日乾乾，夕惕若厉，无咎。','九四：或跃在渊，无咎。','九五：飞龙在天，利见大人。','上九：亢龙，有悔。'],t:'乾卦代表天，象征刚健、进取。此卦预示事业顺利，前途光明。'},
    {n:'坤',s:'☷',b:'000000',j:'元亨，利牝马之贞。',i:'地势坤，君子以厚德载物。',l:['初六：履霜，坚冰至。','六二：直方大，不习无不利。','六三：含章可贞，或从王事。','六四：括囊，无咎无誉。','六五：黄裳，元吉。','上六：龙战于野，其血玄黄。'],t:'坤卦代表地，象征柔顺、包容。以柔克刚，厚积薄发。'},
    {n:'屯',s:'☳☷',b:'100010',j:'元亨，利贞。勿用有攸往。',i:'云雷，屯。君子以经纶。',l:['初九：磐桓，利居贞。','六二：屯如邅如，乘马班如。','六三：即鹿无虞，惟入于林中。','六四：乘马班如，求婚媾。','九五：屯其膏，小贞吉。','上六：乘马班如，泣血涟如。'],t:'屯卦象征初生与萌芽。事物初创阶段充满艰难险阻，坚守正道终能克服。'},
    {n:'蒙',s:'☶☵',b:'010001',j:'亨。匪我求童蒙，童蒙求我。',i:'山下出泉，蒙。君子以果行育德。',l:['初六：发蒙，利用刑人。','九二：包蒙，吉。','六三：勿用取女。','六四：困蒙，吝。','六五：童蒙，吉。','上九：击蒙，不利为寇。'],t:'蒙卦象征启蒙与教育。学习成长阶段，需虚心求教，尊师重道。'},
    {n:'需',s:'☵☰',b:'111010',j:'有孚，光亨，贞吉。',i:'云上于天，需。君子以饮食宴乐。',l:['初九：需于郊，利用恒。','九二：需于沙，小有言。','九三：需于泥，致寇至。','六四：需于血，出自穴。','九五：需于酒食，贞吉。','上六：入于穴，有不速之客三人来。'],t:'需卦象征等待。时机未成熟，需耐心等待。'},
    {n:'讼',s:'☰☵',b:'010111',j:'有孚窒惕，中吉，终凶。',i:'天与水违行，讼。君子以作事谋始。',l:['初六：不永所事，小有言。','九二：不克讼，归而逋。','六三：食旧德，贞厉。','九四：不克讼，复即命。','九五：讼，元吉。','上九：或锡之鞶带，终朝三褫之。'],t:'讼卦象征争端。可能面临纠纷，尽量避免争执。'},
    {n:'师',s:'☷☵',b:'010000',j:'贞，丈人吉，无咎。',i:'地中有水，师。君子以容民畜众。',l:['初六：师出以律，否臧凶。','九二：在师中，吉。','六三：师或舆尸，凶。','六四：师左次，无咎。','六五：田有禽，利执言。','上六：大君有命，开国承家。'],t:'师卦象征军队。领导和团队协作非常重要。'},
    {n:'比',s:'☵☷',b:'000010',j:'吉。原筮，元永贞。',i:'地上有水，比。先王以建万国。',l:['初六：有孚比之，无咎。','六二：比之自内，贞吉。','六三：比之匪人。','六四：外比之，贞吉。','九五：显比，王用三驱。','上六：比之无首，凶。'],t:'比卦象征亲近与辅佐。人际关系与合作至关重要。'},
    {n:'小畜',s:'☴☰',b:'111011',j:'亨。密云不雨。',i:'风行天上，小畜。君子以懿文德。',l:['初九：复自道，何其咎。','九二：牵复，吉。','九三：舆说辐，夫妻反目。','六四：有孚，血去惕出。','九五：有孚挛如，富以其邻。','上九：既雨既处，尚德载。'],t:'小畜卦象征小积蓄。积累阶段，需继续努力厚积薄发。'},
    {n:'履',s:'☰☱',b:'110111',j:'履虎尾，不咥人，亨。',i:'上天下泽，履。君子以辨上下。',l:['初九：素履往，无咎。','九二：履道坦坦，幽人贞吉。','六三：眇能视，跛能履。','九四：履虎尾，愬愬。','九五：夬履，贞厉。','上九：视履考祥，其旋元吉。'],t:'履卦象征实践。行事需谨慎，步步为营。'},
    {n:'泰',s:'☷☰',b:'111000',j:'小往大来，吉，亨。',i:'天地交，泰。后以财成天地之道。',l:['初九：拔茅茹，征吉。','九二：包荒，用冯河。','九三：无平不陂，无往不复。','六四：翩翩，不富以其邻。','六五：帝乙归妹，以祉元吉。','上六：城复于隍，勿用师。'],t:'泰卦象征通达。事业顺利，大吉大利。'},
    {n:'否',s:'☰☷',b:'000111',j:'否之匪人，不利君子贞。',i:'天地不交，否。君子以俭德辟难。',l:['初六：拔茅茹，贞吉。','六二：包承，小人吉。','六三：包羞。','九四：有命无咎。','九五：休否，大人吉。','上九：倾否，先否后喜。'],t:'否卦象征闭塞。困难时期，坚守正道。'},
    {n:'同人',s:'☰☲',b:'101111',j:'同人于野，亨。',i:'天与火，同人。君子以类族辨物。',l:['初九：同人于门。','六二：同人于宗，吝。','九三：伏戎于莽。','九四：乘其墉，弗克攻。','九五：同人，先号咷而后笑。','上九：同人于郊，无悔。'],t:'同人卦象征团结。与人合作，志同道合。'},
    {n:'大有',s:'☲☰',b:'111101',j:'元亨。',i:'火在天上，大有。君子以遏恶扬善。',l:['初九：无交害。','九二：大车以载。','九三：公用亨于天子。','九四：匪其彭，无咎。','六五：厥孚交如，威如。','上九：自天祐之，吉无不利。'],t:'大有卦象征大有成就。事业成功，收获丰盛。'},
    {n:'谦',s:'☷☶',b:'001000',j:'亨，君子有终。',i:'地中有山，谦。君子以裒多益寡。',l:['初六：谦谦君子。','六二：鸣谦，贞吉。','九三：劳谦君子，有终吉。','六四：无不利。','六五：不富以其邻。','上六：鸣谦，利用行师。'],t:'谦卦象征谦虚。谦虚使人进步。'},
    {n:'豫',s:'☳☷',b:'000100',j:'利建侯行师。',i:'雷出地奋，豫。先王以作乐崇德。',l:['初六：鸣豫，凶。','六二：介于石，不终日。','六三：盱豫，悔。','九四：由豫，大有得。','六五：贞疾，恒不死。','上六：冥豫，成有渝。'],t:'豫卦象征愉悦。顺时而动，提前准备。'},
    {n:'随',s:'☱☳',b:'100110',j:'元亨，利贞，无咎。',i:'泽中有雷，随。君子以向晦入宴息。',l:['初九：官有渝，贞吉。','六二：系小子，失丈夫。','六三：系丈夫，失小子。','九四：随有获，贞凶。','九五：孚于嘉，吉。','上六：拘系之，乃从维之。'],t:'随卦象征随从。顺应形势，择善而从。'},
    {n:'蛊',s:'☶☴',b:'011001',j:'元亨，利涉大川。',i:'山下有风，蛊。君子以振民育德。',l:['初六：干父之蛊。','九二：干母之蛊。','九三：干父之蛊，小有悔。','六四：裕父之蛊。','六五：干父之蛊，用誉。','上九：不事王侯，高尚其事。'],t:'蛊卦象征整顿。需要变革创新。'},
    {n:'临',s:'☷☱',b:'110000',j:'元亨，利贞。',i:'泽上有地，临。君子以教思无穷。',l:['初九：咸临，贞吉。','九二：咸临，吉无不利。','六三：甘临，无攸利。','六四：至临，无咎。','六五：知临，大君之宜。','上六：敦临，吉。'],t:'临卦象征亲临。领导要深入基层。'},
    {n:'观',s:'☴☷',b:'000011',j:'盥而不荐，有孚颙若。',i:'风行地上，观。先王以省方观民。',l:['初六：童观，小人无咎。','六二：窥观，利女贞。','六三：观我生，进退。','六四：观国之光。','九五：观我生，君子无咎。','上九：观其生，君子无咎。'],t:'观卦象征观察。善于观察事物本质。'},
    {n:'噬嗑',s:'☲☳',b:'100101',j:'亨。利用狱。',i:'雷电，噬嗑。先王以明罚敕法。',l:['初九：屦校灭趾。','六二：噬肤灭鼻。','六三：噬腊肉，遇毒。','九四：噬干胏，得金矢。','六五：噬干肉，得黄金。','上九：何校灭耳，凶。'],t:'噬嗑卦象征惩治。果断处理问题。'},
    {n:'贲',s:'☶☲',b:'101001',j:'亨。小利有攸往。',i:'山下有火，贲。君子以明庶政。',l:['初九：贲其趾。','六二：贲其须。','九三：贲如濡如。','六四：贲如皤如。','六五：贲于丘园。','上九：白贲，无咎。'],t:'贲卦象征装饰。注重修饰，保持质朴。'},
    {n:'剥',s:'☶☷',b:'000001',j:'不利有攸往。',i:'山附于地，剥。上以厚下安宅。',l:['初六：剥床以足。','六二：剥床以辨。','六三：剥之，无咎。','六四：剥床以肤，凶。','六五：贯鱼，以宫人宠。','上九：硕果不食。'],t:'剥卦象征衰退。需要谨慎行事。'},
    {n:'复',s:'☷☳',b:'100000',j:'亨。出入无疾。',i:'雷在地中，复。先王以至日闭关。',l:['初九：不远复，无祗悔。','六二：休复，吉。','六三：频复，厉。','六四：中行独复。','六五：敦复，无悔。','上六：迷复，凶。'],t:'复卦象征回归。休养生息，重新开始。'},
    {n:'无妄',s:'☰☳',b:'100111',j:'元亨，利贞。',i:'天下雷行，物与无妄。',l:['初九：无妄往，吉。','六二：不耕获，不菑畲。','六三：无妄之灾。','九四：可贞，无咎。','九五：无妄之疾，勿药有喜。','上九：无妄行有眚。'],t:'无妄卦象征不妄动。顺应自然，不妄为。'},
    {n:'大畜',s:'☶☰',b:'111001',j:'利贞。不家食吉。',i:'天在山中，大畜。君子以多识前言。',l:['初九：有厉，利已。','九二：舆说輹。','九三：良马逐。','六四：童牛之牿。','六五：豮豕之牙，吉。','上九：何天之衢，亨。'],t:'大畜卦象征大积蓄。需要厚积薄发。'},
    {n:'颐',s:'☶☳',b:'100001',j:'贞吉。观颐，自求口实。',i:'山下有雷，颐。君子以慎言语。',l:['初九：舍尔灵龟。','六二：颠颐，拂经。','六三：拂颐，贞凶。','六四：颠颐，吉。','六五：拂经，居贞吉。','上九：由颐，厉吉。'],t:'颐卦象征颐养。注重养生之道。'},
    {n:'大过',s:'☱☴',b:'011110',j:'栋桡，利有攸往。',i:'泽灭木，大过。君子以独立不惧。',l:['初六：藉用白茅。','九二：枯杨生稊。','九三：栋桡，凶。','九四：栋隆，吉。','九五：枯杨生华。','上六：过涉灭顶，凶。'],t:'大过卦象征过分。需要谨慎处理。'},
    {n:'坎',s:'☵☵',b:'010010',j:'习坎，有孚维心亨。',i:'水洊至，习坎。君子以常德行。',l:['初六：习坎，入于坎窞。','九二：坎有险，求小得。','六三：来之坎坎。','六四：樽酒簋贰。','九五：坎不盈，祗既平。','上六：系用徽纆。'],t:'坎卦象征险阻。前方充满艰难险阻。'},
    {n:'离',s:'☲☲',b:'101101',j:'利贞，亨。畜牝牛吉。',i:'明两作，离。大人以继明照于四方。',l:['初九：履错然，敬之。','六二：黄离，元吉。','九三：日昃之离。','九四：突如其来如。','六五：出涕沱若。','上九：王用出征。'],t:'离卦象征光明。保持光明正大。'},
    {n:'咸',s:'☱☶',b:'001110',j:'亨，利贞。取女吉。',i:'山上有泽，咸。君子以虚受人。',l:['初六：咸其拇。','六二：咸其腓。','九三：咸其股。','九四：贞吉，悔亡。','九五：咸其脢，无悔。','上六：咸其辅颊舌。'],t:'咸卦象征感应。情感交流与心灵感应。'},
    {n:'恒',s:'☳☴',b:'011100',j:'亨，无咎，利贞。',i:'雷风，恒。君子以立不易方。',l:['初六：浚恒，贞凶。','九二：悔亡。','九三：不恒其德。','九四：田无禽。','六五：恒其德，贞。','上六：振恒，凶。'],t:'恒卦象征恒久。持之以恒。'},
    {n:'遁',s:'☰☶',b:'001111',j:'亨，小利贞。',i:'天下有山，遁。君子以远小人。',l:['初六：遁尾，厉。','六二：执之用黄牛之革。','九三：系遁，有疾厉。','九四：好遁，君子吉。','九五：嘉遁，贞吉。','上九：肥遁，无不利。'],t:'遁卦象征退避。懂得适时退让。'},
    {n:'大壮',s:'☳☰',b:'111100',j:'利贞。',i:'雷在天上，大壮。君子以非礼弗履。',l:['初九：壮于趾，征凶。','九二：贞吉。','九三：小人用壮。','九四：贞吉，悔亡。','六五：丧羊于易。','上六：羝羊触藩。'],t:'大壮卦象征强盛。谨慎使用力量。'},
    {n:'晋',s:'☲☷',b:'000101',j:'康侯用锡马蕃庶。',i:'明出地上，晋。君子以自昭明德。',l:['初六：晋如摧如。','六二：晋如愁如。','六三：众允，悔亡。','九四：晋如鼫鼠。','六五：悔亡，失得勿恤。','上九：晋其角。'],t:'晋卦象征前进。事业蒸蒸日上。'},
    {n:'明夷',s:'☷☲',b:'101000',j:'利艰贞。',i:'明入地中，明夷。君子以莅众。',l:['初九：明夷于飞。','六二：明夷于左股。','九三：明夷于南狩。','六四：入于左腹。','六五：箕子之明夷。','上六：不明晦。'],t:'明夷卦象征光明受损。韬光养晦。'},
    {n:'家人',s:'☴☲',b:'101011',j:'利女贞。',i:'风自火出，家人。君子以言有物。',l:['初九：闲有家。','六二：无攸遂。','九三：家人嗃嗃。','六四：富家。','九五：王假有家。','上九：有孚威如。'],t:'家人卦象征家庭。家庭和睦幸福。'},
    {n:'睽',s:'☲☱',b:'110101',j:'小事吉。',i:'上火下泽，睽。君子以同而异。',l:['初九：悔亡。','九二：遇主于巷。','六三：见舆曳。','九四：睽孤。','六五：悔亡。','上九：睽孤。'],t:'睽卦象征乖离。求同存异。'},
    {n:'蹇',s:'☵☶',b:'001010',j:'利西南，不利东北。',i:'山上有水，蹇。君子以反身修德。',l:['初六：往蹇来誉。','六二：王臣蹇蹇。','九三：往蹇来反。','六四：往蹇来连。','九五：大蹇朋来。','上六：往蹇来硕。'],t:'蹇卦象征艰难。前行困难，需反思。'},
    {n:'解',s:'☳☵',b:'010100',j:'利西南。',i:'雷雨作，解。君子以赦过宥罪。',l:['初六：无咎。','九二：田获三狐。','六三：负且乘。','九四：解而拇。','六五：君子维有解。','上六：公用射隼。'],t:'解卦象征解脱。困难即将解除。'},
    {n:'损',s:'☶☱',b:'110001',j:'有孚，元吉。',i:'山下有泽，损。君子以惩忿窒欲。',l:['初九：已事遄往。','九二：利贞。','六三：三人行则损一人。','六四：损其疾。','六五：或益之十朋之龟。','上九：弗损益之。'],t:'损卦象征减损。有舍才有得。'},
    {n:'益',s:'☴☳',b:'100011',j:'利有攸往。',i:'风雷，益。君子以见善则迁。',l:['初九：利用为大作。','六二：或益之十朋之龟。','六三：益之用凶事。','六四：中行告公从。','九五：有孚惠心。','上九：莫益之。'],t:'益卦象征增益。好运来临，收获增长。'},
    {n:'夬',s:'☱☰',b:'111110',j:'扬于王庭。',i:'泽上于天，夬。君子以施禄及下。',l:['初九：壮于前趾。','九二：惕号莫夜。','九三：壮于頄。','九四：臀无肤。','九五：苋陆夬夬。','上六：无号，终有凶。'],t:'夬卦象征决断。需要果断决策。'},
    {n:'姤',s:'☰☴',b:'011111',j:'女壮，勿用取女。',i:'天下有风，姤。后以施命诰四方。',l:['初六：系于金柅。','九二：包有鱼。','九三：臀无肤。','九四：包无鱼。','九五：以杞包瓜。','上九：姤其角。'],t:'姤卦象征相遇。机缘巧合。'},
    {n:'萃',s:'☱☷',b:'000110',j:'亨，王假有庙。',i:'泽上于地，萃。君子以除戎器。',l:['初六：有孚不终。','六二：引吉。','六三：萃如嗟如。','九四：大吉。','九五：萃有位。','上六：赍咨涕洟。'],t:'萃卦象征聚集。群英荟萃。'},
    {n:'升',s:'☷☴',b:'011000',j:'元亨，用见大人。',i:'地中生木，升。君子以顺德。',l:['初六：允升大吉。','九二：孚乃利用禴。','九三：升虚邑。','六四：王用亨于岐山。','六五：贞吉升阶。','上六：冥升。'],t:'升卦象征上升。步步高升。'},
    {n:'困',s:'☱☵',b:'010110',j:'亨，贞大人吉。',i:'泽无水，困。君子以致命遂志。',l:['初六：臀困于株木。','九二：困于酒食。','六三：困于石。','九四：来徐徐。','九五：劓刖。','上六：困于葛藟。'],t:'困卦象征困境。坚守信念以度难关。'},
    {n:'井',s:'☵☴',b:'011010',j:'改邑不改井。',i:'木上有水，井。君子以劳民劝相。',l:['初六：井泥不食。','九二：井谷射鲋。','九三：井渫不食。','六四：井甃无咎。','九五：井洌寒泉食。','上六：井收勿幕。'],t:'井卦象征井水。滋养万物，稳固根基。'},
    {n:'革',s:'☱☲',b:'101110',j:'己日乃孚，元亨。',i:'泽中有火，革。君子以治历明时。',l:['初九：巩用黄牛之革。','六二：己日乃革之。','九三：征凶贞厉。','九四：悔亡有孚改命。','九五：大人虎变。','上六：君子豹变。'],t:'革卦象征变革。除旧布新。'},
    {n:'鼎',s:'☲☴',b:'011101',j:'元吉，亨。',i:'木上有火，鼎。君子以正位凝命。',l:['初六：鼎颠趾。','九二：鼎有实。','九三：鼎耳革。','九四：鼎折足。','六五：鼎黄耳金铉。','上九：鼎玉铉。'],t:'鼎卦象征鼎器。稳固发展，大吉大利。'},
    {n:'震',s:'☳☳',b:'100100',j:'亨。震来虩虩。',i:'洊雷，震。君子以恐惧修省。',l:['初九：震来虩虩。','六二：震来厉。','六三：震苏苏。','九四：震遂泥。','六五：震往来厉。','上六：震索索。'],t:'震卦象征震动。遇变不惊，反省自修。'},
    {n:'艮',s:'☶☶',b:'001001',j:'艮其背，不获其身。',i:'兼山，艮。君子以思不出其位。',l:['初六：艮其趾。','六二：艮其腓。','九三：艮其限。','六四：艮其身。','六五：艮其辅。','上九：敦艮。'],t:'艮卦象征停止。适可而止。'},
    {n:'渐',s:'☴☶',b:'001011',j:'女归吉，利贞。',i:'山上有木，渐。君子以居贤德善俗。',l:['初六：鸿渐于干。','六二：鸿渐于磐。','九三：鸿渐于陆。','六四：鸿渐于木。','九五：鸿渐于陵。','上九：鸿渐于陆。'],t:'渐卦象征渐进。循序渐进。'},
    {n:'归妹',s:'☳☱',b:'110100',j:'征凶，无攸利。',i:'泽上有雷，归妹。君子以永终知敝。',l:['初九：归妹以娣。','九二：眇能视。','六三：归妹以须。','九四：归妹愆期。','六五：帝乙归妹。','上六：女承筐无实。'],t:'归妹卦象征婚嫁。喜结良缘。'},
    {n:'丰',s:'☳☲',b:'101100',j:'亨，王假之。',i:'雷电皆至，丰。君子以折狱致刑。',l:['初九：遇其配主。','六二：丰其蔀。','九三：丰其沛。','九四：丰其蔀。','六五：来章有庆誉。','上六：丰其屋。'],t:'丰卦象征丰盛。收获满满。'},
    {n:'旅',s:'☲☶',b:'001101',j:'小亨，旅贞吉。',i:'山上有火，旅。君子以明慎用刑。',l:['初六：旅琐琐。','六二：旅即次。','九三：旅焚其次。','九四：旅于处。','六五：射雉一矢亡。','上九：鸟焚其巢。'],t:'旅卦象征旅行。人生如旅。'},
    {n:'巽',s:'☴☴',b:'011011',j:'小亨，利有攸往。',i:'随风，巽。君子以申命行事。',l:['初六：进退利武人之贞。','九二：巽在床下。','九三：频巽吝。','六四：悔亡田获三品。','九五：贞吉悔亡。','上九：巽在床下。'],t:'巽卦象征顺从。以柔克刚。'},
    {n:'兑',s:'☱☱',b:'110110',j:'亨，利贞。',i:'丽泽，兑。君子以朋友讲习。',l:['初九：和兑吉。','九二：孚兑吉。','六三：来兑凶。','九四：商兑未宁。','九五：孚于剥。','上六：引兑。'],t:'兑卦象征喜悦。快乐与交流。'},
    {n:'涣',s:'☴☵',b:'010011',j:'亨，王假有庙。',i:'风行水上，涣。先王以享于帝立庙。',l:['初六：用拯马壮。','九二：涣奔其机。','六三：涣其躬。','六四：涣其群。','九五：涣汗其大号。','上九：涣其血。'],t:'涣卦象征涣散。需凝聚人心。'},
    {n:'节',s:'☵☱',b:'110010',j:'亨，苦节不可贞。',i:'泽上有水，节。君子以制数度。',l:['初九：不出户庭。','九二：不出门庭。','六三：不节若。','六四：安节亨。','九五：甘节吉。','上六：苦节贞凶。'],t:'节卦象征节制。适度约束。'},
    {n:'中孚',s:'☴☱',b:'110011',j:'豚鱼吉，利涉大川。',i:'泽上有风，中孚。君子以议狱缓死。',l:['初九：虞吉有它不燕。','九二：鸣鹤在阴。','六三：得敌。','六四：月几望。','九五：有孚挛如。','上九：翰音登于天。'],t:'中孚卦象征诚信。诚信为本。'},
    {n:'小过',s:'☳☶',b:'001100',j:'亨，利贞。可小事不可大事。',i:'山上有雷，小过。君子以行过乎恭。',l:['初六：飞鸟以凶。','六二：过其祖遇其妣。','九三：弗过防之。','九四：无咎。','六五：密云不雨。','上六：弗遇过之。'],t:'小过卦象征小过失。略有偏差需调整。'},
    {n:'既济',s:'☵☲',b:'101010',j:'亨小，利贞。初吉终乱。',i:'水在火上，既济。君子以思患而豫防之。',l:['初九：曳其轮。','六二：妇丧其茀。','九三：高宗伐鬼方。','六四：繻有衣袽。','九五：东邻杀牛。','上六：濡其首。'],t:'既济卦象征已完成。守成更难。'},
    {n:'未济',s:'☲☵',b:'010101',j:'亨，小狐汔济。',i:'火在水上，未济。君子以慎辨物居方。',l:['初六：濡其尾。','九二：曳其轮。','六三：未济征凶。','九四：贞吉悔亡。','六五：贞吉无悔。','上九：有孚于饮酒。'],t:'未济卦象征未完成。仍有可为，继续努力。'}
];

var isBusy = false;

function doDivine() {
    if (isBusy) return;
    isBusy = true;
    $('btnGo').innerHTML = '占卦中...';
    $('resBox').style.display = 'none';
    $('hz').innerHTML = '☯';
    $('hn').innerHTML = '正在感应天地之气...';
    $('hb').innerHTML = '------';
    setTimeout(function() { doDivine2(); }, 1200);
}

function doDivine2() {
    var idx = Math.floor(Math.random() * hexList.length);
    var h = hexList[idx];
    $('hz').innerHTML = h.s;
    $('hn').innerHTML = h.n + '卦 (' + h.b + ')';
    $('hb').innerHTML = h.j;
    var body = '<p style="margin:0 0 10px;color:#333;">' + h.i + '</p>';
    body += '<p style="margin:0;color:#222;">' + h.t + '</p>';
    $('resBody').innerHTML = body;
    var linesHtml = '<div style="font-weight:bold;margin-bottom:6px;color:#555;">爻辞：</div>';
    for (var i = 0; i < h.l.length; i++) {
        linesHtml += '<div style="padding:3px 0;border-bottom:1px dashed #ddd;color:#456;">' + h.l[i] + '</div>';
    }
    $('resLines').innerHTML = linesHtml;
    $('resBox').style.display = 'block';
    $('btnGo').innerHTML = '再算一卦';
    isBusy = false;
}

// ==================== Tarot ====================
var tarotBusy = false;

var tarotList = [
    {id:0, n:'愚者', e:'The Fool', emo:'🤡', kw:'开始、天真、冒险', up:'新的开始，充满可能性的旅程。带着纯真勇敢迈出第一步。', rev:'鲁莽冲动，不计后果。需要三思而后行。'},
    {id:1, n:'魔术师', e:'The Magician', emo:'🧙', kw:'创造、能力、意志', up:'拥有实现目标的全部资源和能力，创造奇迹的时刻。', rev:'能力被浪费，缺乏自信或欺骗。小心有人利用花言巧语。'},
    {id:2, n:'女祭司', e:'The High Priestess', emo:'🔮', kw:'直觉、潜意识、神秘', up:'倾听内心的声音，相信直觉。隐藏的智慧即将浮现。', rev:'忽视直觉，情绪混乱。秘密被隐藏，需要等待时机。'},
    {id:3, n:'皇后', e:'The Empress', emo:'👸', kw:'丰饶、母性、自然', up:'丰收与繁荣，享受生活的美好。创造力与母性之爱。', rev:'缺乏安全感，过度依赖他人。创造力受阻，需要自我关怀。'},
    {id:4, n:'皇帝', e:'The Emperor', emo:'👑', kw:'权威、秩序、掌控', up:'建立秩序与规则，掌控全局。凭借坚定的意志引领方向。', rev:'滥用权力，独裁专横。失去控制感，需要灵活变通。'},
    {id:5, n:'教皇', e:'The Hierophant', emo:'✝️', kw:'传统、教导、信仰', up:'遵循传统，寻求精神指引。接受正式的教育与训练。', rev:'挑战传统，不盲从权威。需要独立思考，打破常规。'},
    {id:6, n:'恋人', e:'The Lovers', emo:'💑', kw:'爱情、选择、和谐', up:'真挚的爱情或重要的合作。面临重大选择，听从内心。', rev:'关系出现裂痕，价值观冲突。错误的选择会带来遗憾。'},
    {id:7, n:'战车', e:'The Chariot', emo:'🏇', kw:'胜利、意志、前进', up:'以坚强的意志克服一切障碍，向着胜利前进。', rev:'失控与失败，方向迷失。需要停下来重新审视。'},
    {id:8, n:'力量', e:'Strength', emo:'🦁', kw:'勇气、内在力量、耐心', up:'以温柔而坚定的力量克服困难。内在的勇气胜过蛮力。', rev:'软弱无力，被恐惧控制。缺乏自信，需要重建内心力量。'},
    {id:9, n:'隐者', e:'The Hermit', emo:'🧓', kw:'内省、独处、智慧', up:'暂时远离喧嚣，进行深度内省。智慧来自安静地倾听。', rev:'孤僻自闭，拒绝帮助。过分孤立导致视野狭隘。'},
    {id:10, n:'命运之轮', e:'Wheel of Fortune', emo:'🎡', kw:'命运、转变、机遇', up:'命运之轮转动，好运来临。把握转机，顺应变化。', rev:'厄运降临，事与愿违。抗拒变化只会让情况更糟。'},
    {id:11, n:'正义', e:'Justice', emo:'⚖️', kw:'公正、平衡、因果', up:'公正的裁决即将到来。种瓜得瓜，因果报应。', rev:'不公正的对待，逃避责任。法律纠纷，需要据理力争。'},
    {id:12, n:'倒吊人', e:'The Hanged Man', emo:'🙃', kw:'牺牲、换位思考、等待', up:'换个角度看世界，以退为进。暂时的牺牲带来成长。', rev:'无谓的牺牲，固执己见。不愿改变，停滞不前。'},
    {id:13, n:'死神', e:'Death', emo:'💀', kw:'结束、重生、转变', up:'旧的结束意味着新的开始，彻底蜕变与重生。', rev:'抗拒改变，停滞腐烂。恐惧放手，被过去束缚。'},
    {id:14, n:'节制', e:'Temperance', emo:'⚗️', kw:'调和、平衡、耐心', up:'找到平衡点，调和矛盾。中庸之道，万事适度。', rev:'极端失衡，放纵无度。缺乏节制导致混乱。'},
    {id:15, n:'恶魔', e:'The Devil', emo:'😈', kw:'欲望、束缚、诱惑', up:'被物质和欲望束缚，需要正视内心的阴暗面。', rev:'挣脱束缚，摆脱不良习惯。觉醒后重获自由。'},
    {id:16, n:'高塔', e:'The Tower', emo:'🗼', kw:'崩塌、觉醒、剧变', up:'旧秩序的突然崩塌，电闪雷鸣中的觉醒。', rev:'抗拒变革，危机累积。避免更大的灾难需要及时止损。'},
    {id:17, n:'星星', e:'The Star', emo:'⭐', kw:'希望、疗愈、灵感', up:'黑暗中的星光，希望与灵感照亮前路。疗愈与宁静。', rev:'失去希望，灰心丧气。灵感枯竭，需要重新充电。'},
    {id:18, n:'月亮', e:'The Moon', emo:'🌙', kw:'幻觉、恐惧、潜意识', up:'穿过迷雾，面对内心深处的恐惧。潜意识正在说话。', rev:'恐惧消散，真相浮现。走出迷茫，重获清明。'},
    {id:19, n:'太阳', e:'The Sun', emo:'☀️', kw:'快乐、成功、活力', up:'光明灿烂，万物生长的喜悦。成功与幸福近在眼前。', rev:'短暂的阴霾，快乐被推迟。但太阳总会重新升起。'},
    {id:20, n:'审判', e:'Judgement', emo:'📯', kw:'觉醒、重生、召唤', up:'倾听内心的召唤，觉醒重生。接受过去并走向新生。', rev:'逃避召唤，不愿面对自己。错过重要的转变机会。'},
    {id:21, n:'世界', e:'The World', emo:'🌍', kw:'完成、圆满、成就', up:'一个完整的循环圆满结束，达成目标，功德圆满。', rev:'未完成的目标，功亏一篑。需要修补欠缺之处。'}
];

function doTarot() {
    if (tarotBusy) return;
    tarotBusy = true;
    $('btnTarot').innerHTML = '抽牌中...';
    $('tarotDescBox').style.display = 'none';
    var cardIdx = 0;
    var timer = setInterval(function() {
        var c = tarotList[cardIdx % tarotList.length];
        $('tarotEmoji').innerHTML = c.emo;
        $('tarotName').innerHTML = c.n;
        $('tarotEn').innerHTML = c.e;
        cardIdx++;
    }, 80);
    setTimeout(function() {
        clearInterval(timer);
        doTarotResult();
    }, 1500);
}

function doTarotResult() {
    var idx = Math.floor(Math.random() * tarotList.length);
    var card = tarotList[idx];
    var isUpr = Math.random() < 0.5;
    $('tarotEmoji').innerHTML = card.emo;
    $('tarotName').innerHTML = card.n;
    $('tarotEn').innerHTML = card.e;
    var posHtml = isUpr ? '<div class="tarot-pos upright">正位</div>' : '<div class="tarot-pos reversed">逆位</div>';
    $('tarotPosWrap').innerHTML = posHtml;
    var desc = '<p style="margin:0 0 8px;color:#333;"><b>关键词：</b>' + card.kw + '</p>';
    desc += '<p style="margin:0;color:#222;">' + (isUpr ? card.up : card.rev) + '</p>';
    $('tarotDescBody').innerHTML = desc;
    $('tarotDescBox').style.display = 'block';
    $('btnTarot').innerHTML = '再抽一张';
    tarotBusy = false;
}

// ==================== Classic Minesweeper ====================
var mineMode = 'classic';
var mineRows = 9, mineCols = 9, mineTotal = 10;
var mineBoard = [], mineRevealed = [], mineFlagged = [];
var mineTimer = null, mineSeconds = 0, mineGameOver = false, minePlaced = false;
var hexRows = 9, hexCols = 9, hexMines = 10;
var minePreset = 'med';

function openMineSettings() {
    $('settingsOverlay').style.display = 'flex';
    if (mineMode === 'classic') {
        $('setRows').value = mineRows;
        $('setCols').value = mineCols;
        $('setMines').value = mineTotal;
    } else {
        $('setRows').value = hexRows;
        $('setCols').value = hexCols;
        $('setMines').value = hexMines;
    }
    highlightPreset();
}

function closeMineSettings() {
    $('settingsOverlay').style.display = 'none';
}

function highlightPreset() {
    remCls($('presetSmall'), 'active');
    remCls($('presetMed'), 'active');
    remCls($('presetLarge'), 'active');
    var r = parseInt($('setRows').value, 10);
    var c = parseInt($('setCols').value, 10);
    var m = parseInt($('setMines').value, 10);
    if (r === 7 && c === 7 && m === 7) addCls($('presetSmall'), 'active');
    else if (r === 9 && c === 9 && m === 10) addCls($('presetMed'), 'active');
    else if (r === 16 && c === 30 && m === 99) addCls($('presetLarge'), 'active');
}

function applyPreset(p) {
    if (p === 'small') { $('setRows').value = 7; $('setCols').value = 7; $('setMines').value = 7; }
    else if (p === 'med') { $('setRows').value = 9; $('setCols').value = 9; $('setMines').value = 10; }
    else if (p === 'large') { $('setRows').value = 16; $('setCols').value = 30; $('setMines').value = 99; }
    highlightPreset();
}

function saveMineSettings() {
    var rows = parseInt($('setRows').value, 10);
    var cols = parseInt($('setCols').value, 10);
    var mines = parseInt($('setMines').value, 10);
    if (isNaN(rows) || rows < 3) rows = 3;
    if (rows > 30) rows = 30;
    if (isNaN(cols) || cols < 3) cols = 3;
    if (cols > 50) cols = 50;
    if (isNaN(mines) || mines < 1) mines = 1;
    var maxMines = rows * cols - 1;
    if (mines > maxMines) mines = maxMines;
    if (mineMode === 'classic') {
        mineRows = rows; mineCols = cols; mineTotal = mines;
    } else {
        hexRows = rows; hexCols = cols; hexMines = mines;
    }
    closeMineSettings();
    mineStopTimer();
    if (mineMode === 'classic') buildClassic();
    else buildHex();
}

function switchMineMode(mode) {
    mineStopTimer();
    mineMode = mode;
    if (mineMode === 'classic') {
        $('gridHex').style.display = 'none';
        $('gridClassic').style.display = 'block';
        addCls($('modeBtnClassic'), 'active');
        remCls($('modeBtnHex'), 'active');
        buildClassic();
    } else {
        $('gridClassic').style.display = 'none';
        $('gridHex').style.display = 'block';
        addCls($('modeBtnHex'), 'active');
        remCls($('modeBtnClassic'), 'active');
        buildHex();
    }
}

function mineStopTimer() {
    if (mineTimer) { clearInterval(mineTimer); mineTimer = null; }
}

function mineStartTimer() {
    mineStopTimer();
    mineSeconds = 0;
    $('mineTime').innerHTML = '0';
    mineTimer = setInterval(function() {
        mineSeconds++;
        $('mineTime').innerHTML = mineSeconds;
    }, 1000);
}

function placeMinesClassic(sr, sc) {
    var total = 0;
    while (total < mineTotal) {
        var r = Math.floor(Math.random() * mineRows);
        var c = Math.floor(Math.random() * mineCols);
        if (mineBoard[r][c]) continue;
        if (Math.abs(r - sr) <= 1 && Math.abs(c - sc) <= 1) continue;
        mineBoard[r][c] = true;
        total++;
    }
}

function buildClassic() {
    mineBoard = [];
    mineRevealed = [];
    mineFlagged = [];
    mineGameOver = false;
    minePlaced = false;
    mineStopTimer();
    mineSeconds = 0;
    $('mineTime').innerHTML = '0';
    $('mineLeft').innerHTML = mineTotal;
    $('mineMsg').innerHTML = '';
    for (var r = 0; r < mineRows; r++) {
        mineBoard[r] = [];
        mineRevealed[r] = [];
        mineFlagged[r] = [];
        for (var c = 0; c < mineCols; c++) {
            mineBoard[r][c] = false;
            mineRevealed[r][c] = false;
            mineFlagged[r][c] = false;
        }
    }
    renderClassic();
}

function renderClassic() {
    var html = '<table class="mine-table">';
    for (var r = 0; r < mineRows; r++) {
        html += '<tr>';
        for (var c = 0; c < mineCols; c++) {
            var cls = 'mine-cell';
            if (mineRevealed[r][c]) cls += ' revealed';
            if (mineFlagged[r][c]) cls += ' flagged';
            var cellId = 'mc_' + r + '_' + c;
            var clickAttr = 'onclick="mineClick(' + r + ',' + c + ',event)" oncontextmenu="return mineRight(' + r + ',' + c + ',event)"';
            html += '<td id="' + cellId + '" class="' + cls + '" ' + clickAttr + '></td>';
        }
        html += '</tr>';
    }
    html += '</table>';
    $('gridClassic').innerHTML = html;
}

function getNeighborMines(r, c) {
    var count = 0;
    for (var dr = -1; dr <= 1; dr++) {
        for (var dc = -1; dc <= 1; dc++) {
            if (dr === 0 && dc === 0) continue;
            var nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < mineRows && nc >= 0 && nc < mineCols && mineBoard[nr][nc]) count++;
        }
    }
    return count;
}

function mineClick(r, c, ev) {
    if (mineGameOver) return;
    if (mineRevealed[r][c]) return;
    if (mineFlagged[r][c]) return;
    if (!minePlaced) {
        placeMinesClassic(r, c);
        minePlaced = true;
        mineStartTimer();
    }
    if (mineBoard[r][c]) {
        mineGameOver = true;
        mineStopTimer();
        revealAllMinesClassic();
        $('mineMsg').innerHTML = '💥 踩到雷了！游戏结束';
        return;
    }
    floodFillClassic(r, c);
    renderClassic();
    checkWinClassic();
}

function floodFillClassic(r, c) {
    if (r < 0 || r >= mineRows || c < 0 || c >= mineCols) return;
    if (mineRevealed[r][c]) return;
    if (mineFlagged[r][c]) return;
    if (mineBoard[r][c]) return;
    mineRevealed[r][c] = true;
    var count = getNeighborMines(r, c);
    if (count > 0) {
        var cell = $('mc_' + r + '_' + c);
        if (cell) {
            cell.innerHTML = count;
            var colorMap = ['', '#00f', '#090', '#e00', '#008', '#800', '#088', '#000', '#888'];
            cell.style.color = count <= 8 ? colorMap[count] : '#000';
        }
        return;
    }
    var cell = $('mc_' + r + '_' + c);
    if (cell) cell.innerHTML = '';
    for (var dr = -1; dr <= 1; dr++) {
        for (var dc = -1; dc <= 1; dc++) {
            if (dr === 0 && dc === 0) continue;
            floodFillClassic(r + dr, c + dc);
        }
    }
}

function mineRight(r, c, ev) {
    if (mineGameOver) return false;
    if (mineRevealed[r][c]) return false;
    var evt = ev || window.event;
    if (evt && evt.preventDefault) evt.preventDefault();
    if (!minePlaced) {
        placeMinesClassic(r, c);
        minePlaced = true;
        mineStartTimer();
    }
    mineFlagged[r][c] = !mineFlagged[r][c];
    var flagged = 0;
    for (var i = 0; i < mineRows; i++)
        for (var j = 0; j < mineCols; j++)
            if (mineFlagged[i][j]) flagged++;
    $('mineLeft').innerHTML = mineTotal - flagged;
    renderClassic();
    checkWinClassic();
    return false;
}

function revealAllMinesClassic() {
    for (var r = 0; r < mineRows; r++) {
        for (var c = 0; c < mineCols; c++) {
            if (mineBoard[r][c]) {
                mineRevealed[r][c] = true;
                var cell = $('mc_' + r + '_' + c);
                if (cell) {
                    cell.innerHTML = '💣';
                    cell.className = 'mine-cell revealed mine-boom';
                }
            }
        }
    }
}

function checkWinClassic() {
    var allRevealed = true;
    for (var r = 0; r < mineRows; r++) {
        for (var c = 0; c < mineCols; c++) {
            if (!mineBoard[r][c] && !mineRevealed[r][c]) { allRevealed = false; break; }
        }
        if (!allRevealed) break;
    }
    if (allRevealed) {
        mineGameOver = true;
        mineStopTimer();
        $('mineMsg').innerHTML = '🎉 恭喜！你赢了！用时 ' + mineSeconds + ' 秒';
    }
}

function mineRestart() {
    mineStopTimer();
    if (mineMode === 'classic') buildClassic();
    else buildHex();
}

// ==================== Hex Minesweeper ====================
var hexBoard = [], hexRevealed = [], hexFlagged = [], hexGameOver = false, hexPlaced = false;
var hexRowsCount = 9, hexColsCount = 9;

function getHexNeighbors(r, c) {
    var nbrs = [];
    var odd = c % 2 === 0 ? 0 : 1;
    var dirs = [[-1, 0], [1, 0], [0, -1], [0, 1], [-1 + odd, -1], [-1 + odd, 1]];
    for (var i = 0; i < dirs.length; i++) {
        var nr = r + dirs[i][0], nc = c + dirs[i][1];
        if (nr >= 0 && nr < hexRowsCount && nc >= 0 && nc < hexColsCount) nbrs.push([nr, nc]);
    }
    return nbrs;
}

function placeMinesHex(sr, sc) {
    var total = 0;
    while (total < hexMines) {
        var r = Math.floor(Math.random() * hexRowsCount);
        var c = Math.floor(Math.random() * hexColsCount);
        if (hexBoard[r][c]) continue;
        if (r === sr && c === sc) continue;
        var tooClose = false;
        var nbrs = getHexNeighbors(sr, sc);
        for (var i = 0; i < nbrs.length; i++) {
            if (nbrs[i][0] === r && nbrs[i][1] === c) { tooClose = true; break; }
        }
        if (tooClose) continue;
        hexBoard[r][c] = true;
        total++;
    }
}

function getHexNeighborMines(r, c) {
    var nbrs = getHexNeighbors(r, c);
    var count = 0;
    for (var i = 0; i < nbrs.length; i++) {
        if (hexBoard[nbrs[i][0]][nbrs[i][1]]) count++;
    }
    return count;
}

function buildHex() {
    hexRowsCount = hexRows;
    hexColsCount = hexCols;
    hexBoard = [];
    hexRevealed = [];
    hexFlagged = [];
    hexGameOver = false;
    hexPlaced = false;
    mineStopTimer();
    mineSeconds = 0;
    $('mineTime').innerHTML = '0';
    $('mineLeft').innerHTML = hexMines;
    $('mineMsg').innerHTML = '';
    for (var r = 0; r < hexRowsCount; r++) {
        hexBoard[r] = [];
        hexRevealed[r] = [];
        hexFlagged[r] = [];
        for (var c = 0; c < hexColsCount; c++) {
            hexBoard[r][c] = false;
            hexRevealed[r][c] = false;
            hexFlagged[r][c] = false;
        }
    }
    renderHex();
}

function renderHex() {
    var html = '<table class="hex-table">';
    for (var r = 0; r < hexRowsCount; r++) {
        html += '<tr>';
        for (var c = 0; c < hexColsCount; c++) {
            var cls = 'hex-cell';
            var inner = '';
            if (hexRevealed[r][c]) {
                cls += ' revealed';
                if (hexBoard[r][c]) {
                    inner = '💣';
                    cls += ' boom';
                } else {
                    var cnt = getHexNeighborMines(r, c);
                    if (cnt > 0) { inner = '' + cnt; cls += ' n' + cnt; }
                }
            } else if (hexFlagged[r][c]) {
                cls += ' flagged';
                inner = '🚩';
            }
            var offsetStyle = (c % 2 === 0) ? '' : '-ms-transform:translateY(22px);transform:translateY(22px);';
            var cellId = 'hx_' + r + '_' + c;
            var clickAttr = 'onclick="hexClick(' + r + ',' + c + ',event)" oncontextmenu="return hexRight(' + r + ',' + c + ',event)"';
            html += '<td id="' + cellId + '" class="' + cls + '" style="' + offsetStyle + '" ' + clickAttr + '>' + inner + '</td>';
        }
        html += '</tr>';
    }
    html += '</table>';
    $('gridHex').innerHTML = html;
}

function hexClick(r, c, ev) {
    if (hexGameOver) return;
    if (hexRevealed[r][c]) return;
    if (hexFlagged[r][c]) return;
    if (!hexPlaced) {
        placeMinesHex(r, c);
        hexPlaced = true;
        mineStartTimer();
    }
    if (hexBoard[r][c]) {
        hexGameOver = true;
        mineStopTimer();
        revealAllHexMines();
        $('mineMsg').innerHTML = '💥 踩到雷了！游戏结束';
        return;
    }
    hexFlood(r, c);
    renderHex();
    checkWinHex();
}

function hexFlood(r, c) {
    if (r < 0 || r >= hexRowsCount || c < 0 || c >= hexColsCount) return;
    if (hexRevealed[r][c]) return;
    if (hexFlagged[r][c]) return;
    if (hexBoard[r][c]) return;
    hexRevealed[r][c] = true;
    var count = getHexNeighborMines(r, c);
    var cell = $('hx_' + r + '_' + c);
    if (count > 0) {
        if (cell) {
            var colorMap = ['', '#00f', '#090', '#e00', '#008', '#800', '#088'];
            cell.innerHTML = count;
            cell.style.color = count <= 6 ? colorMap[count] : '#000';
        }
        return;
    }
    if (cell) cell.innerHTML = '';
    var nbrs = getHexNeighbors(r, c);
    for (var i = 0; i < nbrs.length; i++) {
        hexFlood(nbrs[i][0], nbrs[i][1]);
    }
}

function hexRight(r, c, ev) {
    if (hexGameOver) return false;
    if (hexRevealed[r][c]) return false;
    var evt = ev || window.event;
    if (evt && evt.preventDefault) evt.preventDefault();
    if (!hexPlaced) {
        placeMinesHex(r, c);
        hexPlaced = true;
        mineStartTimer();
    }
    hexFlagged[r][c] = !hexFlagged[r][c];
    var flagged = 0;
    for (var i = 0; i < hexRowsCount; i++)
        for (var j = 0; j < hexColsCount; j++)
            if (hexFlagged[i][j]) flagged++;
    $('mineLeft').innerHTML = hexMines - flagged;
    renderHex();
    checkWinHex();
    return false;
}

function revealAllHexMines() {
    for (var r = 0; r < hexRowsCount; r++) {
        for (var c = 0; c < hexColsCount; c++) {
            if (hexBoard[r][c]) {
                hexRevealed[r][c] = true;
                var cell = $('hx_' + r + '_' + c);
                if (cell) {
                    cell.innerHTML = '💣';
                    cell.className = 'hex-cell revealed mine-boom';
                }
            }
        }
    }
}

function checkWinHex() {
    var allRevealed = true;
    for (var r = 0; r < hexRowsCount; r++) {
        for (var c = 0; c < hexColsCount; c++) {
            if (!hexBoard[r][c] && !hexRevealed[r][c]) { allRevealed = false; break; }
        }
        if (!allRevealed) break;
    }
    if (allRevealed) {
        hexGameOver = true;
        mineStopTimer();
        $('mineMsg').innerHTML = '🎉 恭喜！你赢了！用时 ' + mineSeconds + ' 秒';
    }
}

// ==================== Roast Mode ====================
var roastList = [
    '你今天的幸运数字是404，幸运方向是not found。',
    '经过精密计算，你今天适合摸鱼。',
    '你的代码写得很好，下次别写了。',
    '关掉这个页面并不能改变你明天还要搬砖的事实。',
    '命运告诉我，你今天有一笔意外之财——中午外卖红包记得抢。',
    '你今天会遇到贵人——那个让你加班到深夜的人。',
    '你的桃花运在...等等，好像没有。',
    '根据量子力学原理，你不看这个结果就等于没算过。',
    '你的运气值：█████░░░░░ 50% ...算了给你凑个整，30%。',
    '今天宜：躺着。今天忌：起床。',
    '你的前世是一棵白菜，所以今生才会这么菜。',
    '你今天的幸运词是：BUG。恭喜你，你将与BUG共度美好的一天。',
    '不要灰心，虽然你没什么特长，但是拖延症很突出啊。',
    '据可靠消息，你下周将会...哦不好意思，看错人了。',
    '你这个月的运势指数：-100到100之间。具体是多少呢？不告诉你。',
    '你不是懒，你只是能量消耗比较低而已。',
    '笑死，你居然真的相信这个东西能算命。',
    '今天适合做决定，但不适合做对的决定。',
    '你的幸运颜色是五彩斑斓的黑。',
    '你今天会有一段奇妙的经历——在梦里。',
    '别说丧气的话，你已经很棒了，虽然我说的是反话。',
    '你今天的运势：🌕🌖🌗🌘🌑 ...全黑了。',
    '你是一个很特别的人，特别到地球人都不敢靠近你。',
    '你的未来充满无限可能——主要是没可能的那些。',
    '今天不要做任何决定，因为做了也会后悔。',
    '你今天需要多喝水，因为脑子进的水需要稀释一下。',
    '你的财运：左口袋进，右口袋出，中间还漏。',
    '你这个人最大的优点就是有自知之明——因为你没有其他优点了。',
    '今天你会收到一个好消息和一个坏消息。好消息是你还活着，坏消息是明天还要上班。',
    '你的智力水平已经超越了90%的...单细胞生物。',
    '你知道吗？你今天中彩票的概率和流星砸中你的概率差不多。',
    '你今天的状态：人模人样，但脑子不在线。',
    '请放心，你的未来不会更差了——因为已经到底了。',
    '你今天的幸运食物是西北风，免费又管饱。',
    '不要假装努力，结果不会陪你演戏。但你可以继续假装，反正也不扣钱。',
    '你的自拍美颜程度已经严重影响了你的自我认知。',
    '其实你很优秀，只是还没有被发现——可能永远也不会。',
    '今天适合表白，适合被拒绝，适合一个人回家哭。',
    '你是一个有深度的人，深到连自己都看不清自己。',
    '你的魅力值已经超出了测量范围——主要是下限超出了。',
    '你今天的运势帮你问过了，它说：再问自杀。',
    '你上辈子是个绕口令，所以这辈子说不清楚话。',
    '你的逻辑思维能力很强，强到可以把对的变成错的。',
    '别担心，船到桥头自然直——但你可能坐的不是船，是泰坦尼克号。',
    '你真的很有想法，虽然没有一个是有用的。',
    '你的方向感就像你的发际线一样，越来越模糊。',
    '今天适合思考人生，但不适合得出任何结论。',
    '你的灵魂很有趣，遗憾的是没有配上合适的肉体。',
    '你今天运气不错，适合去买彩票——当然，中不中是另一回事。',
    '你值得被温柔对待，但温柔说它今天请假了。',
    '你的存在本身就是一种奇迹，因为以你的智商能活到现在不容易。',
    '今天你会有贵人相助——会不会把人吓跑就不知道了。',
    '你的社交能力很强，强到别人都不知道你在说什么。',
    '不要失望，虽然你很普通，但普通也是一种天赋。',
    '你今天的工作效率将达到峰值，虽然峰值本身也不高。',
    '你真的很努力了，虽然方向完全错了。',
    '其实算命挺准的，只是你不愿意承认自己的菜而已。',
    '你今天的运势主打一个波澜不惊，翻译一下就是啥事也没有。',
    '请不要用你的业余爱好去挑战别人的专业，因为你连业余的都不算。',
    '你真的很幽默，虽然你自己并不知道。',
    '你今天适合躺平，因为站起来也改变不了什么。',
    '你今天会有一段难忘的经历——被老板骂。',
    '你的创意无限，就是没有一个能实现的。',
    '你今天的幸运数字是985，指的是你要985那样加班。',
    '你的生活很规律：吃饭、睡觉、被生活毒打。',
    '你知道吗？你和天才之间只差了一个字——你差了个"天"。',
    '你今天的运势关键词是：梦里啥都有。',
    '别放弃，虽然成功不一定会来，但失败一定会来。',
    '你的气场很强大，强大到方圆一米没有人敢靠近——主要是臭到了。',
    '你今天的吉祥物是加班费，但你可能拿不到。',
    '你很有品位，虽然没有人懂你的品位。',
    '你的运气就像WiFi信号，时而满格时而断连，大多数时候断连。',
    '你不是没有才华，你是才华不够。',
    '你今天的幸运方位是东南西北——走哪都行，反正都一样。',
    '你知道吗？你的努力程度和结果成反比。',
    '你很聪明，就是聪明得恰到坏处。',
    '你今天的运势：适合睡觉，不适合醒着。',
    '其实算命不如算钱，算来算去都是零花。',
    '请对着镜子说三遍"我很棒"，然后你会发现镜子在骗你。',
    '你的今天是昨天决定的，你的明天是今天决定的，所以你现在已经完了。',
    '你的心情我理解，但运势不理解。',
    '你知道为什么你的运气不好吗？因为你在看这个。',
    '你的潜力很大，大到让人以为你没有。',
    '你今天的运势指数是π，既无限又无规律。',
    '每个人都是特别的，但你是特别的特别。',
    '你的未来充满光明——等你走出隧道再说。',
    '你很有耐心，毕竟你看了这么久的废话。',
    '你今天的幸运音符是哀乐，建议循环播放。',
    '不要再算命了，命都是算出来的，不算啥事没有。',
    '你这个人很实在，实在到有点多余。',
    '今天你适合回忆过去，因为未来不会比过去更好。',
    '你今天会经历的三个字：唉、哦、嗯。',
    '你知道吗？你的心情好坏和天气无关，和工资有关。',
    '你的成功人士潜质是有的，只是深度潜质，没人能看到。',
    '你今天的运势建议：关闭所有电子产品，早点睡。',
    '你是一个很有个性的人，个性到没人受得了。',
    '好了，别看了，你还要搬砖的。快去工作！',
    '最后一条：其实以上都是假的。但你是真的菜。',
    '真的最后一条：你的运气会在...（信号中断）',
    '说100条怪话真累。你居然看完了。厉害。'
];

var roastIdx = -1;

function doRoast() {
    var newIdx = Math.floor(Math.random() * roastList.length);
    if (newIdx === roastIdx && roastList.length > 1) {
        newIdx = (newIdx + 1) % roastList.length;
    }
    roastIdx = newIdx;
    var msg = roastList[roastIdx];
    var idx = 0;
    var el = $('roastText');
    el.innerHTML = '';
    var timer = setInterval(function() {
        if (idx < msg.length) {
            el.innerHTML += msg.charAt(idx);
            idx++;
        } else {
            clearInterval(timer);
        }
    }, 50);
}

// ==================== Novel Reader ====================
var NOVELS_PER_PAGE = 10;
var novelFilter = '', novelCat = '全部', novelCurIdx = -1, novelCurPage = 0;
var novelCats = ['全部','科幻','文学','悬疑','玄幻','推理','历史','武侠','言情'];

var NOVELS = [
    {id:'san1',title:'三体',author:'刘慈欣',cat:'科幻',intro:'文化大革命如火如荼进行的同时，军方探寻外星文明的绝秘计划"红岸工程"取得了突破性进展。',text:'文化大革命如火如荼进行的同时，军方探寻外星文明的绝秘计划"红岸工程"取得了突破性进展。但在按下发射键的那一刻，历经劫难的叶文洁没有意识到，她彻底改变了人类的命运。\n\n地球文明向宇宙发出的第一声啼鸣，以太阳为中心，以光速向宇宙深处飞驰……\n\n四光年外，"三体文明"正苦苦挣扎——三颗无规则运行的太阳主导下的百余次毁灭与重生逼迫他们逃离母星。而恰在此时，他们接收到了地球发来的信息。\n\n在运用超技术锁死地球人的基础科学之后，三体人庞大的宇宙舰队开始向地球进发……人类的末日悄然来临。\n\n叶文洁站在雷达峰上，望着这座巨大的抛物面天线。她用一生去恨这个疯狂的世界，直到她发现，原来地球之外还有另一种可能。她按下了发射键，把地球的坐标发往了无垠的深空。\n\n四年后，"不要回答"四个字，从距离地球最近的三合星系统回复了过来。那个世界的物理学家1379号用尽一生守护了这个秘密，但一切都太晚了。\n\n二百年后，当人类第一次通过科学实验接触到了"智子"——被锁死的高维粒子——整个理论物理学界陷入了绝望的深渊。粒子加速器再也无法产生有效数据，基础研究被人为地划上了句号。\n\n汪淼是第一个亲眼看到倒计时的人。那串数字出现在照片的底片上，出现在他的视网膜上，出现在他视线所及的每一个角落。它一直在减少，以秒为单位，精准而无情。\n\n他找到了"科学边界"——一个由顶尖科学家组成的秘密组织。他们告诉他：物理学不存在了。而比物理学消失更可怕的是，那些相信物理学的人，一个接一个地自杀了。\n\n"你们是虫子。"这是三体世界对地球人类文明的第一句正式回应。而人类很快发现，在智子的监视下，他们确实如同虫子一般渺小。\n\n但人类从不认输。联合国迅速成立了行星防御理事会，启动了"面壁计划"——四个面壁者被赋予无上的权力，他们将对外界完全保密自己的计划，以智子无法看透的方式，去对抗四百光年外正在赶来的舰队。\n\n罗辑本是一个普通的大学教授，一个玩世不恭的社会学家。他被选为面壁者那一天起，他的人生就再也无法回到从前。他曾经想过放弃，想过逃跑，直到那个冬夜，他在冰面上悟出了宇宙社会学的核心秘密——黑暗森林法则。\n\n在这片黑暗的森林中，每一个文明都是潜伏的猎人。一旦被发现，就只有毁灭一条路。罗辑验证了这个理论后，他终于有了一张可以向宇宙发射的威慑牌，一张可以让整个人类文明和三体文明一起毁灭的王牌。\n\n威慑纪元持续了六十二年，直到一个叫程心的普通女孩接手了剑柄。她没有按下按钮。三体人立刻发动了攻击。人类文明，从此万劫不复。\n\n整个太阳系被二维化——从三维坍缩成一幅没有厚度的巨幅画卷。每一个活生生的人，每一栋建筑，每一片云，都在那一瞬间被压扁，变成这副画卷上的颜料。这就是歌者文明随手抛出的一片"二向箔"所带来的终极命运。'},
    {id:'huo1',title:'活着',author:'余华',cat:'文学',intro:'地主少爷福贵嗜赌成性，终于赌光了家业一贫如洗，穷困之中的福贵因为母亲生病前去求医，没想到半路上被国民党部队抓了壮丁。',text:'我比现在年轻十岁的时候，获得了一个游手好闲的职业，去乡间收集民间歌谣。那一年的整个夏天，我如同一只乱飞的麻雀，游荡在知了和阳光充斥的农村。\n\n我曾经遇到一个老人，他黝黑的脸在阳光里笑得十分生动，脸上的皱纹欢乐地游动着，里面镶满了泥土，就如布满田间的小道。\n\n这位老人叫福贵，他后来和我一起坐在了那棵茂盛的树下，在那个充满阳光的下午，他向我讲述了自己的一生。\n\n四十多年前，福贵家是当地的大地主。他爹穿着黑颜色的绸衣，总是把双手背在身后，在田间走来走去。那时候福贵是个阔少爷，不懂事，整天吊儿郎当，娶了城里的漂亮姑娘家珍，却一点都不珍惜。\n\n他喜欢去城里赌钱，一夜一夜地不回家。家珍怀着他家的孩子，肚子很大了，还要去找他，跪在赌场的地上求他回家。福贵不但不听，还打了她一顿。\n\n终于有一天，福贵输掉了全部家产。当龙二拿着地契走进来的那一刻，他爹当场倒在了堂屋里。老人被气死了，死的时候眼睛都没闭上。\n\n福贵从此一贫如洗。他们从祖屋搬了出来，住进了一间破草屋里。家珍被他爹接回了城里。福贵带着娘和女儿凤霞——一个生下来就聋哑的女孩——艰难地活着。\n\n后来家珍又回来了，带着刚出生的儿子有庆。她说，不管日子多苦，她都要跟福贵在一起。福贵哭了，这是他第一次真正感受到什么是家庭。\n\n但命运没有放过这个家庭。国民党军队撤退的时候，福贵被拉了壮丁。在战场上，他看到了数不清的死人，闻到了永远忘不掉的尸体的气味。他被解放军俘虏了，解放军给了他路费，让他回家。\n\n等他回到家，娘已经去世了。凤霞因为发高烧没人照顾，从此再也不能说话。但家人还在一起，这已经是最幸运的事了。\n\n后来的岁月里，福贵经历了土改、大跃进、文革。他亲眼看着曾经赢了他家产的龙二被枪决，看着儿子有庆因为给县长老婆献血过多而死在医院。他抱着儿子冰冷的身体，在医院的走廊里嚎啕大哭。\n\n女儿凤霞嫁了人，是个老实的搬运工，叫二喜。二喜对凤霞很好，对福贵和家人也很好。凤霞怀了孩子，大家都很高兴。但在生产的那天，凤霞大出血，死在了产房。\n\n家珍也撑不住了一一她的身体本来就有病，儿女的接连去世彻底击垮了她。她走得很安详。福贵说，他一辈子对不起家珍，但家珍一辈子都没怪他。二喜也在搬运时出了事故，被水泥板砸死。\n\n最后，连孙子苦根也因为吃豆子被噎死。福贵把家人都送走了，一个人买下了一头要被宰杀的老牛。村里人都说老牛活不了两年，但福贵和老牛一起，又活了很久很久。\n\n老人说到这里，脸上还是挂着笑容。他说："我这辈子啊，亲人一个个都走在了我前头。但我从来不觉得苦。我经历的事情多着呢，能活着就是最好的。"夕阳里，他和老牛的背影渐渐消失在田埂上。'},
    {id:'dao1',title:'盗墓笔记',author:'南派三叔',cat:'悬疑',intro:'50年前由长沙土夫子出土的战国帛书，记载了一个奇特战国古墓的位置。50年后，一个在他爷爷笔记中发现的秘密，让他走上了一条不归路。',text:'我的爷爷叫吴老狗，在长沙一带是出了名的老盗墓贼。爷爷死的时候，我才五岁。那天晚上，被雷声吵醒的我看到爷爷坐在床前，在闪电中，他的脸苍白得像纸一样，他手里拿着一本发黄的笔记，嘴里不停地说着什么。\n\n我吓得哭了，爷爷一把把我搂在怀里，用手捂住我的嘴。他说他要死了，有些事情必须告诉我，他给了我那本笔记。他说这本笔记里记载了一个秘密，如果他死了，这个秘密就会被永远埋藏，但是他不甘心。\n\n爷爷去世后那本笔记被我父亲收了起来。直到三年前，父亲也离世了，在整理遗物时我才重新找到它。笔记里记载的内容让我毛骨悚然——那是一座位于山东的战国时期古墓的详细记载，墓主是鲁殇王。\n\n我决定去一趟。我找上了三叔，他是我爷爷的徒弟，也是长沙土夫子圈子里响当当的人物。三叔看了笔记后脸色大变，说这是一个他找了大半辈子的地方。\n\n我们一行五人出发了。除了我和三叔，还有两个伙计，一个向导老六。老六是当地人，对这一带的山脉非常熟悉。我们雇了一条船，顺着河道进入了深山。越往里走，雾气越浓，空气中弥漫着一股说不出的腥味。\n\n进山第一天就出了事。老六在清早去探路的时候，失踪了。我们在河边发现了他的鞋子，鞋子里塞着一张纸条，上面歪歪扭扭地画着一个符号——那是爷爷笔记里经常出现的一个符号。\n\n三叔看到这个符号后，让我和两个伙计立刻回去。我当然不肯。三叔说了一句让我至今难忘的话："吴邪，你爷爷当年就是因为这个回来的，他要不是跑得快，你现在就没有爷爷了。"\n\n那天晚上，我在帐篷里怎么也睡不着。月光透过帐篷的缝隙照在那本笔记上，我突然发现有一页之前因为粘在一起我没有看到。那是一张地图，标注着一个叫"云顶天宫"的地方。\n\n第二天，三叔发现洞了。那个洞口隐藏在一片瀑布后面，被藤蔓完全覆盖。洞口不大，但里面出奇地开阔，有一道长长的墓道通往山腹深处。我们点起火把，深吸一口气，走了进去。\n\n墓道的墙壁上刻满了壁画，画的是古代祭祀的场景。但不对劲的是，壁画上的那些人物，眼睛居然是用一种特殊的颜料画的，火光照上去的时候，它们好像都在看着你。更诡异的是，有些人的脸是空白的，好像画到一半就停止了。\n\n我们在墓道里看到了第一具尸体。他不是老六，而是一个外国人，穿着民国时期的衣服，身边有一本德文日记和一个罗盘。纸上最后一个词写得七扭八歪——"它活了。"\n\n那趟盗墓之行，我们失去了两个伙计，三叔也受伤了，但我从此走上了这条路。后来我认识了一个叫张起灵的人，他沉默寡言，身手却惊为天人。他右手的食指和中指奇长，据说这是张家人的特征——一个世代守墓的神秘家族。\n\n我们一起去过云顶天宫，去过西王母宫，去过张家古楼。每一次都九死一生，每一次都离那个庞大的真相更近一步。原来我爷爷、我父亲、甚至我自己，从一开始就被卷入了这个跨越千年的阴谋之中。'},
    {id:'gui1',title:'鬼吹灯',author:'天下霸唱',cat:'悬疑',intro:'胡八一上山下乡来到云南，在一片原始森林中遭遇了前所未见的怪事。一本家传的秘书残卷，揭开了一段尘封的历史。',text:'盗墓不是请客吃饭，不是做文章，不是绘画绣花，不能那样雅致，那样从容不迫，文质彬彬，那样温良恭俭让。盗墓是一门技术，一门进行破坏的技术。\n\n我叫胡八一，本名胡建军，因为上学的时候赶上文化大革命，自己改了个名字叫八一。我家的这本《十六字阴阳风水秘术》是我们祖上传下来的。我祖父胡国华，当年在黄河边上给人看风水，十里八乡都叫他胡半仙。\n\n1969年冬天，我去大兴安岭插队。那里是无人区，一入秋就开始下雪，雪大得把门都堵死。有一天村里的老猎人牛心山来找我，说他在山里发现了一个山洞，洞口有奇怪的石刻。\n\n我跟着老猎人进了山。那山洞藏在一条干涸的河道旁边，洞口被灌木丛遮得严严实实。拔开灌木进去，里面空间大得惊人。洞壁上画满了壁画，还有一尊青铜鼎。鼎里装满了黑色的东西，仔细一看，是人的头发。\n\n我按照《十六字阴阳风水秘术》里的说法，认出了这是辽金时期一个大萨满的墓穴入口。老猎人吓得当场就要往回走，我却鬼迷心窍，非要探个究竟。反正插队也没什么意思，闲着也是闲着。\n\n后来我认识了好搭档王凯旋——人称王胖子。这家伙一米八五，二百多斤，胆子大得没边，属于那种天塌下来当被子盖的人。我们俩一见面就投缘，在大兴安岭的那些日子里，一起掏过不少野兽洞，也见过不少邪门的事。\n\n真正让我走上盗墓这条路的，是回到北京之后。那一年潘家园出了个大事——一个叫大金牙的古董贩子收了一件西汉的玉衣，那东西一出来就轰动了整个行当。大金牙找到我和胖子，说出钱让我们去找一座古墓。\n\n他说墓在云南的龙岭山里。我们到了当地才知道，那地方叫"鬼方"，据说方圆几十里都没有人烟，因为进去的人从来没有活着出来的。当地山民叫它"迷窟"——那里的山洞比蜘蛛网还复杂，走进去就别想再走出来。\n\n我们经历了人面蜘蛛、献王的万年灯、云南献王墓里的红衣女尸、昆仑山冰川下的九层妖楼。每一次都以为走到了尽头，但是更加深不可测的东西总是在下一个转角等着你。\n\n摸金校尉合则生，分则死。这是我祖上传下来的最后一句话。所有的秘密都指向一个终极谜题——雮尘珠。据说雮尘珠是上古时期流传下来的一件神物，能通幽冥，能解诅咒。而我和胖子——我们脖子后面的那块红斑——就是诅咒。\n\n从龙岭到云南，从云南到昆仑，从昆仑到南海归墟。我终于明白，有些真相是不能被揭开的，但当你已经走了一半的时候，你就已经无法回头了。\n\n死人是不会说话的，但死人留下的东西会。每一件随葬品，每一块碑文，每一幅壁画，都在诉说着一个被刻意掩埋的故事。而我的工作，就是把它们挖出来，然后活下去。'},
    {id:'qin1',title:'庆余年',author:'猫腻',cat:'玄幻',intro:'一个年轻的病人，因为一次毫不意外的经历，穿越到了一个完全不同的世界，成为庆国范家的一名私生子。',text:'范闲坐在马车上，安静地看着窗外的风景。他要去的那个地方叫京都，是庆国的都城。他现在叫范闲，是范家的私生子，从小在儋州长大，身边只有一个五竹叔。\n\n五竹是个很奇怪的人。他永远穿着一身黑衣，永远不吃饭不睡觉，眼睛上蒙着一块黑布。范闲从小跟他学武，但从来不知道五竹到底有多强。只知道没有人能打败他。\n\n范闲还有一个师父叫费介。费介是庆国最厉害的用毒高手。他教范闲下毒和解毒的本事，把他练成了一个百毒不侵的小怪物。费师父的教学方式很简单——每天在范闲身上试验各种毒药，让他自己想办法解毒。\n\n他前世是一个重症肌无力患者。在床上躺了很多年，最后孤独地死去。所以穿越到这个世界后，他发誓要好好活一次，绝不浪费这一生。\n\n到了京都，他第一次见到了自己的父亲——庆国户部尚书范建。范建是个精于计算的人，他的书房里永远摆着算盘，不管多大的事都能算出利弊。他让范闲来京都，不是因为什么父子之情，而是因为他需要一个有用的棋子。\n\n范闲也见到了庆帝。这个王国至高无上的统治者坐在龙椅上，用一种欣赏的目光打量着他。范闲第一次感受到了什么叫权势——那是一种能让你浑身汗毛倒竖的压迫感。\n\n但京都最大的秘密，藏在监察院里。那是独立于六部之外的神秘机构，只听命于皇帝一人。监察院的院长陈萍萍，是一个坐在轮椅上的老人，但他掌管着整个庆国最庞大的情报网络。他的眼睛能看穿一切——至少他自己是这么认为的。\n\n京都很大，大到足以容纳所有人的野心。有监察院与户部的权力争斗，有长公主深藏不露的势力，有北齐帝国的虎视眈眈，还有江湖上那些快意恩仇的高手们。\n\n范闲慢慢发现了自己的身世真相——他根本不是范建的私生子。他的母亲叫叶轻眉，是前一代监察院的创始人，是整个庆国最传奇也最神秘的女人。而她死了，被人害死了。杀她的人，至今还活在京都。\n\n从那天起，范闲的目标就变了。他不只是要活下去，他要查清楚母亲死亡的真相。不管幕后之人是谁——哪怕是庆帝本人——他也要让真相大白于天下。\n\n他用了数年的时间，在京都织起了一张自己的网。一边是户部的势力，一边是监察院的资源，再加上五竹那个深不可测的战斗力。他在权谋的漩涡中越来越得心应手。\n\n但在这个世界上，真正决定一切的不是计谋、不是武力，而是那个高高在上的大宗师。整个天下只有四个大宗师——庆帝是其中之一。而范闲最终要面对的，正是这位统治整个庆国的男人。'},
    {id:'dou1',title:'斗破苍穹',author:'天蚕土豆',cat:'玄幻',intro:'这里是属于斗气的世界，没有花俏艳丽的魔法，有的仅仅是繁衍到巅峰的斗气！',text:'"斗之力，三段！"\n\n望着测验魔石碑上面闪亮得甚至有些刺眼的五个大字，少年面无表情，唇角有着一抹自嘲，紧握的手掌，因为大力，而导致略微尖锐的指甲深深的刺进了掌心之中，带来一阵阵钻心的疼痛……\n\n"萧炎，斗之力，三段！级别：低级！"测验魔石碑之旁，一位中年男子，看了一眼碑上所显示出来的信息，语气漠然的将之公布了出来。中年男子话刚刚脱口，便是不出意外的在人头汹涌的广场上带起了一阵嘲讽的骚动。\n\n"三段？嘿嘿，果然不出我所料，这个天才这一年又是在原地踏步！""哎，这废物真是把家族的脸都给丢光了。""要不是族长是他的父亲，这种废物早就被驱赶出家族了。"周围传来的不屑嘲笑以及惋惜轻叹，落在那如木桩待在原地的少年耳中，恍如一根根利刺狠狠地扎在心脏一般。\n\n萧炎慢慢抬起头来，露出一张有些清秀的稚嫩脸庞，漆黑的眸子木然的在周围那些嘲讽的同龄人身上扫过。这些人都是和他一样的萧家子弟，三年前，他们还在仰视着他这个天才。\n\n三年前，他曾经是萧家的骄傲。十一岁成为斗者——这在加玛帝国都是顶尖的天赋。所有长老都认为，他至少能成为斗皇，甚至斗宗级别的强者。那时候的他意气风发，整个乌坦城的人提到萧炎，都竖起大拇指。\n\n但就在他达到斗者之后，诡异的事情发生了——他的斗气开始倒退。短短几个月，从斗者跌回了斗之气，而且越是修炼，倒退得越厉害。没有人知道原因，连家族里最博学的长老也束手无策。\n\n天才变成了废物。未婚妻纳兰嫣然当众退婚——她的家族已经看不上一个只有三段斗之力的废物了。萧炎站在大厅中央，听着纳兰家族使者宣读退婚书，指甲几乎要把掌心刺穿。一句话，让他受尽屈辱。\n\n"三十年河东，三十年河西，莫欺少年穷！"\n\n当着所有人的面，他写下了休书——不是被退婚，而是他休了纳兰嫣然。这个举动惹怒了所有人，但萧炎不在乎。他发了一个誓：三年之内，他要亲手打败纳兰嫣然，让所有看不起他的人跪着道歉。\n\n那天晚上，萧炎在戒指里听到了一个苍老的声音。"小家伙，你的天赋没有消失，只是在供养我恢复灵魂。"一个自称药老的老者从戒指中现出身形。他说他是曾经的斗尊强者，被人暗算，只剩下一缕残魂藏在骨炎戒中。\n\n"拜我为师，三年之内，我让你成为斗皇。"萧炎毫不犹豫，双膝跪地。从这一天起，他的人生开始逆转。焚诀——一部可以吞噬异火来进化的逆天功法，是药老给他最大的财富。\n\n此后三年，萧炎经历了无数次生死。入魔兽山脉历练，去塔戈尔大沙漠寻找青莲地心火，在迦南学院建立磐门势力。他一步一步往上走，三年之约，他做到了——当众击败纳兰嫣然，洗刷了当年的耻辱。\n\n但他的路远没有结束。斗破苍穹，天下强者如云，斗皇、斗宗、斗尊、斗圣，乃至传说中的斗帝。而他最终的敌人，是那个曾经背叛了所有远古八族、妄图统治整个斗气大陆的魂族——魂天帝。'},
    {id:'pin1',title:'平凡的世界',author:'路遥',cat:'文学',intro:'以中国70年代中期到80年代中期十年间为背景，以孙少安和孙少平两兄弟为中心，刻画了当时社会各阶层众多普通人的形象。',text:'一九七五年二三月间，一个平平常常的日子，细蒙蒙的雨丝夹着一星半点的雪花，正纷纷淋淋地向大地飘洒着。时令已快到惊蛰，雪当然再不会存留，往往还没等落地，就已经消失得无踪无影了。\n\n黄土高原严寒而漫长的冬天看来就要过去，但那真正温暖的春天还远远地没有到来。\n\n在这样雨雪交加的日子里，如果没有什么紧要事，人们宁愿一整天足不出户。因此，县城的大街小巷倒也比平时少了许多嘈杂。\n\n但在县城高中的校园里，有一个人却不能不出门。他就是孙少平。他穿着一件洗得发白的旧棉袄，裤腿短了一大截，露出冻得发红的脚踝。他在食堂取了他的两个黑面馍和一碗清汤，躲在没人的角落里，一边做贼似的狼吞虎咽，一边防备着同学们推门进来。\n\n孙少平是双水村的人。他父亲孙玉厚是个老实巴交的农民，一辈子除了种地什么都不会。大哥孙少安十三岁就辍学务农，用稚嫩的肩膀扛起了全家的担子。家里实在太穷了，穷到少平在学校里连一份丙菜都吃不起。\n\n他喜欢读书。他有着和那些城里学生一样的自尊心和求知欲。他曾经在田晓霞的推荐下，读了《参考消息》，了解到了这个世界上还存在着许多他从未想象过的事情。他如饥似渴地阅读一切能找到的书——从《钢铁是怎样炼成的》到《红楼梦》。\n\n田晓霞，是少平生命中最重要的一个人。她是地委书记的女儿，是省报记者。她爱上了这个穷农民的儿子。他们一起在煤矿的废墟上谈文学，在黄原的街头谈理想，在古塔山下谈未来。但就在他们约定见面的前一天，田晓霞为救一个落水儿童，牺牲了。\n\n而大哥孙少安，走的是另一条路。他留在了双水村，留在了那片生他养他的黄土地上。他娶了不要彩礼的贺秀莲——一个朴实的山西姑娘。他带头搞生产责任制，带着全村人烧砖窑，被罚过款，也被批斗过，但他从没服过输。\n\n少安烧的砖一块块垒起来，像一座墩实的碑。他终于用自己的双手，改变了家族的命运。但生活的艰辛是无止境的——秀莲在为他们家的砖窑奔波中，因为过度劳累患上了肺癌。她最后的日子里躺在炕上，少安守着她，为她削了一碗薄薄的饸烙面。\n\n孙少平从高中毕业后，去黄原城揽工。他背过石头，扛过水泥，脊背上压出了一道道血印。后来他当上了煤矿工人，下到几百米深的井下，那时候他才知道什么叫做真正的黑暗。但他的心是亮的，因为他心里装着那个再也见不到的晓霞。\n\n在井下，师傅为了保护他，被塌方砸死了。少平的脸也被砸伤了，留下了一道从眉骨到嘴角的长疤。但他在煤矿立住了脚跟，写下了他人生中第一篇关于煤矿工人的文章。\n\n生活总是这样——把最残酷的磨难给了最善良的人。但孙家兄弟从来没有被打倒过。他们像黄土高原上的老榆树，根深深扎在泥土里，任凭风雨摧折，来年春天仍然是郁郁葱葱。'},
    {id:'wei1',title:'围城',author:'钱钟书',cat:'文学',intro:'围在城里的人想逃出来，城外的人想冲进去。对婚姻也罢，职业也罢，人生的愿望大都如此。',text:'红海早过了，船在印度洋面上开驶着，但是太阳依然不饶人地迟落早起，侵占去大部分的夜。夜仿佛纸浸了油，变成半透明体；它给太阳拥抱住了，分不出身来，也许是给太阳陶醉了，所以夕照晚霞隐褪后的夜色也带着酡红。\n\n到红消醉醒，船舱里的睡人也一身腻汗地醒来，洗了澡赶到甲板上吹海风，又是一天开始。这是七月下旬，合中国旧历的三伏，一年最热的时候。\n\n这条法国邮船白拉日隆子爵号正向中国开来。方鸿渐在美国人开的银行里挂了名的，所以回国前他父亲竭力主张他先谋个职业，免得回来赋闲。结果经朋友介绍，在三闾大学弄了一个副教授的职位。\n\n方鸿渐在欧洲混了几年，博士学位一直没拿到手。最后被父亲和丈人催得急了，花三十美金在纽约买了一张假的克莱登大学博士文凭。他在船上遇到了苏文纨——一个自视甚高的大家闺秀，博士，身上带着一股法国香水的味道。\n\n苏小姐对他颇有好感，但方鸿渐的心思却在那个一起学医的唐晓芙身上。唐小姐是苏文纨的表妹，她生得娇小玲珑，笑起来的时候嘴角有两个小小的梨涡，说话的语气带着一种不经世事的纯真。方鸿渐第一次见到她就觉得自己心里有什么东西被拨动了。\n\n但命运捉弄了他。苏文纨察觉到方鸿渐的心思后，在唐小姐面前把方鸿渐的底细抖了个干净——连那张假文凭的事也说了。唐小姐伤透了心，再也没有理过方鸿渐。\n\n方鸿渐灰溜溜地去了三闾大学。这所大学在湖南的一个穷乡僻壤，校舍简陋得连牛棚都不如。教授们个个勾心斗角，拉帮结派。教哲学的忘了讲康德，只顾着算计明年的聘书；教英文的不会说几句完整的英语，却已经是系主任了。\n\n在这里，方鸿渐认识了孙柔嘉。她是外文系的助教，说话细声细气的，看上去什么都不懂，可实际上她自己什么都算计到了。她一步一步设局，让方鸿渐在众人面前下不了台，最后不得不向她求婚。\n\n婚后两人的生活简直就是一场灾难。孙柔嘉嫌方鸿渐没出息，方鸿渐嫌孙柔嘉世俗势利。为了一点钱、一句气话、一只打碎的花瓶，他们彼此甩出去了最难听的话。两个原本鲜活的年轻人，在婚姻的围城里被磨去了所有的棱角。\n\n方鸿渐最后离开了三闾大学，回到上海投奔朋友赵辛楣。赵辛楣把他安排在一家报社做资料室主任——一个可有可无的清闲职位。方鸿渐终于明白，他这一生学无所成、爱无所归、事业无成。\n\n他像一只飞不高的麻雀，折腾了一圈，最后还是落在了一个比起点高不了多少的地方。当他在报馆的窗户前看着外滩的灯光时，心里涌起的只有深深的疲惫和无奈。\n\n"围在城里的人想逃出来，城外的人想冲进去。对婚姻也罢，职业也罢，人生的愿望大都如此。"这句话，是方鸿渐路过一家老茶馆时，听一个说书先生说的。他愣在门口，觉得这说的就是他。'},
    {id:'xian1',title:'嫌疑人X的献身',author:'东野圭吾',cat:'推理',intro:'百年一遇的数学天才石神，每天唯一的乐趣，便是去固定的便当店买午餐，只为看一眼在那里做事的邻居靖子。',text:'上午七点三十五分，石神像往常一样离开公寓。虽已进入三月，风还是相当冷。他把下巴埋在围巾里，走了约二十米，向右转，来到一条南北向的小路。他瞥了一眼右边，那里是一排投币式储物柜。\n\n石神是高中数学老师，但他真正的天才是数学研究。二十年前，他是帝都大学数学系公认的天才——教授们都说他将来必定能成为改变数学史的人。但现实是残酷的，他没有能留在大学里，最后只能在一所普通高中教一群对数学毫无兴趣的学生。\n\n每天上班的路上，石神都会去弁天轩买一份便当。这家店的特制招牌便当是他每天唯一的期待。但他更期待的，是看到那个在店里打工的女店员——花冈靖子。\n\n靖子以前是做陪酒小姐的，现在她在弁天轩打工，独自抚养着女儿美里。她的前夫富樫慎二是个一无是处的混蛋，两人离婚后还一直纠缠不休。那天晚上，富樫找到了靖子母女的新住处。他喝得醉醺醺的，一进门就开始砸东西，对美里动手动脚。\n\n在一阵混乱中，富樫倒地了。他头上有个洞，血洇了一地。靖子和美里惊恐地看着自己的手，她们不知道具体是谁杀了人，只知道她们杀了人。就在这时，门铃响了。\n\n门外是石神。他住在隔壁，听到了动静。"让我来处理。"他的声音平静得像在讲解一道数学题。从那一刻起，一个数学天才开始用他的逻辑头脑，去为靖子母女制造一个完美的不在场证明。\n\n石神让靖子按照他精确的时间表行事，并让她在警察问话时按他写好的说辞一个字一个字地背。他还故意制造了另一具尸体——破坏面部，抛弃在江户川河岸——让警察误以为富樫是在那里被杀的。看起来滴水不漏。\n\n但警方请来了帝都大学物理学副教授汤川学——石神在帝都大学时代唯一的朋友。汤川是个天才中的天才，他透过层层伪装，逐渐发现了不对劲的地方。当他终于推理到那个可怕的真相时，他沉默了很久。\n\n汤川找到了石神，说："你杀了另一个人。你用那个人的尸体冒充了富樫。"汤川的眼镜后面闪过一丝痛苦，"你为了她们，把自己变成了真正的杀人犯。"\n\n石神微笑着说："我对她的爱，不需要她知道。数学告诉我，有时候最完美的解答，是用毁灭自己来让其他人活下去。"\n\n靖子最终从汤川那里得知了一切。她崩溃了——她一直以为石神只是帮忙处理了尸体，却不知道他为此杀了一个无辜的流浪汉。她跑到警察局自首，跪在调查室里泣不成声。\n\n石神也在看守所。当看到靖子也被带进来的时候，他终于崩溃了。这个永远冷静的数学天才，发出了像是呕出灵魂般的嚎叫。他精心构建的逻辑完美世界，在最后一刻，被他自己爱的人亲手摧毁了。'},
    {id:'min1',title:'明朝那些事儿',author:'当年明月',cat:'历史',intro:'从朱元璋的出身开始写起，到崇祯皇帝自缢明朝灭亡。以史料为基础，以年代和具体人物为主线，加入了小说的笔法。',text:'一切的事情都从1328年的那个夜晚开始。农民朱五四的妻子陈氏生下了一个男婴，大家都知道了，这个男婴就是后来的朱元璋。大凡皇帝出世，史书上都会有一些怪象记载，比如刮风、下暴雨、冒香气、天上星星闪等等。\n\n朱元璋出生时，红光满地，夜间房屋中出现异光，以至于邻居以为失火了，跑来相救。然而当时的农民朱五四的心情并不像今天我们在产房外看到的那些焦急中带着喜悦的父亲们，作为已经有了三个儿子、两个女儿的父亲，他更多的只是发愁——又多了一张吃饭的嘴。\n\n朱重八（朱元璋的原名）从六岁起就给地主刘德放牛。在十六岁之前，他最大的娱乐就是和小伙伴们一起演皇上的游戏。谁也没有想到，这个演皇上演的像模像样的少年，日后真的会成为皇上。\n\n1344年，一场大旱灾和蝗灾席卷了濠州地区。饿殍遍野，尸横满地。四月六日，父亲饿死。四月九日，大哥饿死。四月二十二日，母亲饿死。如果不是好心人给了块坟地，朱家人连葬身之地都没有。为了有口饭吃，他去皇觉寺当了和尚。\n\n但佛门也不是净土。饥荒太严重了，寺里的粮食也吃光了。住持把和尚们叫到跟前，说从今天起各人去化缘吧。实际上就是——你们自己去要饭吧，庙里养不起你们了。\n\n要饭的三年，朱重八走遍了淮西的山山水水。他知道了老百姓最怕的是什么人，最恨的又是什么人。这三年，他看到了人间最悲惨的景象，也在心里立下了一个誓言——如果有一天他有了权力，绝不会再让老百姓过这种日子。\n\n25岁那年，汤和给他来了一封信，大意是——我在郭子兴的队伍里混得不错，你也来吧。其实对朱重八来说，去郭子兴的队伍当兵，或者说造反，跟要饭没什么区别，都是为了活下去。只是他没有料到，他活得比任何人想象的都要好。\n\n郭子兴发现这个朱重八很不一般。他不仅打仗勇猛，而且计谋出众。一个月内，他就从一个大头兵升到了九人长。更让郭子兴高兴的是，这个年轻人还娶了自己的养女马氏——也就是后来的马皇后。\n\n从那时起，朱重八改名叫朱元璋。璋是一种尖锐的玉器，朱元璋就是"诛灭元朝的利器"。这个名字不是他自己取的，是他的老领导郭子兴给他取的名字。而朱元璋果然没有辜负这个名字——他一步一步消灭了陈友谅、张士诚、方国珍，最后把蒙古人赶出了中原，建立大明王朝。\n\n洪武元年正月初四日，朱元璋在应天府登基称帝。他在即位诏书中说："朕本淮右布衣，因天下乱，起兵保民，初无成帝业之心。"他真的是从农民变成了皇帝，这是中国历史上独一无二的奇迹。\n\n但朱元璋的后半生充满了杀戮。他为了给孙子朱允炆铺路，几乎杀光了开国功臣。李善长、蓝玉、胡惟庸……一个甲子前并肩作战的兄弟，一个甲子后被他一个接一个地送上了断头台。\n\n他没有想到的是，在他死后不到一年，他的儿子燕王朱棣就造反了。靖难之役，朱棣夺了他孙子的皇位，把都城从南京迁到了北京，修建了紫禁城。\n\n明朝二百七十六年，十六帝。有永乐盛世、仁宣之治，也有土木堡之变、阉党之祸。最后一任皇帝崇祯在李自成的农民军攻进北京的那一天，在煤山上上吊自尽了。临死前他在自己的袍子上写下了一句话——"任贼分裂朕尸，勿伤百姓一人。"'},
];

function novelGetFiltered() {
    var result = [], i, n;
    for (i = 0; i < NOVELS.length; i++) {
        n = NOVELS[i];
        if (novelCat !== '全部' && n.cat !== novelCat) continue;
        if (novelFilter && n.title.indexOf(novelFilter) === -1 && n.author.indexOf(novelFilter) === -1) continue;
        result.push(n);
    }
    return result;
}

function showNovelList() {
    $('novelShelf').style.display = 'block';
    $('novelReader').style.display = 'none';
}

function showNovelReader() {
    $('novelShelf').style.display = 'none';
    $('novelReader').style.display = 'block';
}

function renderNovelList() {
    var list = novelGetFiltered();
    var totalPages = Math.ceil(list.length / NOVELS_PER_PAGE);
    if (novelCurPage >= totalPages) novelCurPage = 0;
    var start = novelCurPage * NOVELS_PER_PAGE, end = Math.min(start + NOVELS_PER_PAGE, list.length);
    var html = '';
    for (var i = start; i < end; i++) {
        var n = list[i];
        html += '<div class="novel-card" onclick="novelOpen(\'' + n.id + '\')">' +
            '<div class="novel-card-title">' + n.title + '</div>' +
            '<div class="novel-card-author">' + n.author + ' · ' + n.cat + '</div>' +
            '<div class="novel-card-intro">' + n.intro.substring(0, 60) + '...</div>' +
            '</div>';
    }
    $('novelList').innerHTML = html || '<div style="color:#666;padding:20px;">没有找到匹配的小说</div>';
    $('novelPageInfo').innerHTML = (list.length > 0 ? (novelCurPage + 1) + '/' + totalPages + ' 页 (' + list.length + '本)' : '0本');
    $('novelBtnPrev').style.visibility = (novelCurPage > 0 ? 'visible' : 'hidden');
    $('novelBtnNext').style.visibility = (novelCurPage < totalPages - 1 ? 'visible' : 'hidden');
}

function renderNovelCat() {
    var html = '';
    for (var i = 0; i < novelCats.length; i++) {
        var sel = (novelCats[i] === novelCat) ? ' active' : '';
        html += '<span class="novel-cat-tag' + sel + '" onclick="novelCat=\'' + novelCats[i] + '\';novelCurPage=0;renderNovelCat();renderNovelList();">' + novelCats[i] + '</span>';
    }
    $('novelCats').innerHTML = html;
}

function novelNextPage() {
    var list = novelGetFiltered();
    var totalPages = Math.ceil(list.length / NOVELS_PER_PAGE);
    if (novelCurPage < totalPages - 1) { novelCurPage++; renderNovelList(); }
}

function novelPrevPage() {
    if (novelCurPage > 0) { novelCurPage--; renderNovelList(); }
}

function novelSearch() {
    var input = $('novelSearchInput');
    novelFilter = input ? input.value : '';
    novelCurPage = 0;
    renderNovelList();
}

function novelOpen(id) {
    for (var i = 0; i < NOVELS.length; i++) {
        if (NOVELS[i].id === id) { novelCurIdx = i; break; }
    }
    if (novelCurIdx < 0) return;
    novelCurPage = 0;
    showNovelReader();
    renderNovelReader();
}

function novelBack() {
    showNovelList();
    novelCurIdx = -1;
}

function renderNovelReader() {
    if (novelCurIdx < 0) return;
    var n = NOVELS[novelCurIdx];
    var paras = n.text.split('\n\n');
    var linesPerPage = 12;
    var totalPages = Math.max(1, Math.ceil(paras.length / linesPerPage));
    if (novelCurPage >= totalPages) novelCurPage = totalPages - 1;
    var start = novelCurPage * linesPerPage, end = Math.min(start + linesPerPage, paras.length);
    var html = '';
    for (var i = start; i < end; i++) {
        html += '<p class="novel-para">' + paras[i].replace(/\n/g, '<br>') + '</p>';
    }
    $('novelReadTitle').innerHTML = n.title;
    $('novelReadAuthor').innerHTML = n.author + ' · ' + n.cat;
    $('novelReadContent').innerHTML = html;
    $('novelReadPageInfo').innerHTML = (novelCurPage + 1) + '/' + totalPages;
    $('novelReadPrev').style.visibility = (novelCurPage > 0 ? 'visible' : 'hidden');
    $('novelReadNext').style.visibility = (novelCurPage < totalPages - 1 ? 'visible' : 'hidden');
}

function novelReadPrev() {
    if (novelCurPage > 0) { novelCurPage--; renderNovelReader(); }
}

function novelReadNext() {
    var n = NOVELS[novelCurIdx];
    var paras = n.text.split('\n\n');
    var totalPages = Math.max(1, Math.ceil(paras.length / 12));
    if (novelCurPage < totalPages - 1) { novelCurPage++; renderNovelReader(); }
}

// ==================== 2048 Game ====================
var g2048Grid, g2048Score, g2048Best, g2048GameOver;
var G2048_SIZE = 4;

function g2048Init() {
    g2048Grid = [[0,0,0,0],[0,0,0,0],[0,0,0,0],[0,0,0,0]];
    g2048Score = 0;
    g2048GameOver = false;
    var s = safeGet('mg_2048_best');
    g2048Best = s ? parseInt(s, 10) : 0;
    $('g2048Score').innerHTML = '0';
    $('g2048Best').innerHTML = g2048Best;
    g2048Add();
    g2048Add();
    g2048Render();
    window._keyCB = g2048Key;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (e.keyCode === 27) { if (g2048GameOver) goHub(); return; }
        if (window._keyCB) window._keyCB(e);
    };
}

function g2048Add() {
    var empty = [];
    for (var r = 0; r < G2048_SIZE; r++)
        for (var c = 0; c < G2048_SIZE; c++)
            if (g2048Grid[r][c] === 0) empty.push([r, c]);
    if (empty.length === 0) return;
    var p = empty[Math.floor(Math.random() * empty.length)];
    g2048Grid[p[0]][p[1]] = Math.random() < 0.9 ? 2 : 4;
}

function g2048Render() {
    var html = '<table>';
    for (var r = 0; r < G2048_SIZE; r++) {
        html += '<tr>';
        for (var c = 0; c < G2048_SIZE; c++) {
            var v = g2048Grid[r][c], cls = 'g2048-cell', tx = '';
            if (v > 0) { cls += ' g2048-tile-' + (v <= 2048 ? v : 'x'); tx = v; }
            html += '<td class="' + cls + '">' + tx + '</td>';
        }
        html += '</tr>';
    }
    html += '</table>';
    $('g2048Board').innerHTML = html;
}

function g2048CanMove() {
    for (var r = 0; r < G2048_SIZE; r++)
        for (var c = 0; c < G2048_SIZE; c++) {
            if (g2048Grid[r][c] === 0) return true;
            if (c < 3 && g2048Grid[r][c] === g2048Grid[r][c+1]) return true;
            if (r < 3 && g2048Grid[r][c] === g2048Grid[r+1][c]) return true;
        }
    return false;
}

function g2048Move(row, col, dr, dc) {
    var arr = [];
    for (var i = 0; i < G2048_SIZE; i++) {
        var r = row + i * dr, c = col + i * dc;
        arr.push(g2048Grid[r][c]);
    }
    var res = [], merged = [];
    for (var j = 0; j < arr.length; j++) {
        if (arr[j] === 0) continue;
        if (res.length > 0 && res[res.length-1] === arr[j] && !merged[res.length-1]) {
            res[res.length-1] *= 2;
            g2048Score += res[res.length-1];
            merged[res.length-1] = true;
        } else {
            res.push(arr[j]);
        }
    }
    while (res.length < G2048_SIZE) res.push(0);
    var changed = false;
    for (var k = 0; k < G2048_SIZE; k++) {
        var r2 = row + k * dr, c2 = col + k * dc;
        if (g2048Grid[r2][c2] !== res[k]) changed = true;
        g2048Grid[r2][c2] = res[k];
    }
    return changed;
}

function g2048Key(e) {
    if (g2048GameOver) return;
    var dr = 0, dc = 0;
    if (e.keyCode === 37) dc = -1;
    else if (e.keyCode === 38) dr = -1;
    else if (e.keyCode === 39) dc = 1;
    else if (e.keyCode === 40) dr = 1;
    else return;
    e.preventDefault && e.preventDefault();
    e.returnValue = false;
    var changed = false;
    if (dr !== 0) {
        for (var c = 0; c < G2048_SIZE; c++) changed = g2048Move(dr === 1 ? 3 : 0, c, -dr, 0) || changed;
    } else {
        for (var r = 0; r < G2048_SIZE; r++) changed = g2048Move(r, dc === 1 ? 3 : 0, 0, -dc) || changed;
    }
    if (changed) {
        g2048Add();
        $('g2048Score').innerHTML = g2048Score;
        if (g2048Score > g2048Best) { g2048Best = g2048Score; $('g2048Best').innerHTML = g2048Best; safeSet('mg_2048_best', g2048Best); }
        g2048Render();
        if (!g2048CanMove()) { g2048GameOver = true; }
    }
}

function g2048Restart() {
    g2048Init();
}

// ==================== Snake Game ====================
var snakeBody, snakeDir, snakeNextDir, snakeFood, snakeScore, snakeBest, snakeTimer, snakeGameOver;
var SNAKE_COLS = 20, SNAKE_ROWS = 20, SNAKE_CELL = 20;

function snakeInit() {
    snakeBody = [[10, 10]];
    snakeDir = [1, 0];
    snakeNextDir = [1, 0];
    snakeScore = 0;
    snakeGameOver = false;
    var s = safeGet('mg_snake_best');
    snakeBest = s ? parseInt(s, 10) : 0;
    $('snakeScore').innerHTML = '0';
    $('snakeBest').innerHTML = snakeBest;
    $('snakeOverlay').style.display = 'none';
    snakePlaceFood();
    snakeDraw();
    if (snakeTimer) clearInterval(snakeTimer);
    window._keyCB = snakeKey;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (snakeGameOver && e.keyCode === 27) goHub();
        else if (window._keyCB) window._keyCB(e);
    };
    snakeTimer = setInterval(snakeTick, 120);
}

function snakePlaceFood() {
    var free = [];
    var occ = {};
    for (var i = 0; i < snakeBody.length; i++) occ[snakeBody[i][0] + '_' + snakeBody[i][1]] = true;
    for (var r = 0; r < SNAKE_ROWS; r++)
        for (var c = 0; c < SNAKE_COLS; c++)
            if (!occ[r + '_' + c]) free.push([r, c]);
    if (free.length > 0) snakeFood = free[Math.floor(Math.random() * free.length)];
}

function snakeDraw() {
    var cvs = document.getElementById('snakeCanvas');
    if (!cvs || !cvs.getContext) return;
    var ctx = cvs.getContext('2d');
    ctx.fillStyle = '#1a1a35';
    ctx.fillRect(0, 0, 400, 400);

    for (var i = 0; i < snakeBody.length; i++) {
        var b = snakeBody[i];
        ctx.fillStyle = (i === 0) ? '#4ecdc4' : '#3aa89f';
        ctx.fillRect(b[1] * SNAKE_CELL + 1, b[0] * SNAKE_CELL + 1, SNAKE_CELL - 2, SNAKE_CELL - 2);
    }

    ctx.fillStyle = '#ff6b6b';
    ctx.fillRect(snakeFood[1] * SNAKE_CELL + 1, snakeFood[0] * SNAKE_CELL + 1, SNAKE_CELL - 2, SNAKE_CELL - 2);
}

function snakeKey(e) {
    if (snakeGameOver) return;
    var nd = snakeDir;
    if (e.keyCode === 37 && snakeDir[0] !== 0) nd = [0, -1];
    else if (e.keyCode === 38 && snakeDir[1] !== 0) nd = [-1, 0];
    else if (e.keyCode === 39 && snakeDir[0] !== 0) nd = [0, 1];
    else if (e.keyCode === 40 && snakeDir[1] !== 0) nd = [1, 0];
    else return;
    snakeNextDir = nd;
    e.preventDefault && e.preventDefault();
    e.returnValue = false;
}

function snakeTick() {
    if (snakeGameOver) return;
    snakeDir = snakeNextDir;
    var head = snakeBody[0];
    var nr = head[0] + snakeDir[0], nc = head[1] + snakeDir[1];

    if (nr < 0 || nr >= SNAKE_ROWS || nc < 0 || nc >= SNAKE_COLS) { snakeEnd(); return; }
    for (var i = 0; i < snakeBody.length; i++)
        if (snakeBody[i][0] === nr && snakeBody[i][1] === nc) { snakeEnd(); return; }

    snakeBody.unshift([nr, nc]);

    if (nr === snakeFood[0] && nc === snakeFood[1]) {
        snakeScore += 10;
        $('snakeScore').innerHTML = snakeScore;
        if (snakeScore > snakeBest) { snakeBest = snakeScore; $('snakeBest').innerHTML = snakeBest; safeSet('mg_snake_best', snakeBest); }
        snakePlaceFood();
    } else {
        snakeBody.pop();
    }
    snakeDraw();
}

function snakeEnd() {
    snakeGameOver = true;
    if (snakeTimer) { clearInterval(snakeTimer); snakeTimer = 0; }
    $('snakeOverlay').style.display = 'block';
    $('snakeOverlayTitle').innerHTML = '游戏结束\n得分: ' + snakeScore;
    window._keyCB = null;
}

function snakeRestart() {
    var cvs = document.getElementById('snakeCanvas');
    if (cvs && cvs.getContext) { var ctx = cvs.getContext('2d'); ctx.fillStyle = '#1a1a35'; ctx.fillRect(0, 0, 400, 400); }
    if (snakeTimer) clearInterval(snakeTimer);
    snakeInit();
}

// ==================== Tetris Game ====================
var tetrisBoard, tetrisPiece, tetrisPieceX, tetrisPieceY, tetrisNextPiece;
var tetrisScore, tetrisLines, tetrisLevel, tetrisTimer, tetrisGameOver;
var TETRIS_COLS = 10, TETRIS_ROWS = 20, TETRIS_CELL = 24;

var TETRIS_SHAPES = [
    [[1,1,1,1]],                         // I
    [[1,1],[1,1]],                       // O
    [[0,1,0],[1,1,1]],                   // T
    [[1,0,0],[1,1,1]],                   // L
    [[0,0,1],[1,1,1]],                   // J
    [[0,1,1],[1,1,0]],                   // S
    [[1,1,0],[0,1,1]]                    // Z
];

var TETRIS_COLORS = ['#4ecdc4','#f0d040','#c080f0','#f08040','#4080f0','#40f040','#f04040'];

function tetrisInit() {
    tetrisBoard = [];
    for (var r = 0; r < TETRIS_ROWS; r++) { tetrisBoard[r] = []; for (var c = 0; c < TETRIS_COLS; c++) tetrisBoard[r][c] = 0; }
    tetrisScore = 0; tetrisLines = 0; tetrisLevel = 1; tetrisGameOver = false;
    $('tetrisScore').innerHTML = '0'; $('tetrisLines').innerHTML = '0'; $('tetrisLevel').innerHTML = '1';
    $('tetrisOverlay').style.display = 'none';
    tetrisNextPiece = tetrisRand();
    tetrisSpawn();
    tetrisDraw();
    if (tetrisTimer) clearInterval(tetrisTimer);
    window._keyCB = tetrisKey;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (e.keyCode === 27) goHub();
        else if (window._keyCB) window._keyCB(e);
    };
    tetrisTimer = setInterval(tetrisTick, 500);
}

function tetrisRand() {
    var i = Math.floor(Math.random() * TETRIS_SHAPES.length);
    return { shape: TETRIS_SHAPES[i], color: TETRIS_COLORS[i] };
}

function tetrisSpawn() {
    tetrisPiece = tetrisNextPiece;
    tetrisNextPiece = tetrisRand();
    tetrisPieceX = Math.floor((TETRIS_COLS - tetrisPiece.shape[0].length) / 2);
    tetrisPieceY = 0;
}

function tetrisRotate(s) {
    var rows = s.length, cols = s[0].length;
    var r = [];
    for (var c = 0; c < cols; c++) { r[c] = []; for (var rr = rows - 1; rr >= 0; rr--) r[c].push(s[rr][c]); }
    return r;
}

function tetrisCollision(shape, px, py) {
    for (var r = 0; r < shape.length; r++)
        for (var c = 0; c < shape[r].length; c++) {
            if (!shape[r][c]) continue;
            var nr = py + r, nc = px + c;
            if (nc < 0 || nc >= TETRIS_COLS || nr >= TETRIS_ROWS) return true;
            if (nr >= 0 && tetrisBoard[nr][nc]) return true;
        }
    return false;
}

function tetrisPlace() {
    for (var r = 0; r < tetrisPiece.shape.length; r++)
        for (var c = 0; c < tetrisPiece.shape[r].length; c++) {
            if (!tetrisPiece.shape[r][c]) continue;
            var nr = tetrisPieceY + r, nc = tetrisPieceX + c;
            if (nr < 0) { tetrisEnd(); return; }
            tetrisBoard[nr][nc] = tetrisPiece.color;
        }
    var cleared = 0;
    for (var row = TETRIS_ROWS - 1; row >= 0; row--) {
        var full = true;
        for (var c = 0; c < TETRIS_COLS; c++) if (!tetrisBoard[row][c]) { full = false; break; }
        if (full) {
            tetrisBoard.splice(row, 1);
            tetrisBoard.unshift([]);
            for (var cc = 0; cc < TETRIS_COLS; cc++) tetrisBoard[0][cc] = 0;
            cleared++;
            row++;
        }
    }
    if (cleared > 0) {
        var pts = [0, 100, 300, 500, 800];
        tetrisScore += pts[Math.min(cleared, 4)] * tetrisLevel;
        tetrisLines += cleared;
        tetrisLevel = Math.floor(tetrisLines / 10) + 1;
        $('tetrisScore').innerHTML = tetrisScore;
        $('tetrisLines').innerHTML = tetrisLines;
        $('tetrisLevel').innerHTML = tetrisLevel;
        if (tetrisTimer) { clearInterval(tetrisTimer); tetrisTimer = setInterval(tetrisTick, Math.max(80, 500 - (tetrisLevel - 1) * 40)); }
    }
    tetrisSpawn();
    if (tetrisCollision(tetrisPiece.shape, tetrisPieceX, tetrisPieceY)) tetrisEnd();
}

function tetrisEnd() {
    tetrisGameOver = true;
    if (tetrisTimer) { clearInterval(tetrisTimer); tetrisTimer = 0; }
    $('tetrisOverlay').style.display = 'block';
    $('tetrisOverlayTitle').innerHTML = '游戏结束 得分: ' + tetrisScore;
    window._keyCB = null;
}

function tetrisTick() {
    if (tetrisGameOver) return;
    if (!tetrisCollision(tetrisPiece.shape, tetrisPieceX, tetrisPieceY + 1)) {
        tetrisPieceY++;
    } else {
        tetrisPlace();
    }
    tetrisDraw();
}

function tetrisKey(e) {
    if (tetrisGameOver) return;
    if (e.keyCode === 37) {
        if (!tetrisCollision(tetrisPiece.shape, tetrisPieceX - 1, tetrisPieceY)) tetrisPieceX--;
    } else if (e.keyCode === 39) {
        if (!tetrisCollision(tetrisPiece.shape, tetrisPieceX + 1, tetrisPieceY)) tetrisPieceX++;
    } else if (e.keyCode === 40) {
        if (!tetrisCollision(tetrisPiece.shape, tetrisPieceX, tetrisPieceY + 1)) tetrisPieceY++;
    } else if (e.keyCode === 38) {
        var rot = tetrisRotate(tetrisPiece.shape);
        if (!tetrisCollision(rot, tetrisPieceX, tetrisPieceY)) tetrisPiece.shape = rot;
    } else if (e.keyCode === 32) {
        while (!tetrisCollision(tetrisPiece.shape, tetrisPieceX, tetrisPieceY + 1)) tetrisPieceY++;
        tetrisPlace();
    } else return;
    e.preventDefault && e.preventDefault();
    e.returnValue = false;
    tetrisDraw();
}

function tetrisDraw() {
    var cvs = document.getElementById('tetrisCanvas');
    if (!cvs || !cvs.getContext) return;
    var ctx = cvs.getContext('2d');
    ctx.fillStyle = '#1a1a35';
    ctx.fillRect(0, 0, 240, 480);

    for (var r = 0; r < TETRIS_ROWS; r++)
        for (var c = 0; c < TETRIS_COLS; c++) {
            if (tetrisBoard[r][c]) {
                ctx.fillStyle = tetrisBoard[r][c];
                ctx.fillRect(c * TETRIS_CELL + 1, r * TETRIS_CELL + 1, TETRIS_CELL - 2, TETRIS_CELL - 2);
            }
        }

    if (tetrisPiece && !tetrisGameOver) {
        ctx.fillStyle = tetrisPiece.color;
        for (var pr = 0; pr < tetrisPiece.shape.length; pr++)
            for (var pc = 0; pc < tetrisPiece.shape[pr].length; pc++) {
                if (!tetrisPiece.shape[pr][pc]) continue;
                var y = (tetrisPieceY + pr) * TETRIS_CELL + 1;
                if (y < 0) continue;
                ctx.fillRect((tetrisPieceX + pc) * TETRIS_CELL + 1, y, TETRIS_CELL - 2, TETRIS_CELL - 2);
            }
    }

    var nxt = document.getElementById('tetrisNext');
    if (nxt && nxt.getContext && tetrisNextPiece) {
        var nctx = nxt.getContext('2d');
        nctx.fillStyle = '#1a1a35';
        nctx.fillRect(0, 0, 120, 120);
        nctx.fillStyle = tetrisNextPiece.color;
        var sh = tetrisNextPiece.shape;
        var offX = Math.floor((120 - sh[0].length * TETRIS_CELL) / 2);
        var offY = Math.floor((120 - sh.length * TETRIS_CELL) / 2);
        for (var nr = 0; nr < sh.length; nr++)
            for (var nc = 0; nc < sh[nr].length; nc++) {
                if (!sh[nr][nc]) continue;
                nctx.fillRect(offX + nc * TETRIS_CELL + 2, offY + nr * TETRIS_CELL + 2, TETRIS_CELL - 4, TETRIS_CELL - 4);
            }
    }
}

function tetrisRestart() {
    if (tetrisTimer) clearInterval(tetrisTimer);
    tetrisInit();
}

// ==================== Doodle Jump ====================
var doodleAnimId, doodleRunning, doodleScore, doodleBest;
var doodlePlayer, doodlePlatforms, doodleCamY, doodleStartCamY;
var doodleKeys = {left: false, right: false};
var doodleTime = 0;
var DW = 480, DH = 600, DGRAV = 0.45, DJUMP = -12, DMOVE = 5;
var DPLAT_MINW = 60, DPLAT_MAXW = 120, DPLAT_H = 14;
var DPLAT_GAP_MIN = 50, DPLAT_GAP_MAX = 120;
var DPLAYER_W = 36, DPLAYER_H = 40;
var DCAM_LEAD = 0.4;

function doodleInit() {
    doodleRunning = false;
    doodleScore = 0;
    doodleTime = 0;
    doodleCamY = 0;
    doodleKeys = {left: false, right: false};
    var s = safeGet('mg_doodle_best');
    doodleBest = s ? parseInt(s, 10) : 0;
    $('doodleScore').innerHTML = '0';
    $('doodleBest').innerHTML = doodleBest;
    $('doodleOverlay').style.display = 'block';
    $('doodleGOverlay').style.display = 'none';
    doodleGeneratePlatforms();
    doodleDrawFrame();
    window._keyCB = null;
    document.onkeydown = function(e) {
        e = e || window.event;
        if (e.keyCode === 27) goHub();
        else if (doodleRunning) doodleKeyDown(e);
    };
    document.onkeyup = function(e) {
        e = e || window.event;
        if (doodleRunning) doodleKeyUp(e);
    };
}

function doodleKeyDown(e) {
    if (e.keyCode === 37 || e.keyCode === 65) { doodleKeys.left = true; e.returnValue = false; }
    if (e.keyCode === 39 || e.keyCode === 68) { doodleKeys.right = true; e.returnValue = false; }
}

function doodleKeyUp(e) {
    if (e.keyCode === 37 || e.keyCode === 65) doodleKeys.left = false;
    if (e.keyCode === 39 || e.keyCode === 68) doodleKeys.right = false;
}

function doodleMakePlatform(x, y, w, type) {
    var t = type || 'normal';
    var dir = 0, range = 0;
    if (t === 'moving') { dir = Math.random() > 0.5 ? 1 : -1; range = 40 + Math.random() * 40; }
    return {x: x, y: y, w: w, h: DPLAT_H, type: t, dir: dir, range: range, sx: x};
}

function doodleGeneratePlatforms() {
    doodlePlatforms = [];
    var y = DH - 80;
    for (var i = 0; i < 10; i++) {
        var w = DPLAT_MINW + Math.random() * (DPLAT_MAXW - DPLAT_MINW);
        var x = Math.random() * (DW - w);
        var type = 'normal';
        if (i === 0) {
            x = (DW - w) / 2;
        } else {
            var r = Math.random();
            if (r < 0.15) type = 'break';
            else if (r < 0.35) type = 'moving';
        }
        doodlePlatforms.push(doodleMakePlatform(x, y, w, type));
        y -= DPLAT_GAP_MIN + Math.random() * (DPLAT_GAP_MAX - DPLAT_GAP_MIN);
    }
    var first = doodlePlatforms[0];
    doodlePlayer = {x: (DW - DPLAYER_W) / 2, y: first.y - DPLAYER_H - 2, vx: 0, vy: 0, w: DPLAYER_W, h: DPLAYER_H};
    doodleStartCamY = doodlePlayer.y - DH * DCAM_LEAD;
}

function doodleAddPlatformsAbove(topY) {
    var lastY = doodlePlatforms.length ? doodlePlatforms[doodlePlatforms.length - 1].y : topY;
    for (var i = 0; i < doodlePlatforms.length; i++) {
        if (doodlePlatforms[i].y < lastY) lastY = doodlePlatforms[i].y;
    }
    while (lastY > topY - DH - 200) {
        lastY -= DPLAT_GAP_MIN + Math.random() * (DPLAT_GAP_MAX - DPLAT_GAP_MIN);
        var w = DPLAT_MINW + Math.random() * (DPLAT_MAXW - DPLAT_MINW);
        var x = Math.random() * (DW - w);
        var r = Math.random();
        var type = 'normal';
        if (r < 0.12) type = 'break';
        else if (r < 0.32) type = 'moving';
        doodlePlatforms.push(doodleMakePlatform(x, lastY, w, type));
    }
}

function doodleStart() {
    $('doodleOverlay').style.display = 'none';
    $('doodleGOverlay').style.display = 'none';
    doodleRunning = true;
    doodleScore = 0;
    doodleTime = 0;
    doodleCamY = 0;
    doodleKeys = {left: false, right: false};
    $('doodleScore').innerHTML = '0';
    doodleGeneratePlatforms();
    doodleLoop();
}

function doodleLoop() {
    if (!doodleRunning) return;
    doodleTime++;
    if (doodleKeys.left) doodlePlayer.vx = -DMOVE;
    else if (doodleKeys.right) doodlePlayer.vx = DMOVE;
    else doodlePlayer.vx *= 0.85;
    doodlePlayer.x += doodlePlayer.vx;
    if (doodlePlayer.x < 0) doodlePlayer.x = 0;
    if (doodlePlayer.x > DW - doodlePlayer.w) doodlePlayer.x = DW - doodlePlayer.w;
    doodlePlayer.vy += DGRAV;
    doodlePlayer.y += doodlePlayer.vy;

    for (var i = doodlePlatforms.length - 1; i >= 0; i--) {
        var p = doodlePlatforms[i];
        if (p.type === 'moving') {
            p.x = p.sx + Math.sin((doodleTime + p.sx) * 0.03) * p.range * p.dir;
            if (p.x < 0) p.x = 0;
            if (p.x > DW - p.w) p.x = DW - p.w;
        }
        if (p.y - doodleCamY > DH + 100) { doodlePlatforms.splice(i, 1); continue; }
        var pb = doodlePlayer.y + doodlePlayer.h;
        var pt = p.y;
        var ox = doodlePlayer.x + doodlePlayer.w > p.x && doodlePlayer.x < p.x + p.w;
        if (ox && pb >= pt - 2 && pb <= pt + 12 && doodlePlayer.vy >= 0) {
            doodlePlayer.vy = DJUMP;
            doodlePlayer.y = pt - doodlePlayer.h - 1;
            if (p.type === 'break') doodlePlatforms.splice(i, 1);
        }
    }

    var targetCamY = doodlePlayer.y - DH * DCAM_LEAD;
    if (targetCamY < doodleCamY) {
        doodleCamY = targetCamY;
        var ns = Math.max(0, Math.floor((doodleStartCamY - doodleCamY) / 8));
        if (ns > doodleScore) {
            doodleScore = ns;
            $('doodleScore').innerHTML = doodleScore;
            if (doodleScore > doodleBest) {
                doodleBest = doodleScore;
                $('doodleBest').innerHTML = doodleBest;
                safeSet('mg_doodle_best', String(doodleBest));
            }
        }
        doodleAddPlatformsAbove(doodleCamY);
    }

    if (doodlePlayer.y - doodleCamY > DH + 50) {
        doodleEnd();
        return;
    }

    doodleDrawFrame();
    doodleAnimId = requestAnimationFrame(doodleLoop);
}

function doodleEnd() {
    doodleRunning = false;
    if (doodleAnimId) cancelAnimationFrame(doodleAnimId);
    doodleDrawFrame();
    $('doodleFinalScore').innerHTML = doodleScore;
    $('doodleGOverlay').style.display = 'block';
    window._keyCB = null;
}

function doodleDrawFrame() {
    var cvs = document.getElementById('doodleCanvas');
    if (!cvs || !cvs.getContext) return;
    var ctx = cvs.getContext('2d');
    ctx.clearRect(0, 0, DW, DH);

    for (var i = 0; i < doodlePlatforms.length; i++) {
        var p = doodlePlatforms[i];
        var py = p.y - doodleCamY;
        if (py < -DPLAT_H - 20 || py > DH + 50) continue;
        if (p.type === 'normal') { ctx.fillStyle = '#6bcb77'; ctx.strokeStyle = '#4ade80'; }
        else if (p.type === 'break') { ctx.fillStyle = '#c9a959'; ctx.strokeStyle = '#b8860b'; }
        else { ctx.fillStyle = '#4d96ff'; ctx.strokeStyle = '#6eb5ff'; }
        ctx.lineWidth = 2;
        doodleRoundRect(ctx, p.x, py, p.w, p.h, 6);
        ctx.fill();
        ctx.stroke();
    }

    var py2 = doodlePlayer.y - doodleCamY;
    if (py2 >= -doodlePlayer.h - 20 && py2 <= DH + 20) {
        var px = doodlePlayer.x;
        ctx.save();
        ctx.fillStyle = '#2d3436';
        doodleRoundRect(ctx, px, py2, doodlePlayer.w, doodlePlayer.h, 8);
        ctx.fill();
        ctx.fillStyle = '#fff';
        ctx.beginPath();
        ctx.arc(px + 12, py2 + 14, 6, 0, Math.PI * 2);
        ctx.arc(px + doodlePlayer.w - 12, py2 + 14, 6, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = '#2d3436';
        ctx.beginPath();
        ctx.arc(px + 12, py2 + 14, 3, 0, Math.PI * 2);
        ctx.arc(px + doodlePlayer.w - 12, py2 + 14, 3, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }
}

function doodleRoundRect(ctx, x, y, w, h, r) {
    if (r > w/2) r = w/2;
    if (r > h/2) r = h/2;
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.quadraticCurveTo(x + w, y, x + w, y + r);
    ctx.lineTo(x + w, y + h - r);
    ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    ctx.lineTo(x + r, y + h);
    ctx.quadraticCurveTo(x, y + h, x, y + h - r);
    ctx.lineTo(x, y + r);
    ctx.quadraticCurveTo(x, y, x + r, y);
    ctx.closePath();
}

// ==================== Connect Four ====================
var c4Grid, c4Player, c4GameOver;
var C4_ROWS = 6, C4_COLS = 7;

function c4Init() {
    c4Grid = [];
    for (var r = 0; r < C4_ROWS; r++) {
        c4Grid[r] = [];
        for (var c = 0; c < C4_COLS; c++) c4Grid[r][c] = 0;
    }
    c4Player = 1;
    c4GameOver = false;
    $('c4Msg').innerHTML = '';
    c4UpdateHint();
    c4Render();
    window._keyCB = null;
    document.onkeydown = function(e) { e = e || window.event; if (e.keyCode === 27) goHub(); };
}

function c4UpdateHint() {
    var col = c4Player === 1 ? '#4ecdc4' : '#ff6b6b';
    var name = c4Player === 1 ? '绿方' : '红方';
    $('c4PlayerHint').innerHTML = '当前：<b style=\"color:' + col + ';\">' + name + '</b>';
}

function c4Render() {
    var html = '<table>';
    for (var r = 0; r < C4_ROWS; r++) {
        html += '<tr>';
        for (var c = 0; c < C4_COLS; c++) {
            var cls = 'c4-cell';
            if (c4Grid[r][c] === 1) cls += ' c4-green c4-taken';
            else if (c4Grid[r][c] === 2) cls += ' c4-red c4-taken';
            html += '<td class=\"' + cls + '\" onclick=\"c4Click(' + c + ')\"></td>';
        }
        html += '</tr>';
    }
    html += '</table>';
    $('c4Grid').innerHTML = html;
}

function c4Click(col) {
    if (c4GameOver) return;
    var row = -1;
    for (var r = C4_ROWS - 1; r >= 0; r--) {
        if (c4Grid[r][col] === 0) { row = r; break; }
    }
    if (row === -1) return;

    c4Grid[row][col] = c4Player;

    if (c4CheckWin(row, col)) {
        c4GameOver = true;
        var winner = c4Player === 1 ? '🟢 绿方获胜！' : '🔴 红方获胜！';
        $('c4Msg').innerHTML = '<span class=\"win\">' + winner + '</span>';
        c4Render();
        return;
    }

    var full = true;
    for (var c = 0; c < C4_COLS; c++) if (c4Grid[0][c] === 0) { full = false; break; }
    if (full) {
        c4GameOver = true;
        $('c4Msg').innerHTML = '<span style=\"color:#c9a820;\">🤝 平局！</span>';
        c4Render();
        return;
    }

    c4Player = c4Player === 1 ? 2 : 1;
    c4UpdateHint();
    c4Render();
}

function c4CheckWin(row, col) {
    var p = c4Grid[row][col];
    var count;

    count = 0;
    for (var c = 0; c < C4_COLS; c++) {
        if (c4Grid[row][c] === p) { count++; if (count === 4) return true; }
        else count = 0;
    }
    count = 0;
    for (var r = 0; r < C4_ROWS; r++) {
        if (c4Grid[r][col] === p) { count++; if (count === 4) return true; }
        else count = 0;
    }
    count = 0;
    var sr, sc;
    if (row >= col) { sr = row - col; sc = 0; }
    else { sr = 0; sc = col - row; }
    for (; sr <= 5 && sc <= 6; sr++, sc++) {
        if (sr >= 0 && sc >= 0 && sr < C4_ROWS && sc < C4_COLS && c4Grid[sr][sc] === p) { count++; if (count === 4) return true; }
        else count = 0;
    }
    count = 0;
    if (row + col <= 5) { sr = row + col; sc = 0; }
    else { sr = 5; sc = row + col - 5; }
    for (; sr >= 0 && sc <= 6; sr--, sc++) {
        if (sr >= 0 && sc >= 0 && sr < C4_ROWS && sc < C4_COLS && c4Grid[sr][sc] === p) { count++; if (count === 4) return true; }
        else count = 0;
    }
    return false;
}

function c4Restart() {
    c4Init();
}

// ==================== Pac-Man ====================
var pmCanvas, pmCtx;
var pmTileSize = 16;
var pmCorridorScale = 1.25;
var pmCorridorTile;
var pmCols = 28;
var pmRows = 31;
var pmMap = [];
var pmPelletsRemaining = 0;
var pmPacman = { x: 1, y: 29, dirX: 0, dirY: 0, nextDirX: 0, nextDirY: 0, speed: 8, radius: 0 };
var pmGhosts = [];
var pmScore = 0;
var pmLives = 3;
var pmGameOver = false;
var pmLastTime = 0;
var pmGameTime = 0;
var pmAnimId = null;
var pmAudioCtx = null;

var pmLevelLayout = [
    "1111111111111111111111111111",
    "1222222222112222222222222221",
    "1211112112112112112111112121",
    "1311112112112112112111112131",
    "1222222222222222222222222221",
    "1211112111112111112111112121",
    "1222222112222222211222222221",
    "1111112112111112112111111111",
    "0000012112110002112110000000",
    "1111112112111112112111111111",
    "1222222222222112222222222221",
    "1211112111112111112111112121",
    "1222212222222222222222122221",
    "1111212111110001111122111111",
    "0000212110000000000122110000",
    "1111212110111111101122111111",
    "1222222220222222202222222221",
    "1211112112111112112111112121",
    "1222222112222222211222222221",
    "1111112112111112112111111111",
    "0000012112110002112110000000",
    "1111112112111112112111111111",
    "1222222222222222222222222221",
    "1211112111112111112111112121",
    "1311112222222112222222112131",
    "1222222111112111111122222221",
    "1111112112222222212111111111",
    "1222222222112222112222222221",
    "1211111112112112111111112121",
    "1222222222222222222222222221",
    "1111111111111111111111111111"
];

function pmGetAudioContext() {
    if (pmAudioCtx) return pmAudioCtx;
    var Ctx = window.AudioContext || window.webkitAudioContext;
    if (!Ctx) return null;
    pmAudioCtx = new Ctx();
    return pmAudioCtx;
}

function pmPlayPelletSound() {
    var ctx = pmGetAudioContext();
    if (!ctx) return;
    try {
        ctx.resume();
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.type = "square";
        osc.frequency.setValueAtTime(680, ctx.currentTime);
        gain.gain.setValueAtTime(0.12, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.06);
        osc.start(ctx.currentTime);
        osc.stop(ctx.currentTime + 0.06);
    } catch (e) {}
}

function pmPlayDeathSound() {
    var ctx = pmGetAudioContext();
    if (!ctx) return;
    try {
        ctx.resume();
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.type = "sawtooth";
        osc.frequency.setValueAtTime(220, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(55, ctx.currentTime + 0.6);
        gain.gain.setValueAtTime(0.15, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.6);
        osc.start(ctx.currentTime);
        osc.stop(ctx.currentTime + 0.6);
    } catch (e) {}
}

function pmStartAudio() {
    var ctx = pmGetAudioContext();
    if (!ctx) return;
    try { ctx.resume(); } catch (e) {}
}

function pmInitMap() {
    pmMap = [];
    pmPelletsRemaining = 0;
    for (var r = 0; r < pmRows; r++) {
        var row = [];
        for (var c = 0; c < pmCols; c++) {
            var val = parseInt(pmLevelLayout[r].charAt(c), 10);
            if (val === 2 || val === 3) pmPelletsRemaining++;
            row.push(val);
        }
        pmMap.push(row);
    }
}

function pmResetEntities() {
    pmPacman.x = 1;
    pmPacman.y = 29;
    pmPacman.dirX = 0;
    pmPacman.dirY = 0;
    pmPacman.nextDirX = 0;
    pmPacman.nextDirY = 0;
    pmGhosts[0].x = 13;
    pmGhosts[0].y = 14;
    pmGhosts[0].dirX = 1;
    pmGhosts[0].dirY = 0;
    pmGhosts[1].x = 14;
    pmGhosts[1].y = 14;
    pmGhosts[1].dirX = -1;
    pmGhosts[1].dirY = 0;
}

function pmRestart() {
    pmStartAudio();
    pmScore = 0;
    pmLives = 3;
    pmGameOver = false;
    pmInitMap();
    pmResetEntities();
    pmUpdateUI();
    $('pmMsg').innerHTML = '';
}

function pmUpdateUI() {
    $('pmScore').innerHTML = pmScore;
    $('pmLives').innerHTML = pmLives;
}

function pmIsWall(col, row) {
    if (row < 0 || row >= pmRows || col < 0 || col >= pmCols) return true;
    return pmMap[row][col] === 1;
}

function pmHandleInput() {
    var centerCol = Math.round(pmPacman.x);
    var centerRow = Math.round(pmPacman.y);
    var offsetX = Math.abs(pmPacman.x - centerCol);
    var offsetY = Math.abs(pmPacman.y - centerRow);
    var aligned = offsetX < 0.35 && offsetY < 0.35;
    var stopped = pmPacman.dirX === 0 && pmPacman.dirY === 0;
    if (aligned || stopped) {
        var targetCol = centerCol + pmPacman.nextDirX;
        var targetRow = centerRow + pmPacman.nextDirY;
        if (!pmIsWall(targetCol, targetRow)) {
            pmPacman.dirX = pmPacman.nextDirX;
            pmPacman.dirY = pmPacman.nextDirY;
        }
    }
}

function pmMovePacman(deltaSeconds) {
    pmHandleInput();
    var speedPerFrame = pmPacman.speed * deltaSeconds;
    var newX = pmPacman.x + pmPacman.dirX * speedPerFrame;
    var newY = pmPacman.y + pmPacman.dirY * speedPerFrame;
    if (newX < 0) newX = pmCols - 1;
    if (newX > pmCols - 1) newX = 0;
    var nextCol = Math.round(newX);
    var nextRow = Math.round(newY);
    if (pmIsWall(nextCol, nextRow)) {
        var dx = pmPacman.dirX;
        var dy = pmPacman.dirY;
        pmPacman.dirX = 0;
        pmPacman.dirY = 0;
        pmPacman.x = nextCol - dx;
        pmPacman.y = nextRow - dy;
        return;
    }
    pmPacman.x = newX;
    pmPacman.y = newY;
    var col = Math.round(pmPacman.x);
    var row = Math.round(pmPacman.y);
    if (pmMap[row] && (pmMap[row][col] === 2 || pmMap[row][col] === 3)) {
        pmPlayPelletSound();
        if (pmMap[row][col] === 2) pmScore += 10;
        if (pmMap[row][col] === 3) pmScore += 50;
        pmMap[row][col] = 0;
        pmPelletsRemaining--;
        pmUpdateUI();
        if (pmPelletsRemaining <= 0) {
            pmGameOver = true;
            $('pmMsg').innerHTML = '🎉 你吃光了所有豆子！';
        }
    }
}

function pmMoveGhost(ghost, deltaSeconds) {
    var speed = 6 * deltaSeconds;
    var newX = ghost.x + ghost.dirX * speed;
    var newY = ghost.y + ghost.dirY * speed;
    var nextCol = Math.round(newX);
    var nextRow = Math.round(newY);
    if (pmIsWall(nextCol, nextRow)) {
        var dirs = [
            { x: 1, y: 0 },
            { x: -1, y: 0 },
            { x: 0, y: 1 },
            { x: 0, y: -1 }
        ];
        var currentOppX = -ghost.dirX;
        var currentOppY = -ghost.dirY;
        var valid = [];
        for (var i = 0; i < dirs.length; i++) {
            var d = dirs[i];
            if (!(d.x === currentOppX && d.y === currentOppY)) valid.push(d);
        }
        var choice = valid[Math.floor(Math.random() * valid.length)];
        ghost.dirX = choice.x;
        ghost.dirY = choice.y;
        return;
    }
    ghost.x = newX;
    ghost.y = newY;
    if (ghost.x < 0) ghost.x = pmCols - 1;
    if (ghost.x > pmCols - 1) ghost.x = 0;
}

function pmDistance(a, b) {
    var dx = a.x - b.x;
    var dy = a.y - b.y;
    return Math.sqrt(dx * dx + dy * dy);
}

function pmCheckCollisions() {
    for (var i = 0; i < pmGhosts.length; i++) {
        var ghost = pmGhosts[i];
        if (pmDistance(ghost, pmPacman) < 0.7) {
            pmPlayDeathSound();
            pmLives--;
            pmUpdateUI();
            if (pmLives <= 0) {
                pmGameOver = true;
                $('pmMsg').innerHTML = '👻 游戏结束！分数：' + pmScore;
            }
            pmResetEntities();
            break;
        }
    }
}

function pmDrawMap() {
    for (var r = 0; r < pmRows; r++) {
        for (var c = 0; c < pmCols; c++) {
            var val = pmMap[r][c];
            var x = c * pmCorridorTile;
            var y = r * pmCorridorTile;
            if (val === 1) {
                var wallX = x + (pmCorridorTile - pmTileSize) / 2;
                var wallY = y + (pmCorridorTile - pmTileSize) / 2;
                pmCtx.fillStyle = '#001b4d';
                pmCtx.fillRect(wallX, wallY, pmTileSize, pmTileSize);
                pmCtx.strokeStyle = '#0ff';
                pmCtx.lineWidth = 2;
                pmCtx.strokeRect(wallX + 2, wallY + 2, pmTileSize - 4, pmTileSize - 4);
            } else {
                pmCtx.fillStyle = '#000016';
                pmCtx.fillRect(x, y, pmCorridorTile, pmCorridorTile);
                if (val === 2) {
                    pmCtx.fillStyle = '#ffd966';
                    pmCtx.beginPath();
                    pmCtx.arc(x + pmCorridorTile / 2, y + pmCorridorTile / 2, 2, 0, Math.PI * 2);
                    pmCtx.fill();
                } else if (val === 3) {
                    pmCtx.fillStyle = '#ffd966';
                    pmCtx.beginPath();
                    pmCtx.arc(x + pmCorridorTile / 2, y + pmCorridorTile / 2, 4, 0, Math.PI * 2);
                    pmCtx.fill();
                }
            }
        }
    }
}

function pmDrawPacman() {
    var px = pmPacman.x * pmCorridorTile + pmCorridorTile / 2;
    var py = pmPacman.y * pmCorridorTile + pmCorridorTile / 2;
    var angleOffset;
    if (pmPacman.dirX === 1) angleOffset = 0;
    else if (pmPacman.dirX === -1) angleOffset = Math.PI;
    else if (pmPacman.dirY === -1) angleOffset = -Math.PI / 2;
    else if (pmPacman.dirY === 1) angleOffset = Math.PI / 2;
    else angleOffset = 0;
    var chompSpeed = 18;
    var mouthOpen = 0.08 + 0.28 * (0.5 + 0.5 * Math.sin(pmGameTime * chompSpeed));
    pmCtx.fillStyle = '#ffd966';
    pmCtx.beginPath();
    pmCtx.moveTo(px, py);
    pmCtx.arc(px, py, pmCorridorTile * 0.6, angleOffset + mouthOpen, angleOffset + Math.PI * 2 - mouthOpen);
    pmCtx.closePath();
    pmCtx.fill();
}

function pmDrawGhost(ghost) {
    var gx = (Math.round(ghost.x) * pmCorridorTile) + pmCorridorTile / 2;
    var gy = (Math.round(ghost.y) * pmCorridorTile) + pmCorridorTile / 2;
    var r = pmCorridorTile * 0.6;
    pmCtx.fillStyle = ghost.color;
    pmCtx.beginPath();
    pmCtx.arc(gx, gy, r, Math.PI, 0);
    pmCtx.lineTo(gx + r, gy + r);
    pmCtx.lineTo(gx - r, gy + r);
    pmCtx.closePath();
    pmCtx.fill();
    pmCtx.fillStyle = '#fff';
    pmCtx.beginPath();
    pmCtx.arc(gx - r / 3, gy - r / 4, r / 4, 0, Math.PI * 2);
    pmCtx.arc(gx + r / 3, gy - r / 4, r / 4, 0, Math.PI * 2);
    pmCtx.fill();
    pmCtx.fillStyle = '#000';
    pmCtx.beginPath();
    pmCtx.arc(gx - r / 3, gy - r / 4, r / 8, 0, Math.PI * 2);
    pmCtx.arc(gx + r / 3, gy - r / 4, r / 8, 0, Math.PI * 2);
    pmCtx.fill();
}

function pmDraw() {
    pmCtx.clearRect(0, 0, pmCanvas.width, pmCanvas.height);
    pmDrawMap();
    pmDrawPacman();
    for (var i = 0; i < pmGhosts.length; i++) {
        pmDrawGhost(pmGhosts[i]);
    }
}

function pmLoop(timestamp) {
    if (!pmLastTime) pmLastTime = timestamp;
    var delta = (timestamp - pmLastTime) / 1000;
    pmLastTime = timestamp;
    pmGameTime = timestamp / 1000;
    if (!pmGameOver) {
        pmMovePacman(delta);
        for (var i = 0; i < pmGhosts.length; i++) {
            pmMoveGhost(pmGhosts[i], delta);
        }
        pmCheckCollisions();
    }
    pmDraw();
    pmAnimId = requestAnimationFrame(pmLoop);
}

function pmOnKeyDown(e) {
    pmStartAudio();
    var key = e.key || e.keyCode;
    if (key === 'ArrowUp' || key === 'w' || key === 'W' || key === 38) {
        e.preventDefault ? e.preventDefault() : e.returnValue = false;
        pmPacman.nextDirX = 0;
        pmPacman.nextDirY = -1;
    } else if (key === 'ArrowDown' || key === 's' || key === 'S' || key === 40) {
        e.preventDefault ? e.preventDefault() : e.returnValue = false;
        pmPacman.nextDirX = 0;
        pmPacman.nextDirY = 1;
    } else if (key === 'ArrowLeft' || key === 'a' || key === 'A' || key === 37) {
        e.preventDefault ? e.preventDefault() : e.returnValue = false;
        pmPacman.nextDirX = -1;
        pmPacman.nextDirY = 0;
    } else if (key === 'ArrowRight' || key === 'd' || key === 'D' || key === 39) {
        e.preventDefault ? e.preventDefault() : e.returnValue = false;
        pmPacman.nextDirX = 1;
        pmPacman.nextDirY = 0;
    }
}

function pmInit() {
    pmCanvas = $('pmCanvas');
    pmCtx = pmCanvas.getContext('2d');
    pmCorridorTile = Math.round(pmTileSize * pmCorridorScale);
    pmCanvas.width = pmCols * pmCorridorTile;
    pmCanvas.height = pmRows * pmCorridorTile;
    pmScore = 0;
    pmLives = 3;
    pmGameOver = false;
    pmLastTime = 0;
    pmGameTime = 0;
    pmInitMap();
    pmGhosts = [
        { x: 13, y: 14, dirX: 1, dirY: 0, color: '#ff4b4b' },
        { x: 14, y: 14, dirX: -1, dirY: 0, color: '#4bc6ff' }
    ];
    pmResetEntities();
    pmUpdateUI();
    $('pmMsg').innerHTML = '';
    pmRemoveKeyHandler();
    pmAddKeyHandler();
    if (pmAnimId) cancelAnimationFrame(pmAnimId);
    pmAnimId = requestAnimationFrame(pmLoop);
}

var _pmKeyHandler = null;
function pmAddKeyHandler() {
    _pmKeyHandler = pmOnKeyDown;
    if (document.attachEvent) {
        document.attachEvent('onkeydown', _pmKeyHandler);
    } else {
        document.addEventListener('keydown', _pmKeyHandler);
    }
}
function pmRemoveKeyHandler() {
    if (_pmKeyHandler) {
        if (document.detachEvent) {
            document.detachEvent('onkeydown', _pmKeyHandler);
        } else {
            document.removeEventListener('keydown', _pmKeyHandler);
        }
        _pmKeyHandler = null;
    }
}

// ==================== Init ====================
window.onload = function() { goHub(); };