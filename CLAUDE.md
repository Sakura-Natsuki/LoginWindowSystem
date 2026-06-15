# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 构建 / 运行

- 构建：`msbuild LoginWindowSystem.csproj`（或在 Visual Studio 中打开 `LoginWindowSystem.slnx`）
- 项目目标框架为 .NET Framework 4.8（WPF WinExe）
- **依赖**：需要本地 SQL Server 实例（连接字符串 `Data Source=.;Initial Catalog=CyberpunkLoginDB;Integrated Security=True;`）。应用启动时自动建库建表，无需手动准备。

## 架构

MVVM 模式，基于命名空间的扁平分层结构（无独立程序集）：

```
Views (*.xaml + 后置代码)  →  ViewModels  →  Services / Models
         ↑                           ↑
        Helpers (RelayCommand, Converters)
```

- **`Models/UserModel.cs`** — 纯数据对象：Id, Username, PasswordHash (SHA256), Nickname, CreatedAt，映射到数据库 `Users` 表。
- **`ViewModels/BaseViewModel.cs`** — INotifyPropertyChanged 基类，提供 `SetProperty<T>(ref T, T)` 用于属性变更通知。
- **`ViewModels/LoginViewModel.cs`** — 绑定到 LoginWindow。暴露 `Username`、`Password`、`ErrorMessage`、`IsLogging`、`LoginCommand` 和 `OpenRegisterCommand`。登录成功后打开 MainWindow 并关闭自身；点击注册入口时打开 RegisterWindow 并关闭自身。
- **`ViewModels/RegisterViewModel.cs`** — 绑定到 RegisterWindow。暴露 `UserName`、`Password`、`ConfirmPassword`、`NickName`、`ErrorMessage`、`IsRegistering`、`RegisterCommand` 和 `BackToLoginCommand`。注册前校验两次密码一致且长度 ≥6，成功后打开 MainWindow 并关闭 Login/Register 窗口。
- **`ViewModels/MainViewModel.cs`** — 绑定到 MainWindow。通过构造函数接收用户昵称。暴露 `RefreshCommand`（刷新系统时间）和 `DisconnectCommand`（返回登录窗口）。
- **`Services/DatabaseService.cs`** — 所有 SQL Server 数据库访问。`InitDatabase()` 创建 `Users` 表并写入初始数据（admin/123456）。`RegisterUser()` 先查重再插入。`ValidateLogin()` 使用参数化查询 + SHA256 哈希比对。
- **`Services/LogService.cs`** — 线程安全的懒加载单例（双重检查锁定），日志写入 `BaseDirectory\Logs\yyyy-MM-dd.log`。提供 `Info()`、`Warn()`、`Error()` 三级日志。**注意**：属性名为 `Instacne`（有意保留的拼写，不要"修复"为 `Instance`）。
- **`Helpers/RelayCommand.cs`** — 标准 `ICommand` 实现，`CanExecuteChanged` 转发到 `CommandManager.RequerySuggested`。
- **`Helpers/Converters.cs`** — `StringToVisibilityConverter`（空字符串→Collapsed）和 `InverseBoolConverter`。

## 窗口导航流程

```
LoginWindow ──[登录成功]──→ MainWindow ──[断开连接]──→ LoginWindow
    │                            ↑
    └──[注册新账号]──→ RegisterWindow ──[注册成功]──┘
                          │
                          └──[返回登录]──→ LoginWindow
```

窗口切换采用统一模式：先 `Show()` 新窗口，再遍历 `Application.Current.Windows` 关闭旧窗口。

## 关键约定

- **PasswordBox 绑定**：WPF 的 `PasswordBox` 出于安全原因不支持数据绑定。两个使用 PasswordBox 的窗口（LoginWindow、RegisterWindow）均在后置代码中通过 `PasswordChanged` 事件手动将值同步到 ViewModel 的 `Password` 属性。
- **窗口拖拽**：三个窗口均使用 `WindowStyle="None"` 自定义边框，通过 `MouseLeftButtonDown` 事件调用 `DragMove()` 实现拖拽。MainWindow 使用标题栏区域（`TitleBar_Drag`）而非整个窗口作为拖拽手柄。
- **应用启动**（`App.xaml.cs`）：启动时调用 `DatabaseService.InitDatabase()`。如果数据库不可用，弹出 MessageBox 警告——应用程序仍然会打开。
- **自定义窗口控件**：MainWindow 的最小化和关闭按钮通过后置代码中的 Click 事件实现（而非 Command 绑定）。
- **日志**：`LogService` 是懒加载单例——只有在首次访问 `LogService.Instacne` 时才会创建 `Logs` 目录和日志文件。如果尚未触发任何写日志的代码路径，日志文件不会存在。当前日志写入点：登录窗口点击注册入口、注册成功/失败/异常。
