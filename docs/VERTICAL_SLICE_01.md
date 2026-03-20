# Vertical Slice 01 - Motor 基础链路

## 目标
建立第一条最小可运行链路：
- Test 侧准备额定参数
- App 侧准备实时采样
- 两边统一引用 Contracts

## 当前已落地
### Contracts
- `MotorRatedParamsContract`
- `MotorRealtimeSampleContract`
- `ContractTopics`

### Test
- `MotorTestSessionService.BuildDemoRatedParams()`

### App
- `IMotorDeviceGateway`
- `MockMotorDeviceGateway`
- `MotorSamplingService.PublishSample()`

## 下一步
1. 已完成消息总线最小抽象收口：`IMessageBus` + `MessageBusFactory` 已接到 App/Test 入口，下一步补 MQTT 实现并保留 in-memory 测试替身
2. 为 TestCommandContract / DeviceStatusContract 增加 ACK、失败原因与状态流转约束
3. 增加首个设备适配接口族，并从旧 App 提炼一个真实设备网关样板
4. 已将同类 runtime 配置模型平移到 App 侧：`appsettings.app.json` + 环境变量 + 命令行三级入口已接通，`messageBus.*` 连接参数与 Test 侧同构；当前也已补最小配置校验/启动摘要，下一步转向 MQTT provider 落地与更严格的失败策略
