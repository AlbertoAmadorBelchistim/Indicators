<#
.SYNOPSIS
Rewrites or normalizes branch tags in the local repository.

.DESCRIPTION
Utility script to help maintain consistent branch tagging during
rebases and upstream synchronizations.

NOTE
This script does not modify remote tags unless explicitly pushed.
#>

param(
    [string]$OldRef = "backup/prready-main-pre-rebase",
    [string]$NewRef = "prready/main",
    [string]$Upstream = "upstream/Develop"
)

# Ensure we run from the repository root regardless of invocation location
$RepoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
if (-not $RepoRoot) {
    throw "retag.ps1 must be executed within a Git repository working tree."
}
Set-Location $RepoRoot

git fetch --all --tags --prune | Out-Null

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

    Write-Host "MOVED: $t -> $newCommit"
}

Write-Host "Done."
