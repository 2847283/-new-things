# 追逃 AI 小游戏 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 一个有障碍物迷宫的小游戏，包含两个 AI（一个追、一个跑），用 NEAT 遗传算法 + 自我对弈共同进化，训练完成后人可以用键盘加入对战。

**Architecture:** Pygame 负责渲染和游戏循环；`neat-python` 负责神经网络进化；游戏逻辑、物理、NEAT 适配层、训练循环互相解耦。**迷宫在程序启动时随机生成 3 个**（走廊型，带连通性校验），训练全程用这 3 个；通过 `--seed` 可复现某次运行。追/跑是两个独立种群，以 self-play 形式评估。

**Tech Stack:** Python 3.11、pygame-ce（或 pygame）、neat-python、numpy

---

## 总体设计

### 关键参数（集中在一个配置文件里）
```python
# config/game_config.py
import random
WINDOW_W, WINDOW_H = 800, 600
CELL = 20  # 像素
COLS, ROWS = WINDOW_W // CELL, WINDOW_H // CELL  # 40 x 30
FPS = 60
MAX_FRAMES_PER_EPISODE = 600      # 10 秒
SQUARE_SIZE = 14                  # 方块边长，略小于 cell
SQUARE_SPEED = 2.0                # 每帧像素
CHASE_RADIUS = 10                 # 碰撞判定距离（中心距离）
EVAL_OPPONENTS_PER_INDIVIDUAL = 5  # 每个个体打多少场
POP_SIZE = 50                     # 每个种群大小
NUM_MAZES = 3                     # 启动时随机生成几个迷宫
GLOBAL_RNG = random.Random()      # 由 main.py 用 --seed 初始化
```

### 迷宫生成策略（核心设计）
程序启动时用 `GLOBAL_RNG` 生成 `NUM_MAZES` 个走廊型迷宫，特征：
- 外墙一圈
- 2 条横向长墙把场地分上/中/下三带，每条留 1 个开口（开口位置左右交错，强迫 S 型路线）
- 每带内若干梳齿状竖墙，每道墙留 1 个缺口（保证连通）
- `C` 起点固定左上角 (1,1)，`R` 起点固定右下角 (COLS-2, ROWS-2)
- 生成后用 BFS 校验 C→R 连通；不连通就重试（最多 100 次，仍失败则抛错）

随机的内容：①每条横墙开口的列位置 ②每道竖墙缺口的行位置 ③竖墙的数量和位置。这样每次运行布局都不同，但风格统一。

### 文件结构
```
ai小玩意2/
├── config/
│   ├── game_config.py          # 游戏常量 + GLOBAL_RNG
│   └── neat_config.txt         # NEAT 超参
├── src/
│   ├── maze.py                 # Maze 数据类 + 随机生成器 generate_corridor_maze(rng)
│   ├── square.py               # 方块实体（位置、速度、移动、碰撞）
│   ├── sensors.py              # 8 路墙射线 + 对手相对位置
│   ├── game_env.py             # 一局对战的封装：step(action_chase, action_run) -> done, info
│   ├── ai_controller.py        # 把 neat Genome 包成 Controller：sensor -> action
│   ├── human_controller.py     # 键盘输入 -> action（同一接口）
│   ├── renderer.py             # Pygame 绘制
│   ├── neat_evaluator.py       # 适应度函数 + self-play 调度
│   ├── training_loop.py        # 主训练入口（无渲染，快速）
│   ├── play_loop.py            # 人 vs AI 入口（有渲染）
│   └── watch_loop.py           # 观看 AI 自我对弈入口
├── checkpoints/                # 训练中间产物（champion pickle + 当前 seed 的迷宫快照）
├── tests/
│   ├── test_maze.py            # 含生成器测试：连通性、尺寸、起点
│   ├── test_square.py
│   ├── test_sensors.py
│   ├── test_game_env.py
│   └── test_neat_evaluator.py
├── main.py                     # 命令行入口：train / play / watch  + --seed
└── requirements.txt
```

### 适配层接口（各模块边界）
```python
# Action: 一个长度 4 的 bool 列表 [up, down, left, right]
# Controller.protocol: observe(sensor_vec: list[float]) -> list[bool]
#   - human_controller: 读键盘
#   - ai_controller:    net.activate(sensor_vec) -> 取阈值化 4 维

# 一局对战:
#   env = GameEnv(maze)
#   env.reset(*env.default_starts())
#   while not done:
#       a_c = chaser_ctrl.observe(env.chaser_sensors())
#       a_r = runner_ctrl.observe(env.runner_sensors())
#       done, info = env.step(a_c, a_r)
#   info = {"winner": "chaser"|"runner", "frames": N}

# 迷宫获取（所有 loop 统一入口）:
#   from src.maze import make_maze_set
#   mazes = make_maze_set(NUM_MAZES, seed=12345)  # 返回 list[Maze]
```

### 奖励函数（关键，决定 AI 学什么）
- **追方适应度**：抓住 → `1000 - frames_used`（越快越好）；没抓住 → 0 分（不奖励靠近，避免追方学会"贴着不走"）
- **跑方适应度**：存活整局 → `frames_survived * 2`（最高 1200）；被抓住 → `frames_survived`（仍然有分，活得越久越好）

---

## Task 1: 项目骨架 + 依赖

**Files:**
- Create: `requirements.txt`
- Create: `config/game_config.py`
- Create: `config/neat_config.txt`
- Create: `main.py`
- Create: `conftest.py`
- Create: `src/__init__.py`
- Create: `tests/__init__.py`

- [ ] **Step 1: 创建 requirements.txt**

```
pygame-ce>=2.5.0
neat-python>=0.92
numpy>=1.26
pytest>=8.0
```

- [ ] **Step 2: 安装依赖**

Run: `pip install -r requirements.txt`
Expected: 全部安装成功，无依赖冲突

- [ ] **Step 3: 创建 game_config.py**

```python
import random
WINDOW_W, WINDOW_H = 800, 600
CELL = 20
COLS, ROWS = WINDOW_W // CELL, WINDOW_H // CELL  # 40 x 30
FPS = 60
MAX_FRAMES_PER_EPISODE = 600
SQUARE_SIZE = 14
SQUARE_SPEED = 2.0
CHASE_RADIUS = 10
EVAL_OPPONENTS_PER_INDIVIDUAL = 5
POP_SIZE = 50
NUM_MAZES = 3
GLOBAL_RNG = random.Random()  # 由 main.py 用 --seed 初始化，默认随机
```

