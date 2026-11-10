param([switch]$Check)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskManifestPath = Join-Path $taskRoot 'extension.json'
$taskManifest = Get-Content -LiteralPath $taskManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$taskOriginalCommands = $taskManifest.commands | ConvertTo-Json -Depth 20 -Compress
$taskDescriptions = Get-Content -LiteralPath (Join-Path $taskRoot 'command-descriptions.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($taskDescription in $taskDescriptions.PSObject.Properties) {
    $taskCommand = @($taskManifest.commands | Where-Object { $_.id -eq ('at365.windows-tools.' + $taskDescription.Name) })
    if ($taskCommand.Count -ne 1) { throw ('Missing or duplicate described command: ' + $taskDescription.Name) }
    $taskCommand[0] | Add-Member -NotePropertyName description -NotePropertyValue $taskDescription.Value -Force
}
$taskProcesses = Get-Content -LiteralPath (Join-Path $taskRoot 'windows-process-commands.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$taskProcessIds = @($taskProcesses | ForEach-Object { 'at365.windows-tools.' + $_.id })
$taskCommands = @($taskManifest.commands | Where-Object { $_.id -notlike 'at365.windows-tools.windows.*' -and $_.id -ne 'at365.windows-tools.lock-windows' -and $_.id -notin $taskProcessIds })
$taskCommands += [pscustomobject][ordered]@{
    id = 'at365.windows-tools.lock-windows'
    title = 'Windowsをロックする'
    description = 'Windowsを直ちにロックします。開いているアプリは終了しません。ロックの解除にはWindowsへのサインインが必要です。'
}
foreach ($taskProcess in $taskProcesses) {
    $taskCommands += [pscustomobject][ordered]@{
        id = 'at365.windows-tools.' + $taskProcess.id
        title = $taskProcess.title
        description = $taskProcess.description
    }
}
foreach ($taskCommand in $taskCommands) {
    $taskCommand | Add-Member -NotePropertyName automation -NotePropertyValue $true -Force
}
if (@($taskCommands | Where-Object { [string]::IsNullOrWhiteSpace($_.description) }).Count -ne 0) {
    throw 'Every fixed command must have a description.'
}
$taskDocPath = Join-Path $taskRoot 'docs\windows-shortcuts.md'
$taskDocLines = @(
    '# WindowsToolsコマンド一覧', '',
    ('WindowsToolsはウィンドウ操作・消灯・ロック・プロセス操作など固定' + $taskCommands.Count + '件を提供します。'), '',
    'Windows標準キーはAppDock本体のconfigs/command-sets/windows.jsonへ移りました。コマンドセンターの「コマンドセット → Windows標準キー」から割り当ててください。', '',
    'このAppletの固定コマンドはmanifestで共通操作API（MCPを含む）へ公開します。実行には既存のコマンド実行許可と各操作に必要な許可を使います。', '',
    '「Windowsをロックする」はキー送信を使わずWindowsへロックを要求し、無操作時の自動ロックとは独立しています。', ''
)
$taskDocLines += @('', '## ターミナル・電源関連', '',
    'これらはキー送信を使わず、本体のprocesses.startで呼び出します。「設定 → 操作の許可」のApplet列で、情報取得・コマンド実行・プロセス起動を許可してください。実行受付は終了操作やダイアログ表示の完了を保証しません。', '',
    '| コマンド | 説明・利用条件 |', '| --- | --- |')
foreach ($taskProcess in $taskProcesses) {
    $taskDocLines += '| ' + $taskProcess.title + ' | ' + $taskProcess.description + ' |'
}
$taskDocLines += @('', '## リモートデスクトップの最小化・復元', '',
    '対象はmstsc.exeです。キートリガーでは、ScrollLockキーに割り当てて「元の入力をブロックする」を有効にしてください。', '',
    '| コマンド | 説明 |', '| --- | --- |')
foreach ($taskCommand in @($taskCommands | Where-Object { $_.id -like 'at365.windows-tools.rdp-*' })) {
    $taskDocLines += '| ' + $taskCommand.title + ' | ' + $taskCommand.description + ' |'
}
$taskDocText = ($taskDocLines -join "`r`n") + "`r`n"
if ($Check) {
    if (($taskCommands | ConvertTo-Json -Depth 20 -Compress) -cne $taskOriginalCommands) {
        throw 'The static command manifest differs from the native command definitions.'
    }
    if ([IO.File]::ReadAllText($taskDocPath) -cne $taskDocText) { throw 'The shortcut documentation differs from the native command definitions.' }
} else {
    $taskManifest.commands = $taskCommands
    $taskJson = ($taskManifest | ConvertTo-Json -Depth 20) -replace "`r?`n", "`r`n"
    [IO.File]::WriteAllText($taskManifestPath, $taskJson + "`r`n", [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($taskDocPath, $taskDocText, [Text.UTF8Encoding]::new($false))
}
Write-Output ('Native Windows commands: ' + $taskCommands.Count)
