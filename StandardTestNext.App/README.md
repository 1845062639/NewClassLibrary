# StandardTestNext.App

新一代现场采集与控制端。

## 第一版目标
- 消费 `MotorRatedParamsContract`
- 采集实时数据并输出 `MotorRealtimeSampleContract`
- 建立设备驱动适配层，而不是把设备控制直接写进业务窗体

## 当前已接通的运行配置入口
- `AppRuntimeConfiguration` + `AppRuntimeConfigurationLoader`：默认从 `appsettings.app.json` 读取 App 侧运行配置
- `AppStartupOptionsParser`：支持“配置文件 < 环境变量 < 命令行参数”的覆盖链路
- 当前已支持配置项：`deviceId`、`productKind`、`samplingMode`、`messageBusProvider`
- 当前已支持环境变量：`STNEXT_APP_DEVICE_ID`、`STNEXT_APP_PRODUCT_KIND`、`STNEXT_APP_SAMPLING_MODE`、`STNEXT_MESSAGE_BUS`
- 当前已支持命令行参数：`--config`、`--device-id`、`--product-kind`、`--sampling-mode`
- `AppBootstrap` / `MockMotorDeviceGateway` / `AppCommandConsumer` 已真正消费这套配置，避免设备标识、产品型号、采样模式继续写死在 demo 里
- `samplingMode=burst` 时会连续发布两笔样本，先作为后续“采样批次/缓存策略”演进前的最小配置化样板

## 后续建议结构
- `Adapters/`：设备驱动适配
- `Application/`：用例与编排
- `ContractsBridge/`：消息收发
- `UI/`：界面层

## 下一步优先项
- 将 App/Test 双端配置项进一步抽到统一命名规范与公共说明文档，避免并行演进时键名漂移
- 在 `samplingMode` 的最小开关之上，继续补真实采样周期、批次大小、设备连接参数等运行配置
- 已为消息总线切换补最小抽象：`IMessageBus` + `MessageBusFactory` + `messageBusProvider`，当前先落 `inmemory`，后续可在不改 Bootstrap 签名的前提下补 MQTT/其他实现
- 下一步继续把消息总线配置从当前最小 provider 字段扩成独立配置节（连接串、客户端标识、topic 前缀等），而不是继续混在入口参数里