- [ ] **Step 4: 创建 neat_config.txt**（标准 NEAT 配置，输入 10 维、输出 4 维）

```ini
[NEAT]
fitness_criterion     = max
fitness_threshold     = 10000
pop_size              = 50
reset_on_extinction   = False

[DefaultGenome]
num_inputs              = 10
num_hidden              = 0
num_outputs             = 4
initial_connection      = full
feed_forward            = True
compatibility_disjoint_coefficient = 1.0
compatibility_weight_coefficient   = 0.5
conn_add_prob           = 0.5
conn_delete_prob        = 0.5
node_add_prob           = 0.2
node_delete_prob        = 0.2
activation_default      = tanh
activation_mutate_rate  = 0.0
activation_options      = tanh
aggregation_default     = sum
aggregation_mutate_rate = 0.0
aggregation_options     = sum
bias_init_mean          = 0.0
bias_init_stdev         = 1.0
bias_max_value          = 30.0
bias_min_value          = -30.0
bias_mutate_power       = 0.5
bias_mutate_rate        = 0.7
bias_replace_rate       = 0.1
response_init_mean      = 1.0
response_init_stdev     = 0.0
response_max_value      = 30.0
response_min_value      = -30.0
response_mutate_power   = 0.0
response_mutate_rate    = 0.0
response_replace_rate   = 0.0
weight_init_mean        = 0.0
weight_init_stdev       = 1.0
weight_max_value        = 30
weight_min_value        = -30
weight_mutate_power     = 0.5
weight_mutate_rate      = 0.8
weight_replace_rate     = 0.1

[DefaultReproduction]
elitism            = 2
survival_threshold = 0.2

[DefaultSpeciesSet]
compatibility_threshold = 3.0

[DefaultStagnation]
species_fitness_func = max
max_stagnation       = 20
species_elitism      = 2
```

- [ ] **Step 5: 创建空 main.py 占位（含 --seed 解析）**

```python
"""入口：python main.py [--seed N] [train|play|watch]"""
import argparse, sys
from config.game_config import GLOBAL_RNG

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--seed", type=int, default=None, help="随机种子，省略则真随机")
    parser.add_argument("command", nargs="?", default="train",
                        choices=["train", "play", "watch"])
    args = parser.parse_args()
    GLOBAL_RNG.seed(args.seed)
    print(f"[main] command={args.command}  seed={GLOBAL_RNG.getstate()[1][0] if args.seed is not None else 'random'}")
    print(f"TODO: {args.command} 模式将在后续 Task 实现")

if __name__ == "__main__":
    main()
```

- [ ] **Step 6: 创建 conftest.py 和空 __init__.py**

```python
# conftest.py（项目根）
import sys, os
sys.path.insert(0, os.path.dirname(__file__))
```

`src/__init__.py` 和 `tests/__init__.py` 都创建为空文件。

- [ ] **Step 7: 验证 main 能跑**

Run: `python main.py --seed 42 train`
Expected: 打印 `[main] command=train  seed=42` 和 TODO 行，无报错。

- [ ] **Step 8: 初始化 git 并提交**

```bash
git init
echo "checkpoints/" > .gitignore
echo "venv/" >> .gitignore
echo "__pycache__/" >> .gitignore
git add .
git commit -m "chore: 项目骨架与依赖"
```

---

## Task 2: 迷宫数据类 + 随机走廊生成器

**Files:**
- Create: `src/maze.py`
- Test: `tests/test_maze.py`

- [ ] **Step 1: 写失败的测试 `tests/test_maze.py`**

```python
import random
from src.maze import Maze, make_maze_set, generate_corridor_maze

def test_maze_dimensions():
    rng = random.Random(1)
    m = generate_corridor_maze(rng)
    assert m.cols == 40 and m.rows == 30

def test_boundary_is_wall():
    rng = random.Random(1)
    m = generate_corridor_maze(rng)
    assert m.is_wall(0, 0) is True
    assert m.is_wall(39, 29) is True
    assert m.is_wall(0, 15) is True
    assert m.is_wall(39, 15) is True

def test_starts_at_fixed_corners():
    rng = random.Random(1)
    m = generate_corridor_maze(rng)
    assert m.chaser_start_cell == (1, 1)
    assert m.runner_start_cell == (38, 28)

def test_starts_not_wall():
    rng = random.Random(1)
    m = generate_corridor_maze(rng)
    cx, cy = m.chaser_start_cell
    rx, ry = m.runner_start_cell
    assert m.is_wall(cx, cy) is False
    assert m.is_wall(rx, ry) is False

def test_pixel_to_cell():
    rng = random.Random(1)
    m = generate_corridor_maze(rng)
    assert m.pixel_to_cell(5, 5) == (0, 0)
    assert m.pixel_to_cell(25, 45) == (1, 2)

def test_maze_is_connected():
    """C 到 R 必须连通（生成器内部已校验，这里再独立验证）。"""
    from collections import deque
    rng = random.Random(7)
    for _ in range(20):  # 多个种子都要连通
        m = generate_corridor_maze(rng)
        start, end = m.chaser_start_cell, m.runner_start_cell
        seen = {start}; q = deque([start])
        ok = False
        while q:
            x, y = q.popleft()
            if (x, y) == end:
                ok = True; break
            for dx, dy in ((1,0),(-1,0),(0,1),(0,-1)):
                nx, ny = x+dx, y+dy
                if 0 <= nx < m.cols and 0 <= ny < m.rows and (nx,ny) not in seen and not m.is_wall(nx,ny):
                    seen.add((nx,ny)); q.append((nx,ny))
        assert ok, "生成的迷宫不连通"

def test_same_seed_same_maze():
    m1 = generate_corridor_maze(random.Random(123))
    m2 = generate_corridor_maze(random.Random(123))
    assert m1.grid == m2.grid

def test_make_maze_set_count():
    mazes = make_maze_set(3, seed=999)
    assert len(mazes) == 3
    # 三个迷宫布局应有差异（不完全相同）
    grids = [m.grid for m in mazes]
    assert not (grids[0] == grids[1] == grids[2])
```

- [ ] **Step 2: 跑测试确认失败**

Run: `pytest tests/test_maze.py -v`
Expected: FAIL（`ModuleNotFoundError: No module named 'src.maze'`）

- [ ] **Step 3: 实现 `src/maze.py`**

