$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$project = Join-Path $root 'StudyMauiApp\StudyMauiApp.csproj'
if (-not (Test-Path -LiteralPath $project)) {
    throw 'Project not found: StudyMauiApp\StudyMauiApp.csproj. Put this script beside StudyMauiApp.slnx.'
}

$sdkVersion = '9.0.306'
$sdkList = @(& dotnet --list-sdks)
if ($LASTEXITCODE -ne 0 -or (($sdkList -join [Environment]::NewLine) -notmatch '(?m)^9\.0\.306\s')) {
    throw "Required .NET SDK $sdkVersion is not installed."
}

$globalJson = Join-Path $root 'global.json'
if (Test-Path -LiteralPath $globalJson) {
    throw 'global.json already exists in the repository root. This script will not overwrite it. Review that file first.'
}

$globalJsonCreated = $false
$passwordPointer = [IntPtr]::Zero
$locationPushed = $false
try {
    $json = '{"sdk":{"version":"9.0.306","rollForward":"disable"}}'
    [System.IO.File]::WriteAllText($globalJson, $json, (New-Object System.Text.UTF8Encoding($false)))
    $globalJsonCreated = $true
    Push-Location $root
    $locationPushed = $true

    $selected = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0 -or $selected -ne $sdkVersion) {
        throw "Expected SDK $sdkVersion but selected $selected."
    }
    Write-Host "Using .NET SDK $selected" -ForegroundColor Cyan

    Write-Host 'Restoring Android workloads...' -ForegroundColor Cyan
    & dotnet workload restore $project
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet workload restore failed with exit code $LASTEXITCODE."
    }

    $keytoolCommand = Get-Command keytool.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    $keytool = if ($keytoolCommand) { $keytoolCommand.Source } else { $null }
    if (-not $keytool -and $env:JAVA_HOME) {
        $candidate = Join-Path $env:JAVA_HOME 'bin\keytool.exe'
        if (Test-Path -LiteralPath $candidate) { $keytool = $candidate }
    }
    if (-not $keytool) {
        $roots = @((Join-Path $env:ProgramFiles 'Microsoft'), (Join-Path $env:ProgramFiles 'Java')) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
        foreach ($searchRoot in $roots) {
            $found = Get-ChildItem -LiteralPath $searchRoot -Filter 'keytool.exe' -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) { $keytool = $found.FullName; break }
        }
    }
    if (-not $keytool) {
        throw 'keytool.exe not found. Open Visual Studio Installer and install the .NET MAUI development components / Android JDK, then rerun.'
    }

    $signingDir = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'StudyMauiApp-signing'
    New-Item -ItemType Directory -Path $signingDir -Force | Out-Null
    $keystore = Join-Path $signingDir 'studymauiapp.keystore'
    if (-not (Test-Path -LiteralPath $keystore)) {
        Write-Host 'Creating an Android signing key. Save the password securely.' -ForegroundColor Yellow
        Write-Host 'When asked for a separate key password, press ENTER to reuse the keystore password.' -ForegroundColor Yellow
        & $keytool -genkeypair -v -keystore $keystore -alias studymauiapp -keyalg RSA -keysize 2048 -validity 10000
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $keystore)) {
            throw "keytool failed with exit code $LASTEXITCODE."
        }
    }

    $securePassword = Read-Host 'Enter keystore password' -AsSecureString
    $passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $env:ANDROID_KEYSTORE_PASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)

    Write-Host 'Publishing signed APK...' -ForegroundColor Cyan
    $publishArgs = @(
        'publish', $project, '-f', 'net9.0-android', '-c', 'Release',
        '-p:AndroidPackageFormats=apk',
        '-p:AndroidCreatePackagePerAbi=false',
        '-p:AndroidKeyStore=true',
        "-p:AndroidSigningKeyStore=$keystore",
        '-p:AndroidSigningKeyAlias=studymauiapp',
        '-p:AndroidSigningStorePass=env:ANDROID_KEYSTORE_PASSWORD',
        '-p:AndroidSigningKeyPass=env:ANDROID_KEYSTORE_PASSWORD'
    )
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }

    $projectDir = Split-Path -Parent $project
    $publishDir = Join-Path $projectDir 'bin\Release\net9.0-android\publish'
    $apk = Get-ChildItem -LiteralPath $publishDir -Filter '*.apk' -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '(?i)signed.*\.apk$' } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if (-not $apk) {
        $androidOutput = Join-Path $projectDir 'bin\Release\net9.0-android'
        $apk = Get-ChildItem -LiteralPath $androidOutput -Filter '*Signed.apk' -File -Recurse -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    }
    if (-not $apk) {
        throw 'Build succeeded but no signed APK was found under bin\Release\net9.0-android.'
    }

    $desktop = [Environment]::GetFolderPath('Desktop')
    if (-not $desktop -or -not (Test-Path -LiteralPath $desktop)) { $desktop = Join-Path $env:USERPROFILE 'Desktop' }
    $destination = Join-Path $desktop 'StudyMauiApp.apk'
    Copy-Item -LiteralPath $apk.FullName -Destination $destination -Force
    Write-Host "SUCCESS: $destination" -ForegroundColor Green
    Write-Host "Signing key: $keystore" -ForegroundColor Yellow
    Write-Host 'Keep the signing key and password. Future updates need the same key.'
}
finally {
    if ($passwordPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }
    Remove-Item Env:ANDROID_KEYSTORE_PASSWORD -ErrorAction SilentlyContinue
    if ($globalJsonCreated -and (Test-Path -LiteralPath $globalJson)) {
        Remove-Item -LiteralPath $globalJson -Force -ErrorAction SilentlyContinue
    }
    if ($locationPushed) { Pop-Location }
}
