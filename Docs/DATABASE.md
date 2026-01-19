# Cơ sở dữ liệu

## Tổng quan

Phần mềm sử dụng **SQLite** làm cơ sở dữ liệu cục bộ, được lưu trữ tại **máy Giáo viên (Server)**. Điều này đảm bảo:

- ✅ Không cần cài đặt database server riêng
- ✅ Dữ liệu được lưu trữ tập trung
- ✅ Dễ dàng backup và khôi phục
- ✅ Hoạt động offline hoàn toàn trong mạng LAN

## Vị trí lưu trữ

```
📁 C:\Users\{Username}\AppData\Local\IIT\ClassroomManagement\
├── 📄 classroom.db          # Database chính
├── 📄 classroom.db-journal  # Transaction journal
├── 📁 Backups/              # Bản sao lưu tự động
│   ├── classroom_2026-01-15.db
│   └── classroom_2026-01-16.db
└── 📁 Files/                # Tập tin được chia sẻ
    ├── 📁 Uploads/          # File từ học sinh
    └── 📁 Shared/           # File chia sẻ cho học sinh
```

## Sơ đồ Database

### Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐
│     Users       │       │    Sessions     │
├─────────────────┤       ├─────────────────┤
│ id (PK)         │───────│ id (PK)         │
│ username        │       │ user_id (FK)    │
│ password_hash   │       │ start_time      │
│ display_name    │       │ end_time        │
│ role            │       │ class_name      │
│ created_at      │       └─────────────────┘
└─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐       ┌─────────────────┐
│    Students     │       │     Tests       │
├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │
│ machine_id      │───────│ session_id (FK) │
│ display_name    │       │ title           │
│ is_online       │       │ subject         │
│ last_seen       │       │ duration        │
│ session_id (FK) │       │ created_at      │
└─────────────────┘       └─────────────────┘
         │                         │
         │                         │ 1:N
         │                         ▼
         │                ┌─────────────────┐
         │                │   Questions     │
         │                ├─────────────────┤
         │                │ id (PK)         │
         │                │ test_id (FK)    │
         │                │ content         │
         │                │ type            │
         │                │ options (JSON)  │
         │                │ correct_answer  │
         │                └─────────────────┘
         │                         │
         │                         │
         └─────────────────────────┤
                                   │
                          ┌────────┴────────┐
                          │   TestResults   │
                          ├─────────────────┤
                          │ id (PK)         │
                          │ student_id (FK) │
                          │ test_id (FK)    │
                          │ answers (JSON)  │
                          │ score           │
                          │ submitted_at    │
                          └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│  ChatMessages   │       │   FileRecords   │
├─────────────────┤       ├─────────────────┤
│ id (PK)         │       │ id (PK)         │
│ session_id (FK) │       │ session_id (FK) │
│ sender_id       │       │ student_id (FK) │
│ receiver_id     │       │ filename        │
│ content         │       │ filepath        │
│ is_group        │       │ size            │
│ created_at      │       │ direction       │
└─────────────────┘       │ created_at      │
                          └─────────────────┘
```

## Chi tiết các bảng

### 1. Users (Người dùng)

Lưu thông tin tài khoản giáo viên.

```sql
CREATE TABLE Users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    display_name TEXT NOT NULL,
    role TEXT DEFAULT 'teacher',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Tài khoản mặc định
INSERT INTO Users (username, password_hash, display_name, role)
VALUES ('admin', 'SHA256_HASH_OF_123456', 'Quản trị viên', 'admin');
```

### 2. Sessions (Phiên học)

Lưu thông tin các phiên học.

```sql
CREATE TABLE Sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    class_name TEXT NOT NULL,
    subject TEXT,
    start_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME,
    status TEXT DEFAULT 'active',
    FOREIGN KEY (user_id) REFERENCES Users(id)
);
```

### 3. Students (Học sinh)

Lưu thông tin học sinh kết nối.

```sql
CREATE TABLE Students (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    machine_id TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    computer_name TEXT,
    ip_address TEXT,
    is_online INTEGER DEFAULT 0,
    is_locked INTEGER DEFAULT 0,
    mic_enabled INTEGER DEFAULT 1,
    camera_enabled INTEGER DEFAULT 1,
    last_seen DATETIME,
    session_id INTEGER,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);