```python
import random
from collections import deque
from dataclasses import dataclass
from config.game_config import COLS, ROWS, CELL

@dataclass
class Maze:
    cols: int
    rows: int
    grid: list[list[str]]            # grid[y][x]
    chaser_start_cell: tuple[int, int]
    runner_start_cell: tuple[int, int]

    def is_wall(self, cx: int, cy: int) -> bool:
        if cx < 0 or cy < 0 or cx >= self.cols or cy >= self.rows:
            return True
        return self.grid[cy][cx] == "#"

    def pixel_to_cell(self, px: int, py: int) -> tuple[int, int]:
        return px // CELL, py // CELL


def _is_connected(grid) -> bool:
    """BFS 从 (1,1) 到 (COLS-2, ROWS-2)。"""
    start, end = (1, 1), (COLS-2, ROWS-2)
    seen = {start}; q = deque([start])
    while q:
        x, y = q.popleft()
        if (x, y) == end:
            return True
        for dx, dy in ((1,0),(-1,0),(0,1),(0,-1)):
            nx, ny = x+dx, y+dy
            if 0 <= nx < COLS and 0 <= ny < ROWS and (nx,ny) not in seen and grid[ny][nx] != "#":
                seen.add((nx, ny)); q.append((nx, ny))
    return False


def generate_corridor_maze(rng: random.Random) -> Maze:
    """生成一个走廊型迷宫：2 道横墙（开口左右交错）+ 每带若干梳齿竖墙（留缺口）。
    保证 C(1,1) 到 R(COLS-2,ROWS-2) 连通，最多重试 100 次。"""
    for _attempt in range(100):
        grid = [["." for _ in range(COLS)] for _ in range(ROWS)]
        # 外墙
        for x in range(COLS):
            grid[0][x] = "#"; grid[ROWS-1][x] = "#"
        for y in range(ROWS):
            grid[y][0] = "#"; grid[y][COLS-1] = "#"

        band_rows = [1, 10, 20, 29]  # 上带 1-9, 中带 10-19, 下带 20-28
        # 第一道横墙 row 9，开口在右侧（col 30~37）
        gap1 = rng.randint(30, 37)
        for x in range(1, gap1):
            grid[9][x] = "#"
        # 第二道横墙 row 19，开口在左侧（col 2~7）
        gap2 = rng.randint(2, 7)
        for x in range(gap2, COLS-1):
            grid[19][x] = "#"

        # 上带：1~2 道竖墙（col 8~14 范围），每道留顶部缺口
        for col in rng.sample(range(8, 15), k=rng.randint(1, 2)):
            gap_row = rng.randint(1, 3)
            for y in range(1, 9):
                if y != gap_row:
                    grid[y][col] = "#"
        # 中带：1~2 道竖墙（col 25~33），每道留底部缺口
        for col in rng.sample(range(25, 34), k=rng.randint(1, 2)):
            gap_row = rng.randint(16, 18)
            for y in range(10, 19):
                if y != gap_row:
                    grid[y][col] = "#"
        # 下带：1~2 道竖墙（col 8~18），每道留顶部缺口
        for col in rng.sample(range(8, 19), k=rng.randint(1, 2)):
            gap_row = rng.randint(20, 22)
            for y in range(20, 29):
                if y != gap_row:
                    grid[y][col] = "#"

        # 起点
        grid[1][1] = "C"
        grid[ROWS-2][COLS-2] = "R"

        if _is_connected(grid):
            return Maze(cols=COLS, rows=ROWS, grid=grid,
                        chaser_start_cell=(1, 1),
                        runner_start_cell=(COLS-2, ROWS-2))
    raise RuntimeError("100 次重试均无法生成连通迷宫，请检查参数")


def make_maze_set(n: int, seed=None) -> list[Maze]:
    """生成 n 个迷宫。seed 给定时结果可复现。"""
    rng = random.Random(seed)
    return [generate_corridor_maze(rng) for _ in range(n)]
```

- [ ] **Step 4: 跑测试确认通过**

Run: `pytest tests/test_maze.py -v`
Expected: 8 个测试全部 PASS

- [ ] **Step 5: 手动预览一个生成的迷宫（确认风格对）**

Run:
```
python -c "import random; from src.maze import generate_corridor_maze; m=generate_corridor_maze(random.Random(42)); [print(''.join('#' if c=='#' else c for c in row)) for row in m.grid]"
```
Expected: 打印一个 40×30 的走廊迷宫，能看到横墙 + 梳齿竖墙，C 在左上、R 在右下。

- [ ] **Step 6: 提交**

```bash
git add src/maze.py tests/test_maze.py
git commit -m "feat: 迷宫数据类与随机走廊生成器（带连通性校验）"
```

---

## Task 3: 方块实体（位置、移动、贴墙滑行碰撞）

**Files:**
- Create: `src/square.py`
- Test: `tests/test_square.py`

- [ ] **Step 1: 写失败的测试 `tests/test_square.py`**

```python
import random
from src.maze import generate_corridor_maze
from src.square import Square

maze = generate_corridor_maze(random.Random(1))

def test_move_into_free_space():
    cx, cy = maze.chaser_start_cell
    start_px = cx * 20 + 10
    start_py = cy * 20 + 10
    s = Square(start_px, start_py, color=(255,0,0))
    s.move(dx=2, dy=0, maze=maze)
    assert s.x == start_px + 2

def test_blocked_by_wall_keeps_position():
    # 把方块放在左上角，向左走应被外墙挡住
    s = Square(10, 10, color=(255,0,0))
    s.move(dx=-5, dy=0, maze=maze)
    assert s.x == 10  # 没动

def test_slides_along_wall():
    # 顶部外墙：向上会被挡，但同时向右应能走
    s = Square(50, 10, color=(255,0,0))
    s.move(dx=2, dy=-5, maze=maze)
    assert s.x == 52 and s.y == 10
```

- [ ] **Step 2: 跑测试确认失败**

Run: `pytest tests/test_square.py -v`
Expected: FAIL（模块不存在）

- [ ] **Step 3: 实现 `src/square.py`**（轴分离碰撞：x、y 分别尝试）

