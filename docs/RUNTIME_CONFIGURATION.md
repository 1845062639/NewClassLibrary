# StandardTest Next 运行配置约定

## 目标
- 统一 App/Test 双端运行配置键名与部署目录约定
- 明确配置文件、环境变量、命令行参数之间的覆盖优先级
- 为后续接入 MQTT provider、真实设备连接参数、部署脚本提供稳定边界

## 配置来源优先级
统一遵循：

1. 配置文件
2. 环境变量
3. 命令行参数

即：**配置文件 < 环境变量 < 命令行参数**

## 默认配置文件
- App：`appsettings.app.json`
- Test：`appsettings.test.json`

默认查找顺序：
1. 程序输出目录（`AppContext.BaseDirectory`）
2. 当前工作目录
3. 若均不存在，则退回内置默认值

也支持通过命令行显式指定：
- `--config <path>`
- `--config=<path>`

## 共享配置节：messageBus
App/Test 双端统一使用同构配置结构：

```json
{
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-app or stnext-test",
    "topicPrefix": "stnext",
    "username": null,
    "password": null
  }
}
```

### 字段约定
- `provider`：消息总线实现类型；当前已落地 `inmemory`，后续预留 `mqtt`
- `host`：总线服务地址；当前 `inmemory` 模式可忽略，MQTT 模式将使用
- `port`：总线服务端口；当前 `inmemory` 模式可忽略，MQTT 模式将使用
- `clientId`：客户端标识；建议 App/Test 分别使用独立值
- `topicPrefix`：主题前缀；默认 `stnext`
- `username` / `password`：认证参数；当前为预留字段

### 当前环境变量入口
- `STNEXT_MESSAGE_BUS` → 覆盖 `messageBus.provider`
- `STNEXT_MESSAGE_BUS_HOST` → 覆盖 `messageBus.host`
- `STNEXT_MESSAGE_BUS_PORT` → 覆盖 `messageBus.port`
- `STNEXT_MESSAGE_BUS_CLIENT_ID` → 覆盖 `messageBus.clientId`
- `STNEXT_MESSAGE_BUS_TOPIC_PREFIX` → 覆盖 `messageBus.topicPrefix`
- `STNEXT_MESSAGE_BUS_USERNAME` → 覆盖 `messageBus.username`
- `STNEXT_MESSAGE_BUS_PASSWORD` → 覆盖 `messageBus.password`

说明：
- 当前细粒度环境变量已接入 App/Test 双端 Program 启动路径
- 若 `STNEXT_MESSAGE_BUS_PORT` 非法，当前会回退到配置文件中的 `messageBus.port`

## App 侧配置约定
### 配置文件：`appsettings.app.json`

```json
{
  "deviceId": "mock-motor-device",
  "productKind": "Motor_Y",
  "samplingMode": "single",
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-app",
    "topicPrefix": "stnext"
  }
}
```

### 字段说明
- `deviceId`：设备实例标识
- `productKind`：产品型号/产品线标识
- `samplingMode`：采样模式；当前支持 `single` / `burst`

### 环境变量
- `STNEXT_APP_DEVICE_ID`
- `STNEXT_APP_PRODUCT_KIND`
- `STNEXT_APP_SAMPLING_MODE`
- `STNEXT_MESSAGE_BUS`

### 命令行参数
- `--config`
- `--device-id`
- `--product-kind`
- `--sampling-mode`

## Test 侧配置约定
### 配置文件：`appsettings.test.json`

```json
{
  "persistenceMode": "memory",
  "sqliteDbPath": "artifacts/test-persistence/standardtest-next.db",
  "messageBus": {
    "provider": "inmemory",
    "host": "127.0.0.1",
    "port": 1883,
    "clientId": "stnext-test",
    "topicPrefix": "stnext"
  }
}
```

### 字段说明
- `persistenceMode`：持久化模式；当前支持 `memory` / `sqlite`
- `sqliteDbPath`：SQLite 数据库路径；仅在 `sqlite` 模式下生效

### 环境变量
- `STNEXT_TEST_PERSISTENCE`
- `STNEXT_TEST_SQLITE_DB`
- `STNEXT_MESSAGE_BUS`

### 命令行参数
- `--config`
- `--persistence`
- `--sqlite-db`

## 部署目录约定（当前建议）
建议 App/Test 分别部署到独立目录，各自携带自己的配置文件：

```text
/deploy/standardtest-next/app/
  StandardTestNext.App(.exe)
  appsettings.app.json

/deploy/standardtest-next/test/
  StandardTestNext.Test(.exe)
  appsettings.test.json
  artifacts/
```

原则：
- **双端各自维护自己的配置文件**，不要交叉读取
- **共享键名保持同构**，但配置文件保持独立
- **provider 切换不改变 Bootstrap 主干签名**，只在配置与工厂层扩展

## 当前限制
- `MessageBusFactory` 目前仅实现 `inmemory`
- `host/port/clientId/topicPrefix/username/password` 在 `inmemory` 模式下主要是结构占位
- 当前已补最小 `RuntimeConfigurationValidator` + `RuntimeConfigurationConsoleReporter`，但仍属于控制台告警级别，尚未形成严格失败策略或独立部署自检命令

## 下一步
- 在 `MessageBusFactory` 中接入 `mqtt` provider
- 将当前配置校验从“控制台提示”推进到更严格的非法值拒绝/启动失败策略
- 增加运行配置自检与错误提示
- 将本文件中的配置约定同步进部署脚本/样例配置模板
