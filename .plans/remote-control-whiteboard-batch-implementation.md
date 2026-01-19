# Implementation Plan: Remote Control + Whiteboard + Batch Operations

## Overview

This document outlines the implementation of three major features:

1. Remote Control functionality
2. Collaborative Whiteboard
3. Batch operations with "Select All" checkbox

---

## Phase 1: Remote Control Feature

### 1.1 New Windows/Views

- **RemoteControlWindow.xaml**: Main remote control interface
  - Toolbar with controls (Back, Refresh, Screenshot, etc.)
  - Screen display area
  - Status bar (FPS, latency, connection status)
  - Virtual keyboard overlay

### 1.2 Services/Models

- **RemoteControlService.cs**: Manages remote sessions
  - Connect/Disconnect
  - Send input events (mouse, keyboard)
  - Receive screen updates
  - Lock/unlock student input

- **RemoteSession.cs**: Model for remote session state
  - Student info
  - Connection status
  - Quality settings
  - Lock state

### 1.3 Network Protocol Extensions

- Add to NetworkCommands:
  - `REMOTE_CONTROL_REQUEST`
  - `REMOTE_CONTROL_ACCEPT`
  - `REMOTE_CONTROL_INPUT`
  - `REMOTE_CONTROL_LOCK`
  - `REMOTE_CONTROL_DISCONNECT`

### 1.4 Student-Side Implementation

- Input interceptor/forwarder
- Screen capture with configurable FPS
- Lock overlay when controlled
- Notification system

---

## Phase 2: Whiteboard Feature

### 2.1 New Windows/Views

- **WhiteboardWindow.xaml**: Collaborative drawing canvas
  - Drawing tools toolbar (Pen, eraser, shapes, text)
  - Color picker
  - Canvas with real-time sync
  - Participant list
  - Save/Load/Clear options

### 2.2 Services/Models

- **WhiteboardService.cs**: Manages whiteboard state
  - Drawing synchronization
  - Participant management
  - Canvas state persistence

- **DrawingStroke.cs**: Model for drawing strokes
  - Type (line, rectangle, circle, text)
  - Color, thickness
  - Points/coordinates
  - Timestamp, author

### 2.3 Network Protocol Extensions

- Add to NetworkCommands:
  - `WHITEBOARD_START`
  - `WHITEBOARD_STROKE`
  - `WHITEBOARD_CLEAR`
  - `WHITEBOARD_SYNC`
  - `WHITEBOARD_STOP`

### 2.4 UI Components

- **DrawingToolbar**: Tool selection
- **ColorPicker**: Color selection
- **WhiteboardCanvas**: Custom InkCanvas with sync

---

## Phase 3: Batch Operations

### 3.1 UI Updates

- **MainTeacherWindow.xaml**: Add "Select All" checkbox in student list header
- Context menu with batch actions:
  - Lock/Unlock selected
  - Send message to selected
  - Send file to selected
  - Enable/Disable camera for selected
  - Enable/Disable mic for selected

### 3.2 Services Updates

- **SessionManager.cs**: Add batch operation methods
  - `LockStudentsAsync(List<string> machineIds, bool locked)`
  - `SendMessageToStudentsAsync(List<string> machineIds, string message)`
  - `ToggleCameraForStudentsAsync(List<string> machineIds, bool enabled)`
  - etc.

### 3.3 Selection State Management

- Track selected students
- Update UI based on selection state
- Keyboard shortcuts (Ctrl+A for select all)

---

## Implementation Order

### Week 1: Remote Control Foundation

1. Create RemoteControlWindow UI
2. Implement RemoteControlService
3. Add network protocol commands
4. Basic screen capture and display

### Week 2: Remote Control Completion

1. Mouse/keyboard input forwarding
2. Lock functionality
3. Toolbar features (screenshot, file send)
4. Performance optimization

### Week 3: Whiteboard

1. Create WhiteboardWindow UI
2. Implement local drawing
3. Add network synchronization
4. Test with multiple participants

### Week 4: Batch Operations

1. Add "Select All" checkbox
2. Implement selection state tracking
3. Add batch action menu items
4. Implement batch service methods

---

## Technical Considerations

### Remote Control

- **Performance**: Use efficient screen encoding (JPEG with configurable quality)
- **Latency**: Target < 50ms for local network
- **Security**: Validate all input events, log remote sessions
- **Compatibility**: Handle different screen resolutions

### Whiteboard

- **Sync**: Debounce stroke updates to reduce network traffic
- **Storage**: Save whiteboard state for session recovery
- **Permissions**: Teacher can clear, students can only draw
- **Export**: Allow saving as image file

### Batch Operations

- **UI Feedback**: Show progress for batch operations
- **Error Handling**: Continue operation even if some students fail
- **Undo**: Consider implementing undo for batch actions

---

## .NET Framework Version Note

**Current**: .NET 10.0-windows
**Request**: Downgrade to .NET Framework 4.8

**Analysis**:

- MaterialDesignThemes 5.1.0 requires .NET 6+
- Microsoft.Data.Sqlite 10.x requires .NET 6+
- System.Drawing.Common 10.x requires .NET 6+

**Recommendation**:
Keep .NET 10 and create self-contained deployment instead:

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

This bundles .NET runtime with the app, eliminating need for separate .NET installation.

Alternative: Downgrade to .NET 8 LTS (still modern, longer support)