```python
from dataclasses import dataclass
from config.game_config import SQUARE_SIZE

@dataclass
class Square:
    x: float
    y: float
    color: tuple[int, int, int]

    @property
    def half(self) -> float:
        return SQUARE_SIZE / 2

    def _collides(self, x: float, y: float, maze) -> bool:
        """检查方块（以 x,y 为中心）在四个角是否压到墙。"""
        for dx in (-self.half, self.half):
            for dy in (-self.half, self.half):
                cx, cy = maze.pixel_to_cell(int(x + dx), int(y + dy))
                if maze.is_wall(cx, cy):
                    return True
        return False

    def move(self, dx: float, dy: float, maze) -> None:
        # 轴分离：先尝试 x
        if not self._collides(self.x + dx, self.y, maze):
            self.x += dx
        # 再尝试 y（贴墙滑行就靠这步独立判断）
        if not self._collides(self.x, self.y + dy, maze):
            self.y += dy
```

- [ ] **Step 4: 跑测试确认通过**

Run: `pytest tests/test_square.py -v`
Expected: 3 个 PASS

- [ ] **Step 5: 提交**

```bash
git add src/square.py tests/test_square.py
git commit -m "feat: 方块实体与轴分离碰撞"
```

---

## Task 4: 传感器（8 路墙射线 + 对手相对位置）

**Files:**
- Create: `src/sensors.py`
- Test: `tests/test_sensors.py`

- [ ] **Step 1: 写失败的测试 `tests/test_sensors.py`**

```python
import math, random
from src.maze import generate_corridor_maze
from src.sensors import build_sensor_vector, RAY_DIRS

maze = generate_corridor_maze(random.Random(1))

def test_sensor_vector_length():
    vec = build_sensor_vector(self_x=200.0, self_y=200.0,
                              opp_x=300.0, opp_y=200.0, maze=maze)
    assert len(vec) == 10  # 8 射线 + 2 对手位置

def test_ray_count_is_eight():
    assert len(RAY_DIRS) == 8

def test_opponent_relative_normalized():
    vec = build_sensor_vector(200.0, 200.0, 300.0, 200.0, maze)
    dx, dy = vec[8], vec[9]
    assert abs(dx - 1.0) < 0.05   # 对手在正右方
    assert abs(dy) < 0.05

def test_ray_hits_nearby_wall_returns_small_distance():
    vec = build_sensor_vector(10.0, 10.0, 100.0, 100.0, maze)
    assert min(vec[:8]) < 1.0     # 左上角贴近墙，至少一束射线很短
```

- [ ] **Step 2: 跑测试确认失败**

Run: `pytest tests/test_sensors.py -v`
Expected: FAIL（模块不存在）

- [ ] **Step 3: 实现 `src/sensors.py`**

```python
import math
from config.game_config import CELL, WINDOW_W, WINDOW_H

# 8 个方向（单位向量），顺时针从右开始
RAY_DIRS = [
    ( 1.0,  0.0),
    ( 0.7071,  0.7071),
    ( 0.0,  1.0),
    (-0.7071,  0.7071),
    (-1.0,  0.0),
    (-0.7071, -0.7071),
    ( 0.0, -1.0),
    ( 0.7071, -0.7071),
]
MAX_RAY_LEN = 8.0  # 最多看 8 个 cell（160px）

def _cast_ray(x, y, dx, dy, maze) -> float:
    step = CELL / 4
    dist = 0.0
    px, py = x, y
    while dist < MAX_RAY_LEN:
        px += dx * step
        py += dy * step
        dist += step / CELL
        cx, cy = maze.pixel_to_cell(int(px), int(py))
        if maze.is_wall(cx, cy):
            return dist
    return MAX_RAY_LEN

def build_sensor_vector(self_x, self_y, opp_x, opp_y, maze) -> list[float]:
    rays = [_cast_ray(self_x, self_y, dx, dy, maze) / MAX_RAY_LEN for dx, dy in RAY_DIRS]
    norm = math.hypot(WINDOW_W, WINDOW_H) / 2
    rel_x = (opp_x - self_x) / norm
    rel_y = (opp_y - self_y) / norm
    return rays + [rel_x, rel_y]
```

- [ ] **Step 4: 跑测试确认通过**

Run: `pytest tests/test_sensors.py -v`
Expected: 4 个 PASS

- [ ] **Step 5: 提交**

```bash
git add src/sensors.py tests/test_sensors.py
git commit -m "feat: 8 路墙射线 + 对手相对位置传感器"
```

---

## Task 5: 游戏环境（一局对战封装）

**Files:**
- Create: `src/game_env.py`
- Test: `tests/test_game_env.py`

- [ ] **Step 1: 写失败的测试 `tests/test_game_env.py`**

```python
import random
from config.game_config import MAX_FRAMES_PER_EPISODE
from src.maze import generate_corridor_maze
from src.game_env import GameEnv

maze = generate_corridor_maze(random.Random(1))

def test_reset_places_at_starts():
    env = GameEnv(maze)
    env.reset(*env.default_starts())
    cx, cy = maze.chaser_start_cell
    assert abs(env.chaser.x - (cx*20+10)) < 0.01

def test_step_returns_done_false_initially():
    env = GameEnv(maze); env.reset(*env.default_starts())
    done, info = env.step([False]*4, [False]*4)
    assert done is False

def test_chaser_catches_runner_when_adjacent():
    env = GameEnv(maze); env.reset(*env.default_starts())
    env.chaser.x = env.runner.x + 5
    env.chaser.y = env.runner.y
    done, info = env.step([False]*4, [False]*4)
    assert done is True
    assert info["winner"] == "chaser"

def test_action_moves_chaser():
    env = GameEnv(maze); env.reset(*env.default_starts())
    x0 = env.chaser.x
    env.step([False, False, False, True], [False]*4)  # 追方向右
    assert env.chaser.x > x0
```

- [ ] **Step 2: 跑测试确认失败**

Run: `pytest tests/test_game_env.py -v`
Expected: FAIL（模块不存在）

- [ ] **Step 3: 实现 `src/game_env.py`**

