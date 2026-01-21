
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

- **Step 10: Implement File Collection (Feature 4)**
  - Files changed: `Models/FileCollectionModels.cs`, `Services/FileCollectionService.cs`, `Services/NetworkClientService.cs`, `Services/SessionManager.cs`, `Views/FileCollectionWindow.xaml`, `Views/FileCollectionWindow.xaml.cs`, `Views/MainTeacherWindow.xaml`
  - What changed:
    - Defined models for File Collection Request/Response.
    - Implemented client-side recursive file scanning and uploading.
    - Implemented server-side file receiving and storage organization (Session/Student/File).
    - Created Teacher UI to configure path, extensions, and monitor progress.
    - Replaced "Thu bài" button logic to open File Collection Window.
  - Verification: `dotnet build` passed.

- **Step 11: Implement Bulk File Send (Feature 6)**
  - Files changed: `Models/BulkFileModels.cs`, `Models/NetworkModels.cs`, `Services/BulkFileSender.cs`, `Services/FileReceiverService.cs`, `Services/NetworkClientService.cs`, `Views/BulkFileSendWindow.xaml`, `Views/BulkFileSendWindow.xaml.cs`, `Views/FileNotificationPopup.xaml`, `Views/FileNotificationPopup.xaml.cs`, `Views/StudentWindow.xaml.cs`, `Views/MainTeacherWindow.xaml`, `Views/MainTeacherWindow.xaml.cs`
  - What changed:
    - Defined models for Bulk File Transfer protocol.
    - Implemented `BulkFileSender` service on Server to chunk and broadcast files.
    - Implemented `FileReceiverService` on Client to handle requests, show Popup, and reassemble files.
    - Created Teacher UI (`BulkFileSendWindow`) and Student UI (`FileNotificationPopup`).
    - Integrated "Gửi File" feature into Main Teacher Window.
  - Verification: `dotnet build` passed.

- **Step 12: Implement Realtime Polling (Feature 7)**
  - Files changed: Models/PollModels.cs, Models/NetworkModels.cs, Services/PollService.cs, Services/SessionManager.cs, Services/NetworkClientService.cs, Views/CreatePollWindow.xaml, Views/CreatePollWindow.xaml.cs, Views/VotePollWindow.xaml, Views/VotePollWindow.xaml.cs, Views/MainTeacherWindow.xaml, Views/MainTeacherWindow.xaml.cs, Views/StudentWindow.xaml.cs 
