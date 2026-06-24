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