```python
from config.game_config import (SQUARE_SPEED, SQUARE_SIZE, CELL,
                                CHASE_RADIUS, MAX_FRAMES_PER_EPISODE)
from src.square import Square
from src.sensors import build_sensor_vector

CHASE_COLOR = (255, 60, 60)
RUN_COLOR = (60, 200, 90)

class GameEnv:
    def __init__(self, maze):
        self.maze = maze
        self.chaser: Square = None
        self.runner: Square = None
        self.frames = 0

    def default_starts(self):
        cx, cy = self.maze.chaser_start_cell
        rx, ry = self.maze.runner_start_cell
        return (cx*CELL + CELL/2, cy*CELL + CELL/2,
                rx*CELL + CELL/2, ry*CELL + CELL/2)

    def reset(self, chaser_x, chaser_y, runner_x, runner_y):
        self.chaser = Square(chaser_x, chaser_y, CHASE_COLOR)
        self.runner = Square(runner_x, runner_y, RUN_COLOR)
        self.frames = 0

    def _apply_action(self, sq, action):
        up, down, left, right = action
        dx = (1 if right else 0) - (1 if left else 0)
        dy = (1 if down  else 0) - (1 if up   else 0)
        if dx and dy:
            dx *= 0.7071; dy *= 0.7071  # 对角不加速
        sq.move(dx * SQUARE_SPEED, dy * SQUARE_SPEED, self.maze)

    def step(self, action_chaser, action_runner):
        self._apply_action(self.chaser, action_chaser)
        self._apply_action(self.runner, action_runner)
        self.frames += 1
        d = ((self.chaser.x - self.runner.x)**2 +
             (self.chaser.y - self.runner.y)**2) ** 0.5
        if d <= CHASE_RADIUS + SQUARE_SIZE/2:
            return True, {"winner": "chaser", "frames": self.frames}
        if self.frames >= MAX_FRAMES_PER_EPISODE:
            return True, {"winner": "runner", "frames": self.frames}
        return False, {"winner": None, "frames": self.frames}

    def chaser_sensors(self):
        return build_sensor_vector(self.chaser.x, self.chaser.y,
                                   self.runner.x, self.runner.y, self.maze)
    def runner_sensors(self):
        return build_sensor_vector(self.runner.x, self.runner.y,
                                   self.chaser.x, self.chaser.y, self.maze)
```

- [ ] **Step 4: 跑测试确认通过**

Run: `pytest tests/test_game_env.py -v`
Expected: 4 个 PASS

- [ ] **Step 5: 提交**

```bash
git add src/game_env.py tests/test_game_env.py
git commit -m "feat: 一局对战的游戏环境封装"
```

---

## Task 6: AI 控制器（Genome → Controller）

**Files:**
- Create: `src/ai_controller.py`
- Modify: `tests/test_game_env.py`（追加冒烟测试）

- [ ] **Step 1: 实现 `src/ai_controller.py`**

```python
class AIController:
    """把 neat 的 FeedForwardNetwork 包成 Controller 接口。
    输出 4 维（tanh ∈ [-1,1]）阈值化为 bool：>0 视为按下。"""
    def __init__(self, net):
        self.net = net

    def observe(self, sensor_vec):
        out = self.net.activate(sensor_vec)
        return [o > 0.0 for o in out]
```

- [ ] **Step 2: 追加冒烟测试到 `tests/test_game_env.py` 末尾**

```python
def test_ai_controller_with_stub_net():
    from src.ai_controller import AIController
    class StubNet:
        def activate(self, v):
            return [0.5, -0.5, 0.0, 0.9]
    ctrl = AIController(StubNet())
    action = ctrl.observe([0.0]*10)
    assert action == [True, False, False, True]  # 阈值是严格 > 0
```

- [ ] **Step 3: 跑测试**

Run: `pytest tests/test_game_env.py::test_ai_controller_with_stub_net -v`
Expected: PASS

- [ ] **Step 4: 提交**

```bash
git add src/ai_controller.py tests/test_game_env.py
git commit -m "feat: AI 控制器适配层"
```

---

## Task 7: NEAT 自我对弈评估器（适应度 + 双种群调度）

**Files:**
- Create: `src/neat_evaluator.py`
- Test: `tests/test_neat_evaluator.py`

- [ ] **Step 1: 写失败的测试 `tests/test_neat_evaluator.py`**

```python
import os, math, random, neat
from src.maze import make_maze_set
from src.neat_evaluator import SelfPlayEvaluator

CFG_PATH = os.path.join(os.path.dirname(__file__), "..", "config", "neat_config.txt")

def _make_cfg():
    return neat.Config(neat.DefaultGenome, neat.DefaultReproduction,
                       neat.DefaultSpeciesSet, neat.DefaultStagnation, CFG_PATH)

def _fresh_genomes(cfg, n=3):
    pop = neat.Population(cfg)
    return list(pop.population.values())[:n]

def test_evaluate_assigns_fitness_to_all():
    cfg = _make_cfg()
    mazes = make_maze_set(2, seed=123)
    ev = SelfPlayEvaluator(mazes, cfg)
    chasers = _fresh_genomes(cfg, 3)
    runners  = _fresh_genomes(cfg, 3)
    ev.evaluate_generation(chasers, runners)
    assert all(g.fitness is not None for g in chasers)
    assert all(g.fitness is not None for g in runners)
    assert all(g.fitness >= 0 for g in chasers)

def test_fitness_is_finite():
    cfg = _make_cfg()
    mazes = make_maze_set(2, seed=123)
    ev = SelfPlayEvaluator(mazes, cfg)
    chasers = _fresh_genomes(cfg, 2)
    runners  = _fresh_genomes(cfg, 2)
    ev.evaluate_generation(chasers, runners)
    for g in chasers + runners:
        assert math.isfinite(g.fitness)
```

- [ ] **Step 2: 跑测试确认失败**

Run: `pytest tests/test_neat_evaluator.py -v`
Expected: FAIL（模块不存在）

- [ ] **Step 3: 实现 `src/neat_evaluator.py`**

