# App 维护文档（本地待同步草稿）

## 本次维护说明
- 时间：2026-03-20 22:15（Asia/Shanghai）
- 本轮先检查腾讯文档 App 维护文档 `DWWN4S01oY21JaFVI`，但腾讯文档 MCP 仍受 `access limit (400007)` 限制，暂时无法在线读取/回写。
- 因在线文档额度受限，本次先将真实维护内容落到仓库本地草稿，待额度恢复后优先同步。
- 本轮继续按待办前推 next-gen/App 主干，重点不是再补口头说明，而是把 MQTT provider 从“识别配置”推进到“更稳的最小实现”。

## 代码修改记录
- 调整 `StandardTestNext.App/ContractsBridge/MqttMessageBus.cs`
  - 新增 `ConnectedAsync` / `DisconnectedAsync` 事件处理
  - 连接成功后自动重订阅已登记 topic，断线后清空订阅状态，避免后续重连时误判已订阅
  - 增加 `_resubscribing` 防重入保护，减少重复重订阅
  - `Publish<T>` 复用已解析 topic，避免重复构造
  - 将 MQTT 连接选项切到 `clean session = false`，为后续稳定联调保留更合理的会话语义
  - `OnMessageReceivedAsync` 改为向 handler 传递 `JsonElement.Clone()`，避免文档释放后继续引用 payload 根节点
- 更新 `docs/RUNTIME_CONFIGURATION.md`
  - 把 MQTT provider 当前能力更新为：已支持连接后自动重订阅、断线后订阅状态清理、重复订阅控制
  - 下一步待办改为主动重连/backoff、失败诊断、发布/订阅超时治理
- 更新 `StandardTestNext.App/README.md`
  - 同步本小时 MQTT provider 实际增强内容
- 复验结果
  - `dotnet build StandardTestNext.sln --no-restore` 通过，`0 Warning / 0 Error`

## Git 提交记录
- 当前仓库最近提交（维护前基线）
  - `0f08893 docs: align runtime docs with mqtt implementation`
  - `8757e5a feat: add minimal mqtt message bus provider`
  - `e31d4a6 refactor: fail fast on invalid runtime configuration`
- 本次维护截至当前：代码已完成，待整理后提交 git

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步回在线文档 `DWWN4S01oY21JaFVI`
- 继续推进 MQTT provider 的主动重连/backoff 与连接失败诊断
- 将当前 fail-fast 护栏继续扩到 `host` 连通性、认证参数、真实部署自检
- 在 `samplingMode` 基础上补真实采样周期、批次大小、设备连接参数配置

## 下次更新时间
- 2026-03-20 23:10（Asia/Shanghai）或下次定时维护时
