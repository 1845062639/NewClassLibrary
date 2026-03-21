## 本次维护说明
- 时间：2026-03-21 14:48（Asia/Shanghai）
- 本轮先尝试检查并更新腾讯文档 App/Test 在线维护文档，但腾讯文档 MCP 继续返回 `400007 access limit`，本小时仍无法在线写回，因此先把维护内容落到本地草稿，待额度恢复后直接同步。
- App 侧本小时仍未扩散到旧 App 历史仓库改造，继续坚持“旧 App 只作参考样本，新主干在 next-gen 正向建设”的路线。
- 虽然本轮主要代码变更仍发生在 Test 查询层，但这部分继续直接影响后续 App 列表页/详情页/API 对记录摘要口径的消费，因此已同步纳入 App 维护文档。
- 这轮继续往前推进了一步：新增 `TestRecordQueryFacade`，把 `ListRecentForApp / GetDetailForApp` 从底层查询服务中显式抬出来，作为面向 App/API 的轻量查询出口，后续 App 接入时不必再同时理解 summary/detail 两套模型与 mapper 细节。

## 代码修改记录
- App 侧本小时未新增旧 App 仓库源码修改。
- 与 App/Test 协作边界直接相关的真实变更位于 `next-gen/StandardTestNext.Test`：
  - 新增 `Application/Services/TestRecordQueryFacade.cs`
  - 更新 `Application/TestBootstrap.cs`
  - 更新 `README.md`
- 本小时新增修正点：
  - `TestBootstrap` 改为通过 `TestRecordQueryFacade` 获取 `Recent record views` 与 `Reloaded record view`，不再直接由 demo 启动链路混用底层 query service 与 view mapper 细节。
  - `README.md` 已同步记录这层“面向 App/API 的轻量查询出口”，避免维护文档与仓库真实边界脱节。
- 本轮真实验证：
  - `dotnet build /root/.openclaw/workspace/next-gen/StandardTestNext.sln --no-restore` 通过，`0 Warning / 0 Error`
  - `dotnet run --project /root/.openclaw/workspace/next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build` 通过，运行输出已继续包含 `Recent record views`、`Reloaded record view`、`Primary record report`、`Lightweight report artifact`

## Git 提交记录
- 本轮新增真实 git 提交后再回填；当前先完成代码与文档整理。
- 提交目标：把 `TestRecordQueryFacade` 与本轮维护文档一并整理成一笔可追踪提交。

## 待办事项
- 等腾讯文档额度恢复后，把本地草稿同步回在线 App/Test 维护文档。
- 让 `StandardTestNext.App` 后续直接接 `TestRecordQueryFacade` 暴露的轻量查询出口，而不是继续依赖底层 summary/detail DTO 与 mapper 细节。
- 继续把查询 DTO 与宿主 API 草案收口，减少后续 App 接 UI 时再次自行拼摘要字段的风险。

## 下次更新时间
- 2026-03-21 15:45（Asia/Shanghai）或下次定时维护时