```python
import random
import neat
from config.game_config import EVAL_OPPONENTS_PER_INDIVIDUAL
from src.game_env import GameEnv
from src.ai_controller import AIController

class SelfPlayEvaluator:
    def __init__(self, mazes, cfg, rng=None):
        self.mazes = mazes
        self.cfg = cfg
        self.rng = rng or random.Random()
        self._net_cache: dict = {}

    def _net_for(self, genome):
        key = id(genome)
        if key not in self._net_cache:
            self._net_cache[key] = neat.nn.feed_forward.create(self.cfg.genome, genome)
        return self._net_cache[key]

    def evaluate_generation(self, chaser_genomes, runner_genomes):
        self._net_cache.clear()
        for g in chaser_genomes + runner_genomes:
            g.fitness = 0.0
        for g in chaser_genomes:
            try:
                g.fitness = self._eval_chaser(g, runner_genomes)
            except Exception as e:
                print(f"[warn] chaser {g.key} 评估失败: {e}")
                g.fitness = 0.0
        for g in runner_genomes:
            try:
                g.fitness = self._eval_runner(g, chaser_genomes)
            except Exception as e:
                print(f"[warn] runner {g.key} 评估失败: {e}")
                g.fitness = 0.0

    def _run_episode(self, chaser_ctrl, runner_ctrl, maze):
        env = GameEnv(maze)
        env.reset(*env.default_starts())
        while True:
            done, info = env.step(
                chaser_ctrl.observe(env.chaser_sensors()),
                runner_ctrl.observe(env.runner_sensors()),
            )
            if done:
                return info

    def _eval_chaser(self, genome, runner_pool):
        net = self._net_for(genome)
        opponents = self.rng.sample(runner_pool,
                                    min(EVAL_OPPONENTS_PER_INDIVIDUAL, len(runner_pool)))
        total = 0.0
        for opp in opponents:
            opp_net = self._net_for(opp)
            for maze in self.mazes:
                info = self._run_episode(AIController(net), AIController(opp_net), maze)
                if info["winner"] == "chaser":
                    total += 1000 - info["frames"]
        return total / max(1, len(opponents) * len(self.mazes))

    def _eval_runner(self, genome, chaser_pool):
        net = self._net_for(genome)
        opponents = self.rng.sample(chaser_pool,
                                    min(EVAL_OPPONENTS_PER_INDIVIDUAL, len(chaser_pool)))
        total = 0.0
        for opp in opponents:
            opp_net = self._net_for(opp)
            for maze in self.mazes:
                info = self._run_episode(AIController(opp_net), AIController(net), maze)
                survived = info["frames"]
                total += survived * 2 if info["winner"] == "runner" else survived
        return total / max(1, len(opponents) * len(self.mazes))
```

- [ ] **Step 4: 跑测试确认通过**

Run: `pytest tests/test_neat_evaluator.py -v`
Expected: 2 个 PASS

- [ ] **Step 5: 提交**

```bash
git add src/neat_evaluator.py tests/test_neat_evaluator.py
git commit -m "feat: NEAT 自我对弈双种群评估器"
```

---

## Task 8: 训练主循环

**Files:**
- Create: `src/training_loop.py`

- [ ] **Step 1: 实现训练主循环**

```python
import os, pickle, random, neat
from config.game_config import NUM_MAZES, GLOBAL_RNG
from src.maze import make_maze_set
from src.neat_evaluator import SelfPlayEvaluator

CKPT = os.path.join(os.path.dirname(__file__), "..", "checkpoints")
os.makedirs(CKPT, exist_ok=True)

def run(generations: int = 100, neat_config_path: str = "config/neat_config.txt",
        seed=None):
    cfg = neat.Config(neat.DefaultGenome, neat.DefaultReproduction,
                      neat.DefaultSpeciesSet, neat.DefaultStagnation, neat_config_path)
    # 用 GLOBAL_RNG 生成迷宫（已被 main.py 用 --seed 初始化）
    mazes = make_maze_set(NUM_MAZES, seed=GLOBAL_RNG.random())
    # 把当前迷宫也存一份快照，便于 play/watch 复现
    with open(os.path.join(CKPT, "mazes.pkl"), "wb") as f:
        pickle.dump(mazes, f)
    print(f"[train] 生成 {len(mazes)} 个迷宫，已存 checkpoints/mazes.pkl")

    evaluator = SelfPlayEvaluator(mazes, cfg, rng=GLOBAL_RNG)
    chaser_pop = neat.Population(cfg)
    runner_pop = neat.Population(cfg)
    chaser_pop.add_reporter(neat.StdOutReporter(True))
    runner_pop.add_reporter(neat.StdOutReporter(True))

    for gen in range(generations):
        chaser_genomes = list(chaser_pop.population.values())
        runner_genomes = list(runner_pop.population.values())
        evaluator.evaluate_generation(chaser_genomes, runner_genomes)

        cb = max((g.fitness for g in chaser_genomes if g.fitness is not None), default=0.0)
        rb = max((g.fitness for g in runner_genomes if g.fitness is not None), default=0.0)
        print(f"[Gen {gen}] 追方最佳={cb:.1f}  跑方最佳={rb:.1f}")

        _save_champion(chaser_genomes, os.path.join(CKPT, f"chaser_gen{gen}.pkl"))
        _save_champion(runner_genomes, os.path.join(CKPT, f"runner_gen{gen}.pkl"))

        chaser_pop = _next_generation(chaser_pop, cfg)
        runner_pop = _next_generation(runner_pop, cfg)

    _save_champion(list(chaser_pop.population.values()), os.path.join(CKPT, "chaser_final.pkl"))
    _save_champion(list(runner_pop.population.values()), os.path.join(CKPT, "runner_final.pkl"))

def _save_champion(genomes, path):
    best = max(genomes, key=lambda g: g.fitness if g.fitness else 0.0)
    with open(path, "wb") as f:
        pickle.dump(best, f)

def _next_generation(pop, cfg):
    pop.population = pop.reproduction.reproduce(pop.config, pop.species,
                                                pop.config.pop_size, pop.generation)
    pop.generation += 1
    pop.species.speciate(pop.config, pop.population, pop.generation)
    return pop
```

- [ ] **Step 2: 短训练冒烟测试（3 代）**

Run: `python main.py --seed 1 train 3`
Expected: 打印 3 代进度，`checkpoints/chaser_gen2.pkl`、`runner_gen2.pkl`、`mazes.pkl` 存在，最佳适应度非全 0。

- [ ] **Step 3: 提交**

```bash
git add src/training_loop.py
git commit -m "feat: NEAT 自我对弈训练主循环（含迷宫快照）"
```

---

## Task 9: Pygame 渲染器

**Files:**
- Create: `src/renderer.py`

- [ ] **Step 1: 实现 `src/renderer.py`**

```python
import pygame
from config.game_config import WINDOW_W, WINDOW_H, CELL, COLS, ROWS

WALL_COLOR = (40, 40, 50)
BG_COLOR = (20, 22, 30)
TEXT_COLOR = (220, 220, 220)

class Renderer:
    def __init__(self, screen):
        self.screen = screen
        self.font = pygame.font.SysFont("consolas", 16)

    def draw(self, env, info_text: str = ""):
        self.screen.fill(BG_COLOR)
        for y in range(ROWS):
            for x in range(COLS):
                if env.maze.is_wall(x, y):
                    pygame.draw.rect(self.screen, WALL_COLOR,
                                     pygame.Rect(x*CELL, y*CELL, CELL, CELL))
        for sq in (env.chaser, env.runner):
            if sq is not None:
                size = 14
                pygame.draw.rect(self.screen, sq.color,
                                 pygame.Rect(int(sq.x - size/2), int(sq.y - size/2), size, size))
        if info_text:
            surf = self.font.render(info_text, True, TEXT_COLOR)
            self.screen.blit(surf, (8, WINDOW_H - 22))
        pygame.display.flip()
```

