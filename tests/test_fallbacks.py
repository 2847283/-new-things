"""超时/异常兜底测试：覆盖训练-对战全链路的"出错不崩"边界。

包括：
    - evaluator 单个体评估抛异常 → fitness 回落 0.0，不影响其他个体
    - play_loop 在缺 checkpoint 时抛清晰的 FileNotFoundError
    - play_loop 非法角色名抛 ValueError
    - renderer draw 幂等、无头环境不崩
"""
import os

# 无头环境先于 pygame 导入设置
os.environ.setdefault("SDL_VIDEODRIVER", "dummy")

import neat
import pytest

from src.maze import make_maze_set
from src.neat_evaluator import SelfPlayEvaluator

CFG_PATH = os.path.join(os.path.dirname(__file__), "..", "config", "neat_config.txt")


def _make_cfg() -> neat.Config:
    return neat.Config(
        neat.DefaultGenome, neat.DefaultReproduction,
        neat.DefaultSpeciesSet, neat.DefaultStagnation, CFG_PATH,
    )


class _BoomGenome:
    """缺 connections 等属性，FeedForwardNetwork.create 会抛 AttributeError。"""
    key = "boom"
    fitness = None


def test_evaluator_swallows_individual_failure():
    """一个坏 genome 不应拖垮整代评估：兜底成 0.0，正常个体仍出分。"""
    cfg = _make_cfg()
    mazes = make_maze_set(1, seed=1)
    ev = SelfPlayEvaluator(mazes, cfg)
    good = list(neat.Population(cfg).population.values())[:2]
    chasers = [_BoomGenome()] + good
    runners = list(neat.Population(cfg).population.values())[:2]
    ev.evaluate_generation(chasers, runners)
    # 坏个体被兜底
    assert chasers[0].fitness == 0.0
    # 正常个体仍有有限分
    for g in chasers[1:]:
        assert g.fitness is not None and g.fitness >= 0


def test_play_raises_when_checkpoint_missing(tmp_path, monkeypatch):
    """缺训练产物时 play 应给清晰指引，而非晦涩的 pickle 报错。"""
    import src.play_loop as pl
    # 把 checkpoint 目录指到空临时目录
    monkeypatch.setattr(pl, "CKPT", str(tmp_path))
    with pytest.raises(FileNotFoundError, match="请先运行"):
        pl._load_checkpoints()


def test_play_rejects_invalid_role():
    import src.play_loop as pl
    with pytest.raises(ValueError):
        pl.play(human_role="wizard")


def test_renderer_draw_is_idempotent_headless():
    """无头环境下连画两帧不应崩（覆盖缓存 + HUD 路径）。"""
    import pygame
    pygame.init()
    try:
        from src.game_env import GameEnv
        from src.renderer import Renderer
        maze = make_maze_set(1, seed=3)[0]
        r = Renderer(maze, title="headless test")
        env = GameEnv(maze)
        env.step(4, 3)
        r.draw(env, {"caption": "a"})
        env.step(4, 3)
        r.draw(env, {"caption": "b", "fps": 30})
        # pump_events 在无头下返回空列表，不崩
        assert r.pump_events() == []
    finally:
        pygame.quit()
