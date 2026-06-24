"""Pygame 渲染器：把 Maze + GameEnv 画到屏幕上。

设计要点：
    - 纯绘制，不含游戏循环逻辑。调用方负责 step()，每帧调 draw()。
    - 用 Surface 缓存迷宫墙，避免每帧重画 1200 个格子（性能）。
    - 方块、追逐圈、HUD 文字每帧重画（动态元素）。
    - import pygame 延迟到 Renderer.__init__，便于无头环境 import 本模块（测试）。
"""
import pygame

from config.game_config import (
    WINDOW_W, WINDOW_H, CELL, COLS, ROWS, SQUARE_SIZE, CHASE_RADIUS, FPS,
)
from src.game_env import CHASER_COLOR, RUNNER_COLOR
from src.maze import Maze

# 颜色调色板
BG_COLOR = (24, 24, 32)           # 走廊底色（深蓝灰）
WALL_COLOR = (200, 200, 210)      # 墙体（浅灰）
GRID_COLOR = (40, 40, 52)         # 网格线（比 BG 略亮，辅助看 cell 边界）
TEXT_COLOR = (235, 235, 235)


class Renderer:
    """pygame 渲染器。draw() 幂等，可被任意循环驱动。"""

    def __init__(self, maze: Maze, title: str = "追逃 AI"):
        self.screen = pygame.display.set_mode((WINDOW_W, WINDOW_H))
        pygame.display.set_caption(title)
        self.maze = maze
        # 字体在 set_mode 之后才能初始化
        self.font = pygame.font.SysFont("consolas,arial", 16)
        self.big_font = pygame.font.SysFont("consolas,arial", 28, bold=True)
        self._wall_surface = self._build_wall_surface(maze)
        self.clock = pygame.time.Clock()

    # ---------- 缓存 ----------
    @staticmethod
    def _build_wall_surface(maze: Maze) -> pygame.Surface:
        """把迷宫的墙一次性画到一张 Surface 上，之后每帧 blit 即可。"""
        surf = pygame.Surface((WINDOW_W, WINDOW_H))
        surf.fill(BG_COLOR)
        # 浅网格线（调试感、看 cell 边界）
        for x in range(0, WINDOW_W, CELL):
            pygame.draw.line(surf, GRID_COLOR, (x, 0), (x, WINDOW_H))
        for y in range(0, WINDOW_H, CELL):
            pygame.draw.line(surf, GRID_COLOR, (0, y), (WINDOW_W, y))
        # 墙
        for cy in range(ROWS):
            for cx in range(COLS):
                if maze.is_wall(cx, cy):
                    rect = pygame.Rect(cx * CELL, cy * CELL, CELL, CELL)
                    pygame.draw.rect(surf, WALL_COLOR, rect)
        return surf

    # ---------- 主绘制 ----------
    def draw(self, env, info: dict | None = None) -> None:
        """画一帧。

        env: GameEnv —— 取 chaser / runner / frame / winner
        info: 可选 dict，键：caption / fps（HUD 与节流用）
        """
        info = info or {}
        self.screen.blit(self._wall_surface, (0, 0))

        # 追逐判定圈（追方周围的捕获半径，可视化“危险区”）
        self._draw_chase_radius(env.chaser.x, env.chaser.y)

        # 方块
        self._draw_square(env.chaser.x, env.chaser.y, CHASER_COLOR)
        self._draw_square(env.runner.x, env.runner.y, RUNNER_COLOR)

        # HUD
        self._draw_hud(env, info)

        pygame.display.flip()
        fps = info.get("fps") or FPS
        self.clock.tick(int(fps))

    # ---------- 子元件 ----------
    def _draw_square(self, x: float, y: float, color) -> None:
        half = SQUARE_SIZE / 2
        rect = pygame.Rect(int(x - half), int(y - half), SQUARE_SIZE, SQUARE_SIZE)
        pygame.draw.rect(self.screen, color, rect)
        # 黑描边让方块在浅墙背景上更突出
        pygame.draw.rect(self.screen, (0, 0, 0), rect, width=1)

    def _draw_chase_radius(self, x: float, y: float) -> None:
        pygame.draw.circle(self.screen, (255, 120, 120),
                           (int(x), int(y)), CHASE_RADIUS, width=1)

    def _draw_hud(self, env, info: dict) -> None:
        lines = []
        if env.winner == "chaser":
            lines.append(("追方捕获！", (255, 140, 140)))
        elif env.winner == "runner":
            lines.append(("跑方逃脱成功！", (140, 180, 255)))
        lines.append((f"帧 {env.frame}", TEXT_COLOR))
        if info.get("caption"):
            lines.append((str(info["caption"]), TEXT_COLOR))

        # HUD 左上角，加半透明底避免被墙盖住
        y = 4
        for text, color in lines:
            surf = self.font.render(text, True, color)
            bg = pygame.Surface((surf.get_width() + 8, surf.get_height() + 2),
                                pygame.SRCALPHA)
            bg.fill((0, 0, 0, 140))
            self.screen.blit(bg, (4, y - 1))
            self.screen.blit(surf, (8, y))
            y += surf.get_height() + 2

    # ---------- 事件泵（调用方每帧应先调一次）----------
    @staticmethod
    def pump_events() -> list:
        """处理窗口事件，返回本帧发生的特殊信号列表。

        返回 "__quit__" 表示用户关窗或按 ESC；其余为 pygame KEYDOWN 的 key。
        """
        out = []
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return ["__quit__"]
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    return ["__quit__"]
                out.append(event.key)
        return out
