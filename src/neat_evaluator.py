"""NEAT 自我对弈评估器。

两个独立种群（追方 / 跑方）以 self-play 形式评估：
每个个体从对面种群随机抽 EVAL_OPPONENTS_PER_INDIVIDUAL 个对手，
在所有迷宫上各打一局，奖励按下列公式累加并取均值。

奖励函数（决定 AI 学什么）：
    追方：抓住 → 1000 - frame（越快越好）；没抓住 → 0
    跑方：存活整局 → frame * 2；被抓住 → frame（活得越久越好）
"""
import random

import neat

from config.game_config import EVAL_OPPONENTS_PER_INDIVIDUAL, MAX_FRAMES_PER_EPISODE
from src.game_env import GameEnv
from src.controller import Controller


class SelfPlayEvaluator:
    """双种群 self-play 适应度评估器。"""

    def __init__(self, mazes, cfg, rng=None):
        self.mazes = mazes
        self.cfg = cfg
        self.rng = rng or random.Random()
        self._net_cache: dict = {}

    # ---------- 网络缓存 ----------
    def _net_for(self, genome):
        """同一代内反复评估同一个 genome 时复用网络，避免反复 build。"""
        key = id(genome)
        if key not in self._net_cache:
            self._net_cache[key] = neat.nn.FeedForwardNetwork.create(genome, self.cfg)
        return self._net_cache[key]

    # ---------- 适应度公式（独立、纯函数、便于单测） ----------
    @staticmethod
    def _score_chaser(winner: str, frame: int) -> float:
        """追方奖励：抓住越快越好；没抓住 0 分。"""
        if winner == "chaser":
            return 1000.0 - frame
        return 0.0

    @staticmethod
    def _score_runner(winner: str, frame: int) -> float:
        """跑方奖励：存活翻倍，被抓也按存活帧计分。"""
        if winner == "runner":
            return frame * 2.0
        return float(frame)

    # ---------- 单局对战 ----------
    def _run_episode(self, chaser_ctrl: Controller, runner_ctrl: Controller, maze):
        env = GameEnv(maze)
        while True:
            res = env.step(
                chaser_ctrl.act(env.sense("chaser")),
                runner_ctrl.act(env.sense("runner")),
            )
            if res.done:
                return res.winner, res.frame

    # ---------- 单个体评估 ----------
    def _eval_chaser(self, genome, runner_pool) -> float:
        net = self._net_for(genome)
        k = min(EVAL_OPPONENTS_PER_INDIVIDUAL, len(runner_pool))
        opponents = self.rng.sample(runner_pool, k)
        total = 0.0
        n_games = 0
        for opp in opponents:
            opp_net = self._net_for(opp)
            for maze in self.mazes:
                winner, frame = self._run_episode(
                    Controller(net), Controller(opp_net), maze,
                )
                total += self._score_chaser(winner, frame)
                n_games += 1
        return total / n_games if n_games else 0.0

    def _eval_runner(self, genome, chaser_pool) -> float:
        net = self._net_for(genome)
        k = min(EVAL_OPPONENTS_PER_INDIVIDUAL, len(chaser_pool))
        opponents = self.rng.sample(chaser_pool, k)
        total = 0.0
        n_games = 0
        for opp in opponents:
            opp_net = self._net_for(opp)
            for maze in self.mazes:
                winner, frame = self._run_episode(
                    Controller(opp_net), Controller(net), maze,
                )
                total += self._score_runner(winner, frame)
                n_games += 1
        return total / n_games if n_games else 0.0

    # ---------- 一代评估（入口） ----------
    def evaluate_generation(self, chaser_genomes, runner_genomes) -> None:
        """评估一代两个种群，把适应度写回每个 genome.fitness。

        任何个体评估抛错都兜底成 0.0，不让单点异常拖垮整代训练。
        """
        self._net_cache.clear()
        for g in chaser_genomes:
            g.fitness = 0.0
        for g in runner_genomes:
            g.fitness = 0.0

        for g in chaser_genomes:
            try:
                g.fitness = self._eval_chaser(g, runner_genomes)
            except Exception as e:  # noqa: BLE001 —— 训练要稳，单个体失败兜底
                print(f"[warn] chaser {g.key} 评估失败: {e}")
                g.fitness = 0.0

        for g in runner_genomes:
            try:
                g.fitness = self._eval_runner(g, chaser_genomes)
            except Exception as e:  # noqa: BLE001
                print(f"[warn] runner {g.key} 评估失败: {e}")
                g.fitness = 0.0
