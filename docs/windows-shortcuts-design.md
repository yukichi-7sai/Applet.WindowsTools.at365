# WindowsToolsと本体コマンドセットの分担

WindowsToolsは、ウィンドウ操作・消灯予約・ロック・プロセス操作の固定22件と、プロセス指定の最小化・復元を提供する。Windows標準キー73件は本体の[コマンドセット](../../AppDock.at365/docs/command-sets.md)へ分離した。最低本体版は0.209.0。

## 定義と登録

ウィンドウ操作など16件の説明はcommand-descriptions.json、ターミナル・電源操作5件はwindows-process-commands.jsonを正本とし、専用ロック1件を加える。scripts/update-shortcut-manifest.ps1でmanifestと一覧を生成し、-Checkで一致を確認する。キー定義の埋め込みやinput.sendKeys呼び出しは持たない。

残る固定コマンドのID・タイトル・説明・実行処理を保ち、dynamic-commandsのReplaceAsyncで設定更新後も同じ集合を登録する。manifestのautomation宣言に従って既存の共通操作APIへ公開し、コマンド実行・プロセス起動の権限を維持する。固定コマンドの生成から公開権限を推測しない。

## キー操作

Windowsキー・仮想デスクトップ・メディア・音量の定義は、AppDock本体のresources/configs/command-sets/windows.jsonが正本。コマンドセンター、パレット、キー・マウス・タッチパッド・トレイの共通候補へ表示する。新IDはcommandset.windows.<id>となり、旧at365.windows-tools.windows.<id>への別名や設定移行を追加しない。

JSONコマンド自体をMCP/Appletへ自動公開しない。外部からキー送信を実行する場合は本体の既存の公開可能なカスタムコマンドと操作許可を使う。WindowsToolsのロックはキー送信ではなく従来のLockWorkStationを使い、自動ロックの設定と独立させる。

## 検証

本体でキー定義の形式・全件の送信コンテキスト、ブラウザーの専用受信Windowへの実送信を検証する。WindowsToolsでは22件の登録と説明、旧キーIDの非登録、設定変更・取消・停止、MCP公開、実ウィンドウ操作を検証する。シャットダウン・実ロック・消灯とOSの予約キーは自動実行しない。
