param([switch]$Check)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskManifestPath = Join-Path $taskRoot 'extension.json'
$taskManifest = Get-Content -LiteralPath $taskManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$taskCatalog = @(Get-Content -LiteralPath (Join-Path $taskRoot 'windows-shortcuts.json') -Raw -Encoding UTF8 | ConvertFrom-Json)
$taskCommands = @($taskManifest.commands | Where-Object { $_.id -notlike 'at365.windows-tools.windows.*' -and $_.id -ne 'at365.windows-tools.lock-windows' })
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
$taskDocPath = Join-Path $taskRoot 'docs\windows-shortcuts.md'
$taskDocLines = @(
    '# Windows標準コマンド一覧', '',
    'WindowsToolsには、Windows標準のキー操作66件と「Windowsをロックする」が組み込まれています。キー操作を個別に作成・保存しなくても利用できます。', '',
    'AppDockのコマンドパレットやショートカット・ジェスチャー・トレイの割り当てから選べます。送信先は実行時の最前面です。別のアプリを操作するときはグローバルショートカットなどから実行してください。', '',
    'Windowsのバージョン、機器、インストール済みアプリ、設定によって利用できる操作が異なります。コマンドを用意するだけで、必要な機能の導入や設定の変更は行いません。キー送信の完了は、Windows側の動作を確認できたという意味ではありません。', '',
    '「Windowsをロックする」はキー送信を使わず、Windowsへロックを要求します。実行すると直ちにロックされ、解除にはWindowsへのサインインが必要です。無操作時の自動ロックとは独立しています。', '',
    '対象は[Microsoftの一覧](https://support.microsoft.com/ja-jp/accessibility/windows/keyboard-shortcuts-in-windows)の「Windows キー キーボード ショートカット」と「複数のデスクトップのキーボード ショートカット」です。Win+Ctrl+Shift+B、Win+L、Win+カンマは含めません。Win+Tabの重複と、Win+Cの条件別の説明はまとめています。', '',
    '| コマンド | キー操作 | 説明・利用条件 |',
    '| --- | --- | --- |'
)
foreach ($taskShortcut in $taskCatalog) {
    $taskDocLines += '| ' + $taskShortcut.title + ' | `' + $taskShortcut.keys + '` | ' + $taskShortcut.description + ' |'
}
$taskDocText = ($taskDocLines -join "`r`n") + "`r`n"
if ($Check) {
    if (($taskCommands | ConvertTo-Json -Depth 20 -Compress) -cne ($taskManifest.commands | ConvertTo-Json -Depth 20 -Compress)) {
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
