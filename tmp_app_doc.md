# App维护文档

## 本次维护说明
- 维护时间：2026-03-21 23:41 Asia/Shanghai
- 本小时先尝试检查并更新腾讯文档 App 维护文档 `DWWN4S01oY21JaFVI`，但腾讯文档 MCP 仍返回 `400007 access limit`，所以本轮继续先把真实维护内容回填到本地草稿，待额度恢复后同步。
- 本轮没有停在 build 通过，而是继续按待办推进了真实开发验证：检查并确认 `scripts/run-mqtt-smoke.sh` 已落地可用，并实际跑了一次 App/Test 双进程 MQTT smoke。
- 真实运行结果不是“假成功”：当前机器 `127.0.0.1:1883` 没有可用 broker，App/Test 都在 MQTT 连接阶段报 `Connection refused`。这说明 smoke 脚本和双端 MQTT 接入链路已经真正跑到联网阶段，而不是只停在参数拼装。
- 当前状态可以明确归纳为：构建链路已稳定通过，MQTT smoke 入口已存在且执行过，下一步瓶颈从“代码结构/编译问题”转到了“真实 broker 环境与连接诊断”。

## 代码修改记录
- 本小时检查并确认已存在/纳入维护的脚本与文档：
  - `next-gen/scripts/run-mqtt-smoke.sh`
  - `next-gen/docs/RUNTIME_CONFIGURATION.md`
  - `next-gen/StandardTestNext.App/README.md`
  - `next-gen/StandardTestNext.Test/README.md`
- 本小时真实更新的维护文档草稿：
  - `next-gen/tmp_app_doc.md`
  - `next-gen/tmp_test_doc.md`
- 本小时真实验证涉及代码/入口：
  - `next-gen/StandardTestNext.App/Program.cs`
  - `next-gen/StandardTestNext.Test/Program.cs`
  - `next-gen/scripts/run-mqtt-smoke.sh`
- 真实执行记录：
  - 运行 `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln`，结果通过，0 Warning / 0 Error
  - 运行 `BROKER_HOST=127.0.0.1 BROKER_PORT=1883 RUN_SECONDS=3 bash scripts/run-mqtt-smoke.sh`
  - 输出日志落地：
    - `next-gen/artifacts/logs/app-mqtt-smoke.log`
    - `next-gen/artifacts/logs/test-mqtt-smoke.log`
- 真实验证结果：
  - App 侧在 `MqttMessageBus.EnsureConnected()` 阶段连接 `127.0.0.1:1883` 被拒绝
  - Test 侧在 `MqttMessageBus.EnsureConnected()` 阶段连接 `127.0.0.1:1883` 被拒绝
  - 说明双端都已实际走到 MQTT 连接动作，当前失败点是 broker 环境缺失，而不是代码未接通

## Git 提交记录
- 本小时提交前状态：已存在未提交代码改动，最近提交为 `7b44fbd refactor: remove test project dependency on app runtime bridge`
- 本轮准备补充一笔提交，用于固化 smoke 验证与维护文档更新结果。

## 待办事项
- 腾讯文档额度恢复后，把本地草稿同步到 App 维护文档 `DWWN4S01oY21JaFVI`。
- 在当前机器补 broker 条件后，再跑一轮真实 MQTT smoke，优先验证：
  - App 订阅命令是否成功
  - Test 发布命令与记录链路是否成功
  - SQLite 持久化产物是否生成
- 若下一轮仍失败，继续把连接诊断从 TCP/Socket 层推进到更明确的 MQTT 认证、ACL、topic 级诊断。

## 下次更新时间
- 2026-03-22 00:41 Asia/Shanghai
