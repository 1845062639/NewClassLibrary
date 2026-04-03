Test维护文档

本次维护说明
维护时间：2026-03-23 12:15 Asia/Shanghai
本小时原计划先直接更新腾讯文档 App/Test 维护页，但在读取两份远端文档时腾讯文档 MCP 返回 400007 access limit，当前未能成功写回远端。因此先基于 next-gen/ 当前工作区与本地维护草稿继续推进可直接落地的开发，避免因文档接口额度问题停滞。

本小时 Test 侧的真实进展主要有两部分：
1. 继续保留并复验已有 legacy payload 解析与 query adapter smoke tests，确认本轮 App 侧 query gateway smoke harness 加入后没有破坏 Test 端验证链路。
2. 在尝试把 App query gateway 校验塞进 Test 项目时，真实暴露出“Test 不能反向依赖 App 命名空间”的编译边界；已及时回退该尝试，保持 Test 项目只验证自身与共享 Contracts，不引入反向依赖污染。

代码修改记录
本小时 Test 侧直接发生的真实变更：
- next-gen/StandardTestNext.Test/Program.cs
  - 一度临时接入 AppQueryGatewaySmokeTests.Run() 以尝试统一 smoke 入口。
  - 因 Test 项目不直接暴露/依赖 App 命名空间，编译报错后已回退，恢复为仅执行 TestRecordLegacyPayloadReaderSmokeTests 与 TestRecordQueryGatewayAdapterSmokeTests。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordLegacyPayloadReaderSmokeTests.cs
  - 本小时未新增内容，但已再次参与 smoke 验证，继续承担 legacy payload JSON 解析与摘要格式化的最小回归支点。
- next-gen/StandardTestNext.Test/Application/Services/TestRecordQueryGatewayAdapterSmokeTests.cs
  - 本小时未新增内容，但已再次参与 smoke 验证，继续承担 query adapter detail/list 摘要映射验证。

本轮 Test 侧阶段性结论：
- Test 项目适合继续承载“payload reader / query adapter / runtime bridge”这类共享与测试端验证，不适合为了 App 入口配置校验去反向拉 App 依赖。
- App 入口配置与 query gateway 模式校验应留在 App 项目本身，通过独立 smoke harness 验证；Test 侧继续保持边界清晰。

Git 提交记录
本小时没有新增 git 提交。
当前 next-gen/ 最近相关提交仍为：
- f3e83b7 feat: expose legacy payload summaries in query contracts
- cd7973d refactor: add null fallback for app query gateway
- 21bf4fe refactor: align test runtime bridge with shared contracts
- a954cac refactor: unify test record contracts and partition pipeline
- a9b8448 refactor: decouple app seed gateway wiring

本小时真实验证
- 读取腾讯文档 App/Test 维护文档：失败，腾讯文档 MCP 返回 400007 access limit。
- 初次构建尝试：因将 App smoke 测试放入 Test 项目导致命名空间/依赖边界编译失败，已识别并回退。❌ -> 已修正
- dotnet build next-gen/StandardTestNext.sln --no-restore -nologo ✅
- dotnet run --project next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj --no-build -- --smoke ✅
当前结果：0 Warning / 0 Error。

待办事项
- 腾讯文档额度恢复后，优先把本小时 Test/App 维护内容真实写回远端维护文档。
- 继续让 Test 侧 smoke tests 聚焦 payload reader、query adapter、runtime bridge 等共享边界，不把 App 入口配置验证硬塞回 Test。
- 继续为 TestRecordQueryGatewayAdapter 增补更贴近真实 query contract 的最小回归验证。
- 在 legacy 摘要链路稳定后，再继续推进 measurement / report artifact contract 的细化。

下次更新时间
2026-03-23 13:11（Asia/Shanghai）
