# Applet.WindowsTools.at365 開発ガイド

機能の説明・操作方法・設定・既定値・動作環境は[機能・操作・設定](docs/usage.md)を参照してください。共通の文書構成は[ドキュメント方針](../AppDock.at365/docs/documentation.md)に従います。

## 発行・配置ファイルの保存先

標準発行先は `A:\40.ARTIFACT\Applet.WindowsTools.at365` です。`publish.bat`はソースrepo、`deploy.bat`は発行先から実行します。配置用ファイル・設定の保持とテンプレートの更新は[共通発行手順](../AppDock.at365/docs/publishing.md)を参照してください。

実装時のテスト選択、コミット前の必要回帰、リリース前のコミット/プッシュ確認と検証証跡の再利用は、[本体・Applet共通手順](../AppDock.at365/docs/development-workflow.md)に従います。この文書の試験コマンドは、その段階に応じて実行します。

インストールとビルドの概要は[README.md](README.md)、実測結果と未確認事項は[VERIFICATION.md](VERIFICATION.md)を参照してください。

## Windows標準コマンド

[設計書](docs/windows-shortcuts-design.md)と[コマンド一覧](docs/windows-shortcuts.md)を参照してください。キー送信定義の正本はwindows-shortcuts.json、ターミナル・電源関連5件の正本はwindows-process-commands.jsonです。ウィンドウ操作など13件の説明はcommand-descriptions.jsonから実行時登録とmanifestへ共用します。DLLへの埋め込みとmanifest/一覧の生成を共用します。静的73件は本体のinput.sendKeysへ直接委譲し、専用ロックは既存のLockWorkStation経路を使用します。固定コマンドは92件です。

定義を変更したらscripts/update-shortcut-manifest.ps1を実行し、-Checkで生成物を照合します。最低本体版は0.85.9です。

## ビルド・配置

.NET 10 SDKを使用します。隣接する AppDock.at365 のSDKを既定の参照先としています。

```powershell
dotnet build .\Applet.WindowsTools.at365.slnx -c Release
.\publish.bat
```

別の配置場所にあるAppDockのSDKを使う場合:

```powershell
.\publish.bat -AppDockRoot "A:\30.PROJECT\AppDock.at365"
# ソリューション全体のビルド時は -p:AppDockRoot=... を指定
```

発行先は `A:/40.ARTIFACT/Applet.WindowsTools.at365/Applet.WindowsTools.at365/` です。`-OutputDirectory` で変更できます。

動的コマンド対応版AppDockを終了し、そのEXEがあるフォルダーを指定して配置します。

```powershell
A:\40.ARTIFACT\Applet.WindowsTools.at365\deploy.bat "A:\Apps\AppDock.at365"
```

引数なしの場合は無視対象の `deploy.local.txt` の1行目を使います。[記入例](deployment/deploy.local.txt.example)も参照してください。どちらも未指定なら配置しません。`deploy.bat` は既定の発行先からDLL・deps.json・manifestを `extensions/Applet.WindowsTools.at365` へコピーします。AppDock.SDKはホストが供給します。

AppDockを起動し直してAppletを有効にしてください。既定では自動ロックは無効です。実利用環境への配置は発行と分けて行います。

## 検証

```powershell
dotnet run --project .\Applet.WindowsTools.RegressionTests -c Release
```

消灯・ロック・待ち時間のテストは模擬APIと仮想時刻を使用します。

発行済みDLLの登録・設定更新・停止と、専用WinFormsウィンドウの実移動を確認できます。

```powershell
dotnet run --project .\Applet.WindowsTools.RegressionTests -c Release -- --protocol-smoke "..\AppDock.at365\.artifacts\dotnet-host\AppDock.ExtensionHost.exe" "A:\40.ARTIFACT\Applet.WindowsTools.at365\Applet.WindowsTools.at365\Applet.WindowsTools.at365.dll"
dotnet run --project .\Applet.WindowsTools.RegressionTests -c Release -- --native-smoke "..\AppDock.at365\.artifacts\dotnet-host\AppDock.ExtensionHost.exe" "A:\40.ARTIFACT\Applet.WindowsTools.at365\Applet.WindowsTools.at365\Applet.WindowsTools.at365.dll"
```

実移動試験は試験専用ウィンドウだけを操作し、移動・高さ・中央・最大化/復元・最小化・取消可能な通常終了、ジェスチャーの対象固定と元の前面/カーソル保持を確認します。終了時は元のフォーカスへ戻します。GUI試験は直列で実行してください。

Windowsが試験画面へのフォーカス移動を許可しない環境では、`--gesture-native-smoke`で開始対象HWNDを明示する経路を検証できます。この成功を通常の最前面取得経路の成功へ読み替えません。

