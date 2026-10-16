param([string]$AppDockDirectory)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not $AppDockDirectory) {
    $taskConfig = Join-Path $taskRoot 'deploy.local.txt'
    if (Test-Path -LiteralPath $taskConfig -PathType Leaf) { $AppDockDirectory = Get-Content -LiteralPath $taskConfig -Encoding UTF8 -TotalCount 1 }
}
if ([string]::IsNullOrWhiteSpace($AppDockDirectory)) { throw 'Pass an AppDock directory or create deploy.local.txt.' }
$taskHost = [IO.Path]::GetFullPath($AppDockDirectory)
if (-not (Test-Path -LiteralPath (Join-Path $taskHost 'AppDock.at365.exe') -PathType Leaf)) { throw 'AppDock.at365.exeがあるフォルダーを指定してください。' }
$taskSource = Join-Path $taskRoot 'Applet.WindowsTools.at365'
$taskFiles = @('Applet.WindowsTools.at365.dll', 'Applet.WindowsTools.at365.deps.json', 'extension.json')
foreach ($taskFile in $taskFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $taskSource $taskFile) -PathType Leaf)) { throw '先にpublish.batを実行してください。' }
}
$taskDestination = Join-Path $taskHost 'extensions\Applet.WindowsTools.at365'
New-Item -ItemType Directory -Path $taskDestination -Force | Out-Null
# AppDock.SDK is supplied by the DLL host, not privately deployed by the applet.
foreach ($taskFile in $taskFiles) { Copy-Item -LiteralPath (Join-Path $taskSource $taskFile) -Destination (Join-Path $taskDestination $taskFile) -Force }
Write-Output "Appletを配置しました: $taskDestination"
Write-Output 'AppDockを起動し直し、Applet.WindowsTools.at365を有効にしてください。'
