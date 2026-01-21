# Manual Test Plan: Assignment Submission

## Prerequisites
1. Build the solution: `dotnet build`
2. Run the application executable twice (once as Teacher, once as Student) or use two different machines.

## Test Steps

### 1. Start Server (Teacher)
- Launch the application.
- Select "Giáo viên".
- Start a Session (Lớp học).
- Note the IP address and Connection Code.

### 2. Connect Client (Student)
- Launch the application instance.
- Select "Học sinh".
- Enter Student Name (e.g., "Nguyen Van A").
- Connect to the Teacher using IP.

### 3. Student Submission
- In the Student Window sidebar, click the **"Nộp Bài Tập"** button.
- A dialog should appear.
- Click "Thêm file" and select a test file (e.g., a text file or small image < 5MB).
- Enter a note (e.g., "Em nộp bài tập 1").
- Click "Nộp bài".
- **Expected Result**:
  - Loading toast "Đang gửi...".
  - Success message box "Đã nộp bài thành công!".

### 4. Verify Server Reception
- On the Teacher Window, look at the Debug Log (or Status Bar messages).
- **Expected Result**: Server logs receiving `AssignmentSubmit` message. Toast notification "Học sinh Nguyen Van A đã nộp bài." appears.

### 5. Teacher Management
- In the Teacher Window sidebar (Right side), scroll to "Quản lý" section.
- Click **"Bài tập đã nộp"**.
- A new window "Danh Sách Bài Tập" should open.
- **Expected Result**:
  - The list contains 1 row with Student "Nguyen Van A".
  - Files column shows the submitted filename.
  - Note column shows "Em nộp bài tập 1".

### 6. Verify File Storage
- In the "Danh Sách Bài Tập" window, click the **"Folder"** button in the "Thao tác" column.
- **Expected Result**:
  - Windows Explorer opens.
  - Path should be roughly: `bin\Debug\net10.0-windows\Author data\Assignments\{SessionID}\{StudentName}_{ID}\`.
  - The submitted file should exist and be readable.

## Troubleshooting
- If "Nộp bài" fails immediately: Check Network Connection.
- If "Nộp bài" hangs: Check Server Firewall or File Size (keep < 50MB is vital, < 5MB recommended).
- If List is empty: Check `Assignments` table in SQLite DB or ensure Session ID matches.