キー送信は本体のscripts/native-input-ui-test.cjsとshortcut-foreground-ui-test.cjsで、このAppletなしの本体種類と共通入力サービスを確認します。このrepoでは静的73件の共通APIへの委譲を模擬試験で確認します。GUI/入力試験は直列で実行します。

NodeとPlaywrightはAppDock側の依存を使用します。第1引数にホストソースのパス、第2引数に配布版EXEのパスを指定すると配布版でも検証できます。

実測結果は [VERIFICATION.md](VERIFICATION.md) に記載します。

## 実装上の補足

WindowLayout/WindowMover/NativeWindowsはこのrepoで所有します。9つの固定ウィンドウコマンドを他の固定コマンドとともに毎回ReplaceAsyncへ渡し、設定更新後も92件を維持します。固定コマンドはmanifestにも宣言し、停止中も割り当て候補を表示します。全92件をmanifestのcommands[].automation=trueで共通操作API（MCPを含む）へ公開します。公開権限はmanifestを正本とし、runtime登録から権限を追加しません。

Win32操作はApplet内で同期実行し、スレッドDPIコンテキストを同じスレッドで復元します。通常実行では最前面HWNDを1回取得し、ジェスチャーは既存の共通CommandExecution.Currentから開始HWND/プロセスを検証して固定します。取消を操作前と再配置前に検査します。WM_SYSCOMMANDによる閉じる/最小化/最大化/復元と移動計算は旧実装の動作を維持します。ウィンドウ操作の共通API契約は維持します。メディア・音量コマンドでは、本体とSDKの送信キー文法へ7キーを追加しています。

ジェスチャーのprocessは本体が渡す拡張子なしのプロセス名として、HWNDの所有プロセス名と大文字小文字を区別せず照合します。従来の数値PIDを渡す呼出しも照合して受け付けます。不正なHWND・一致しない名前/PID・消滅した対象では操作せず、別の前面ウィンドウへ切り替えません。NativeSmokeは実際の本体と同じ名前形式で左右移動・高さ調整等を検証します。

dynamic-commands capabilityは固定92件のReplaceAsyncに使用します。無操作判定はGetLastInputInfo、ジェスチャーの対象は共通CommandExecutionを維持します。

## 公開設定

固定コマンドのID・タイトル・説明はcommands.list/applets.getで取得でき、引数なしコマンドとしてショートカット等の割り当て候補になります。MCPからの直接実行も可能で、呼び出し元のコマンド実行許可、キー送信・プロセス起動ではMCPとApplet双方の該当許可を維持します。ID・実行処理・既定割り当て・設定の公開範囲は変更しません。公開宣言を変更したAppletの配置後はAppDockを再起動してください。基盤の公開/実行の分離やApplet単位の公開設定は今回追加していません。

外部公開項目はextension.jsonのsettings[].automationで宣言します。取得とrevision付き更新は[共通操作API](../AppDock.at365/docs/automation.md)を使い、Applet固有のAPIや本体側の許可一覧は追加しません。booleanのON/OFF/toggleはgenerateCommandsで明示生成します。設定反映は既存Settings.OnChangedを共用します。

GUI試験にscripts/settings-mcp-check.cjsによる隔離MCP確認を含みます。公開schema・読取り・更新、非公開項目/不正値の拒否、書込許可、dryRun、古いrevisionの拒否と、各Applet実プロセスへの反映を確認します。実利用Codex設定やモデルは使用しません。

scripts/commands-mcp-check.cjsで全92件の公開・説明・割当候補を確認し、無効な試験用割当のdryRun/保存/取得/削除、既存割当保持と安全な消灯予約取消のMCP実行・実行許可の拒否を確認します。シャットダウン・再起動・サインアウト・ロックや撮影・録画等はこの試験で実行しません。

## 設定画面の構成

`extension.json`の`settingSections`で用途別のカードを宣言します。描画・余白・入力・検索・保存は[本体の共通テンプレート](../AppDock.at365/docs/extensions.md#共通の設定ページ)へ委譲し、Applet固有の設定フォームやCSSを複製しません。設定キー・既定値・実行処理を変えずに並び順を調整できます。最低本体版は0.85.9です。

## 共通の運用

文書構成・生成物・版更新・配布は[共通手順](../AppDock.at365/docs/development-workflow.md)、[文書方針](../AppDock.at365/docs/documentation.md)、[発行と配置](../AppDock.at365/docs/publishing.md)、[更新パッケージ](../AppDock.at365/docs/updates.md)、[生成物の整理](../AppDock.at365/DEVELOPMENT.md#作業完了時のテストフォルダー整理)を参照する。Applet固有の実行条件と結果は[検証記録](VERIFICATION.md)へ記載する。
