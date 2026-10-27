# RAXY Menu

RAXY Menu provides a reusable menu stack for Unity uGUI. It keeps menu instances cached, opens scene objects or prefabs, supports Addressable menus, and provides back navigation and tab selection.

## Components

- `MenuManagerBase`: registers menu IDs, resolves instances, and manages the navigation stack.
- `MenuEntry`: configures direct scene objects, direct prefabs, or Addressable prefabs.
- `MenuBase` and `OpenMenuConfigBase`: base types for project-specific screens and their opening context.
- `BackButton`: connects a uGUI button to the owning menu manager.
- `TabGroup`: selects toggles and their associated panels.

## Installation

Add `com.raxy.menu` from the Git repository or a local path through Unity Package Manager. Install its dependencies in the consuming project: RAXY Core, RAXY Utility, UniTask, Addressables, and UGUI. The current source also uses Odin Inspector attributes, so the consuming project must provide Odin Inspector as a project plugin, following the convention used by other RAXY packages.

## Usage

1. Derive a manager from `MenuManagerBase` and assign `menuRoot`, a `mainMenuId`, and `menuEntries` in the Inspector.
2. Derive screens from `MenuBase`. Pass an `OpenMenuConfigBase` containing the manager when opening a screen that uses `BackButton`.
3. Call `OpenMenu(menuId, config)`, `Back()`, or `CloseAll()` from the project-specific manager.
4. Use `TabGroup` when one screen contains multiple tabs. Tab changes do not enter the menu navigation stack.

The package contains generic navigation only. Project Alice's pause manager, inventory, armory, and blacksmith screens remain in Project Alice.

## Migration note

`RAXY.Menu` already exists under Project Alice's `Assets/_RAXY_DEV_/Scripts/Menu System`. Remove that local assembly before installing this package in the same project; two assemblies with the name `RAXY.Menu` cannot coexist. The scripts in this package retain the original Unity `.meta` GUIDs to preserve serialized references during that migration.

## Current behavior

This initial package preserves the existing navigation behavior. The caller owns menu session lifetime. `OpenMenu` loads Addressables asynchronously, and callers should avoid overlapping open requests until transition serialization is added. `BackButton` and `TabGroup` replace button or toggle runtime listeners when they initialize.
