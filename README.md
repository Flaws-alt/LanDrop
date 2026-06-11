# LanDrop - 局域网快传

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**LanDrop** 是一款 Windows 局域网文件与剪贴板传输工具。基于 UDP 广播自动发现局域网内设备，通过 TCP 实现点对点文件与文本的高速传输。界面采用 Catppuccin Mocha 暗色主题，简洁美观。

## 功能

- **设备自动发现** — UDP 广播 + 心跳机制，自动发现局域网内其他 LanDrop 设备
- **文件传输** — 选择文件或拖拽到目标设备即可发送，支持多文件批量发送
- **剪贴板共享** — 一键将剪贴板文本发送到其他设备，接收端自动写入剪贴板
- **离线检测** — 10 秒无心跳自动标记设备离线
- **持久化设置** — 显示名称、接收目录、开机自启等配置自动保存

## 系统要求

- Windows 10 / 11 (x64)
- .NET 8.0 Desktop Runtime（若使用框架依赖版）
- 两台及以上位于同一局域网内的电脑

## 快速开始

### 下载运行

从 [Releases](https://github.com/Flaws-alt/LanDrop/releases) 页面下载最新的 `LanDrop.exe`，双击即可运行。

### 开发构建

```bash
# 克隆仓库
git clone https://github.com/Flaws-alt/LanDrop.git
cd LanDrop

# 构建
dotnet build LanDrop.sln

# 运行测试
dotnet test LanDrop.sln

# 发布为单文件可执行程序
dotnet publish LanDrop\LanDrop.csproj -c Release
```

## 使用方式

1. 在局域网内的所有电脑上启动 **LanDrop**
2. 主界面会自动显示局域网内所有在线设备
3. 选中目标设备，点击 **发文件** 发送文件，或点击 **发剪贴板** 发送剪贴板文本
4. 也可以直接将文件拖拽到在线设备上完成发送
5. 接收的文件默认保存在 `桌面\LanDrop\` 目录下

## 项目结构

```
LanDrop/
├── LanDrop/                # WPF 主程序
│   ├── Models/             # 数据模型 (DiscoveryMessage, TransferMetadata, AppSettings)
│   ├── Services/           # 核心服务 (DiscoveryService, TransferService, ClipboardService)
│   ├── ViewModels/         # MVVM ViewModel (MainViewModel, DeviceItemViewModel)
│   ├── Converters/         # 值转换器
│   └── App.xaml / MainWindow.xaml
├── LanDrop.Tests/          # xUnit 单元测试
└── LanDrop.sln
```

## 技术栈

| 类别 | 技术 |
|------|------|
| 运行时 | .NET 8.0 (Windows) |
| UI 框架 | WPF |
| MVVM | CommunityToolkit.Mvvm |
| 序列化 | System.Text.Json |
| 测试 | xUnit |
| 主题 | Catppuccin Mocha |

## 许可证

本项目采用 [MIT License](LICENSE) 开源许可证。
