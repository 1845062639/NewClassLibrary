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
1. 增加 TestCommandContract
2. 增加 DeviceStatusContract
3. 增加真实消息总线抽象（先 MQTT，再可替换）
4. 增加首个设备适配接口族
