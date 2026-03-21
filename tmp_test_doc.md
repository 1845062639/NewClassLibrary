# Test维护文档

## 本次维护说明
- 维护时间：2026-03-21 23:41 Asia/Shanghai
- 本小时先尝试检查并更新腾讯文档 Test 维护文档 `DWUNpRERyY01pcVhK`，但腾讯文档 MCP 仍返回 `400007 access limit`，因此本轮继续先更新本地草稿，待额度恢复后再同步。
- 按文档待办继续直接推进了开发，不等用户喊继续：对现有 `scripts/run-mqtt-smoke.sh` 做了真实执行，验证当前 Test 侧 MQTT runtime bridge 是否已经真正进入跨进程 smoke 阶段。
- 真实结果：`dotnet build StandardTestNext.sln` 通过；随后运行 `BROKER_HOST=127.0.0.1 BROKER_PORT=1883 RUN_SECONDS=3 bash scripts/run-mqtt-smoke.sh`，Test 进程在 `MqttMessageBus.EnsureConnected()` 处因 `127.0.0.1:1883` 无 broker 而报 `Connection refused`。
- 这个结果是有效推进：它证明 Test 侧不是还停在编译期或配置装配期，而是已经实际走到了 MQTT provider 的真实连接路径。当前待解决问题已收敛为 broker 环境/连接诊断，而不是项目引用或 RuntimeBridge 结构问题。

## 代码修改记录
- 本小时检查并确认/纳入维护的文件：
  - `next-gen/scripts/run-mqtt-smoke.sh`
  - `next-gen/StandardTestNext.Test/Program.cs`
  - `next-gen/StandardTestNext.App/Program.cs`
  - `next-gen/docs/RUNTIME_CONFIGURATION.md`
- 本小时真实更新的维护文档草稿：
  - `next-gen/tmp_test_doc.md`
  - `next-gen/tmp_app_doc.md`
- 本小时真实执行记录：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln`
  - `BROKER_HOST=127.0.0.1 BROKER_PORT=1883 RUN_SECONDS=3 bash scripts/run-mqtt-smoke.sh`
- 本小时新增运行产物：
  - `next-gen/artifacts/logs/app-mqtt-smoke.log`
  - `next-gen/artifacts/logs/test-mqtt-smoke.log`
- 真实验证结果：
  - Test 侧在 `StandardTestNext.Test.Application.RuntimeBridge.MqttMessageBus.EnsureConnected()` 连接 broker 时收到 `SocketException (111): Connection refused`
  - App 侧也在对应 `MqttMessageBus.EnsureConnected()` 路径收到相同错误
  - 说明当前双端 MQTT 接入代码都已真实执行到连接阶段，失败原因集中在 broker 缺失/不可达

## Git 提交记录
- 本小时提交前状态：最近提交仍为 `7b44fbd refactor: remove test project dependency on app runtime bridge`
- 本轮准备补一笔提交，固化 smoke 验证与维护文档更新结果。

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步到 Test 维护文档 `DWUNpRERyY01pcVhK`。
- 在本机补可用 broker 后，再跑一轮真实 MQTT smoke，重点检查：
  - Test 发布 `TestCommandContract` 是否可被 App 消费
  - Test 持久化 SQLite 文件是否按预期生成
  - 双端日志中是否已出现完整的 publish/subscribe 路径
- 若仍失败，继续补更细的 MQTT 级错误诊断，而不是只停留在 socket refused。

## 下次更新时间
- 2026-03-22 00:41 Asia/Shanghai
