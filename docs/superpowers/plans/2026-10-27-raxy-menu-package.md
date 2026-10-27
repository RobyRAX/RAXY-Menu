# RAXY Menu Package Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task by task. Checkboxes track progress. Robby's instructions prohibit automated tests by default, so verification here uses file inspection, JSON parsing, and Git checks.

**Goal:** Create a reusable `com.raxy.menu` UPM package and publish its initial Git commit.

**Architecture:** Preserve the five generic menu scripts and asmdef from Project Alice, including Unity GUIDs. Add package metadata and documentation in the same layout as RAXY UI, then initialize a dedicated repository and push it if a remote can be provisioned.

**Tech Stack:** Unity C#, UPM, Git, PowerShell.

**Spec:** `docs/superpowers/specs/2026-10-27-raxy-menu-package-design.md`

## Global Constraints

- Do not modify Project Alice or its assets.
- Do not run automated tests, `dotnet build`, or launch Unity.
- Keep documentation and notes in English.
- Preserve class inheritance and existing public API.

## Review Focus

- Source `.meta` GUIDs must match the source project to preserve Unity references.
- Package ID and asmdef name must match established RAXY conventions.
- Declared dependencies must cover the imported namespaces.
- Consumers need Odin Inspector installed for current source to compile.
- The local Project Alice assembly cannot coexist with an installed package of the same assembly name.

---

### Task 1: Package sources and metadata

**Files:** Create `Scripts/*.cs`, corresponding `.meta`, `Scripts/RAXY.Menu.asmdef` and `.meta`, `Scripts.meta`, `package.json` and `.meta`, `.gitignore`, `LICENSE` and `.meta`.

**Interfaces:** Preserve `RAXY.Menu` namespace, assembly name, public types, and menu script GUIDs.

- [ ] Copy the five menu scripts, asmdef, and `.meta` files from Project Alice.
- [ ] Copy sibling package structural files, then set `package.json` to `com.raxy.menu` version `1.0.0` and document dependencies.
- [ ] Parse `package.json` and asmdef JSON; compare copied script and `.meta` hashes with source.

### Task 2: Documentation and review

**Files:** Create `README.md`, `README.md.meta`, `CHANGELOG.md`, `CHANGELOG.md.meta`; retain this spec and plan.

**Interfaces:** Document UPM installation, API roles, required dependencies, and the duplicate-assembly caveat.

- [ ] Write README and changelog in English.
- [ ] Inspect the complete file list and content; verify required metadata and no Alice-specific scripts entered the package.

### Task 3: Repository and publication

**Files:** Create `.git` repository.

**Interfaces:** Branch `main`, remote `origin` at `https://github.com/RobyRAX/RAXY-Menu.git` if available.

- [ ] Initialize Git and inspect `git status` plus `git diff --check` before committing.
- [ ] Commit the package as an initial release.
- [ ] Create or attach the GitHub remote using available authentication, push `main`, and verify the remote HEAD. If credentials or repository creation are unavailable, report the exact blocker and leave the local commit intact.
