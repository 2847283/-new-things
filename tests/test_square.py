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
    # 顶部外墙：方块紧贴上墙（中心 y=27、half=7 → 上边像素 20 落在 cell row 1，
    # cell row 0 才是外墙）。向上 dy=-5 会让上边越入 cell row 0 → 被挡；
    # 同时向右 dx=2 的 cell(2,1) 畅通 → 贴墙滑行只走 x。
    s = Square(50, 27, color=(255,0,0))
    s.move(dx=2, dy=-5, maze=maze)
    assert s.x == 52 and s.y == 27
