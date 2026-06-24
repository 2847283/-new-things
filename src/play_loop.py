"""人 vs AI 对战模式。

加载训练产物（checkpoints/chaser_final.pkl 或 runner_final.pkl）+ mazes.pkl，
人扮演指定角色，AI 扮演对方，pygame 实时渲染 + 键盘控制。

操作：
    方向键 / WASD 控制人类角色
    ESC 或关窗退出
人类动作映射到 game_env.ACTIONS：
    0 不动  1 上  2 下  3 左  4 右
"""
import os
import pickle

import neat
import pygame

from config.game_config import NEAT_CONFIG_PATH, GLOBAL_RNG
from src.game_env import GameEnv
from src.controller import Controller, controller_from_genome
from src.renderer import Renderer

CKPT = os.path.join(os.path.dirname(__file__), "..", "checkpoints")

# 键 → 动作 ID（与 game_env.ACTIONS 对齐）
KEY_TO_ACTION = {
    pygame.K_UP: 1, pygame.K_w: 1,
    pygame.K_DOWN: 2, pygame.K_s: 2,
    pygame.K_LEFT: 3, pygame.K_a: 3,
    pygame.K_RIGHT: 4, pygame.K_d: 4,
}


def _load_neat_config() -> neat.Config:
    return neat.Config(
        neat.DefaultGenome, neat.DefaultReproduction,
        neat.DefaultSpeciesSet, neat.DefaultStagnation, NEAT_CONFIG_PATH,
    )


def _load_checkpoints() -> tuple:
    """返回 (maze, chaser_genome, runner_genome)。

    优先用 *_final.pkl；缺失则给出清晰报错，指引先训练。
    """
    mazes_path = os.path.join(CKPT, "mazes.pkl")
    chaser_path = os.path.join(CKPT, "chaser_final.pkl")
    runner_path = os.path.join(CKPT, "runner_final.pkl")
    for p in (mazes_path, chaser_path, runner_path):
        if not os.path.exists(p):
            raise FileNotFoundError(
                f"找不到 {p}。请先运行 `python main.py train` 完成训练。"
            )
    with open(mazes_path, "rb") as f:
        maze_data = pickle.load(f)
    maze = maze_data["mazes"][0] if isinstance(maze_data, dict) else maze_data[0]
    with open(chaser_path, "rb") as f:
        chaser_genome = pickle.load(f)
    with open(runner_path, "rb") as f:
        runner_genome = pickle.load(f)
    return maze, chaser_genome, runner_genome


def _human_action(pressed_keys) -> int:
    """把当前按下的键转成动作。多键时取一个确定优先级（上>下>左>右）。"""
    for key, action in KEY_TO_ACTION.items():
        if pressed_keys[key]:
            return action
    return 0


def play(human_role: str = "chaser") -> None:
    """human_role: 'chaser' | 'runner' —— 人扮演谁，AI 扮演对方。"""
    if human_role not in ("chaser", "runner"):
        raise ValueError(f"human_role 必须是 'chaser' 或 'runner'，收到 {human_role!r}")

    maze, chaser_genome, runner_genome = _load_checkpoints()
    cfg = _load_neat_config()
    ai_role = "runner" if human_role == "chaser" else "chaser"
    ai_genome = runner_genome if ai_role == "runner" else chaser_genome
    ai_ctrl = controller_from_genome(ai_genome, cfg)

    pygame.init()
    env = GameEnv(maze)
    title = f"人 vs AI  你={human_role}（红追/蓝逃）  方向键/WASD 移动  ESC退出"
    renderer = Renderer(maze, title=title)
    print(f"[play] 你扮演 {human_role}，AI 扮演 {ai_role}")

    try:
        while True:
            if "__quit__" in renderer.pump_events():
                break

            keys = pygame.key.get_pressed()
            human_action = _human_action(keys)

            if human_role == "chaser":
                ai_action = ai_ctrl.act(env.sense(ai_role))
                res = env.step(human_action, ai_action)
            else:
                ai_action = ai_ctrl.act(env.sense(ai_role))
                res = env.step(ai_action, human_action)

            caption = f"你={human_role}  AI={ai_role}"
            renderer.draw(env, {"caption": caption})

            if res.done:
                # 结束后停留一帧让玩家看到结果，再等 ESC/关窗
                renderer.draw(env, {"caption": caption + "  按ESC退出"})
                _wait_for_exit(renderer)
                break
    finally:
        pygame.quit()


def _wait_for_exit(renderer: Renderer) -> None:
    """游戏结束后阻塞，直到用户 ESC 或关窗。"""
    while True:
        if "__quit__" in renderer.pump_events():
            return
        renderer.clock.tick(15)  # 低帧率空转，省 CPU
