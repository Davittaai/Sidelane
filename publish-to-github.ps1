# Run inside the Sidelane source folder on Windows after `gh auth login`.
# Creates/updates ONLY Davittaai/Sidelane and never touches other projects.
$ErrorActionPreference = 'Stop'
$repo = 'Davittaai/Sidelane'
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'Install GitHub CLI: https://cli.github.com/' }
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Install Git: https://git-scm.com/' }
if (-not (Test-Path 'src/Sidelane/Sidelane.csproj')) { throw 'Run from the extracted Sidelane root folder.' }
& gh auth status
if ($LASTEXITCODE -ne 0) { throw 'Authenticate first with: gh auth login' }
if (-not (Test-Path '.git')) { & git init -b main; if ($LASTEXITCODE -ne 0) { throw 'git init failed' } }
& git add -A
if ($LASTEXITCODE -ne 0) { throw 'git add failed' }
& git diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
    & git commit -m 'release: Sidelane v1.0 Windows source'
    if ($LASTEXITCODE -ne 0) { throw 'git commit failed. Configure your git user.name and user.email.' }
}
# A preloaded Git bundle may already be named origin; never push to its local path.
& gh repo view $repo --json name 2>$null | Out-Null
$exists = $LASTEXITCODE -eq 0
& git remote get-url github 2>$null | Out-Null
$hasRemote = $LASTEXITCODE -eq 0
if ($hasRemote) {
    $remoteUrl = (& git remote get-url github).Trim()
    if ($remoteUrl -notin @('https://github.com/Davittaai/Sidelane.git', 'https://github.com/Davittaai/Sidelane', 'git@github.com:Davittaai/Sidelane.git')) {
        throw 'Existing github remote does not point to Davittaai/Sidelane. Refusing to push elsewhere.'
    }
}
if (-not $exists) {
    if ($hasRemote) { throw 'Remote github already exists but the target repository does not. Check remotes before continuing.' }
    & gh repo create $repo --public --source . --remote github --push
} else {
    if (-not $hasRemote) {
        & git remote add github "https://github.com/$repo.git"
        if ($LASTEXITCODE -ne 0) { throw 'Unable to add remote' }
    }
    & git push -u github HEAD:main
}
if ($LASTEXITCODE -ne 0) { throw 'GitHub publication failed; no successful upload was confirmed.' }
Write-Host "Source uploaded: https://github.com/$repo"
Write-Host 'Wait for the Windows build to succeed and manually test the executable before tagging v1.0.0.'