- [ ] **Step 2: 手动冒烟（画一帧保存图片看效果）**

Run:
```
python -c "import pygame,random; from src.maze import generate_corridor_maze; from src.game_env import GameEnv; from src.renderer import Renderer; pygame.init(); s=pygame.display.set_mode((800,600)); m=generate_corridor_maze(random.Random(42)); env=GameEnv(m); env.reset(*env.default_starts()); Renderer(s).draw(env,'smoke'); pygame.image.save(s,'smoke.png'); print('ok')"
```
Expected: 生成 `smoke.png`，打开能看到墙、红方块、绿方块。然后 `del smoke.png` 删除。

- [ ] **Step 3: 提交**

```bash
git add src/renderer.py
git commit -m "feat: Pygame 渲染器"
```

---

## Task 10: 人 vs AI 对战模式（play）

**Files:**
- Create: `src/human_controller.py`
- Create: `src/play_loop.py`
- Modify: `main.py`

- [ ] **Step 1: 写 human_controller.py**

```python
import pygame

class HumanController:
    """从键盘读 WASD/方向键，输出长度 4 的 bool 列表 [up,down,left,right]。"""
    def __init__(self, label: str = "你"):
        self.label = label

    def observe(self, sensor_vec):
        keys = pygame.key.get_pressed()
        up    = keys[pygame.K_w] or keys[pygame.K_UP]
        down  = keys[pygame.K_s] or keys[pygame.K_DOWN]
        left  = keys[pygame.K_a] or keys[pygame.K_LEFT]
        right = keys[pygame.K_d] or keys[pygame.K_RIGHT]
        return [up, down, left, right]
```

- [ ] **Step 2: 写 play_loop.py（用训练时存的 mazes.pkl，保证 play 的迷宫和训练一致）**

```python
import os, pickle
import pygame, neat
from config.game_config import WINDOW_W, WINDOW_H, FPS, MAX_FRAMES_PER_EPISODE
from src.game_env import GameEnv
from src.ai_controller import AIController
from src.human_controller import HumanController
from src.renderer import Renderer

CKPT = os.path.join(os.path.dirname(__file__), "..", "checkpoints")

def play(human_role: str = "chaser",
         chaser_ckpt: str = "chaser_final.pkl",
         runner_ckpt: str = "runner_final.pkl"):
    cfg = neat.Config(neat.DefaultGenome, neat.DefaultReproduction,
                      neat.DefaultSpeciesSet, neat.DefaultStagnation, "config/neat_config.txt")

    # 读训练时存的迷宫快照（与训练时同一套迷宫）
    mazes_path = os.path.join(CKPT, "mazes.pkl")
    if not os.path.exists(mazes_path):
        raise FileNotFoundError("未找到 checkpoints/mazes.pkl，请先 python main.py train")
    with open(mazes_path, "rb") as f:
        mazes = pickle.load(f)
    maze = mazes[0]  # 用第一个迷宫对战

    with open(os.path.join(CKPT, chaser_ckpt), "rb") as f:
        chaser_genome = pickle.load(f)
    with open(os.path.join(CKPT, runner_ckpt), "rb") as f:
        runner_genome = pickle.load(f)

    chaser_ctrl = AIController(neat.nn.feed_forward.create(cfg.genome, chaser_genome))
    runner_ctrl = AIController(neat.nn.feed_forward.create(cfg.genome, runner_genome))
    if human_role == "chaser":
        chaser_ctrl = HumanController("你（追）")
    else:
        runner_ctrl = HumanController("你（跑）")

    pygame.init()
    screen = pygame.display.set_mode((WINDOW_W, WINDOW_H))
    clock = pygame.time.Clock()
    renderer = Renderer(screen)
    env = GameEnv(maze)
    env.reset(*env.default_starts())

    frames = 0; running = True
    while running:
        for e in pygame.event.get():
            if e.type == pygame.QUIT: running = False
            elif e.type == pygame.KEYDOWN and e.key == pygame.K_ESCAPE: running = False
        a_c = chaser_ctrl.observe(env.chaser_sensors())
        a_r = runner_ctrl.observe(env.runner_sensors())
        done, info = env.step(a_c, a_r)
        renderer.draw(env, info_text=f"frames={frames}  (ESC 退出)")
        frames += 1
        if done or frames >= MAX_FRAMES_PER_EPISODE:
            print(f"结果: {info.get('winner') or 'runner(超时)'}")
            running = False
        clock.tick(FPS)
    pygame.quit()
```

- [ ] **Step 3: 更新 main.py 接入所有子命令（含代数参数）**

```python
"""入口：python main.py [--seed N] [train [代数]|play [chaser|runner]|watch]"""
import argparse
from config.game_config import GLOBAL_RNG

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--seed", type=int, default=None, help="随机种子，省略则真随机")
    parser.add_argument("command", nargs="?", default="train",
                        choices=["train", "play", "watch"])
    parser.add_argument("arg", nargs="?", default=None,
                        help="train: 代数(默认100)  play: chaser|runner(默认chaser)")
    args = parser.parse_args()
    GLOBAL_RNG.seed(args.seed)

    if args.command == "train":
        from src.training_loop import run as train_run
        gens = int(args.arg) if args.arg else 100
        train_run(generations=gens)
    elif args.command == "play":
        from src.play_loop import play
        play(human_role=args.arg or "chaser")
    elif args.command == "watch":
        from src.watch_loop import watch
        watch()

if __name__ == "__main__":
    main()
```

- [ ] **Step 4: 手动验证（需先完成 Task 8 训练）**

Run: `python main.py --seed 1 play chaser`
Expected: 窗口出现，你用 WASD 控制红色追方，绿色跑方是训练好的 AI。ESC 退出。再试 `python main.py --seed 1 play runner`。

- [ ] **Step 5: 提交**

```bash
git add src/human_controller.py src/play_loop.py main.py
git commit -m "feat: 人 vs AI 对战模式"
```

---

