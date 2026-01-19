# Build and Deployment Guide

## Self-Contained Deployment (Không cần cài .NET Runtime)

### Cấu hình đã được thiết lập

Project đã được cấu hình để build dưới dạng **self-contained deployment**, nghĩa là:

- ✅ Không cần cài đặt .NET Runtime riêng
- ✅ Tất cả dependencies được đóng gói trong 1 file .exe
- ✅ Tự động nén để giảm kích thước
- ✅ Tối ưu hiệu năng với ReadyToRun

### Publish Release Build

#### 1. Build cho Production

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

#### 2. Output

File thực thi sẽ nằm tại:

```
bin\Release\net10.0-windows\win-x64\publish\ClassroomManagement.exe
```

#### 3. Kích thước ước tính

- **Uncompressed**: ~150-200 MB (bao gồm .NET runtime)
- **Compressed single file**: ~80-120 MB

### Tạo Installer (Optional)

#### Sử dụng Inno Setup

1. Download Inno Setup: https://jrsoftware.org/isdl.php
2. Tạo file `installer.iss`:

```innosetup
[Setup]
AppName=Quản lý Lớp học IIT
AppVersion=1.0.0
DefaultDirName={pf}\IIT\ClassroomManagement
DefaultGroupName=IIT Classroom
OutputDir=.\installer
OutputBaseFilename=ClassroomManagement-Setup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "bin\Release\net10.0-windows\win-x64\publish\ClassroomManagement.exe"; DestDir: "{app}"
Source: "bin\Release\net10.0-windows\win-x64\publish\*.dll"; DestDir: "{app}"

[Icons]
Name: "{group}\Quản lý Lớp học"; Filename: "{app}\ClassroomManagement.exe"
Name: "{commondesktop}\Quản lý Lớp học"; Filename: "{app}\ClassroomManagement.exe"
```

3. Build installer:

```powershell
iscc installer.iss
```

### Alternative: Framework-Dependent Deployment

Nếu muốn file nhỏ hơn (chỉ ~10-20 MB) nhưng yêu cầu cài .NET Runtime:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

Kèm theo installer sẽ kiểm tra và download .NET Runtime nếu chưa có.

### Kiểm tra Build

```powershell
# Build debug (để test)
dotnet build

# Build release với optimization
dotnet build -c Release

# Publish self-contained
dotnet publish -c Release

# Test file đã publish
.\bin\Release\net10.0-windows\win-x64\publish\ClassroomManagement.exe
```

### Notes

- **PublishTrimmed**: Tắt để tránh lỗi reflection với WPF/MaterialDesign
- **ReadyToRun**: Bật để tăng tốc startup time
- **SingleFile**: Tất cả trong 1 file .exe, dễ deploy
- **win-x64**: Chỉ chạy trên Windows 64-bit

### System Requirements

**Minimum:**

- Windows 10 (64-bit) hoặc mới hơn
- 4 GB RAM
- 500 MB disk space

**Recommended:**

- Windows 10/11 (64-bit)
- 8 GB RAM
- 1 GB disk space
