# RAXY Menu Package Design

## Intent
Extract the generic `RAXY.Menu` assembly from Project Alice into a reusable UPM package while keeping Project Alice game menus in its own assembly.

## Scope
Copy `MenuManagerBase`, `MenuBase`, `MenuEntry`, `BackButton`, `TabGroup`, and `RAXY.Menu.asmdef` with their Unity `.meta` files. Preserve their current public API, namespace, assembly name, and GUIDs. Follow sibling RAXY package layout and metadata. Do not alter Project Alice in this task.

## Constraints
No automated tests or `dotnet build` without Robby's explicit request. No Unity Editor launch. Use read-only inspection, JSON parsing, Git checks, and source comparison for verification. Keep notes and documentation in English. Do not declare a new sealed class.

## Packaging
Package ID `com.raxy.menu`, initial version `1.0.0`, folder `RAXY Menu`, GitHub repository name `RAXY-Menu` under `RobyRAX` if credentials permit creation. Document Odin Inspector as a required project plugin, consistent with RAXY UI. Direct dependencies are RAXY Core, UniTask, Addressables, and UGUI; RAXY Utility remains an asmdef dependency for source parity.

## Limit
This is a faithful extraction. Navigation behavior and project integration remain unchanged; known lifecycle and concurrent-open concerns require a separate behavior change. Installing the package in Project Alice also requires removing or relocating the local copy to avoid duplicate `RAXY.Menu` assemblies, which is outside this request.