## Task 11: 观看 AI 自我对弈（watch）

**Files:**
- Create: `src/watch_loop.py`

- [ ] **Step 1: 写 watch_loop.py**

```python
import os, pickle
import pygame, neat
from config.game_config import WINDOW_W, WINDOW_H, FPS, MAX_FRAMES_PER_EPISODE
from src.game_env import GameEnv
from src.ai_controller import AIController
from src.renderer import Renderer

CKPT = os.path.join(os.path.dirname(__file__), "..", "checkpoints")

def _latest(prefix):
    files = [f for f in os.listdir(CKPT) if f.startswith(prefix)]
    if not files:
        raise FileNotFoundError(f"未找到 {prefix}* 训练结果，请先 python main.py train")
    files.sort(key=lambda f: os.path.getmtime(os.path.join(CKPT, f)), reverse=True)
    return files[0]

def watch():
    cfg = neat.Config(neat.DefaultGenome, neat.DefaultReproduction,
                      neat.DefaultSpeciesSet, neat.DefaultStagnation, "config/neat_config.txt")

    with open(os.path.join(CKPT, "mazes.pkl"), "rb") as f:
        mazes = pickle.load(f)
    maze = mazes[0]

    c_file = _latest("chaser_")
    r_file = _latest("runner_")
    print(f"使用 {c_file} vs {r_file}")
    with open(os.path.join(CKPT, c_file), "rb") as f: chaser_g = pickle.load(f)
    with open(os.path.join(CKPT, r_file), "rb") as f: runner_g = pickle.load(f)
    chaser_ctrl = AIController(neat.nn.feed_forward.create(cfg.genome, chaser_g))
    runner_ctrl = AIController(neat.nn.feed_forward.create(cfg.genome, runner_g))

    pygame.init()
    screen = pygame.display.set_mode((WINDOW_W, WINDOW_H))
    clock = pygame.time.Clock()
    renderer = Renderer(screen)
    env = GameEnv(maze); env.reset(*env.default_starts())

    frames = 0; running = True
    while running:
        for e in pygame.event.get():
            if e.type == pygame.QUIT: running = False
            elif e.type == pygame.KEYDOWN and e.key == pygame.K_ESCAPE: running = False
        a_c = chaser_ctrl.observe(env.chaser_sensors())
        a_r = runner_ctrl.observe(env.runner_sensors())
        done, info = env.step(a_c, a_r)
        renderer.draw(env, info_text=f"frames={frames}  (ESC 退出)")
        frames += 1
        if done or frames >= MAX_FRAMES_PER_EPISODE:
            print(f"结果: {info.get('winner') or 'runner(超时)'}")
            running = False
        clock.tick(FPS)
    pygame.quit()
```

- [ ] **Step 2: 手动验证**

Run: `python main.py watch`
Expected: 两个 AI 在窗口里对战，ESC 退出，控制台打印胜负。

- [ ] **Step 3: 提交**

```bash
git add src/watch_loop.py
git commit -m "feat: 观看 AI 自我对弈模式"
```

---

## Task 12: 超时兜底测试（收尾）

**Files:**
- Modify: `tests/test_game_env.py`（追加）

- [ ] **Step 1: 追加测试——两个 AI 都不动，600 帧后跑方赢**

```python
# 追加到 tests/test_game_env.py
def test_no_movement_runner_wins_on_timeout():
    env = GameEnv(maze)
    env.reset(*env.default_starts())
    done = False; info = {}
    for _ in range(MAX_FRAMES_PER_EPISODE + 5):
        done, info = env.step([False]*4, [False]*4)
        if done: break
    assert done is True
    assert info["winner"] == "runner"
```

- [ ] **Step 2: 跑全部测试**

Run: `pytest tests/ -v`
Expected: 全部 PASS

- [ ] **Step 3: 提交**

```bash
git add tests/test_game_env.py
git commit -m "test: 超时兜底测试"
```

---

## 自查 (Self-Review)

**Spec 覆盖：**
- 两个小方块（追/跑） → Task 3 ✓
- 有障碍物迷宫 → Task 2 ✓（走廊型，运行时随机生成）
- 机器学习训练 → Task 4–8 ✓
- 自我对弈（双种群共同进化） → Task 7 ✓
- 人控制一个（键盘 WASD/方向键） → Task 10 ✓
- AI 控制另一个 → Task 6 + Task 10 ✓
- 看训练（watch） + 跟 AI 对战（play） → Task 10 + Task 11 ✓
- 运行时生成 3 个迷宫 + seed 可复现 → Task 1(main.py) + Task 2(generator) + Task 8(快照) ✓

**Placeholder 扫描：** 计划中所有代码块均为完整可运行代码，无 TBD/TODO/"稍后实现"。

**Type/命名一致性：**
- `Action` = `list[bool]` 长度 4（[up, down, left, right]）—— 全部任务统一
- `Controller.observe(sensor_vec) -> list[bool]` —— human/ai/watch 三处签名完全一致
- `GameEnv.reset(cx,cy,rx,ry)` / `step(a_c, a_r) -> (done, info)` / `default_starts() -> (4元组)` —— 全部一致
- `info["winner"]` = `"chaser"|"runner"|None` —— Task 5 定义，Task 8/10/11 读取方式一致
- `Square.move(dx, dy, maze)` —— Task 3 定义，Task 5 调用一致
- `Maze.is_wall / .pixel_to_cell / .chaser_start_cell / .runner_start_cell` —— Task 2 定义，后续全部一致
- `generate_corridor_maze(rng) -> Maze` / `make_maze_set(n, seed) -> list[Maze]` —— Task 2 定义，Task 7/8 调用一致
- `build_sensor_vector(self_x, self_y, opp_x, opp_y, maze) -> list[float]` —— Task 4 定义，Task 5 调用一致
- `SelfPlayEvaluator(mazes, cfg, rng)` + `evaluate_generation(chasers, runners)` —— Task 7 定义，Task 8 调用一致
- `mazes.pkl` 快照：Task 8 写入，Task 10/11 读取 —— 路径和格式一致

---

## 执行交接

**计划已保存到 `docs/superpowers/plans/2026-06-20-chase-and-run-ai.md`。两种执行方式：**

**1. Subagent-Driven（推荐）** — 每个 Task 派一个独立 subagent 实现，Task 之间我帮你 review，迭代快

**2. Inline Execution** — 在当前会话里按 Task 执行，批量推进 + 检查点

**选哪种？**
