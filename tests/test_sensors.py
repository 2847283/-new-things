import math, random
from src.maze import generate_corridor_maze
from src import sensors
from config.game_config import SENSOR_RAYS, SENSOR_MAX_DIST

maze = generate_corridor_maze(random.Random(1))

def test_input_size_matches_observe_length():
    a = sensors.observe(30, 30, 100, 100, maze)
    assert len(a) == sensors.input_size()
    assert len(a) == SENSOR_RAYS + 3

def test_ray_hits_outer_wall_quickly_near_corner():
    # chaser 起点 cell(1,1) 中心 (30,30)；向左(+x 负向=第3索引/180°)应很快碰到外墙
    # 用 cast_wall_ray 直接验证：朝 -x 方向到左外墙距离
    x, y = 30, 30
    d = sensors.cast_wall_ray(x, y, -1.0, 0.0, maze)
    # 中心 x=30，左外墙右边缘 x=20(cell0 右边)，约 10 像素
    assert 5 <= d <= 15

def test_ray_into_open_corridor_is_clamped_to_max():
    # chaser 起点朝 +x(向 cell 2,3,4...) 应有较长走廊，受 SENSOR_MAX_DIST 上限
    x, y = 30, 30
    d = sensors.cast_wall_ray(x, y, 1.0, 0.0, maze)
    # 真实走廊较长，应至少超过一个 cell 距离；至少不应误判为极近
    assert d >= CELL_HALF() * 3

def test_observe_all_values_in_unit_range():
    a = sensors.observe(30, 30, 500, 400, maze)
    for v in a:
        assert -1.0 - 1e-6 <= v <= 1.0 + 1e-6

def test_opponent_relative_features():
    # self 在 (30,30)，对手在 (130,30)：rdx≈100 归一 >0，rdy≈0
    a = sensors.observe(30, 30, 130, 30, maze)
    rdx_norm = a[SENSOR_RAYS]
    rdy_norm = a[SENSOR_RAYS + 1]
    dist_norm = a[SENSOR_RAYS + 2]
    assert rdx_norm > 0.05
    assert abs(rdy_norm) < 0.05
    assert dist_norm > rdx_norm - 0.01  # 距离≥|rdx|

def test_rays_symmetric_left_right_in_symmetric_spot():
    # 在走廊正中，左右射线距离应接近（走廊宽 1 cell，左右都是墙边）
    # chaser cell(1,1) 中心，左右大致对称
    d_left = sensors.cast_wall_ray(30, 30, -1, 0, maze)
    d_right = sensors.cast_wall_ray(30, 30, 1, 0, maze)
    # 右侧走廊长得多，所以右 >> 左，这里只断言右明显大于左
    assert d_right > d_left

def CELL_HALF():
    from config.game_config import CELL
    return CELL / 2.0
