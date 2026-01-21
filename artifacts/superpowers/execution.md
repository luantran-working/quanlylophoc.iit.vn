
- **Step 1: Define Models & Messages**
  - Files changed: `Models/AssignmentModels.cs`, `Models/NetworkModels.cs`
  - What changed:
    - Created `AssignmentModels.cs` with `AssignmentSubmission` and `SubmittedFile` classes.
    - Updated `NetworkModels.cs` to include `AssignmentSubmit`, `AssignmentSubmitAck`, `AssignmentList` message types.
  - Verification: `dotnet build` passed.

- **Step 2: Update Database Schema**
  - Files changed: `Services/DatabaseService.cs`
  - What changed:
    - Added SQL for creating `Assignments` and `AssignmentFiles` tables in `InitializeDatabase`.
    - Added `SaveAssignment` and `GetAssignments` methods to handle data persistence.
  - Verification: `dotnet build` passed.

- **Step 3: Implement Assignment Service**
  - Files changed: `Services/AssignmentService.cs`
  - What changed:
    - Implemented `AssignmentService` to handle file saving logic and database persistence.
    - Added helper methods for directory management and file sanitization.
  - Verification: `dotnet build` passed.

- **Step 4: Integrate Network Layer**
  - Files changed: `Services/SessionManager.cs`
  - What changed:
    - Updated `OnMessageReceived` to handle `AssignmentSubmit` message type.
    - Implemented `HandleAssignmentSubmit` to deserialize payload, call `AssignmentService.ProcessSubmissionAsync`, notify UI, and send ACK.
  - Verification: `dotnet build` passed.

- **Step 5: Implement Student UI (Submission)**
  - Files changed: `Services/NetworkClientService.cs`, `Views/SubmitAssignmentDialog.xaml`, `Views/SubmitAssignmentDialog.xaml.cs`, `Views/StudentWindow.xaml`, `Views/StudentWindow.xaml.cs`
  - What changed:
    - Added `SubmitAssignmentAsync` to `NetworkClientService`.
    - Created `SubmitAssignmentDialog` for file selection.
    - Added "Nộp Bài Tập" button to `StudentWindow`.
    - Implemented logic to send submission with file data to server.
  - Verification: `dotnet build` passed.

- **Step 6: Implement Teacher UI (Management)**
  - Files changed: `Views/AssignmentListWindow.xaml`, `Views/AssignmentListWindow.xaml.cs`, `Views/MainTeacherWindow.xaml`, `Views/MainTeacherWindow.xaml.cs`
  - What changed:
    - Created `AssignmentListWindow` to view submissions.
    - Added "Bài tập" button to `MainTeacherWindow` sidebar.
  - Verification: `dotnet build` passed.

- **Step 7: End-to-End Test Tweaks**
  - Files changed: `Services/NetworkServerService.cs`, `Views/SubmitAssignmentDialog.xaml.cs`
  - What changed:
    - Increased Server Network Buffer to 50MB to accommodate file submissions.
    - Updated `SubmitAssignmentDialog` to warn if file size > 5MB.
  - Verification: `dotnet build` passed.
- **Step 8: Implement Computer Info (Feature 2)**
  - Files changed: `Models/SystemInfoModels.cs`, `Services/SystemInfoService.cs`, `Views/SystemConfigTableWindow.xaml`, `Views/ComputerSpecsWindow.xaml`, `Services/NetworkClientService.cs`, `Services/SessionManager.cs`, `Views/MainTeacherWindow.xaml`
  - What changed:
    - Updated `feature-2-computer-info.md` workflow to focus on comprehensive hardware specs and "Configuration Table".
    - Created models for CPU, RAM, GPU, OS, Motherboard, and Disk info.
    - Implemented hardware collection using WMI (System.Management).
    - Added "Bảng cấu hình" button and individual student config view via context menu.
  - Verification: `dotnet build` passed.

- **Step 9: Implement App Management (Feature 3)**
  - Files changed: `Models/ProcessModels.cs`, `Services/ProcessManagerService.cs`, `Views/ProcessManagerWindow.xaml`, `Views/ProcessManagerWindow.xaml.cs`, `Services/NetworkClientService.cs`, `Services/SessionManager.cs`, `Controls/ScreenThumbnailControl.xaml`, `Controls/ScreenThumbnailControl.xaml.cs`, `Models/NetworkModels.cs`
  - What changed:
    - Defined `ProcessInfo` model and new Network Messages.
    - Implemented `ProcessManagerService` to list/kill processes on client.
    - Added UI for Teacher to view student processes and send Kill command.
    - Integrated "Quản lý ứng dụng" option into student thumbnail context menu.
  - Verification: `dotnet build` passed.
