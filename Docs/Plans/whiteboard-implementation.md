### Goal
Implement a fully functional Whiteboard feature embedded in the Main Teacher Window, supporting drawing, shapes, multi-page, and saving/loading.

### Assumptions
- The `WhiteboardWindow` code is a good starting point but needs refactoring into a UserControl.
- `InkCanvas` will be used for drawing strokes.
- Shapes will be drawn as Children of `InkCanvas` or custom strokes.
- `WhiteboardService` manages the state.

### Plan
1. Create WhiteboardView UserControl
   - Files: `Views/WhiteboardView.xaml`, `Views/WhiteboardView.xaml.cs`
   - Change: extract content from `WhiteboardWindow.xaml` into a `UserControl`. Remove Window-specific code (minimize/maximize/close).
   - Verify: `dotnet build`

2. Integrate WhiteboardView into MainTeacherWindow
   - Files: `Views/MainTeacherWindow.xaml`
   - Change: Replace the "Bảng trắng" Tab content with `<views:WhiteboardView />`.
   - Verify: Run app, check if Whiteboard tab shows the UI.

3. Implement Shape Drawing Logic
   - Files: `Views/WhiteboardView.xaml.cs`
   - Change: Implement `Canvas_MouseDown`, `MouseMove`, `MouseUp` to handle `DrawingType.Rectangle`, `Circle`, `Line`.
     - Use `InkCanvas.Children.Add()` for temporary preview.
     - Add final shape to `WhiteboardService.Strokes`.
   - Verify: Run app, draw shapes.

4. Implement Multi-page Support
   - Files: `Services/WhiteboardService.cs`, `Views/WhiteboardView.xaml`, `Views/WhiteboardView.xaml.cs`
   - Change: Add `Pages` collection to Service. Add Next/Previous page buttons and logic in View.
   - Verify: Click Next page, check if canvas clears. Click Previous, check if strokes return.

5. Implement Save Functionality
   - Files: `Views/WhiteboardView.xaml.cs`
   - Change: Update Save logic to support saving current page as Image.
   - Verify: Click Save, check if file is created.

### Risks & mitigations
- **Risk**: InkCanvas editing mode might conflict with custom shape drawing.
  - **Mitigation**: Set `EditingMode` to `None` when drawing shapes.
- **Risk**: Serialization of Shapes vs Strokes.
  - **Mitigation**: `DrawingStroke` model already supports shape properties (`Width`, `Height`).

### Rollback plan
- Revert changes to `MainTeacherWindow.xaml`.
- Delete `WhiteboardView.xaml`.
