"""入口：python main.py [--seed N] [train [代数]|play [chaser|runner]|watch]"""
import argparse

from config.game_config import GLOBAL_RNG


def main():
    parser = argparse.ArgumentParser(
        description="追逃 AI 小游戏：NEAT 双种群 self-play 训练 + 人机对战"
    )
    parser.add_argument("--seed", type=int, default=None, help="随机种子，省略则真随机")
    parser.add_argument("command", nargs="?", default="train",
                        choices=["train", "play", "watch"])
    parser.add_argument("arg", nargs="?", default=None,
                        help="train: 代数(默认100)  play: chaser|runner(默认chaser)")
    args = parser.parse_args()

    GLOBAL_RNG.seed(args.seed)
    seed_desc = args.seed if args.seed is not None else "random"
    print(f"[main] command={args.command}  seed={seed_desc}")

    if args.command == "train":
        from src.training_loop import run as train_run
        gens = int(args.arg) if args.arg else 100
        train_run(generations=gens, seed=args.seed)
    elif args.command == "play":
        from src.play_loop import play
        play(human_role=args.arg or "chaser")
    elif args.command == "watch":
        from src.watch_loop import watch
        watch()


if __name__ == "__main__":
    main()
