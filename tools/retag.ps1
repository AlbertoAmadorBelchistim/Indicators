<#
.SYNOPSIS
Rewrites or normalizes branch tags in the local repository.

.DESCRIPTION
Utility script to help maintain consistent branch tagging during
rebases and upstream synchronizations.

NOTE
This script rewrites local tags AND force-pushes moved tags to origin.
#>

param(
    [string]$OldRef = "backup/prready-main-pre-rebase",
    [string]$NewRef = "prready/main",
    [string]$Upstream = "upstream/Develop",
    [switch]$SkipFetch
)

# Ensure we run from the repository root regardless of invocation location
$RepoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
if (-not $RepoRoot) {
    throw "retag.ps1 must be executed within a Git repository working tree."
}
Set-Location $RepoRoot

if (-not $SkipFetch) {
    git fetch upstream --prune | Out-Null
    git fetch origin --prune | Out-Null
    # IMPORTANT: do not fetch tags here; the wrapper sync step handles tags explicitly
}

# New base for scanning only "own" commits
$baseNew = (git merge-base $NewRef $Upstream).Trim()
if (-not $baseNew) {
    throw "Unable to compute merge-base for NewRef='$NewRef' and Upstream='$Upstream'."
}

$newCommits = git rev-list --reverse "$baseNew..$NewRef"
# Build map: patch-id -> new commit
$patchToNew = @{}

foreach ($c in $newCommits) {
    $pidLine = (git show $c --pretty=format: | git patch-id --stable)
    if (-not $pidLine) { continue }

    $patchId = $pidLine.Split()[0]
    if (-not $patchToNew.ContainsKey($patchId)) {
        $patchToNew[$patchId] = $c
    }
}

# Tags to move (only tags reachable from old ref)
$tags = git tag --merged $OldRef

# Track tags actually moved (for automatic remote push)
$movedTags = New-Object System.Collections.Generic.List[string]

# Ensure temp dir exists for annotated tag messages
$tagDir = Join-Path $env:TEMP ("tagmsg_retag_" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tagDir | Out-Null

function Get-SafeFileName([string]$name) {
    # Replace characters illegal in Windows filenames + path separators
    $safe = $name -replace '[<>:"/\\|?*\x00-\x1F]', '_'
    return $safe
}

foreach ($t in $tags) {
    $oldCommit = (git rev-list -n 1 $t).Trim()
    if (-not $oldCommit) { continue }

    $pidLine = (git show $oldCommit --pretty=format: | git patch-id --stable)
    if (-not $pidLine) {
        Write-Host "SKIP (no patch-id): $t"
        continue
    }

    $patchId = $pidLine.Split()[0]

    if (-not $patchToNew.ContainsKey($patchId)) {
        Write-Host "WARNING (no match in new branch): $t"
        continue
    }

    $newCommit = $patchToNew[$patchId]
    $tagType = (git cat-file -t "refs/tags/$t").Trim()

    if ($tagType -eq "tag") {
        $msg = git for-each-ref "refs/tags/$t" --format="%(contents)"
        $safeName = Get-SafeFileName $t
        $tmp = Join-Path $tagDir ("tagmsg_" + $safeName + ".txt")

        Set-Content -Path $tmp -Value $msg -Encoding utf8
        git tag -a -f $t $newCommit -F $tmp | Out-Null
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
    else {
        git tag -f $t $newCommit | Out-Null
    }

    $movedTags.Add($t) | Out-Null
    Write-Host "MOVED: $t -> $newCommit"
}

# Automatically force-push moved tags to origin
if ($movedTags.Count -gt 0) {
    Write-Host "Pushing $($movedTags.Count) moved tag(s) to origin..." -ForegroundColor Cyan

    foreach ($t in $movedTags) {
        git push --force origin "refs/tags/$t"
    }

    Write-Host "Remote tag push complete." -ForegroundColor Green
}
else {
    Write-Host "No tags were moved; nothing to push to origin."
}

Write-Host "Done."
