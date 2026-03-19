# StandardTestNext.App

新一代现场采集与控制端。

## 第一版目标
- 消费 `MotorRatedParamsContract`
- 采集实时数据并输出 `MotorRealtimeSampleContract`
- 建立设备驱动适配层，而不是把设备控制直接写进业务窗体

## 后续建议结构
- `Adapters/`：设备驱动适配
- `Application/`：用例与编排
- `ContractsBridge/`：消息收发
- `UI/`：界面层
