"""一局对战封装：reset / step / sense。

动作编码（5 个离散动作，环境与控制器解耦）：
    0 不动
    1 上  2 下  3 左  4 右
"""
import math
from dataclasses import dataclass
from config.game_config import (
    CELL, SQUARE_SPEED, CHASE_RADIUS, MAX_FRAMES_PER_EPISODE,
)
from src.square import Square
from src.maze import Maze

# (dx, dy) 像素位移
ACTIONS = {
    0: (0.0, 0.0),
    1: (0.0, -SQUARE_SPEED),
    2: (0.0, SQUARE_SPEED),
    3: (-SQUARE_SPEED, 0.0),
    4: (SQUARE_SPEED, 0.0),
}
NUM_ACTIONS = len(ACTIONS)

CHASER_COLOR = (220, 60, 60)    # 红 = 追
RUNNER_COLOR = (60, 140, 240)   # 蓝 = 逃


def cell_center(cx: int, cy: int) -> tuple[float, float]:
    """cell 坐标 → 该 cell 中心像素。"""
    return cx * CELL + CELL / 2.0, cy * CELL + CELL / 2.0


@dataclass
class StepResult:
    done: bool
    winner: str | None   # "chaser" | "runner" | None(未结束)
    frame: int


class GameEnv:
    def __init__(self, maze: Maze):
        self.maze = maze
        self.chaser: Square = None  # type: ignore[assignment]
        self.runner: Square = None  # type: ignore[assignment]
        self.frame = 0
        self.done = False
        self.winner: str | None = None
        self.reset()

    # ---------- 生命周期 ----------
    def reset(self) -> None:
        ccx, ccy = self.maze.chaser_start_cell
        rcx, rcy = self.maze.runner_start_cell
        cpx, cpy = cell_center(ccx, ccy)
        rpx, rpy = cell_center(rcx, rcy)
        self.chaser = Square(cpx, cpy, color=CHASER_COLOR)
        self.runner = Square(rpx, rpy, color=RUNNER_COLOR)
        self.frame = 0
        self.done = False
        self.winner = None

    # ---------- 观测 ----------
    def sense(self, role: str) -> list[float]:
        """role: 'chaser' | 'runner' —— 返回该角色的归一化观测。"""
        from src.sensors import observe
        if role == "chaser":
            return observe(self.chaser.x, self.chaser.y,
                           self.runner.x, self.runner.y, self.maze)
        return observe(self.runner.x, self.runner.y,
                       self.chaser.x, self.chaser.y, self.maze)

    # ---------- 推进 ----------
    def step(self, chaser_action: int, runner_action: int) -> StepResult:
        if self.done:
            return StepResult(True, self.winner, self.frame)

        cdx, cdy = ACTIONS[chaser_action]
        rdx, rdy = ACTIONS[runner_action]
        self.chaser.move(cdx, cdy, self.maze)
        self.runner.move(rdx, rdy, self.maze)
        self.frame += 1

        # 判定：两方块中心欧氏距离（像素）≤ CHASE_RADIUS 视为捕获。
        dist = math.hypot(self.chaser.x - self.runner.x,
                          self.chaser.y - self.runner.y)
        if dist <= CHASE_RADIUS:
            self.done = True
            self.winner = "chaser"
        elif self.frame >= MAX_FRAMES_PER_EPISODE:
            self.done = True
            self.winner = "runner"   # 超时 = runner 逃脱成功
        return StepResult(self.done, self.winner, self.frame)
