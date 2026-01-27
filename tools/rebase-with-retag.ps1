# NOTE:
# This script rewrites internal prready-* tags on origin after rebasing.
# Do NOT use with public or release tags.

param(
    [string]$Upstream = "upstream/Develop",
    [switch]$Resume
)

# --- Resolve repo root ---
$RepoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
if (-not $RepoRoot) {
    throw "Not inside a Git repository."
}
Set-Location $RepoRoot

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Exec-Git([string[]]$Args) {
& git @Args
if ($LASTEXITCODE -ne 0) {
throw "git $($Args -join ' ') failed with exit code $LASTEXITCODE"
}
}

# --- Current branch ---
$Branch = (git branch --show-current).Trim()
if (-not $Branch) {
    throw "Detached HEAD. Checkout a branch before rebasing."
}

Write-Host "INFO Branch: $Branch"
Write-Host "INFO Upstream: $Upstream"

# --- Fetch ---
Write-Host "INFO Fetching remotes..."
Exec-Git fetch upstream --prune
Exec-Git fetch origin --prune

# --- Snapshot ---
$BackupRef = "backup/prready-main-pre-rebase"

if (-not $Resume) {
    # --- Ensure snapshot branch is fresh ---
    git show-ref --verify --quiet "refs/heads/$BackupRef" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "INFO Removing existing snapshot branch: $BackupRef"
        Exec-Git branch -D $BackupRef
    }

    Write-Host "INFO Creating snapshot: $BackupRef"
    Exec-Git branch $BackupRef
}
else {
    # In resume mode we MUST NOT overwrite the snapshot.
    git show-ref --verify --quiet "refs/heads/$BackupRef" 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Resume mode requires snapshot branch '$BackupRef' to exist. Run the script without -Resume first."
    }
}
# --- Rebase ---
if (-not $Resume) {
Write-Host "INFO Rebasing onto $Upstream"
git rebase $Upstream
$rebaseExit = $LASTEXITCODE


if ($rebaseExit -ne 0) {
Write-Host "WARN Rebase requires manual conflict resolution."
Write-Host "WARN Resolve conflicts and run: git rebase --continue"
Write-Host "WARN When the rebase completes, run:"
Write-Host " powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\rebase-with-retag.ps1 -Resume"
exit $rebaseExit
}
}
else {
Write-Host "INFO Resume mode: skipping rebase, running retag + sync"
}

# --- Retag ---
$RetagScript = Join-Path $RepoRoot "tools\retag.ps1"
if (-not (Test-Path $RetagScript)) {
    throw "retag.ps1 not found in tools/."
}

Write-Host "INFO Retagging using snapshot $BackupRef"
powershell -NoProfile -ExecutionPolicy Bypass -File $RetagScript -OldRef $BackupRef -NewRef $Branch -Upstream $Upstream -SkipFetch
if ($LASTEXITCODE -ne 0) { throw "retag.ps1 failed with exit code $LASTEXITCODE" }

# --- Sync moved prready tags (delete + recreate) ---
Write-Host ""
Write-Host "INFO Syncing moved prready tags with origin..."

$MovedTags = @()

foreach ($tag in (Exec-Git tag -l "prready-*")) {
    $local = (Exec-Git rev-parse "$tag^{}").Trim()
    $remoteLine = Exec-Git ls-remote --tags origin "$tag^{}"

    if (-not $remoteLine) {
    # Fallback for lightweight tags if needed
        $remoteLine = Exec-Git ls-remote --tags origin $tag
        if (-not $remoteLine) { continue }
    }
    
    $remote = (($remoteLine -split "`t")[0]).Trim()
    if ($local -ne $remote) {
        $MovedTags += $tag
    }
}

if ($MovedTags.Count -eq 0) {
    Write-Host "OK No moved tags detected."
}
else {
    Write-Host "WARN Moved tags detected:"
    $MovedTags | ForEach-Object { Write-Host "  - $_" }

    Write-Host "INFO Deleting moved tags from origin..."
    foreach ($tag in $MovedTags) {
        Exec-Git push origin ":refs/tags/$tag"
    }

    Write-Host "INFO Re-pushing moved tags..."
    Exec-Git push origin $MovedTags

    Write-Host "INFO Verifying tags..."

    foreach ($tag in $MovedTags) {
        $local = Exec-Git rev-parse $tag
        $remoteLine = Exec-Git ls-remote --tags origin $tag
        $remote = ($remoteLine -split "`t")[0]

        if ($local -eq $remote) {
            Write-Host "  OK $tag OK"
        }
        else {
            throw "Tag $tag still mismatched after re-push."
        }
    }
}

Write-Host ""
Write-Host "DONE Rebase + retag completed successfully."
Write-Host "Next steps:"
Write-Host "  git push origin $Branch"
Write-Host "  git push --force-with-lease origin <affected-tags>"