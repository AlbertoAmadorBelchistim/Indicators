# NOTE:
# This script rewrites internal prready-* tags on origin after rebasing.
# Do NOT use with public or release tags.

param(
    [string]$Upstream = "upstream/Develop"
)

# --- Resolve repo root ---
$RepoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
if (-not $RepoRoot) {
    throw "Not inside a Git repository."
}
Set-Location $RepoRoot

# --- Current branch ---
$Branch = (git branch --show-current).Trim()
if (-not $Branch) {
    throw "Detached HEAD. Checkout a branch before rebasing."
}

Write-Host "INFO Branch: $Branch"
Write-Host "INFO Upstream: $Upstream"

# --- Fetch ---
Write-Host "INFO Fetching remotes..."
git fetch upstream
git fetch origin --tags

# --- Snapshot ---
$BackupRef = "backup/prready-main-pre-rebase"

# --- Ensure snapshot branch is fresh ---
git show-ref --verify --quiet refs/heads/$BackupRef
if ($LASTEXITCODE -eq 0) {
    Write-Host "INFO Removing existing snapshot branch: $BackupRef"
    git branch -D $BackupRef
}

Write-Host "INFO Creating snapshot: $BackupRef"
git branch $BackupRef

# --- Rebase ---
Write-Host "INFO Rebasing onto $Upstream"
git rebase $Upstream
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERR Rebase failed. Snapshot preserved: $BackupRef"
    exit 1
}

# --- Retag ---
$RetagScript = Join-Path $RepoRoot "tools\retag.ps1"
if (-not (Test-Path $RetagScript)) {
    throw "retag.ps1 not found in tools/."
}

Write-Host "INFO Retagging using snapshot $BackupRef"
powershell -NoProfile -ExecutionPolicy Bypass -File $RetagScript -OldRef $BackupRef -NewRef $Branch -Upstream $Upstream

# --- Sync moved prready tags (delete + recreate) ---
Write-Host ""
Write-Host "INFO Syncing moved prready tags with origin..."

git fetch origin --tags --prune | Out-Null

$MovedTags = @()

foreach ($tag in (git tag -l "prready-*")) {
    $local = git rev-parse $tag
    $remoteLine = git ls-remote --tags origin $tag
    if (-not $remoteLine) {
        continue
    }

    $remote = ($remoteLine -split "`t")[0]

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
        git push origin ":refs/tags/$tag"
    }

    Write-Host "INFO Re-pushing moved tags..."
    git push origin $MovedTags

    Write-Host "INFO Verifying tags..."
    git fetch origin --tags --prune | Out-Null

    foreach ($tag in $MovedTags) {
        $local = git rev-parse $tag
        $remoteLine = git ls-remote --tags origin $tag
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