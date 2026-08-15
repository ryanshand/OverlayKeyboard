# Overlay Keyboard

A Windows application that provides an on-screen keyboard for touchscreen computers. The keyboard is designed to be semi-transparent and resizable, allowing users to see content behind it while typing.

## Features

- **Resizable Interface**: Drag the corner handle to resize the keyboard
- **Show/Hide Toggle**: Click HIDE to minimize to a small SHOW button, click SHOW to expand
- **Draggable SHOW Button**: When hidden, the SHOW button can be dragged around the screen
- **Full QWERTY Layout**: Complete keyboard with numbers, letters, and special keys
- **Shift Functionality**: Toggle between uppercase and lowercase letters
- **Configuration**: Customizable sizing through JSON configuration file
- **Touch-Friendly**: Optimized for 13-inch 1920x1080 touchscreen displays

## Keyboard Layout

- **Number Row**: 1, 2, 3, 4, 5, 6, 7, 8, 9, 0
- **Letter Rows**: Standard QWERTY layout
- **Special Keys**: 
  - Shift (toggles case)
  - Space bar (centered)
  - Colon/Semicolon (;)
  - Quotation mark (")
  - Enter/Return
  - Backspace (above Enter)

## Configuration

The application uses `keyboard_config.json` to store default settings:

```json
{
  "DefaultWidth": 1200,
  "DefaultHeight": 300,
  "KeyFontSize": 14,
  "NumberRowHeight": 40,
  "LetterRowHeight": 50,
  "SpacebarHeight": 50,
  "KeySpacing": 2
}
```

## Usage

1. Run the application
2. The keyboard will appear at the bottom of the screen
3. Click keys to type (works with any application)
4. Use SHIFT to toggle between uppercase and lowercase
5. Click HIDE to minimize to a small SHOW button
6. Drag the SHOW button to reposition it
7. Click SHOW to expand the keyboard at the new location
8. Drag the corner handle to resize the keyboard

## Building

This is a .NET 6 Windows Forms application. To build:

```bash
dotnet build
dotnet run
```

## Requirements

- Windows 10/11
- .NET 6.0 Runtime
- Touchscreen or mouse input
