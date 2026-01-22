---
description: Workflow phát triển tính năng Thi trực tuyến (Feature 12) - Giáo viên tạo kỳ thi với mật khẩu, học sinh tham gia thi, tự động chấm và báo cáo.
---

# Phát triển Tính năng Thi trực tuyến

## Tổng quan

- Giáo viên tạo kỳ thi với thời gian bắt đầu/kết thúc
- Đặt mật khẩu bảo vệ bài thi
- Học sinh nhập mật khẩu để vào thi
- Giám sát học sinh trong khi thi
- Tự động chấm điểm và báo cáo kết quả

## Khác biệt với Bài kiểm tra (Feature 11)

| Bài kiểm tra   | Thi trực tuyến                |
| -------------- | ----------------------------- |
| Gửi trực tiếp  | Có mật khẩu truy cập          |
| Không lên lịch | Có thời gian bắt đầu/kết thúc |
| Đơn giản       | Giám sát anti-cheat           |
| Nhanh gọn      | Có báo cáo chi tiết           |

## Các bước thực hiện

### 1. Cập nhật Models

**Files:**

- `Models/ExamModels.cs` (Tạo mới)

**ExamModels.cs:**

```csharp
public enum ExamStatus { Draft, Scheduled, InProgress, Completed, Cancelled }

public class Exam
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Password { get; set; } = ""; // Mật khẩu vào thi
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public List<Question> Questions { get; set; } = new();
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public bool ShuffleQuestions { get; set; } = true;
    public bool AllowLateEntry { get; set; } = false;
    public int LateEntryMinutes { get; set; } = 10;
}

public class ExamParticipant
{
    public string ExamId { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public DateTime? JoinTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public bool IsSubmitted { get; set; }
    public double Score { get; set; }
}
```

### 2. Implement Services

**Files:**

