# CHANGELOG 2026-03-19

## 本次修改目标
基于当前从 git 拉下来的旧 Test 与旧 App，整理既有架构与数据模型经验，并在 `next-gen/` 中建立新的主干项目骨架。

---

## 一、新建的 next-gen 项目文件

### 解决方案
- `next-gen/StandardTestNext.sln`

### Contracts
- `next-gen/StandardTestNext.Contracts/StandardTestNext.Contracts.csproj`
- `next-gen/StandardTestNext.Contracts/ContractTopics.cs`
- `next-gen/StandardTestNext.Contracts/MotorRatedParamsContract.cs`
- `next-gen/StandardTestNext.Contracts/MotorRealtimeSampleContract.cs`

### App
- `next-gen/StandardTestNext.App/StandardTestNext.App.csproj`
- `next-gen/StandardTestNext.App/Program.cs`
- `next-gen/StandardTestNext.App/Application/AppBootstrap.cs`
- `next-gen/StandardTestNext.App/Application/MotorSamplingService.cs`
- `next-gen/StandardTestNext.App/ContractsBridge/IMessagePublisher.cs`
- `next-gen/StandardTestNext.App/ContractsBridge/InMemoryMessageBus.cs`
- `next-gen/StandardTestNext.App/Devices/IMotorDeviceGateway.cs`
- `next-gen/StandardTestNext.App/Devices/MockMotorDeviceGateway.cs`

### Test
- `next-gen/StandardTestNext.Test/StandardTestNext.Test.csproj`
- `next-gen/StandardTestNext.Test/Program.cs`
- `next-gen/StandardTestNext.Test/Application/TestBootstrap.cs`
- `next-gen/StandardTestNext.Test/Application/Services/MotorTestSessionService.cs`

### 文档
- `next-gen/README.md`
- `next-gen/WORKLOG_2026-03-19.md`
- `next-gen/docs/ARCHITECTURE.md`
- `next-gen/docs/MIGRATION_PLAN.md`
- `next-gen/docs/VERTICAL_SLICE_01.md`

---

## 二、当前已落地内容

### 1. 新系统分层
已明确拆分为：
- `StandardTestNext.App`
- `StandardTestNext.Test`
- `StandardTestNext.Contracts`

### 2. 第一批共享契约
已定义：
- `MotorRatedParamsContract`
- `MotorRealtimeSampleContract`
- `ContractTopics`

### 3. 第一条垂直切片骨架
已建立：
- Test 侧生成额定参数示例
- App 侧读取模拟设备采样并发布样本
- 两侧统一通过 Contracts 交换边界模型

---

## 三、参考旧系统提炼出的事实
本轮主要参考来源：
- `ClassLibary/StandardTest.Model/`
- `ClassLibary/StandardTest.ViewModel/`
- `ClassLibary/StandardTest/`
- `app-repos/StandardTestApp/`

目前已确认：
1. 旧 Test 侧的核心实体主线为：
   - `ProductType`
   - `TestRecord`
   - `TestRecordItem`
   - `FileAttachment`
2. 旧系统中附件更多是挂在 `TestRecordItem` 维度
3. 旧 App 中包含大量设备侧控制界面与现场变体项目
4. 新系统应吸收其设备控制经验，但不继承其历史耦合结构

---

## 四、后续计划
1. 在 `next-gen/Contracts` 中补：
   - `TestCommandContract`
   - `DeviceStatusContract`
2. 在 `next-gen/Test` 中补试验记录主线模型
3. 在 `next-gen/App` 中补设备适配层抽象
4. 继续从旧 Test / App 中提炼记录、附件、控制动作的稳定模型

---

## 五、说明
旧仓库中的部分改动仍保留在工作区，当前主方向已经转为 `next-gen/` 新项目主干建设。后续检查时，可优先查看 `next-gen/` 下文件。 
