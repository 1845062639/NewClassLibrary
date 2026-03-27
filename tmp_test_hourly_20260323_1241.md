# Test维护文档

## 本次维护说明
- 维护时间：2026-03-23 12:41 Asia/Shanghai
- 本小时先尝试检查并更新腾讯文档 Test 维护文档，但腾讯文档 MCP 在读取阶段返回 **400007 access limit**，因此远端文档本轮未能直接更新。
- 尽管远端受限，本小时仍继续执行文档待办对应的真实开发工作，重点推进 **Test 提供给 App 的 query gateway 边界**：
  - 让 App 显式声明需要 `auto / seeded-inproc / null-fallback` 哪种查询来源；
  - 让 Test 侧 smoke 验证从“正常启动必跑”改为“显式 `--smoke` 才跑”；
  - 继续补齐 legacy payload summary 在 Test->App query 链路上的最小验证闭环。
- 已完成真实验证：
  - `dotnet build StandardTestNext.sln --no-restore -nologo` ✅
  - `dotnet run --project StandardTestNext.App/StandardTestNext.App.csproj --no-build -nologo -- --smoke` ✅
  - `dotnet run --project StandardTestNext.Test/StandardTestNext.Test.csproj --no-build -nologo -- --smoke` ✅
- 当前结果：Test 侧对 App 的 seeded query 出口已能被显式选择；正常 Test 启动不再自动执行 smoke；legacy payload summary 相关 smoke 覆盖仍保持可跑通。

## 代码修改记录
- `next-gen/StandardTestNext.Test/Application/Services/TestStartupOptions.cs`
  - 新增 `RunSmokeTests` 选项。
- `next-gen/StandardTestNext.Test/Application/Services/TestStartupOptionsParser.cs`
  - 支持 `--smoke` / `--run-smoke-tests` / `--no-smoke` 参数解析。
- `next-gen/StandardTestNext.Test/Program.cs`
  - 改成仅在显式 smoke 参数下运行 `TestRecordLegacyPayloadReaderSmokeTests` 与 `TestRecordQueryGatewayAdapterSmokeTests`。
- `next-gen/StandardTestNext.Test/Application/TestBootstrap.cs`
  - 从正常运行路径移除默认 smoke 调用，避免主流程每次启动都混入验证逻辑。
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordLegacyPayloadReaderSmokeTests.cs`
  - 保持并补强对 legacy payload 解析、曲线图片计数、incoming/winding 标志与 list summary 的 smoke 覆盖。
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryGatewayAdapterSmokeTests.cs`
  - 验证 Test query gateway adapter 向 App 暴露的 list/detail/legacy summary 是否符合预期。
- `next-gen/StandardTestNext.Test/Application/Services/TestRecordConsolePresenter.cs`
  - 新增 legacy payload summary 输出，便于 Test 侧控制台快速判断当前记录负载结构。
- `next-gen/StandardTestNext.App/Application/InProcAppQueryGatewayFactory.cs`
- `next-gen/StandardTestNext.App/Application/Services/AppQueryGatewayMode.cs`
- `next-gen/StandardTestNext.App/Application/Services/AppStartupOptions.cs`
- `next-gen/StandardTestNext.App/Application/Services/AppStartupOptionsParser.cs`
- `next-gen/StandardTestNext.App/Program.cs`
- `next-gen/StandardTestNext.App/StandardTestNext.App.csproj`
  - 以上改动共同把 Test 对 App 的 seeded query gateway 暴露为显式、可观测、可 smoke 的接入方式。

## Git 提交记录
- 本小时已新增 git 提交：
  - `2de2c7c refactor: add explicit app query gateway smoke entrypoints`
- 当前 next-gen 最近提交：
  - `2de2c7c refactor: add explicit app query gateway smoke entrypoints`
  - `f3e83b7 feat: expose legacy payload summaries in query contracts`
  - `cd7973d refactor: add null fallback for app query gateway`

## 待办事项
1. 在腾讯文档额度恢复后，把本地这轮维护记录同步回 Test 维护文档 `DWUNpRERyY01pcVhK`。
2. 继续把 Test 对 App 的 query 供给从 seeded in-proc 推进到更真实的 sqlite-backed / remote gateway 方案。
3. 再补一轮更贴近 App 消费端的 smoke/回归验证，覆盖 requested/resolved 与实际 detail summary 是否一致。
4. 后续在 query 来源更稳定后，评估是否拆回更轻的边界层，避免 App 长期直接引用 Test 工程。

## 下次更新时间
- 预计下次更新时间：2026-03-23 13:41（Asia/Shanghai）
