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

Write-Host "▶ Branch: $Branch"
Write-Host "▶ Upstream: $Upstream"

# --- Fetch ---
Write-Host "▶ Fetching remotes..."
git fetch upstream
git fetch origin --tags

# --- Ensure snapshot branch is fresh ---
git show-ref --verify --quiet refs/heads/$BackupRef
if ($LASTEXITCODE -eq 0) {
    Write-Host "▶ Removing existing snapshot branch: $BackupRef"
    git branch -D $BackupRef
}

# --- Snapshot ---
$BackupRef = "backup/prready-main-pre-rebase"

Write-Host "▶ Creating snapshot: $BackupRef"
git branch $BackupRef

# --- Rebase ---
Write-Host "▶ Rebasing onto $Upstream"
git rebase $Upstream
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Rebase failed. Snapshot preserved: $BackupRef"
    exit 1
}

# --- Retag ---
$RetagScript = Join-Path $RepoRoot "tools\retag.ps1"
if (-not (Test-Path $RetagScript)) {
    throw "retag.ps1 not found in tools/."
}

Write-Host "▶ Retagging using snapshot $BackupRef"
powershell $RetagScript -OldRef $BackupRef -NewRef $Branch -Upstream $Upstream

Write-Host ""
Write-Host "✅ Rebase + retag completed successfully."
Write-Host "Next steps:"
Write-Host "  git push origin $Branch"
Write-Host "  git push --force-with-lease origin <affected-tags>"