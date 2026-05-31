# Test script: Launch the EXE and verify extracted files
$exePath = "f:\东西\智能体\神奇小玩意\神奇的小玩意.exe"

Write-Host "============================================"
Write-Host "  EXE File Verification Test"
Write-Host "============================================"

# Check EXE exists
if (Test-Path $exePath) {
    $size = (Get-Item $exePath).Length / 1MB
    Write-Host "[PASS] EXE exists: $([math]::Round($size,1)) MB"
} else {
    Write-Host "[FAIL] EXE not found"
    exit 1
}

# Launch EXE
Write-Host "Launching EXE..."
$proc = Start-Process -FilePath $exePath -PassThru

# Wait for it to start and extract files
Start-Sleep -Seconds 4

# Find temp directory
$tempDirs = Get-ChildItem "$env:TEMP" -Directory | Where-Object { $_.Name -like "MG*" } | Sort-Object LastWriteTime -Descending
if ($tempDirs.Count -gt 0) {
    $tempDir = $tempDirs[0].FullName
    Write-Host "[PASS] Temp directory found: $tempDir"
    
    # Check files
    $files = Get-ChildItem $tempDir -Recurse | Select-Object Name, Length, FullName
    Write-Host "`nFiles extracted:"
    foreach ($f in $files) {
        $sizeKB = [math]::Round($f.Length / 1KB, 1)
        Write-Host "  $($f.Name) - $sizeKB KB"
    }
    
    # Check index.html exists
    $htmlPath = Join-Path $tempDir "index.html"
    if (Test-Path $htmlPath) {
        $htmlContent = Get-Content $htmlPath -Raw -Encoding UTF8
        $hasNovelScript = $htmlContent -match 'novels_data\.js'
        $hasAppScript = $htmlContent -match 'app\.js'
        Write-Host "`n[PASS] index.html exists ($($htmlContent.Length) chars)"
        Write-Host "  Has novels_data.js script tag: $hasNovelScript"
        Write-Host "  Has inline app.js: $hasAppScript"
    } else {
        Write-Host "[FAIL] index.html NOT found"
    }
    
    # Check novels_data.js exists and has content
    $novelDataPath = Join-Path $tempDir "novels_data.js"
    if (Test-Path $novelDataPath) {
        $novelContent = Get-Content $novelDataPath -Raw -Encoding UTF8
        $size = ($novelContent.Length / 1KB)
        $hasWindowNV = $novelContent -match 'window\.__NV'
        $novelCount = ([regex]::Matches($novelContent, '"nv\d+"')).Count
        Write-Host "`n[PASS] novels_data.js exists ($([math]::Round($size,0)) KB)"
        Write-Host "  Has window.__NV: $hasWindowNV"
        Write-Host "  Novel entries detected: $novelCount"
        
        # Verify it starts correctly
        $preview = $novelContent.Substring(0, [Math]::Min(200, $novelContent.Length))
        Write-Host "  Preview: $preview"
        
        # Try to parse as JSON
        try {
            $jsonStr = $novelContent -replace '^window\.__NV=', ''
            $jsonStr = $jsonStr -replace ';$', ''
            $data = $jsonStr | ConvertFrom-Json
            $keys = $data | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name
            Write-Host "`n[PASS] Valid JSON with $($keys.Count) novel(s)"
            foreach ($k in $keys) {
                $contentLen = $data.$k.Length
                $paras = ($data.$k -split '\n\n').Count
                Write-Host "  $k : $contentLen chars, $paras paragraphs"
            }
        } catch {
            Write-Host "[FAIL] JSON parse error: $_"
        }
    } else {
        Write-Host "[FAIL] novels_data.js NOT found!"
    }

    # Cleanup
    Write-Host "`nClosing EXE..."
    $proc.CloseMainWindow()
    Start-Sleep -Seconds 1
    if (-not $proc.HasExited) { $proc.Kill() }
    
    # Verify cleanup
    if (Test-Path $tempDir) {
        Write-Host "[INFO] Temp dir still exists (will be cleaned on next run)"
    }
} else {
    Write-Host "[FAIL] No temp directory found (EXE may not have started properly)"
}

Write-Host "`n============================================"
Write-Host "  Test Complete"
Write-Host "============================================"
