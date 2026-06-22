"""入口：python main.py [--seed N] [train|play|watch]"""
import argparse, sys
from config.game_config import GLOBAL_RNG

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--seed", type=int, default=None, help="随机种子，省略则真随机")
    parser.add_argument("command", nargs="?", default="train",
                        choices=["train", "play", "watch"])
    args = parser.parse_args()
    GLOBAL_RNG.seed(args.seed)
    print(f"[main] command={args.command}  seed={GLOBAL_RNG.getstate()[1][0] if args.seed is not None else 'random'}")
    print(f"TODO: {args.command} 模式将在后续 Task 实现")

if __name__ == "__main__":
    main()
