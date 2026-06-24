from src.controller import Controller
from src.game_env import NUM_ACTIONS


class FakeNet:
    """模拟 neat 网络：activate 返回预设的固定输出向量。"""
    def __init__(self, outputs: list[float]):
        self.outputs = outputs
        self.last_obs = None

    def activate(self, obs):
        self.last_obs = obs
        return self.outputs


def test_act_picks_index_of_max():
    # 输出向量里下标 2 最大 → 动作 2
    c = Controller(FakeNet([0.1, 0.2, 0.9, 0.3, 0.0]))
    assert c.act([0.0] * 11) == 2


def test_act_returns_valid_action_id():
    net = FakeNet([0.5, 0.5, 0.5, 0.5, 0.5])
    c = Controller(net)
    a = c.act([1.0] * 11)
    assert 0 <= a < NUM_ACTIONS


def test_act_ties_pick_lowest_index():
    # 并列最大（下标 1 与 3 同值）→ 取较小下标 1
    c = Controller(FakeNet([0.2, 0.8, 0.1, 0.8, 0.0]))
    assert c.act([0.0] * 11) == 1


def test_act_passes_observation_to_network():
    net = FakeNet([0.0, 1.0, 0.0, 0.0, 0.0])
    c = Controller(net)
    obs = [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, -1.0]
    c.act(obs)
    assert net.last_obs == obs


def test_act_consistent_on_repeated_calls():
    """同一观测多次调用，结果应稳定可复现（确定性 argmax）。"""
    net = FakeNet([0.3, 0.7, 0.7, 0.2, 0.1])
    c = Controller(net)
    results = [c.act([0.5] * 11) for _ in range(5)]
    assert all(r == results[0] for r in results)


def test_controller_from_genome_bridges_net():
    """controller_from_genome 应调用 neat.nn.FeedForwardNetwork.create 并包装。"""
    import src.controller as ctrl_mod

    created = {}
    fake_net = FakeNet([0.0, 0.0, 1.0, 0.0, 0.0])

    class FakeFeedForwardNet:
        @staticmethod
        def create(genome, config):
            created["genome"] = genome
            created["config"] = config
            return fake_net

    class FakeNeatMod:
        nn = type("nn", (), {"FeedForwardNetwork": FakeFeedForwardNet})

    # 替换 import neat 得到的模块
    import sys
    sys.modules["neat"] = FakeNeatMod()

    genome = object()
    config = object()
    c = ctrl_mod.controller_from_genome(genome, config)
    assert created["genome"] is genome and created["config"] is config
    assert c.act([0.0] * 11) == 2  # fake_net 输出下标 2 最大