```

### 4. Tests (Bài kiểm tra)

```sql
CREATE TABLE Tests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER,
    title TEXT NOT NULL,
    subject TEXT,
    duration INTEGER DEFAULT 900, -- Seconds (15 minutes)
    total_questions INTEGER DEFAULT 0,
    shuffle_questions INTEGER DEFAULT 0,
    shuffle_answers INTEGER DEFAULT 0,
    show_result INTEGER DEFAULT 1,
    status TEXT DEFAULT 'draft',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);
```

### 5. Questions (Câu hỏi)

```sql
CREATE TABLE Questions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    test_id INTEGER NOT NULL,
    order_index INTEGER DEFAULT 0,
    content TEXT NOT NULL,
    type TEXT DEFAULT 'multiple_choice',
    options TEXT, -- JSON array: ["A", "B", "C", "D"]
    correct_answer TEXT,
    points INTEGER DEFAULT 1,
    FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE CASCADE
);
```

### 6. TestResults (Kết quả kiểm tra)

```sql
CREATE TABLE TestResults (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    student_id INTEGER NOT NULL,
    test_id INTEGER NOT NULL,
    answers TEXT, -- JSON object: {"1": "A", "2": "C", ...}
    correct_count INTEGER DEFAULT 0,
    total_count INTEGER DEFAULT 0,
    score REAL DEFAULT 0,
    started_at DATETIME,
    submitted_at DATETIME,
    status TEXT DEFAULT 'in_progress',
    FOREIGN KEY (student_id) REFERENCES Students(id),
    FOREIGN KEY (test_id) REFERENCES Tests(id)
);
```

### 7. ChatMessages (Tin nhắn)

```sql
CREATE TABLE ChatMessages (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER NOT NULL,
    sender_type TEXT NOT NULL, -- 'teacher' or 'student'
    sender_id INTEGER NOT NULL,
    receiver_id INTEGER, -- NULL = group chat
    content TEXT NOT NULL,
    is_group INTEGER DEFAULT 1,
    is_read INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);
```

### 8. FileRecords (Lịch sử file)

```sql
CREATE TABLE FileRecords (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER NOT NULL,
    student_id INTEGER,
    filename TEXT NOT NULL,
    original_name TEXT,
    filepath TEXT NOT NULL,
    size INTEGER DEFAULT 0,
    direction TEXT, -- 'upload' (từ HS) or 'download' (đến HS)
    status TEXT DEFAULT 'completed',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id),
    FOREIGN KEY (student_id) REFERENCES Students(id)
);
```

## Tài khoản mặc định

| Field        | Value         |
| ------------ | ------------- |
| Username     | `admin`       |
| Password     | `123456`      |
| Display Name | Quản trị viên |
| Role         | admin         |

> ⚠️ **Bảo mật**: Mật khẩu được lưu dưới dạng hash SHA-256. Luôn thay đổi mật khẩu mặc định sau lần đăng nhập đầu tiên.

## Sao lưu & Khôi phục

### Sao lưu tự động

- Database được sao lưu tự động mỗi ngày
- Giữ lại 7 bản backup gần nhất
- Vị trí: `%LOCALAPPDATA%\IIT\ClassroomManagement\Backups\`

### Sao lưu thủ công

```powershell
# Sao lưu database
Copy-Item "$env:LOCALAPPDATA\IIT\ClassroomManagement\classroom.db" `
          "D:\Backup\classroom_$(Get-Date -Format 'yyyy-MM-dd').db"
```

### Khôi phục

```powershell
# Khôi phục database
Stop-Process -Name "ClassroomManagement" -Force
Copy-Item "D:\Backup\classroom_2026-01-15.db" `
          "$env:LOCALAPPDATA\IIT\ClassroomManagement\classroom.db"
```

## Dọn dẹp dữ liệu

### Xóa dữ liệu cũ

```sql
-- Xóa phiên học cũ hơn 30 ngày
DELETE FROM Sessions WHERE end_time < datetime('now', '-30 days');

-- Xóa tin nhắn cũ hơn 7 ngày
DELETE FROM ChatMessages WHERE created_at < datetime('now', '-7 days');

-- Xóa file records cũ
DELETE FROM FileRecords WHERE created_at < datetime('now', '-30 days');
```

### Reset toàn bộ

```sql
-- CẢNH BÁO: Xóa toàn bộ dữ liệu!
DELETE FROM TestResults;
DELETE FROM Questions;
DELETE FROM Tests;
DELETE FROM ChatMessages;
DELETE FROM FileRecords;
DELETE FROM Students;
DELETE FROM Sessions;
-- Giữ lại Users để không mất tài khoản admin
```

---

_Cập nhật: Tháng 01/2026_
