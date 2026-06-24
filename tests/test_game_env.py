import random
from src.maze import generate_corridor_maze
from src.game_env import GameEnv, NUM_ACTIONS, ACTIONS, cell_center
from src.sensors import input_size
from config.game_config import CELL, SQUARE_SPEED, CHASE_RADIUS, MAX_FRAMES_PER_EPISODE

maze = generate_corridor_maze(random.Random(1))

def test_reset_places_squares_at_starts():
    env = GameEnv(maze)
    cpx, cpy = cell_center(*maze.chaser_start_cell)
    rpx, rpy = cell_center(*maze.runner_start_cell)
    assert (env.chaser.x, env.chaser.y) == (cpx, cpy)
    assert (env.runner.x, env.runner.y) == (rpx, rpy)
    assert env.frame == 0 and env.done is False and env.winner is None

def test_reset_idempotent_resets_state():
    env = GameEnv(maze)
    env.step(2, 1)  # 动几步
    assert env.frame > 0
    env.reset()
    assert env.frame == 0 and env.done is False

def test_sense_returns_correct_size_for_both_roles():
    env = GameEnv(maze)
    assert len(env.sense("chaser")) == input_size()
    assert len(env.sense("runner")) == input_size()

def test_runner_wins_on_timeout():
    env = GameEnv(maze)
    # 双方都不动（动作0），跑满帧数 → runner 逃脱
    for _ in range(MAX_FRAMES_PER_EPISODE - 1):
        res = env.step(0, 0)
        assert not res.done
    res = env.step(0, 0)
    assert res.done and res.winner == "runner"

def test_chaser_wins_on_capture():
    # 手工把 runner 摆到 chaser 旁边，验证一步即捕获
    env = GameEnv(maze)
    env.reset()
    cx, cy = env.chaser.x, env.chaser.y
    env.runner.x = cx + CHASE_RADIUS  # 刚好在半径边上
    env.runner.y = cy
    res = env.step(0, 0)  # 都不动
    assert res.done and res.winner == "chaser"

def test_step_moves_square_when_unobstructed():
    env = GameEnv(maze)
    before_x = env.chaser.x
    # chaser 起点 cell(1,1) 中心(30,30)，向右(动作4)应能走 SPEED
    res = env.step(4, 0)
    assert env.chaser.x == before_x + SQUARE_SPEED
    assert res.frame == 1

def test_step_after_done_is_safe():
    env = GameEnv(maze)
    env.runner.x = env.chaser.x + 1
    env.runner.y = env.chaser.y
    res = env.step(0, 0)
    assert res.done
    # 再次 step 不应抛异常，状态保持
    res2 = env.step(1, 1)
    assert res2.done and res2.winner == "chaser"

def test_num_actions_is_5():
    assert NUM_ACTIONS == 5
    assert set(ACTIONS.keys()) == {0, 1, 2, 3, 4}
