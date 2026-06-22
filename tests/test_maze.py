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
