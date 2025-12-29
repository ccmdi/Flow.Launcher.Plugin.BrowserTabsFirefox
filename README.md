# Browser Tabs (Firefox) - Flow Launcher plugin

Flow Launcher plugin for searching and switching Firefox tabs. Displays favicons and supports fuzzy search.

## Requirements

- [Flow Launcher](https://www.flowlauncher.com/)
- [ff-tabs-host](https://github.com/yourusername/ff-tabs-host) running with Firefox extension loaded

## Installation

1. Build the plugin:
   ```bash
   dotnet build -c Release
   ```

2. Copy the `bin` output to Flow Launcher plugins folder:
   ```
   %APPDATA%\FlowLauncher\Plugins\
   ```

3. Restart Flow Launcher

## Usage

1. Open Flow Launcher
2. Type your plugin keyword followed by a search term
3. Select a tab to switch to it
