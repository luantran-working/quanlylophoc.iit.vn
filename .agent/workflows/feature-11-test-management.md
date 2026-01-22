---
description: Workflow phát triển tính năng Quản lý Bài kiểm tra (Feature 11) - Tạo bài kiểm tra trắc nghiệm, gửi đến học sinh, tự động chấm điểm.
---

# Phát triển Tính năng Quản lý Bài kiểm tra

## Tổng quan

- Giáo viên tạo bài kiểm tra trắc nghiệm
- Hỗ trợ nhiều loại câu hỏi (Multiple choice, True/False)
- Gửi bài kiểm tra đến học sinh đã chọn
- Tự động chấm điểm và thống kê kết quả
- Lưu trữ ngân hàng câu hỏi

## Các bước thực hiện

### 1. Cập nhật Models

**Files:**

- `Models/TestModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Thêm MessageType)

**TestModels.cs:**

```csharp
public enum QuestionType { MultipleChoice, TrueFalse }

public class Test
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Subject { get; set; } = "";
    public int DurationMinutes { get; set; } = 15;
    public List<Question> Questions { get; set; } = new();
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleAnswers { get; set; } = true;
    public bool ShowResult { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = "";
    public QuestionType Type { get; set; }
    public List<Answer> Answers { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public int Points { get; set; } = 1;
}

public class Answer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = "";
}

public class TestSubmission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TestId { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public Dictionary<string, int> Answers { get; set; } = new(); // QuestionId -> AnswerIndex
    public DateTime StartTime { get; set; }
    public DateTime SubmitTime { get; set; }
    public int CorrectCount { get; set; }
    public double Score { get; set; }
}
```

**NetworkModels.cs - Thêm:**

```csharp
TestStart = 0x80, TestData = 0x81, TestSubmit = 0x82, TestResult = 0x83
```

### 2. Implement Services

**Files:**

- `Services/TestService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Thêm bảng Tests, Questions, TestSubmissions)
- `Services/SessionManager.cs` (Tích hợp TestService)

**TestService.cs:**

```csharp
public class TestService
{
    public async Task<Test> CreateTestAsync(Test test);
    public async Task<bool> SendTestToStudentsAsync(string testId, List<string> studentIds);
    public TestSubmission GradeSubmission(Test test, Dictionary<string, int> studentAnswers);
    public async Task<List<TestSubmission>> GetTestResultsAsync(string testId);
}
```

### 3. Implement Views

**Files:**

- `Views/TestCreationWindow.xaml` & `.cs` (Tạo/chỉnh sửa bài kiểm tra)
- `Views/TestListWindow.xaml` & `.cs` (Danh sách bài kiểm tra)
- `Views/TestResultsWindow.xaml` & `.cs` (Kết quả bài kiểm tra)
- `Views/TakeTestWindow.xaml` & `.cs` (Học sinh làm bài)

**UI - TestCreationWindow:**

```
┌──────────────────────────────────────────────────────┐
│  📝 Tạo bài kiểm tra                          [✕]  │
├──────────────────────────────────────────────────────┤
│  Tên bài: [________________________]                │
│  Môn học: [____________] Thời gian: [15] phút      │
│                                                      │
│  CÂU HỎI                            [+ Thêm câu]   │
│  ┌──────────────────────────────────────────────┐  │
│  │ 1. [Nội dung câu hỏi...]                      │  │
│  │    ○ A. [Đáp án A]  ○ B. [Đáp án B]          │  │
│  │    ● C. [Đáp án C]  ○ D. [Đáp án D]          │  │
│  │    (● = đáp án đúng)                [🗑️]     │  │
│  └──────────────────────────────────────────────┘  │
│                                                      │
│  ☑ Xáo trộn câu hỏi  ☑ Xáo trộn đáp án            │
│  ☑ Hiển thị kết quả sau khi nộp                    │
├──────────────────────────────────────────────────────┤
│                        [Hủy] [💾 Lưu] [📤 Gửi bài] │
└──────────────────────────────────────────────────────┘
```

**UI - TakeTestWindow (Học sinh):**

```
┌──────────────────────────────────────────────────────┐
│  Kiểm tra: Chương 1        ⏱️ Còn lại: 12:45       │
├──────────────────────────────────────────────────────┤
│                                                      │
│  Câu 1/10                                           │
│  ┌──────────────────────────────────────────────┐  │
│  │ Nội dung câu hỏi số 1?                        │  │
│  │                                               │  │
│  │  ○ A. Đáp án A                               │  │
│  │  ● B. Đáp án B                               │  │
│  │  ○ C. Đáp án C                               │  │
│  │  ○ D. Đáp án D                               │  │
│  └──────────────────────────────────────────────┘  │
│                                                      │
│  [◀ Câu trước]   ① ② ③ ④ ⑤ ⑥ ⑦ ⑧ ⑨ ⑩  [Câu sau ▶]│
│                                                      │
├──────────────────────────────────────────────────────┤
│  Đã trả lời: 5/10                    [📤 Nộp bài] │
└──────────────────────────────────────────────────────┘
```

### 4. Database Schema

```sql
CREATE TABLE Tests (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    subject TEXT,
    duration_minutes INTEGER DEFAULT 15,
    shuffle_questions INTEGER DEFAULT 1,
    shuffle_answers INTEGER DEFAULT 1,
    show_result INTEGER DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    questions TEXT -- JSON array of questions
);

CREATE TABLE TestSubmissions (
    id TEXT PRIMARY KEY,
    test_id TEXT NOT NULL,
    student_id TEXT NOT NULL,
    student_name TEXT,
    answers TEXT, -- JSON
    start_time DATETIME,
    submit_time DATETIME,
    correct_count INTEGER,
    score REAL,
    FOREIGN KEY (test_id) REFERENCES Tests(id)
);
```

### 5. Luồng xử lý

```
1. Giáo viên tạo bài kiểm tra
2. Giáo viên gửi đến học sinh
3. Học sinh nhận popup thông báo
4. Học sinh làm bài (có countdown)
5. Học sinh nộp bài hoặc hết giờ auto-submit
6. Server chấm điểm tự động
7. Kết quả hiển thị cho học sinh (nếu bật)
8. Giáo viên xem thống kê kết quả
```

## Verification

- [ ] Tạo bài kiểm tra với nhiều câu hỏi
- [ ] Gửi đến học sinh
- [ ] Học sinh làm bài với timer
- [ ] Nộp bài và xem điểm
- [ ] Giáo viên xem kết quả thống kê
