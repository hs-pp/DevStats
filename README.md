# DevStats

Time Tracking for Unity powered by WakaTime.

DevStats is your personal time tracker built into the Unity editor. It automatically tracks how much time you spend editing scenes, prefabs, UI documents, and more.

Combine it with another WakaTime plugin for your IDE to achieve complete, project-wide tracking and better understand how you spend your development time.

Supports both Windows and Mac.

# Features

## Stats Panel
Visualize basic stats from the Stats panel. Stats are automatically updated at an interval adjustable in the settings. You can also force update the stats through a button.

<img src="./Images/StatsPanel.png" width="420">

More detailed stats can be found on WakaTime.

## Heartbeats Panel
From the <b>Heartbeats Panel</b> you can see your queued heartbeats that have yet to be sent, a history of the most recent heartbeats, and a list of failed-to-send heartbeats that can be reattempted through an easy click of a button.

Heartbeats are collected and sent to the Wakatime CLI once every 2 minutes.

<img src="./Images/HeartbeatsPanel.png" width="420">

## Settings Panel

Settings are very straightforward.

<img src="./Images/SettingsPanel.png" width="420">

DevStats does not run unless both `IsEnabled = true` and the WakaTime API Key is valid. The Stats Panel also needs a valid OAuth Token.

# Setup

Use Unity's Package Manager to add a package through git URL:
https://github.com/hs-pp/DevStats.git

Once it's installed, you can find the DevStats editor window at `Window/DevStats`.

<img src="./Images/MenuItem.png" width="256">

In the settings panel, fill in two values from WakaTime:
- **API Key:** your WakaTime `Secret API Key`, from https://wakatime.com/api-key. WakaTime API keys are write-only, so DevStats only uses this one to send heartbeats.
- **OAuth Token:** your personal OAuth token, from https://wakatime.com/oauth. DevStats uses this one to read your stats.

If the status indicator on the top right of the settings page says RUNNING, you're golden!