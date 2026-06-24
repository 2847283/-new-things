"""NEAT 自我对弈评估器测试。"""
import math
import os

import neat

from src.maze import make_maze_set
from src.neat_evaluator import SelfPlayEvaluator

CFG_PATH = os.path.join(os.path.dirname(__file__), "..", "config", "neat_config.txt")


def _make_cfg() -> neat.Config:
    return neat.Config(
        neat.DefaultGenome, neat.DefaultReproduction,
        neat.DefaultSpeciesSet, neat.DefaultStagnation, CFG_PATH,
    )


def _fresh_genomes(cfg: neat.Config, n: int = 3) -> list:
    pop = neat.Population(cfg)
    return list(pop.population.values())[:n]


def test_evaluate_assigns_fitness_to_all():
    cfg = _make_cfg()
    mazes = make_maze_set(2, seed=123)
    ev = SelfPlayEvaluator(mazes, cfg)
    chasers = _fresh_genomes(cfg, 3)
    runners = _fresh_genomes(cfg, 3)
    ev.evaluate_generation(chasers, runners)
    assert all(g.fitness is not None for g in chasers)
    assert all(g.fitness is not None for g in runners)
    assert all(g.fitness >= 0 for g in chasers)
    assert all(g.fitness >= 0 for g in runners)


def test_fitness_is_finite():
    cfg = _make_cfg()
    mazes = make_maze_set(2, seed=123)
    ev = SelfPlayEvaluator(mazes, cfg)
    chasers = _fresh_genomes(cfg, 2)
    runners = _fresh_genomes(cfg, 2)
    ev.evaluate_generation(chasers, runners)
    for g in chasers + runners:
        assert math.isfinite(g.fitness)


def test_config_dims_match_network():
    """NEAT 配置的输入/输出维度必须和 sensors/game_env 对齐。"""
    cfg = _make_cfg()
    from src.sensors import input_size
    from src.game_env import NUM_ACTIONS
    assert cfg.genome_config.num_inputs == input_size()
    assert cfg.genome_config.num_outputs == NUM_ACTIONS


def test_runner_who_survives_scores_more_than_caught():
    """跑方存活越久分越高：构造一个被立刻抓住 vs 一个撑满 600 帧的对照。

    直接用内部 _score_runner 验证奖励公式形状，避免依赖网络策略。
    """
    cfg = _make_cfg()
    mazes = make_maze_set(1, seed=7)
    ev = SelfPlayEvaluator(mazes, cfg)
    survive = ev._score_runner("runner", 600)   # 撑满
    caught = ev._score_runner("chaser", 5)      # 5 帧被抓
    assert survive > caught
    assert survive == 600 * 2                    # frames_survived * 2
    assert caught == 5                           # 仅 frames_survived


def test_chaser_who_catches_scores_more_than_misses():
    cfg = _make_cfg()
    mazes = make_maze_set(1, seed=7)
    ev = SelfPlayEvaluator(mazes, cfg)
    fast = ev._score_chaser("chaser", 5)   # 5 帧抓住
    miss = ev._score_chaser("runner", 600)  # 没抓住（runner 赢）
    assert fast == 1000 - 5
    assert miss == 0
