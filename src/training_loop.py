"""训练主循环：无渲染、纯计算，跑 N 代双种群 self-play 共同进化。

产物（写到 checkpoints/）：
    mazes.pkl              —— 当前 seed 生成的 NUM_MAZES 个迷宫快照，供 play/watch 复现
    chaser_gen{N}.pkl      —— 每代追方 champion genome
    runner_gen{N}.pkl      —— 每代跑方 champion genome
    chaser_final.pkl       —— 训练结束追方 champion
    runner_final.pkl       —— 训练结束跑方 champion
    chaser_pop_final.pkl   —— 最后一代追方整代（含 config 友好的可复现存档）
    runner_pop_final.pkl   —— 最后一代跑方整代

复现契约：play/watch 读 mazes.pkl，保证对战用的迷宫与训练一致。
"""
import os
import pickle

import neat

from config.game_config import NUM_MAZES, GLOBAL_RNG, NEAT_CONFIG_PATH
from src.maze import make_maze_set
from src.neat_evaluator import SelfPlayEvaluator

# checkpoints/ 目录（项目根下），与 main.py 的运行目录解耦
CKPT = os.path.join(os.path.dirname(__file__), "..", "checkpoints")


def _ensure_ckpt_dir() -> str:
    os.makedirs(CKPT, exist_ok=True)
    return CKPT


def _save_champion(genomes, path: str) -> None:
    """把适应度最高的 genome pickle 落盘。fitness 为 None 按 0 处理。"""
    best = max(genomes, key=lambda g: g.fitness if g.fitness is not None else 0.0)
    with open(path, "wb") as f:
        pickle.dump(best, f)


def _next_generation(pop: neat.Population, cfg: neat.Config) -> neat.Population:
    """手动推进一代：reproduce → 物种重新划分 → 代号 +1。

    neat-python 的 Population.run() 用回调驱动，但 self-play 需要我们在
    evaluate_genomes 回调之外同时拿到两个种群的 genome 列表，所以这里
    显式调用 reproduction/species，自己控制迭代节奏。
    """
    pop.reproduction.reproduce(cfg, pop.species, cfg.pop_size, pop.generation)
    pop.generation += 1
    pop.species.speciate(cfg, pop.population, pop.generation)
    return pop


def run(generations: int = 100,
        neat_config_path: str = NEAT_CONFIG_PATH,
        seed=None) -> None:
    """跑 generations 代 self-play 训练。

    seed 仅作日志展示用；迷宫复现完全靠 GLOBAL_RNG（由 main.py --seed 初始化），
    这样 play/watch 只要加载 mazes.pkl 即可，不必重传 seed。
    """
    cfg = neat.Config(
        neat.DefaultGenome, neat.DefaultReproduction,
        neat.DefaultSpeciesSet, neat.DefaultStagnation, neat_config_path,
    )

    ckpt = _ensure_ckpt_dir()

    # —— 用 GLOBAL_RNG 生成迷宫，并把 seed 留痕快照 ——
    maze_seed = GLOBAL_RNG.random()
    mazes = make_maze_set(NUM_MAZES, seed=maze_seed)
    mazes_path = os.path.join(ckpt, "mazes.pkl")
    with open(mazes_path, "wb") as f:
        # 存 dict，便于未来扩展（加版本号/seed），向后兼容旧读法也可
        pickle.dump({"mazes": mazes, "seed": seed}, f)
    print(f"[train] 生成 {len(mazes)} 个迷宫，已存 {mazes_path}")

    # —— 双种群 + 评估器 ——
    evaluator = SelfPlayEvaluator(mazes, cfg, rng=GLOBAL_RNG)
    chaser_pop = neat.Population(cfg)
    runner_pop = neat.Population(cfg)

    for gen in range(generations):
        chaser_genomes = list(chaser_pop.population.values())
        runner_genomes = list(runner_pop.population.values())

        evaluator.evaluate_generation(chaser_genomes, runner_genomes)

        cb = max((g.fitness for g in chaser_genomes
                  if g.fitness is not None), default=0.0)
        rb = max((g.fitness for g in runner_genomes
                  if g.fitness is not None), default=0.0)
        print(f"[Gen {gen}] 追方最佳={cb:.1f}  跑方最佳={rb:.1f}")

        # 每代 champion 落盘（用 _final 作占位，循环结束再覆盖最新）
        _save_champion(chaser_genomes, os.path.join(ckpt, "chaser_gen%d.pkl" % gen))
        _save_champion(runner_genomes, os.path.join(ckpt, "runner_gen%d.pkl" % gen))

        chaser_pop = _next_generation(chaser_pop, cfg)
        runner_pop = _next_generation(runner_pop, cfg)

    # —— 收尾：最终 champion ——
    chaser_genomes = list(chaser_pop.population.values())
    runner_genomes = list(runner_pop.population.values())
    _save_champion(chaser_genomes, os.path.join(ckpt, "chaser_final.pkl"))
    _save_champion(runner_genomes, os.path.join(ckpt, "runner_final.pkl"))
    print("[train] 训练完成：chaser_final.pkl / runner_final.pkl 已落盘")
