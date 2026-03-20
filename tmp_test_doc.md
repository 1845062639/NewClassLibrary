# Test 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-20 22:15（Asia/Shanghai）
- 本轮先检查腾讯文档 Test 维护文档 `DWUNpRERyY01pcVhK`，但腾讯文档 MCP 仍受 `access limit (400007)` 限制，暂时无法在线读取/回写。
- 因在线文档额度受限，本次先将真实维护内容落到仓库本地草稿，待额度恢复后优先同步。
- 本轮同时复验 `next-gen` 主干可构建性与 Test 运行链路，并继续按待办把 MQTT provider 从“最小能跑”往“更稳联调”方向推进。

## 代码修改记录
- 复验 `dotnet build StandardTestNext.sln --no-restore`：通过，`0 Warning / 0 Error`
- 复验 `dotnet run --project StandardTestNext.Test --no-build -- --message-bus=inmemory --message-bus-client-id=stnext-test-hourly --message-bus-topic-prefix=stnext --persistence=memory`：通过
- 本轮直接推进的代码/文档变更：
  - `StandardTestNext.App/ContractsBridge/MqttMessageBus.cs`：补连接成功后自动重订阅、断线后订阅状态清理、重复订阅控制、`clean session = false`、`JsonElement.Clone()` 传递
  - `docs/RUNTIME_CONFIGURATION.md`：同步 MQTT provider 当前真实能力与下一步待办
  - `StandardTestNext.App/README.md`：同步消息总线实现增强说明
- 影响：虽然本轮改动主落点在 App 侧总线实现，但 Test 侧后续切 MQTT 联调时会直接受益；当前 Test 主干在 `inmemory` 模式下已再次验证可运行

## Git 提交记录
- 当前仓库最近提交（维护前基线）
  - `0f08893 docs: align runtime docs with mqtt implementation`
  - `8757e5a feat: add minimal mqtt message bus provider`
  - `e31d4a6 refactor: fail fast on invalid runtime configuration`
- 本次维护截至当前：代码已完成，待整理后提交 git

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步回在线文档 `DWUNpRERyY01pcVhK`
- 继续推进 MQTT provider 的主动重连/backoff 与连接失败诊断
- 将 fail-fast 护栏继续扩到 SQLite 目录权限、host 连通性、认证参数与部署自检
- 继续把报告查询/导出边界从控制台验证推进到更稳定的 API/DTO 视图

## 下次更新时间
- 2026-03-20 23:10（Asia/Shanghai）或下次定时维护时
