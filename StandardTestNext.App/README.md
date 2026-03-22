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
- 当前已支持环境变量：`STNEXT_APP_DEVICE_ID`、`STNEXT_APP_PRODUCT_KIND`、`STNEXT_APP_SAMPLING_MODE`、`STNEXT_MESSAGE_BUS`、`STNEXT_MESSAGE_BUS_HOST`、`STNEXT_MESSAGE_BUS_PORT`、`STNEXT_MESSAGE_BUS_CLIENT_ID`、`STNEXT_MESSAGE_BUS_TOPIC_PREFIX`、`STNEXT_MESSAGE_BUS_USERNAME`、`STNEXT_MESSAGE_BUS_PASSWORD`、`STNEXT_MESSAGEBUS_PUBLISH_TIMEOUT_SECONDS`、`STNEXT_MESSAGEBUS_SUBSCRIBE_TIMEOUT_SECONDS`
- 当前已支持命令行参数：`--config`、`--device-id`、`--product-kind`、`--sampling-mode`、`--message-bus`、`--message-bus-host`、`--message-bus-port`、`--message-bus-client-id`、`--message-bus-topic-prefix`、`--message-bus-username`、`--message-bus-password`
- App 入口 `Program.cs` 已修正为只启动 App 自身，不再误拉 Test 侧 Bootstrap/配置；消息总线配置改为直接从 `AppStartupOptions.MessageBus` 透传到 `MessageBusFactory`
- 已补 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，启动时会打印当前配置摘要，并对 provider、port、clientId、topicPrefix、samplingMode 做校验；当前已将明显非法配置（如不支持的 provider、空 `clientId` / `topicPrefix`、非法端口、非法 `samplingMode`）从“仅告警”推进为启动前直接失败，避免假绿配置继续进入运行态
- 当 `messageBus.provider=mqtt` 时，启动前还会额外检查 `messageBus.host` 非空，并对 `host:port` 做一次轻量 TCP probe；当前摘要已能区分 `reachable / timeout / connection-refused / dns-failed / auth-failed / probe-failed`，探测失败先记 warning，不直接阻断离线开发场景
- `AppBootstrap` / `MockMotorDeviceGateway` / `AppCommandConsumer` 已真正消费这套配置，避免设备标识、产品型号、采样模式继续写死在 demo 里
- `samplingMode=burst` 时会连续发布两笔样本，先作为后续“采样批次/缓存策略”演进前的最小配置化样板

## 后续建议结构
- `Adapters/`：设备驱动适配
- `Application/`：用例与编排
- `ContractsBridge/`：消息收发
- `UI/`：界面层

## 本小时进展补充
- App 默认查询主路径已不再把 in-proc adapter 组装逻辑硬编码在 `Program.cs`：当前统一经 `InProcAppQueryGatewayFactory` 进入默认 gateway 创建入口，主流程只保留一个共享接线点。
- 当前这条入口不再依赖编译期 `StandardTestNext.App -> StandardTestNext.Test` 直接项目引用，而是通过反射装配 `StandardTestNext.Test.Application.AppSide.InProcAppQueryGatewaySeedFactory`；这样 App 主项目仍只显式依赖 `StandardTestNext.Contracts`，但默认 recent/detail 查询预览已经能吃到 seeded in-proc 的真实记录/报告数据，而不是空仓储或纯 stub。
- Test 侧新增 `TestRecordQuerySeedFactory`，把默认 seeded rated params + realtime samples 收成共享种子契约；App 默认查询入口与后续 smoke/demo 宿主都可以复用同一套最小样本，不必再在多个入口各自手搓假数据。
- `TestReportSelection.SelectLightweight(...)` 已收口成显式 fallback：优先 `IsLightweightEntry`，再退 `manifest`，最后退到最早保存的一条报告；recent/detail 查询组装不再依赖仓储偶然顺序。
- `dotnet build StandardTestNext.sln --no-restore` 已再次复验通过，当前仍为 `0 warning / 0 error`。

## 下一步优先项
- App/Test 双端统一配置约定已整理到 `docs/RUNTIME_CONFIGURATION.md`，后续新增运行参数优先补这份公共说明，再分别落到双端 README 与样例配置
- 在 `samplingMode` 的最小开关之上，继续补真实采样周期、批次大小、设备连接参数等运行配置
- 已为消息总线切换补最小抽象：`IMessageBus` + `MessageBusFactory`，当前已落 `inmemory` 与 `mqtt` 两类 provider；后续可在不改 Bootstrap 签名的前提下继续补更完整的生命周期与其他实现
- 本小时继续清理消息总线兼容层：`IMessageBus` 不再继承已标记 obsolete 的 `IMessageSubscriber` shim，`Subscribe<T>` 已直接收口到总线主接口，当前 `dotnet build StandardTestNext.sln --no-restore` 已恢复为 0 warning / 0 error
- App 侧 `TestRecordQueryGatewayStub` 已同步补齐 `ItemDetails / ReportSummaries / Primary/Lightweight report` 占位输出，避免 Contracts 已扩展而 App stub 仍停留在旧版 detail 结构；后续接入真实 Test 查询网关时，App UI/API 可直接按新契约消费记录详情与轻量报告摘要
- 本小时继续把 App 消费路径从“只打 recent list 预览”前推到“真实读取 detail 契约摘要”：`AppBootstrap` 在打印 recent record 之后，会继续拉取 `GetDetailAsync(recordCode)` 并输出 `items / samples / keyPoints / continuous / report artifact` 摘要，先把 App 侧读取 detail 合同的最小消费面接上，为后续替换掉 stub、接入真实 Test 查询桥接铺路
- 本小时继续把默认查询入口从“主流程里直接 new stub”收口成独立工厂：新增 `Application/TestRecordQueryGatewayFactory.cs`，`AppBootstrap` 统一经由 `TestRecordQueryGatewayFactory.Create(() => _testRecordGateway)` 解析默认网关；当前仍默认回退到 `TestRecordQueryGatewayStub`，但后续接入进程内 adapter / 真实跨边界查询时，只需替换工厂解析逻辑，不必再回头改 App 主流程
- MQTT provider 本小时进一步补了连接后自动重订阅、断线后订阅状态清理、重复订阅控制，并切到 `clean session = false`，避免后续联调时一断线就把订阅上下文丢干净
- Test 侧 `Application/RuntimeBridge/ConnectivityProbeResult.cs` 已与 App 侧保持一致，改为直接复用 `StandardTestNext.Contracts.ConnectivityProbeResult` 全局别名，避免同一类启动前连通性诊断在双端继续分叉维护
- 新增 `scripts/run-mqtt-smoke.sh`：在本机已有 MQTT broker 的前提下，可一键拉起 App/Test 双进程 smoke run，统一透传 `mqtt` provider、broker 地址、topicPrefix、clientId，并把日志落到 `artifacts/logs/`，作为真实跨进程联调入口。
- 本小时继续补了发布/订阅超时治理：`MessageBusOptions` 新增 `PublishTimeoutSeconds` / `SubscribeTimeoutSeconds`，当前通过环境变量 `STNEXT_MESSAGEBUS_PUBLISH_TIMEOUT_SECONDS`、`STNEXT_MESSAGEBUS_SUBSCRIBE_TIMEOUT_SECONDS` 注入；MQTT 发布/订阅超时会抛出明确的 `TimeoutException` 并补发重连调度
- App/Test 两侧现已都使用独立 `messageBus` 配置节（连接参数级），并已补齐对应 CLI 覆盖参数，下一步重点转为真正 provider 落地与更严格校验，而不是再回头改入口参数结构
