import random
WINDOW_W, WINDOW_H = 800, 600
CELL = 20
COLS, ROWS = WINDOW_W // CELL, WINDOW_H // CELL  # 40 x 30
FPS = 60
MAX_FRAMES_PER_EPISODE = 600
SQUARE_SIZE = 14
SQUARE_SPEED = 2.0
CHASE_RADIUS = 10
SENSOR_RAYS = 8                 # 8 方向墙射线
SENSOR_MAX_DIST = 160.0         # 单条射线最大探测距离（像素）≈ 8 个 cell
OPPONENT_MAX_DIST = 1000.0      # 对手相对位置归一化分母（≈ 屏幕对角线）
EVAL_OPPONENTS_PER_INDIVIDUAL = 5
POP_SIZE = 50
NUM_MAZES = 3
# NEAT 配置文件路径（相对项目根，由 main.py 的运行目录解析）
NEAT_CONFIG_PATH = "config/neat_config.txt"
GLOBAL_RNG = random.Random()  # 由 main.py 用 --seed 初始化，默认随机
