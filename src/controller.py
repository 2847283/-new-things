"""AI 控制器：把观测向量（来自 sensors.observe）映射到一个离散动作 ID。

动作 ID 约定见 game_env.ACTIONS（0 不动 / 1 上 / 2 下 / 3 左 / 4 右）。
控制器本身不关心网络实现，只要 net.activate(obs)->list[float]
返回长度 == NUM_ACTIONS 的激活值即可（neat-python 的前馈网络正好满足）。
"""
from src.game_env import NUM_ACTIONS


class Controller:
    """包装一个带 activate() 的网络，argmax 选动作。"""

    def __init__(self, net):
        self.net = net

    def act(self, obs: list[float]) -> int:
        out = self.net.activate(obs)
        # argmax：取最大激活值对应的动作；并列时取最小下标（确定、可复现）
        best, best_i = out[0], 0
        for i in range(1, len(out)):
            if out[i] > best:
                best, best_i = out[i], i
        return best_i


def controller_from_genome(genome, config) -> Controller:
    """用 neat-python 把 genome 实例化成前馈网络并包成 Controller。

    genome/config 的真实类型由 Task 7（NEAT 评估器）提供；
    这里只依赖 neat.nn.FeedForwardNetwork.create 的契约。
    """
    import neat
    net = neat.nn.FeedForwardNetwork.create(genome, config)
    return Controller(net)
