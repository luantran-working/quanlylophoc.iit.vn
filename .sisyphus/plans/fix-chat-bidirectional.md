# Fix Chat Bidirectional Communication

## TL;DR
> **Objective**: Make chat work bidirectionally between Teacher and Student
> **Deliverables**: Working ChatService event flow on both sides
> **Estimated Effort**: Quick (3 small file edits)

---

## Context
Currently, the chat UI exists and messages are sent, but:
- Students don't see messages in their ChatView because StudentWindow doesn't trigger ChatService events
- IsTeacherMode detection in ChatView is unreliable
- Need to ensure proper message flow

## Work Objectives

### Core Objective
Enable bidirectional chat with proper event triggering

### Concrete Deliverables
- [x] Update `Services/ChatService.cs` - Add IsTeacherMode property
- [ ] Update `Views/StudentWindow.xaml.cs` - Trigger ChatService events
- [ ] Update `Views/ChatView.xaml.cs` - Use new IsTeacherMode property
- [ ] Build and verify

---

## Implementation Steps

### Step 1: Add IsTeacherMode Property to ChatService

**File**: `Services/ChatService.cs`
**Location**: After line 21 (after the events), before the constructor

**Add this code**:
```csharp
public bool IsTeacherMode => _server != null;
```

**Full context** (lines 20-24):
```csharp
        public event EventHandler<ChatMessage>? MessageReceived;
        public event EventHandler<ChatGroup>? GroupCreated;

        public bool IsTeacherMode => _server != null;  // ADD THIS LINE

        public ChatService()
```

---

### Step 2: Fix StudentWindow OnMessageReceived

**File**: `Views/StudentWindow.xaml.cs`
**Location**: Method `OnMessageReceived()` at line 301-321

**Replace the entire method** with:
```csharp
        private void OnMessageReceived(object? sender, NetworkMessage message)
        {
            Dispatcher.Invoke(() =>
            {
                switch (message.Type)
                {
                    case MessageType.ChatMessage:
                    case MessageType.ChatPrivate:
                        // Trigger ChatService event so ChatView receives the message
                        if (message.Payload != null)
                        {
                            try
                            {
                                var chatMsg = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(message.Payload);
                                if (chatMsg != null)
                                {
                                    ChatService.Instance.OnMessageReceived(chatMsg);
                                }
                            }
                            catch { }
                        }
                        ShowChatNotification(message.SenderName, message.Payload ?? "");
                        break;

                    case MessageType.TestStart:
                        ShowTestNotification(message.Payload ?? "");
                        break;

                    case MessageType.Notification:
                        ShowNotification(message.Payload ?? "");
                        break;
                }
            });
        }
```

**What changed**: Added the deserialization and `ChatService.Instance.OnMessageReceived(chatMsg)` call before showing notification.

---

### Step 3: Fix ChatView IsTeacherMode Detection

**File**: `Views/ChatView.xaml.cs`
**Location**: Line 67 in the constructor

**Change FROM**:
```csharp
IsTeacherMode = ChatService.Instance.IsMyMessage(new ChatMessage { SenderType = "teacher" });
```

**Change TO**:
```csharp
IsTeacherMode = ChatService.Instance.IsTeacherMode;
```

**Full context** (lines 66-72):
```csharp
            // Check Mode based on ChatService state
            IsTeacherMode = ChatService.Instance.IsTeacherMode;  // CHANGED THIS LINE

            if (IsTeacherMode)
            {
                 LoadGroups();
```

---

### Step 4: Build and Verify

After making all changes:

```bash
dotnet build
```

Expected output: Build succeeded with 0 errors

---

## Verification Checklist

After build succeeds:
- [ ] No compilation errors
- [ ] Ready for manual testing:
  - [ ] Start Teacher window
  - [ ] Start Student window  
  - [ ] Teacher sends message → Student sees it in ChatView
  - [ ] Student opens Chat (via "Chat với Giáo viên" button)
  - [ ] Student sends message → Teacher sees it in ChatView
  - [ ] Messages appear in correct sender/receiver bubbles
  - [ ] Public chat works (all students see messages)

---

## Message Flow Diagram

```
TEACHER SENDS MESSAGE:
Teacher ChatView → ChatService.BroadcastMessageAsync 
  → SessionManager._networkServer.BroadcastToAllAsync
  → All Students receive NetworkMessage
  → StudentWindow.OnMessageReceived
  → ChatService.Instance.OnMessageReceived(chatMsg)  ← NEW CODE
  → ChatView.MessageReceived event handler
  → Message displayed in ChatView

STUDENT SENDS MESSAGE:
Student ChatView → ChatService.SendTextMessageAsync
  → NetworkClient.SendMessageAsync
  → Server receives → SessionManager.HandleChatMessage
  → ChatService.BroadcastMessageAsync
  → Back to Teacher & all Students (same flow as above)
```

---

## Files Modified

1. `Services/ChatService.cs` - Added `IsTeacherMode` property
2. `Views/StudentWindow.xaml.cs` - Fixed `OnMessageReceived` to trigger ChatService event
3. `Views/ChatView.xaml.cs` - Use new `IsTeacherMode` property

Total: 3 files, ~10 lines of code changed

---

## Notes

- The existing `ChatService.OnMessageReceived(ChatMessage msg)` method already exists (line 193-196), we're just calling it now from StudentWindow
- SessionManager already handles chat messages correctly (HandleChatMessage at line 417-432)
- No changes needed to network layer - it already works
- This is purely fixing the event flow on the student side
