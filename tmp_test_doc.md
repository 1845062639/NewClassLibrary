# Test 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-20 18:45（Asia/Shanghai）
- 本轮先检查腾讯文档 Test 维护文档 `DWUNpRERyY01pcVhK`，但腾讯文档 MCP 仍受 `access limit (-32603)` 限制，暂时无法在线读取/回写。
- 因在线文档额度受限，本次先将真实维护内容落到仓库本地草稿，待额度恢复后优先同步。
- 本轮同时复验 `next-gen` 主干可构建性，并继续按待办把 App/Test 运行配置护栏从“提示”推进到“启动前失败”。

## 代码修改记录
- 复验 `dotnet build StandardTestNext.sln --no-restore`：通过，`0 Warning / 0 Error`
- 复验 `dotnet run --project StandardTestNext.Test --no-build -- --message-bus=inmemory --message-bus-client-id=stnext-test-hourly --message-bus-topic-prefix=stnext --persistence=memory`：通过
- 本轮直接推进的代码变更：
  - `StandardTestNext.App/ContractsBridge/IMessageBus.cs` 改为直接声明 `Subscribe<T>`，去掉对 obsolete 兼容接口的间接依赖
  - `StandardTestNext.App/ContractsBridge/RuntimeConfigurationValidationResult.cs` 新增 `Errors` / `HasErrors`
  - `StandardTestNext.Test/Application/Services/TestRuntimeConfigurationSupport.cs` 将未实现的 `mqtt` provider、非法端口、空 `clientId` / `topicPrefix`、非法 `persistenceMode` 提升为 error，并新增 `ThrowIfInvalid`
  - `StandardTestNext.Test/Program.cs` 在真正启动前执行 `ThrowIfInvalid(validation)`
- 影响：Test 侧当前不再只是“打印 warning 然后继续跑”，而是对显著误配置直接 fail-fast，减少后续部署假绿

## Git 提交记录
- 当前仓库最近提交（维护前基线）
  - `bcfeda1 refactor: normalize runtime overrides and query projections`
  - `881763b fix: surface mqtt runtime gap in config validation`
  - `a37ea7a refactor: decouple shared message bus config shape`
- 本次维护截至当前：代码已完成，待整理后提交 git

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步回在线文档 `DWUNpRERyY01pcVhK`
- 继续推进 `MessageBusFactory` 的 MQTT provider 实现
- 将 fail-fast 护栏继续扩到 SQLite 目录权限、host 连通性、认证参数与部署自检
- 继续把报告查询/导出边界从控制台验证推进到更稳定的 API/DTO 视图

## 下次更新时间
- 2026-03-20 19:45（Asia/Shanghai）或下次定时维护时
