# StandardTestNext.Test

新一代试验业务与数据处理端。

## 第一版目标
- 管理试验任务与记录中间态
- 接收 App 上传的实时样本
- 完成解析、校验、计算与报告输出接口抽象

## 后续建议结构
- `Domain/`：核心领域模型
- `Application/`：用例服务
- `Infrastructure/`：存储、消息、文件
- `Reports/`：报表接口与实现
