# App 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-20 18:45（Asia/Shanghai）
- 本轮先检查腾讯文档 App 维护文档 `DWWN4S01oY21JaFVI`，但腾讯文档 MCP 仍受 `access limit (-32603)` 限制，暂时无法在线读取/回写。
- 因在线文档额度受限，本次先将真实维护内容落到仓库本地草稿，待额度恢复后优先同步。
- 本轮已继续按待办前推 next-gen/App 主干，不只停留在说明层，而是把配置校验真正推进到启动前失败策略并完成 build/run 复验。

## 代码修改记录
- 调整 `StandardTestNext.App/ContractsBridge/IMessageBus.cs`
  - 取消 `IMessageBus : IMessagePublisher, IMessageSubscriber` 对已废弃兼容接口的继承
  - 改为直接在 `IMessageBus` 上声明 `Subscribe<T>`，避免运行主干仍被 obsolete shim 牵连
- 调整 `StandardTestNext.App/ContractsBridge/RuntimeConfigurationValidationResult.cs`
  - 新增 `Errors` / `HasErrors`
  - 让配置校验结果不再只有 warning，能表达真正阻断启动的非法配置
- 调整 `StandardTestNext.App/Application/Services/RuntimeConfigurationValidator.cs`
  - 将未实现的 `mqtt` provider、非法端口、空 `clientId` / `topicPrefix`、非法 `samplingMode` 由 warning 提升为 error
- 调整 `StandardTestNext.App/Application/Services/RuntimeConfigurationConsoleReporter.cs`
  - 新增 `ThrowIfInvalid`
  - 启动前先输出配置摘要，再对 error 直接 fail-fast
- 调整 `StandardTestNext.App/Program.cs`
  - 在创建 MessageBus 前执行 `ThrowIfInvalid(validation)`
- 复验结果
  - `dotnet build StandardTestNext.sln --no-restore` 通过，`0 Warning / 0 Error`
  - `dotnet run --project StandardTestNext.App --no-build -- --message-bus=inmemory --message-bus-client-id=stnext-app-hourly --message-bus-topic-prefix=stnext --sampling-mode=single` 通过

## Git 提交记录
- 当前仓库最近提交（维护前基线）
  - `bcfeda1 refactor: normalize runtime overrides and query projections`
  - `881763b fix: surface mqtt runtime gap in config validation`
  - `a37ea7a refactor: decouple shared message bus config shape`
- 本次维护截至当前：代码已完成，待整理后提交 git

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步回在线文档 `DWWN4S01oY21JaFVI`
- 继续推进 `MessageBusFactory` 的 MQTT provider 实现，而不是只停留在配置层识别
- 将当前 fail-fast 护栏继续扩到 `host` 连通性、认证参数、真实部署自检
- 在 `samplingMode` 基础上补真实采样周期、批次大小、设备连接参数配置

## 下次更新时间
- 2026-03-20 19:45（Asia/Shanghai）或下次定时维护时
