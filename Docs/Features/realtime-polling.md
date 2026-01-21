# Bình chọn Thời gian Thực (Polling)

## Tổng quan

Tính năng cho phép giáo viên tạo cuộc bình chọn/khảo sát và học sinh vote theo thời gian thực. Kết quả được cập nhật ngay lập tức.

## Các tính năng

### 1. Tạo Poll (Giáo viên)
- Nhập câu hỏi
- Thêm các lựa chọn (A, B, C, D...)
- Chọn loại: Single choice hoặc Multiple choice
- Tùy chọn hiển thị kết quả cho học sinh

### 2. Vote (Học sinh)
- Nhận popup khi có poll mới
- Chọn đáp án và gửi
- Xem kết quả (nếu được phép)

### 3. Kết quả Realtime (Giáo viên)
- Biểu đồ cập nhật theo thời gian thực
- Số lượng đã vote
- Phần trăm mỗi lựa chọn

## Giao diện

### Tạo Poll (Giáo viên)

```
┌─────────────────────────────────────────────────────┐
│  📊 Tạo Bình chọn Mới                          × │
├─────────────────────────────────────────────────────┤
│  Câu hỏi:                                           │
│  ┌───────────────────────────────────────────────┐ │
│  │ Các em đã hiểu bài hôm nay chưa?             │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  Loại: ◉ Chọn một   ○ Chọn nhiều                   │
│                                                     │
│  LỰA CHỌN:                                         │
│  [A] [Hiểu rõ                    ] [❌]            │
│  [B] [Hiểu một phần              ] [❌]            │
│  [C] [Chưa hiểu                  ] [❌]            │
│  [D] [Cần giải thích thêm        ] [❌]            │
│                        [+ Thêm lựa chọn]           │
│                                                     │
│  ☑ Hiển thị kết quả cho học sinh                   │
│                                                     │
│              [Tạo và Bắt đầu]    [Hủy]             │
└─────────────────────────────────────────────────────┘
```

### Kết quả Realtime (Giáo viên)

```
┌─────────────────────────────────────────────────────┐
│  📊 Kết quả Bình chọn                    🔴 LIVE  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Các em đã hiểu bài hôm nay chưa?                  │
│                                                     │
│  A. Hiểu rõ                                         │
│  [████████████████████░░░░░░░░░░] 55% (11)         │
│                                                     │
│  B. Hiểu một phần                                   │
│  [████████░░░░░░░░░░░░░░░░░░░░░░] 25% (5)          │
│                                                     │
│  C. Chưa hiểu                                       │
│  [████░░░░░░░░░░░░░░░░░░░░░░░░░░] 10% (2)          │
│                                                     │
│  D. Cần giải thích thêm                             │
│  [███░░░░░░░░░░░░░░░░░░░░░░░░░░░] 10% (2)          │
│                                                     │
│  ─────────────────────────────────────────────────  │
│  Đã vote: 20/30 học sinh                           │
│                                                     │
│  [👁 Ẩn/Hiện kết quả]  [⏹ Kết thúc]  [📊 Xuất]    │
└─────────────────────────────────────────────────────┘
```

### Vote (Học sinh)

```
┌─────────────────────────────────────────────────────┐
│  📊 Bình chọn từ Giáo viên                     × │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Các em đã hiểu bài hôm nay chưa?                  │
│                                                     │
│  ○ A. Hiểu rõ                                      │
│  ● B. Hiểu một phần                                │
│  ○ C. Chưa hiểu                                    │
│  ○ D. Cần giải thích thêm                          │
│                                                     │
│                      [✓ Gửi Phiếu bầu]             │
└─────────────────────────────────────────────────────┘
```

## Protocol Messages

| Code | Type       | Mô tả              |
|------|------------|--------------------|
| 0xB0 | PollCreate | Tạo poll mới       |
| 0xB1 | PollStart  | Bắt đầu poll       |
| 0xB2 | PollVote   | Gửi vote           |
| 0xB3 | PollResult | Kết quả realtime   |
| 0xB4 | PollClose  | Kết thúc poll      |

## Payload Format

```json
// PollCreate
{
  "pollId": "uuid",
  "question": "Các em đã hiểu bài chưa?",
  "options": [
    {"id": "opt-1", "text": "Hiểu rõ"},
    {"id": "opt-2", "text": "Hiểu một phần"}
  ],
  "type": "single",
  "showResults": true
}

// PollVote
{
  "pollId": "uuid",
  "studentId": "student-1",
  "selectedOptions": ["opt-1"]
}

// PollResult (broadcast)
{
  "pollId": "uuid",
  "totalVotes": 20,
  "results": {
    "opt-1": 11,
    "opt-2": 5,
    "opt-3": 2,
    "opt-4": 2
  }
}
```

## Database Tables

```sql
CREATE TABLE Polls (
    id TEXT PRIMARY KEY,
    session_id INTEGER,
    question TEXT NOT NULL,
    poll_type TEXT DEFAULT 'single',
    show_results INTEGER DEFAULT 1,
    is_active INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    closed_at DATETIME
);

CREATE TABLE PollOptions (
    id TEXT PRIMARY KEY,
    poll_id TEXT,
    option_text TEXT,
    vote_count INTEGER DEFAULT 0,
    FOREIGN KEY (poll_id) REFERENCES Polls(id)
);

CREATE TABLE PollVotes (
    poll_id TEXT,
    student_id TEXT,
    option_id TEXT,
    voted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (poll_id, student_id, option_id)
);
```

## Realtime Update Flow

```
1. Học sinh vote
       │
       ▼
2. Server nhận vote, cập nhật database
       │
       ▼
3. Server broadcast PollResult đến tất cả
       │
       ▼
4. Giáo viên: Cập nhật biểu đồ
   Học sinh: Cập nhật kết quả (nếu showResults=true)
```

## Giới hạn

| Thông số              | Giá trị    |
|-----------------------|------------|
| Số lựa chọn tối đa    | 10         |
| Độ dài câu hỏi        | 500 ký tự  |
| Độ dài lựa chọn       | 200 ký tự  |
| Số poll đồng thời     | 1          |

---

_Xem: [Workflow](../../.agent/workflows/new-features-development.md)_
