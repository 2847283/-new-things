"""传感器：给 AI 提供的环境观测向量。

观测 = 8 路墙射线距离 + 对手相对位置/距离。
全部归一化到 [0,1]，长度固定、与 NEAT 输入数对齐。
"""
import math
from config.game_config import (
    CELL, COLS, ROWS,
    SENSOR_RAYS, SENSOR_MAX_DIST, OPPONENT_MAX_DIST,
)

# 8 方向单位向量（弧度），从 +x(0) 开始逆时针每 45°
RAY_DIRS = [(math.cos(2 * math.pi * i / SENSOR_RAYS),
             math.sin(2 * math.pi * i / SENSOR_RAYS))
            for i in range(SENSOR_RAYS)]


def cast_wall_ray(x: float, y: float, dx: float, dy: float, maze) -> float:
    """从 (x,y) 沿单位方向 (dx,dy) 发射射线，返回到第一面墙的像素距离。

    用 1 像素细步进采样，避免 DDA 网格边界像素判定歧义（cell 边界归到哪一格）。
    SENSOR_MAX_DIST 内最多 ~160 次采样，8 路射线完全可承受。
    起步先迈 1px，避免方块自身所在 cell 边界立刻命中。
    """
    dist = 1.0
    while dist <= SENSOR_MAX_DIST:
        px = x + dx * dist
        py = y + dy * dist
        cx, cy = int(px // CELL), int(py // CELL)
        if cx < 0 or cy < 0 or cx >= COLS or cy >= ROWS or maze.grid[cy][cx] == "#":
            return dist
        dist += 1.0
    return SENSOR_MAX_DIST


def observe(self_x: float, self_y: float,
            opp_x: float, opp_y: float, maze) -> list[float]:
    """返回观测向量（固定长度 SENSOR_RAYS + 3）。"""
    out: list[float] = []
    # —— 8 路墙射线 ——
    for dx, dy in RAY_DIRS:
        d = cast_wall_ray(self_x, self_y, dx, dy, maze)
        out.append(min(d / SENSOR_MAX_DIST, 1.0))
    # —— 对手相对位置 ——
    rdx = opp_x - self_x
    rdy = opp_y - self_y
    out.append(rdx / OPPONENT_MAX_DIST)                 # ~[-1,1]
    out.append(rdy / OPPONENT_MAX_DIST)                 # ~[-1,1]
    out.append(min(math.hypot(rdx, rdy) / OPPONENT_MAX_DIST, 1.0))  # 距离
    return out


def input_size() -> int:
    """NEAT 网络输入维度，需与 observe() 输出长度一致。"""
    return SENSOR_RAYS + 3
