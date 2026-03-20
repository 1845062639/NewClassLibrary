# StandardTestNext.App

新一代现场采集与控制端。

## 第一版目标
- 消费 `MotorRatedParamsContract`
- 采集实时数据并输出 `MotorRealtimeSampleContract`
- 建立设备驱动适配层，而不是把设备控制直接写进业务窗体

## 当前已接通的运行配置入口
- `AppRuntimeConfiguration` + `AppRuntimeConfigurationLoader`：默认从 `appsettings.app.json` 读取 App 侧运行配置
- `AppStartupOptionsParser`：支持“配置文件 < 环境变量 < 命令行参数”的覆盖链路
- 当前已支持配置项：`deviceId`、`productKind`、`samplingMode`、`messageBus.provider|host|port|clientId|topicPrefix|username|password`
- 当前已支持环境变量：`STNEXT_APP_DEVICE_ID`、`STNEXT_APP_PRODUCT_KIND`、`STNEXT_APP_SAMPLING_MODE`、`STNEXT_MESSAGE_BUS`、`STNEXT_MESSAGE_BUS_HOST`、`STNEXT_MESSAGE_BUS_PORT`、`STNEXT_MESSAGE_BUS_CLIENT_ID`、`STNEXT_MESSAGE_BUS_TOPIC_PREFIX`、`STNEXT_MESSAGE_BUS_USERNAME`、`STNEXT_MESSAGE_BUS_PASSWORD`
- 当前已支持命令行参数：`--config`、`--device-id`、`--product-kind`、`--sampling-mode`
- App 入口 `Program.cs` 已修正为只启动 App 自身，不再误拉 Test 侧 Bootstrap/配置；消息总线配置改为直接从 `AppStartupOptions.MessageBus` 透传到 `MessageBusFactory`
- 已补 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，启动时会打印当前配置摘要，并对 provider、port、clientId、topicPrefix、samplingMode 做最小告警，先把“配置校验/启动前自检”从待办推进到可运行骨架
- `AppBootstrap` / `MockMotorDeviceGateway` / `AppCommandConsumer` 已真正消费这套配置，避免设备标识、产品型号、采样模式继续写死在 demo 里
- `samplingMode=burst` 时会连续发布两笔样本，先作为后续“采样批次/缓存策略”演进前的最小配置化样板

## 后续建议结构
- `Adapters/`：设备驱动适配
- `Application/`：用例与编排
- `ContractsBridge/`：消息收发
- `UI/`：界面层

## 下一步优先项
- App/Test 双端统一配置约定已整理到 `docs/RUNTIME_CONFIGURATION.md`，后续新增运行参数优先补这份公共说明，再分别落到双端 README 与样例配置
- 在 `samplingMode` 的最小开关之上，继续补真实采样周期、批次大小、设备连接参数等运行配置
- 已为消息总线切换补最小抽象：`IMessageBus` + `MessageBusFactory`，当前先落 `inmemory`，后续可在不改 Bootstrap 签名的前提下补 MQTT/其他实现
- App/Test 两侧现已都使用独立 `messageBus` 配置节（连接参数级），下一步重点转为统一配置目录约定说明与真正 provider 落地，而不是再回头改入口参数结构
