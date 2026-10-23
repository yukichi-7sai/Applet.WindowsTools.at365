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
$taskCatalog = Get-Content -LiteralPath (Join-Path $taskRoot 'windows-shortcuts.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$taskProcesses = Get-Content -LiteralPath (Join-Path $taskRoot 'windows-process-commands.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$taskProcessIds = @($taskProcesses | ForEach-Object { 'at365.windows-tools.' + $_.id })
$taskCommands = @($taskManifest.commands | Where-Object { $_.id -notlike 'at365.windows-tools.windows.*' -and $_.id -ne 'at365.windows-tools.lock-windows' -and $_.id -notin $taskProcessIds })
$taskCommands += [pscustomobject][ordered]@{
    id = 'at365.windows-tools.lock-windows'
    title = 'Windowsをロックする'
    description = 'Windowsを直ちにロックします。開いているアプリは終了しません。ロックの解除にはWindowsへのサインインが必要です。'
}
foreach ($taskShortcut in $taskCatalog) {
    $taskCommands += [pscustomobject][ordered]@{
        id = 'at365.windows-tools.windows.' + $taskShortcut.id
        title = $taskShortcut.title
        description = '送信キー: ' + $taskShortcut.keys + '。' + $taskShortcut.description
    }
}
foreach ($taskProcess in $taskProcesses) {
    $taskCommands += [pscustomobject][ordered]@{
        id = 'at365.windows-tools.' + $taskProcess.id
        title = $taskProcess.title
        description = $taskProcess.description
    }
}
if (@($taskCommands | Where-Object { [string]::IsNullOrWhiteSpace($_.description) }).Count -ne 0) {
    throw 'Every fixed command must have a description.'
}
$taskDocPath = Join-Path $taskRoot 'docs\windows-shortcuts.md'
$taskDocLines = @(
    '# Windows標準コマンド一覧', '',
    ('WindowsToolsには、Windows標準のキー操作' + $taskCatalog.Count + '件と「Windowsをロックする」、ターミナル・電源関連の' + $taskProcesses.Count + '件が組み込まれています。既存のウィンドウ操作などと合わせて固定' + $taskCommands.Count + '件です。'), '',
    'AppDockのコマンドパレットやショートカット・ジェスチャー・トレイの割り当てから選べます。送信先は実行時の最前面です。別のアプリを操作するときはグローバルショートカットなどから実行してください。', '',
    'Windowsのバージョン、機器、インストール済みアプリ、設定によって利用できる操作が異なります。コマンドを用意するだけで、必要な機能の導入や設定の変更は行いません。キー送信の完了は、Windows側の動作を確認できたという意味ではありません。', '',
    '「Windowsをロックする」はキー送信を使わず、Windowsへロックを要求します。実行すると直ちにロックされ、解除にはWindowsへのサインインが必要です。無操作時の自動ロックとは独立しています。', '',
    'Windowsキーと仮想デスクトップの66件に、メディア・音量キー7件を加えています。[Microsoftの一覧](https://support.microsoft.com/ja-jp/accessibility/windows/keyboard-shortcuts-in-windows)のWin+Ctrl+Shift+B、Win+L、Win+カンマは含めません。Win+Tabの重複と、Win+Cの条件別の説明はまとめています。', '',
    '| コマンド | キー操作 | 説明・利用条件 |',
    '| --- | --- | --- |'
)
foreach ($taskShortcut in $taskCatalog) {
    $taskDocLines += '| ' + $taskShortcut.title + ' | `' + $taskShortcut.keys + '` | ' + $taskShortcut.description + ' |'
}
$taskDocLines += @('', '## ターミナル・電源関連', '',
    'これらはキー送信を使わず、本体のprocesses.startで呼び出します。「設定 → 操作の許可」のApplet列で、情報取得・コマンド実行・プロセス起動を許可してください。実行受付は終了操作やダイアログ表示の完了を保証しません。', '',
    '| コマンド | 説明・利用条件 |', '| --- | --- |')
foreach ($taskProcess in $taskProcesses) {
    $taskDocLines += '| ' + $taskProcess.title + ' | ' + $taskProcess.description + ' |'
}
$taskDocText = ($taskDocLines -join "`r`n") + "`r`n"
if ($Check) {
    if (($taskCommands | ConvertTo-Json -Depth 20 -Compress) -cne $taskOriginalCommands) {
        throw 'The static command manifest differs from windows-shortcuts.json.'
    }
    if ([IO.File]::ReadAllText($taskDocPath) -cne $taskDocText) { throw 'The shortcut documentation differs from windows-shortcuts.json.' }
} else {
    $taskManifest.commands = $taskCommands
    $taskJson = ($taskManifest | ConvertTo-Json -Depth 20) -replace "`r?`n", "`r`n"
    [IO.File]::WriteAllText($taskManifestPath, $taskJson + "`r`n", [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($taskDocPath, $taskDocText, [Text.UTF8Encoding]::new($false))
}
Write-Output ('Static Windows shortcuts: ' + $taskCatalog.Count + '; fixed commands: ' + $taskCommands.Count)
