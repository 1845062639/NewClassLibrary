# App维护文档

## 本次维护说明
- 维护时间：2026-03-21 17:11 Asia/Shanghai
- 本小时先按要求检查/更新腾讯文档 App 维护文档 `DWWN4S01oY21JaFVI`，但再次调用腾讯文档 MCP 读取时仍被 `400007 access limit` 拦截，在线文档本小时依旧无法实际回写；因此已继续维护本地草稿，待额度恢复后同步。
- 本轮继续按待办往前推进，不再停留在“App 不该直接引用 Test”的口头结论，而是实际把 `StandardTestNext.App` 对 `StandardTestNext.Test` 的项目引用拆掉并重新构建验证。
- 真实验证结果比预期更关键：拆掉 `App -> Test` 之后，solution 构建立刻暴露出更深一层双向耦合——除了 App 仍有 `TestRecordQueryGatewayAdapter` 直接引用 `TestRecordQueryFacade / TestRecordListView / TestRecordDetailView`，Test 侧自身也已经通过 `StandardTestNext.App.ContractsBridge` 与 `StandardTestNext.App.Application` 吃进了 App 类型。这说明当前问题不是单点引用，而是 App/Test 边界整体还没真正抽离。
- 因为这轮目标是持续推进而不是制造更大破坏，我保留了这次“拆引用即爆出真实双向耦合”的验证结果，先不在本小时硬拆整个消息总线与查询桥接层，避免把主干拉进更大范围半成品重构。

## 代码修改记录
- 本小时真实修改的 App 侧文件：
  - `next-gen/StandardTestNext.App/StandardTestNext.App.csproj`
- 真实变更内容：
  - 删除了 `StandardTestNext.App.csproj` 中对 `..\StandardTestNext.Test\StandardTestNext.Test.csproj` 的 `ProjectReference`，用真实编译来验证 App 是否已经具备独立宿主边界。
- 真实构建结果：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` 失败
  - 暴露出的关键耦合点包括：
    - App 侧 `Application/TestRecordQueryGatewayAdapter.cs` 仍直接依赖 `StandardTestNext.Test.Application.Services`
    - Test 侧 `Application/TestBootstrap.cs` 仍直接依赖 `StandardTestNext.App.Application`
    - Test 侧多个运行时配置/编排文件仍依赖 `StandardTestNext.App.ContractsBridge`
- 这轮没有继续提交更多源码改动到桥接实现层，原因是先确认了真正需要拆的是“共享消息/查询 contract 层”，而不只是删一条项目引用。

## Git 提交记录
- 本小时没有新的 git 提交。
- 原因：当前工作停在一个真实但未收敛的架构验证点（拆掉 App->Test 后暴露双向耦合），现在提交会把主干留在不可构建状态，不值得硬提。

## 待办事项
- 新建真正独立的共享桥接层（优先放到 `StandardTestNext.Contracts` 或新增独立共享项目），把以下跨宿主类型从 App/Test 双方各自实现里抽出来：
  - 消息总线接口与配置契约（当前散落在 `StandardTestNext.App.ContractsBridge`）
  - Test 查询对外 contract（当前 App 合同对象与 Test view 还未收敛到同一共享边界）
- App 侧处理顺序：
  - 先移除/重写 `Application/TestRecordQueryGatewayAdapter.cs` 对 Test 实现层的直接依赖
  - 再让 App 仅依赖共享 contract + stub/adapter 接口
- Test 侧处理顺序：
  - 先把 `StandardTestNext.App.ContractsBridge` 相关依赖迁走
  - 再让 `TestBootstrap / TestRuntimeOrchestrator / TestRuntimeConfiguration*` 只依赖共享 contract 项目
- 腾讯文档额度恢复后，把本地草稿同步回 App 维护文档 `DWWN4S01oY21JaFVI`。

## 下次更新时间
- 2026-03-21 18:11 Asia/Shanghai
