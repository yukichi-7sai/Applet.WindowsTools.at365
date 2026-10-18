# Applet.WindowsTools.at365 開発ガイド

## 発行・配置ファイルの保存先

標準発行先は `A:\40.ARTIFACT\Applet.WindowsTools.at365` です。`publish.bat`はソースrepo、`deploy.bat`は発行先から実行します。配置用ファイル・設定の保持とテンプレートの更新は[共通発行手順](../AppDock.at365/docs/publishing.md)を参照してください。

実装時のテスト選択、コミット前の必要回帰、リリース前のコミット/プッシュ確認と検証証跡の再利用は、[本体・Applet共通手順](../AppDock.at365/docs/development-workflow.md)に従います。この文書の試験コマンドは、その段階に応じて実行します。

利用方法は[README.md](README.md)、実測結果と未確認事項は[VERIFICATION.md](VERIFICATION.md)を参照してください。

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

AppDockを起動し直してAppletを有効にしてください。既定では自動ロックは無効、キー送信一覧は空です。実利用環境への配置・自動ロックの有効化はこの実装作業では行っていません。

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

実キー送信は本体のscripts/native-input-ui-test.cjsで、発行した.NET Appletから共通API経由で専用ウィンドウへ送信して確認します。従来の単独NativeSend試験はこの統合試験へ置き換えました。GUI/入力試験は直列で実行します。

[scripts/test-host.cjs](scripts/test-host.cjs) は専用プロファイルでAppDockを起動し、設定画面からの追加・変更・削除、動的ホットキーの登録解除、取消、再起動と停止を検証します。NodeとPlaywrightはAppDock側の依存を使用します。第1引数にホストソースのパス、第2引数に配布版EXEのパスを指定すると配布版でも検証できます。

実測結果は [VERIFICATION.md](VERIFICATION.md) に記載します。

## 実装上の補足

0.1.5でWindowMoverのWindowLayout/WindowMover/NativeWindowsをこのrepoへ移しています。旧AppletのDLLやソースへの参照は不要です。8つの固定ウィンドウコマンドを既存4コマンドとともに毎回ReplaceAsyncへ渡すため、キー送信設定の追加・削除・名称変更でも消えません。固定コマンドはmanifestにも宣言し、停止中も割り当て候補を表示します。外部実行の公開は追加していません。

Win32操作はApplet内で同期実行し、スレッドDPIコンテキストを同じスレッドで復元します。通常実行では最前面HWNDを1回取得し、ジェスチャーは既存の共通CommandExecution.Currentから開始HWND/プロセスを検証して固定します。取消を操作前と再配置前に検査します。WM_SYSCOMMANDによる閉じる/最小化/最大化/復元と移動計算は旧実装の動作を維持します。本体・SDK・共通操作APIは変更していません。

ジェスチャーのprocessは本体が渡す拡張子なしのプロセス名として、HWNDの所有プロセス名と大文字小文字を区別せず照合します。従来の数値PIDを渡す呼出しも照合して受け付けます。不正なHWND・一致しない名前/PID・消滅した対象では操作せず、別の前面ウィンドウへ切り替えません。NativeSmokeは実際の本体と同じ名前形式で左右移動・高さ調整等を検証します。

動的コマンド更新とshortcut-listを使います。無操作判定はGetLastInputInfoを維持します。0.1.3からキー送信はcontext.AppDock.CallAsyncのinput.sendKeysへ委譲し、キー解析はSDKのInputKeyChordを共有します。最低本体版0.26.37とappdock-api capabilityが必要です。許可・実行文脈・対象変更・取消・再入防止は[本体共通操作API](../AppDock.at365/docs/appdock-api.md)へ集約します。

## 文書の更新

READMEには動作環境・導入・操作・設定・利用上の制約を記載します。開発環境・ビルド・テスト・発行・開発者用配置・実装の説明はこのファイル、実測結果と未検証事項はVERIFICATION.mdへ記載します。共通方針は[AppDockのドキュメント方針](../AppDock.at365/docs/documentation.md)を参照してください。

## 更新配布物の発行

`publish.bat`は通常の発行先を生成した後、兄弟のAppDockリポジトリにある`scripts/pack-applet-update.ps1`で`A:/40.ARTIFACT/Applet.WindowsTools.at365/update.json`と`A:/40.ARTIFACT/Applet.WindowsTools.at365/update.zip`を自動生成します。共通パッカーのビルドに.NET 10 SDKが必要です。Gmail以外のAppletは、このパッケージ生成のためにNode.jsを導入する必要はありません。

ZIP直下に`extension.json`と実行ファイル一式を置き、JSONにID・版・必要な本体版・ZIPのサイズとSHA256を記録します。`OutputDirectory`を指定できる発行スクリプトでも、指定先の配布内容を読み、更新用JSON/ZIPの出力先は`A:\40.ARTIFACT\Applet.WindowsTools.at365`です。通常配置用サブフォルダーへJSON/ZIPを混ぜず、`deploy.bat`の配置対象も増やしません。

Web配布やGitHub Releaseには同じ発行で生成したJSONとZIPを一緒に置き、JSONを最後に公開してください。ソースコードの自動生成ZIPは使用しません。発行スクリプトから外部公開は行いません。[共通更新仕様](../AppDock.at365/docs/updates.md)と[配布先の確認手順](../AppDock.at365/docs/update-checklist.md)を参照してください。

## 開発生成物の保存先

開発・テストの生成物は`.artifacts`へ保存します。2026-10-10に旧`artifacts`を中身を保持して改名しました。過去の検証記録内の当repoの`artifacts/`は`.artifacts/`へ読み替えてください。保存済みログ/JSONの内部パスは実行当時の値として保持しています。作業完了時の整理は[AppDockの共通手順](../AppDock.at365/DEVELOPMENT.md#作業完了時のテストフォルダー整理)に従い、実行中・状態不明・未解決の失敗記録・再利用する資料を保持します。

## 公開設定

外部公開項目はextension.jsonのsettings[].automationで宣言します。取得とrevision付き更新は[共通操作API](../AppDock.at365/docs/automation.md)を使い、Applet固有のAPIや本体側の許可一覧は追加しません。booleanのON/OFF/toggleはgenerateCommandsで明示生成します。設定反映は既存Settings.OnChangedを共用します。

GUI試験にscripts/settings-mcp-check.cjsによる隔離MCP確認を含みます。公開schema・読取り・更新、非公開項目/不正値の拒否、書込許可、dryRun、古いrevisionの拒否と、各Applet実プロセスへの反映を確認します。実利用Codex設定やモデルは使用しません。

## 設定画面の構成

`extension.json`の`settingSections`で用途別のカードを宣言します。描画・余白・入力・検索・保存は[本体の共通テンプレート](../AppDock.at365/docs/extensions.md#共通の設定ページ)へ委譲し、Applet固有の設定フォームやCSSを複製しません。設定キー・既定値・実行処理を変えずに並び順を調整できます。最低本体版は0.36.8です。
