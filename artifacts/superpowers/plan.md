## Goal
Implement Feature 5: Assignment Submission for Classroom Management Software.
This will allow students to submit assignments (files) to the teacher, and teachers to view and manage these submissions.

## Assumptions
- The existing network infrastructure (TCP) can handle file transfers via `FileStart`, `FileData`, `FileEnd` messages, or we need to implement specific handling for assignments.
- `SessionManager` is the central point for handling network events.
- Database is SQLite and available via `DatabaseService`.

## Plan

1.  **Define Models & Messages**
    -   Files: `Models/AssignmentModels.cs`, `Models/NetworkModels.cs`
    -   Change: Create `AssignmentModels.cs` with `AssignmentSubmission`, `SubmittedFile`. Update `NetworkModels.cs` with `MessageType` for assignments (`0x90`-`0x92`).
    -   Verify: `dotnet build`

2.  **Update Database Schema**
    -   Files: `Services/DatabaseService.cs`
    -   Change: Add `Assignments` and `AssignmentFiles` tables in `InitializeDatabase`. Add methods to save and retrieve assignments.
    -   Verify: `dotnet test` (if DB tests exist) or check `database.db` creation.

3.  **Implement Assignment Service (logic)**
    -   Files: `Services/AssignmentService.cs`
    -   Change: Create service to handle submission logic: receiving files, saving to `%LOCALAPPDATA%\IIT\ClassroomManagement\Assignments\`, saving to DB, notifying teacher.
    -   Verify: Unit tests for service logic.

4.  **Integrate Network Layer**
    -   Files: `Services/NetworkServerService.cs`, `Services/SessionManager.cs`
    -   Change: Handle `AssignmentSubmit` message in `SessionManager`. Delegate to `AssignmentService`.
    -   Verify: Mock network message and check if service is called.

5.  **Implement Student UI (Submission)**
    -   Files: `Views/StudentWindow.xaml`, `Views/StudentWindow.xaml.cs`, `Views/SubmitAssignmentDialog.xaml`, `Views/SubmitAssignmentDialog.xaml.cs`
    -   Change: Add "Nộp bài" button to StudentWindow. Create Dialog for file selection and submission. Wire up to `AssignmentService` (client side logic to send files).
    -   Verify: Run Student app, open dialog, select file.

6.  **Implement Teacher UI (Management)**
    -   Files: `Views/AssignmentListWindow.xaml`, `Views/AssignmentListWindow.xaml.cs`, `Views/MainTeacherWindow.xaml`
    -   Change: Add "Bài tập" menu/button to TeacherWindow. Create Window to list submissions from DB. Add "Download/Open" functionality.
    -   Verify: Run Teacher app, view list (empty/mocked), check interaction.

7.  **End-to-End Test**
    -   Files: None
    -   Change: Manual test.
    -   Verify: Student submits file -> Teacher gets notification -> Teacher sees file in list -> Teacher opens file.

## Risks & mitigations
-   **Risk:** Large file uploads blocking the UI or network thread.
    -   **Mitigation:** Use async/await and potentially separate thread/task for file transfer. Ensure chunking is used (already in network protocol design).
-   **Risk:** Concurrent submissions from many students.
    -   **Mitigation:** Ensure unique filenames/folders per student. Database locking handled by SQLite, but file I/O should be robust.
-   **Risk:** File system permission issues.
    -   **Mitigation:** Use `LocalApplicationData` which is writable. Handle exceptions gracefully.

## Rollback plan
-   Revert changes to `Models/NetworkModels.cs`, `Services/DatabaseService.cs`.
-   Delete created files (`AssignmentModels.cs`, `AssignmentService.cs`, Views).