- `Services/ExamService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Thêm bảng Exams, ExamParticipants)

**ExamService.cs:**

```csharp
public class ExamService
{
    public async Task<Exam> CreateExamAsync(Exam exam);
    public async Task<bool> ScheduleExamAsync(string examId, DateTime start, DateTime end);
    public async Task<bool> ValidatePasswordAsync(string examId, string password);
    public async Task<Exam> JoinExamAsync(string examId, string studentId, string password);
    public async Task<bool> SubmitExamAsync(string examId, string studentId, Dictionary<string, int> answers);
    public async Task<List<ExamParticipant>> GetParticipantsAsync(string examId);
}
```

### 3. Implement Views

**Files:**

- `Views/ExamCreationWindow.xaml` & `.cs` (Tạo kỳ thi)
- `Views/ExamDashboardWindow.xaml` & `.cs` (Giám sát kỳ thi)
- `Views/JoinExamDialog.xaml` & `.cs` (Học sinh nhập mật khẩu)
- `Views/ExamWindow.xaml` & `.cs` (Học sinh làm bài thi)
- `Views/ExamResultsWindow.xaml` & `.cs` (Kết quả kỳ thi)

**UI - ExamCreationWindow:**

```
┌──────────────────────────────────────────────────────┐
│  🎓 Tạo Kỳ thi trực tuyến                     [✕]  │
├──────────────────────────────────────────────────────┤
│  Thông tin kỳ thi:                                  │
│  Tên kỳ thi: [________________________]             │
│  Mật khẩu:   [________]  [👁 Hiện]                  │
│                                                      │
│  Thời gian:                                         │
│  Bắt đầu: [22/01/2026] [14:00]                     │
│  Kết thúc: [22/01/2026] [15:00]                    │
│  Thời gian làm bài: [45] phút                      │
│                                                      │
│  ☑ Cho phép vào muộn (tối đa 10 phút)             │
│  ☑ Xáo trộn câu hỏi và đáp án                      │
│                                                      │
│  CÂU HỎI                           [+ Thêm câu]    │
│  ┌────────────────────────────────────────────┐    │
│  │ (Danh sách câu hỏi như Test)               │    │
│  └────────────────────────────────────────────┘    │
├──────────────────────────────────────────────────────┤
│                    [Hủy] [💾 Lưu] [📅 Lên lịch]   │
└──────────────────────────────────────────────────────┘
```

**UI - JoinExamDialog (Học sinh):**

```
┌──────────────────────────────────────────────────────┐
│  🎓 Tham gia Kỳ thi                                 │
├──────────────────────────────────────────────────────┤
│                                                      │
│  Kỳ thi: Kiểm tra giữa kỳ - Toán 10                │
│  Thời gian: 14:00 - 15:00, 22/01/2026              │
│  Thời gian làm bài: 45 phút                        │
│                                                      │
│  Nhập mật khẩu:                                     │
│  ┌──────────────────────────────────────────┐      │
│  │  [•••••••••••]                           │      │
│  └──────────────────────────────────────────┘      │
│                                                      │
│  ⚠️ Lưu ý:                                          │
│  - Không được thoát khỏi màn hình thi              │
│  - Bài thi sẽ tự động nộp khi hết giờ              │
│                                                      │
├──────────────────────────────────────────────────────┤
│                        [Hủy] [🎓 Vào thi]          │
└──────────────────────────────────────────────────────┘
```

**UI - ExamDashboardWindow (Giám sát):**

```
┌──────────────────────────────────────────────────────┐
│  🎓 Giám sát: Kiểm tra giữa kỳ       ⏱️ 32:15     │
├──────────────────────────────────────────────────────┤
│  Thống kê:                                          │
│  📊 Đã vào: 28/30 │ Đang làm: 25 │ Đã nộp: 3      │
│                                                      │
│  Danh sách thí sinh:                                │
│  ┌────────────────────────────────────────────┐    │
│  │ ✅ Nguyễn Văn An   | Đang làm | 15/20 câu  │    │
│  │ ✅ Trần Thị Bình   | Đang làm | 12/20 câu  │    │
│  │ ✅ Lê Hoàng Cường  | Đã nộp   | 9.0 điểm   │    │
│  │ ⏳ Phạm Thu Dung   | Chưa vào |            │    │
│  └────────────────────────────────────────────┘    │
│                                                      │
├──────────────────────────────────────────────────────┤
│  [🔔 Nhắc nhở] [⏹️ Kết thúc sớm] [📊 Xem kết quả] │
└──────────────────────────────────────────────────────┘
```

### 4. Database Schema

```sql
CREATE TABLE Exams (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    subject TEXT,
    password TEXT NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NOT NULL,
    duration_minutes INTEGER,
    status TEXT DEFAULT 'Draft',
    questions TEXT, -- JSON
    settings TEXT -- JSON
);

CREATE TABLE ExamParticipants (
    id TEXT PRIMARY KEY,
    exam_id TEXT NOT NULL,
    student_id TEXT NOT NULL,
    student_name TEXT,
    join_time DATETIME,
    submit_time DATETIME,
    answers TEXT, -- JSON
    score REAL,
    FOREIGN KEY (exam_id) REFERENCES Exams(id)
);
```

### 5. Luồng xử lý

```
1. Giáo viên tạo kỳ thi với mật khẩu
2. Lên lịch thời gian bắt đầu/kết thúc
3. Đến giờ, thông báo cho học sinh
4. Học sinh nhập mật khẩu để vào thi
5. Học sinh làm bài (fullscreen, anti-cheat)
6. Giáo viên giám sát realtime
7. Học sinh nộp hoặc auto-submit khi hết giờ
8. Chấm điểm tự động
9. Hiển thị kết quả và báo cáo
```

## Verification

- [ ] Tạo kỳ thi với mật khẩu
- [ ] Lên lịch thời gian thi
- [ ] Học sinh nhập mật khẩu vào thi
- [ ] Làm bài với fullscreen mode
- [ ] Giáo viên giám sát realtime
- [ ] Auto-submit khi hết giờ
- [ ] Xem kết quả và báo cáo
