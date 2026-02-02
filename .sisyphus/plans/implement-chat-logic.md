# Implement Chat Logic

## TL;DR
> **Objective**: Make chat functional with real private/public routing and database persistence.
> **Deliverables**: Working ChatService and ChatView.
> **Estimated Effort**: Medium

---

## Context
The UI for Chat is ready, but the logic currently broadcasts everything to everyone. We need to implement proper routing for private messages and ensure clients can send targeted messages.

## Work Objectives

### Core Objective
Enable private and public messaging with correct network routing.

### Concrete Deliverables
- [ ] Update `Services/ChatService.cs`
- [ ] Update `Views/ChatView.xaml.cs`

---

## TODOs

- [ ] 1. Update `Services/ChatService.cs` - BroadcastMessageAsync
  **What to do**:
  - Modify `BroadcastMessageAsync` to check `msg.ReceiverId`.
  - If `IsGroup` is true, continue using `BroadcastToAllAsync`.
  - If `IsGroup` is false (Private):
    - Find the `Student` in `SessionManager.OnlineStudents` matching `ReceiverId`.
    - Use `_server.SendToClientAsync(student.MachineId, netMsg)` to send only to that student.

- [ ] 2. Update `Services/ChatService.cs` - SendTextMessageAsync
  **What to do**:
  - Change signature to `public async Task SendTextMessageAsync(string content, int? receiverId = null)`.
  - In the method, set `ReceiverId = receiverId` and `IsGroup = receiverId == null`.

- [ ] 3. Update `Views/ChatView.xaml.cs` - SendBtn_Click
  **What to do**:
  - In `SendBtn_Click` for Student mode (else block):
  - Determine `receiverId` from `_selectedConversation`.
  - Call `await ChatService.Instance.SendTextMessageAsync(content, receiverId);`.

---

## Verification
- Build project successfully.
- Verify Teacher sends private message -> Only target Student receives.
- Verify Student sends private message -> Teacher receives.
