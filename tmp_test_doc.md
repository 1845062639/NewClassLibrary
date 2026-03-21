# Test维护文档

## 本次维护说明
- 维护时间：2026-03-21 17:11 Asia/Shanghai
- 本小时先按要求检查/更新腾讯文档 Test 维护文档 `DWUNpRERyY01pcVhK`，但腾讯文档 MCP 读取仍被 `400007 access limit` 拦截，在线文档本小时依旧无法同步；已继续维护本地草稿，待额度恢复后补写。
- 这轮继续沿着待办推进，不再只是从 Test 内部口头判断“App 不应直接引用 Test 实现层”，而是通过真实代码变更做反证：删除 `StandardTestNext.App` 对 `StandardTestNext.Test` 的项目引用，然后重新构建整个 solution。
- 真实结果说明 Test 侧当前承担的耦合责任比前一小时判断得更重：构建失败不仅因为 App 侧 adapter 仍依赖 `TestRecordQueryFacade`，还因为 Test 侧自己已经反向依赖 `StandardTestNext.App.Application` 与 `StandardTestNext.App.ContractsBridge`。也就是说，当前 App/Test 并不是“App 单向引用 Test”这么简单，而是已经形成了真实双向耦合。
- 这使得本轮结论更加明确：如果要把 Test 查询边界真正整理成可被 App/API/宿主复用的输出，下一步不能只动 facade 或删某个引用，而是必须先抽出独立共享 contract / bridge 层，把消息总线与查询 contract 从 App/Test 两边都剥离出来。

## 代码修改记录
- 本小时没有直接修改 Test 侧源码文件；主要通过真实构建验证暴露 Test 当前对 App 层的反向依赖。
- 本轮通过 `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` 暴露出的关键 Test 侧问题：
  - `Application/TestBootstrap.cs` 仍 `using StandardTestNext.App.Application` 与 `using StandardTestNext.App.ContractsBridge`
  - `Application/Services/TestRuntimeConfiguration.cs`
  - `Application/Services/TestRuntimeConfigurationSupport.cs`
  - `Application/Services/TestRuntimeOrchestrator.cs`
  - `Program.cs`
  - 上述文件都还依赖 App 层的消息总线/运行时桥接类型
- 这轮得到的真实技术结论：
  - `TestRecordQueryFacade` 本身不是当前唯一阻塞点
  - 更核心的阻塞点是 Test 运行时基础设施仍借用了 App 的 ContractsBridge/Application 类型
  - 如果不先抽共享 bridge 层，后续不管是 App 接 Test，还是 API 接 Test，都会继续卡在跨宿主耦合上

## Git 提交记录
- 本小时没有新的 git 提交。
- 当前产出是“通过真实构建失败定位出双向耦合点”，还没有收敛成可提交的安全改动。

## 待办事项
- 从 Test 侧开始梳理并迁移当前反向依赖的 App 类型，优先处理：
  - `IMessageBus / IMessagePublisher / IMessageBusConfiguration / MessageBusOptions / RuntimeConfigurationValidationResult / ConnectivityProbeResult`
- 为 Test 查询边界建立真正独立的共享出口：
  - 查询 contract（列表项/详情项）
  - 消息总线基础契约
  - 运行时配置必要契约
- 在共享 bridge 层完成前，不要再尝试让 App 直接消费 `StandardTestNext.Test` 程序集实现。
- 腾讯文档额度恢复后，把本地草稿同步回 Test 维护文档 `DWUNpRERyY01pcVhK`。

## 下次更新时间
- 2026-03-21 18:11 Asia/Shanghai
