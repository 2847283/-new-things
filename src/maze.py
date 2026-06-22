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
