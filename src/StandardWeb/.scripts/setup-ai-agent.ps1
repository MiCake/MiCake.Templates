# ============================================================
# MiCake AI Agent Setup Script (PowerShell)
# This script downloads and integrates AI Agent from GitHub
# ============================================================

$ErrorActionPreference = 'Stop'

$repoUrl = 'https://github.com/MiCake/MiCake.AI.Agent'
$branch = 'main'
$tempDir = Join-Path $env:TEMP ("MiCake.AI.Agent." + (Get-Random))
$zipUrl = "$repoUrl/archive/refs/heads/$branch.zip"
$zipFile = Join-Path $tempDir 'repo.zip'

Write-Host ''
Write-Host '============================================================'
Write-Host '  MiCake AI Agent Setup'
Write-Host '============================================================'
Write-Host ''
Write-Host "Downloading AI Agent from: $repoUrl"
Write-Host ''

try {
    # Create temp directory
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    # Set TLS 1.2 for HTTPS
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $ProgressPreference = 'SilentlyContinue'
    
    # Download ZIP
    Write-Host '[INFO] Downloading ZIP archive...'
    Invoke-WebRequest -Uri $zipUrl -OutFile $zipFile -UseBasicParsing
    
    # Extract ZIP
    Write-Host '[INFO] Extracting files...'
    Expand-Archive -Path $zipFile -DestinationPath $tempDir -Force
    
    # Find extracted directory
    $extractedDir = Get-ChildItem -Path $tempDir -Directory | 
        Where-Object { $_.Name -like 'MiCake.AI.Agent-*' } | 
        Select-Object -First 1
    
    if ($extractedDir) {
        # Copy files excluding README files
        Write-Host '[INFO] Copying files to project directory...'
        Get-ChildItem -Path $extractedDir.FullName | 
            Where-Object { $_.Name -notmatch '^README' } | 
            ForEach-Object { 
                Copy-Item -Path $_.FullName -Destination . -Recurse -Force 
            }
    }
    
    # Cleanup temp directory
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    
    # Update preferences.yaml to link with project's copilot-instructions.md
    $preferencesFile = '.micake\agents\config\preferences.yaml'
    if (Test-Path $preferencesFile) {
        Write-Host '[INFO] Configuring AI Agent preferences...'
        $content = Get-Content -Path $preferencesFile -Raw -Encoding UTF8
        # Update custom_practices.file_path value
        $content = $content -replace '(custom_practices:[\s\S]*?file_path:\s*)""', '$1".github/copilot-instructions.md"'
        Set-Content -Path $preferencesFile -Value $content -Encoding UTF8 -NoNewline
        Write-Host '[INFO] Linked AI Agent with .github/copilot-instructions.md'
    }
    
    Write-Host ''
    Write-Host '[SUCCESS] AI Agent has been successfully integrated!'
    Write-Host ''
    
} catch {
    Write-Host "[ERROR] Failed to download AI Agent: $_"
    Write-Host 'Please manually download from: https://github.com/MiCake/MiCake.AI.Agent'
    
    # Cleanup on error
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    exit 1
}
