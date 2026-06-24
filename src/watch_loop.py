"""观看 AI 自我对弈模式。

加载训练产物（chaser_final / runner_final）+ mazes.pkl，
让两个 champion AI 在每个迷宫上各打一局，pygame 实时渲染。

操作：
    R —— 重开当前局
    N —— 跳到下一个迷宫
    ESC / 关窗 —— 退出
    每局结束自动短暂停顿后切下一局；所有迷宫打完循环回第一个。
"""
import os
import pickle
import time

import neat
import pygame

from config.game_config import NEAT_CONFIG_PATH
from src.game_env import GameEnv
from src.controller import controller_from_genome
from src.renderer import Renderer
from src.play_loop import _load_checkpoints, _load_neat_config

# 一局结束后停留多久再切下一局（秒）
END_PAUSE = 1.5


def _make_controllers(cfg):
    """加载双 champion，返回 (chaser_ctrl, runner_ctrl)。"""
    _, chaser_genome, runner_genome = _load_checkpoints()
    chaser_ctrl = controller_from_genome(chaser_genome, cfg)
    runner_ctrl = controller_from_genome(runner_genome, cfg)
    return chaser_ctrl, runner_ctrl


def _load_mazes():
    mazes_path = os.path.join(
        os.path.dirname(__file__), "..", "checkpoints", "mazes.pkl"
    )
    with open(mazes_path, "rb") as f:
        data = pickle.load(f)
    return data["mazes"] if isinstance(data, dict) else data


def watch(fps: int | None = None) -> None:
    """fps=None 用配置默认；可传整数加速/减速观战。

    循环逻辑：当前局结束 → 停 END_PAUSE 秒 → 下一迷宫 → 全部打完回到第 0 个。
    """
    cfg = _load_neat_config()
    chaser_ctrl, runner_ctrl = _make_controllers(cfg)
    mazes = _load_mazes()
    if not mazes:
        raise FileNotFoundError("mazes.pkl 为空，请先运行 `python main.py train`。")

    pygame.init()
    renderer = Renderer(mazes[0], title="观看 AI 自我对弈  R重开 N下一局 ESC退出")
    print(f"[watch] 共 {len(mazes)} 个迷宫，chaser vs runner 对战开始")

    maze_idx = 0
    env = GameEnv(mazes[maze_idx])

    try:
        while True:
            events = renderer.pump_events()
            if "__quit__" in events:
                break
            if pygame.K_n in events:
                maze_idx = (maze_idx + 1) % len(mazes)
                env = GameEnv(mazes[maze_idx])
                renderer.maze = mazes[maze_idx]
                renderer._wall_surface = renderer._build_wall_surface(mazes[maze_idx])
                continue
            if pygame.K_r in events:
                env = GameEnv(mazes[maze_idx])
                continue

            if not env.done:
                res = env.step(
                    chaser_ctrl.act(env.sense("chaser")),
                    runner_ctrl.act(env.sense("runner")),
                )
                caption = f"迷宫 {maze_idx + 1}/{len(mazes)}"
                renderer.draw(env, {"caption": caption, "fps": fps})
                if res.done:
                    winner = "追方捕获！" if res.winner == "chaser" else "跑方逃脱！"
                    print(f"[watch] 迷宫 {maze_idx + 1}: {winner} (帧 {res.frame})")
                    time.sleep(END_PAUSE)
                    maze_idx = (maze_idx + 1) % len(mazes)
                    env = GameEnv(mazes[maze_idx])
                    renderer.maze = mazes[maze_idx]
                    renderer._wall_surface = renderer._build_wall_surface(mazes[maze_idx])
            else:
                # 结束后空转一帧等用户/或停顿结束切场
                renderer.draw(env, {"caption": f"迷宫 {maze_idx + 1}/{len(mazes)}"})
    finally:
        pygame.quit()
