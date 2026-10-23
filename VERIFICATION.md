# 検証記録

対象の現行版は`0.2.5`。以下は記録した条件での検証結果であり、外部サービスや実機条件の現在の動作保証とは区別する。

## 2026-10-23: 固定91コマンドのMCP公開（0.2.5）

- main／clean／origin/main追跡を確認し、pull --ff-only成功（ff2adb4から更新なし）。全固定コマンドへmanifestのautomation=trueを追加し、再生成でも保持する。意味的差分は版と91件の公開フラグだけで、ID・タイトル・説明・設定の公開範囲・既定割当は不変。公開権限は既存manifestで管理し、MCP基盤・操作許可・実行処理は変更していない。
- 全件の内容を発行前レビュー。任意のコマンド文字列・任意キー・実行引数を受け取る機能はない。終了/ロック/消灯、前面対象の終了・移動、撮影/録画/マイク切替等の影響を確認し、要望どおり除外なし。古い非公開記述と設定revisionをまたぐ試験順序を修正し、阻害指摘なし。設定・保存ファイルの追加削除や破壊的変更がないため0.2.4→0.2.5へパッチ更新し、標準publish.batで発行した。
- .NET回帰 **42/42**、Windows PowerShell 5.1の生成チェック、manifestの意味的差分・BOM/CRLF検査、発行DLLの実ExtensionHost/模擬API試験が成功。73入力/5プロセスの固定要求と取消・停止・再起動を模擬検証。実シャットダウン・再起動・サインアウト・ロック、全キー操作の実効果は試していない。
- 同じ固定AppDock 0.85.9 EXEと最終Appletの隔離GUI **8群** が成功。全91件の公開と説明が設定変更・再起動・停止後も維持され、停止時は実行不可。MCPのcommands.list/applets.getで全件の説明、shortcuts.getで全件の割当候補を確認。全91件の無効な試験用割当についてdryRun・保存・取得・削除と既存割当の完全保持を確認した。直接実行は安全なcancel-display-offだけで成功/実行許可OFF時の拒否を検証し、未知の旧コマンドも拒否した。
- GUIの既存確認も成功。実Terminal起動後に新規ウィンドウだけを閉じ、Windowsの終了選択画面は表示後に取消。旧設定・割当保持、MCPの設定更新/入力拒否/書込許可/dryRun/競合、消灯予約取消と正常終了0を確認。証跡: `.artifacts/host-1792763179192/`（result.json、commands-mcp.json、terminal-window.json、shutdown-dialog.json等）。実利用設定やCodex登録は変更していない。
- 配布ZIPは **31,733 bytes**／SHA256 `ebf807edc3fc6e120f99fe7d69f7a8a145d1ef1168341c9eb6f7183d10744a8f`、DLL SHA256 `f75ad620ab37941b647e48e0bf8b16f9e9a1a6ceaec249ba43a5939471fac8c9`。source/配布/ZIPのmanifest、DLL/deps、feedの版・最低本体0.85.9・size/hashが一致。DLL版0.2.5.0。本体EXEのhashは前版検証時と同じ`39297303f8e28aa28bb7bb2899a0eb96934c224b76118f39d654ef0c1f9c0514`で再発行なし。
- 証跡: `.artifacts/mcp-publication-20261023/` のreview.json、各試験の完全ログ/終了コード、inputs-before/after.json、published.json、validation.json。入力hashは前後一致。制限内.NET/EXE起動失敗は保存し同一コマンドを通常ユーザーで再実行して成功。AQUA先端も通常ユーザーで読取り確認。必要回帰は今回実行済みで、未変更の本体/SDK全回帰や兄弟Applet試験は追加していない。作業中に本体repoへ現れたpackage.json/pnpm-lock.yamlの別変更は触らず、対象へ含めない。
- テスト専用フォルダーを確認し、直近成功3件・既存の検証/再利用資料・失敗/状態不明を保持して削除0件。コミット/プッシュの先端とstage/commit treeは同証跡に記録する。GitHub Releases公開・実利用先deployは行わない。利用先へ更新を配置した後、公開宣言の反映にAppDockの再起動が必要。

## 2026-10-23: 説明の全件追加・ターミナル実行エイリアス修正とコミット前照合（0.2.4）

- 前工程の未コミット変更を保持して開始。mainのAppDock 34c1f14とWindowsTools 67bb6f6を確認し、作業ツリーがdirtyのためpull/stashを行っていない。GitHubのmainは各HEADと一致、AQUAは本体34c1f14、Applet805d5a3を読み取り確認。実効author/committerとrepo-local/globalの設定は同じで変更なし。
- descriptionのなかった既存12コマンドへ対象・動作・制約の説明を追加。command-descriptions.jsonをDLLとmanifest生成で共用し、実行時/停止中の91件すべてでdescriptionを保持する。静的登録のID・動作・既存の説明は保持した。
- ターミナル起動の利用者報告を通常ユーザー環境で再現。実体はAppExecutionAlias（ReparsePoint、0 bytes）で、NodeのstatはEACCES、lstatはsymbolic linkとなった。固定PowerShellのStart-Process -FilePath 'wt.exe' -ErrorAction Stopへ変更し、既存processes.startと許可・取消を使う。本体の通常EXE検査は緩めない。最低本体は0.85.9を維持した。
- 発行前レビューで説明の共有・全件一致、terminal経路・許可・非再送・既存動作への影響を確認し阻害指摘なし。製品の追加修正のため0.2.3→0.2.4へパッチ更新して再発行。本体0.85.9の製品・EXEは変更/再発行していない。
- .NET回帰 **42/42**、Windows PowerShell 5.1の生成チェック、発行DLLの実ExtensionHost/模擬APIによる91件の登録・73入力/5プロセス委譲・設定更新/停止/再起動が成功。description全件と正本の一致も確認した。
- 最終Appletと同じhashの固定本体でGUI **7群** が成功。91件のdescriptionのmanifest/runtime一致と説明の表示を確認。正規のlaunch-terminalコマンドから実Terminalの新規ウィンドウが開き、その新規ウィンドウだけを閉じた。終了選択画面の表示・選択欄/OK/キャンセルと取消、旧設定/割当保持、MCP設定反映/拒否/dryRun/競合、消灯予約の取消・再起動/停止も成功。証跡: `.artifacts/host-1792761086335/`（terminal-window.json、shutdown-dialog.json、added-description.png、result.json）。実シャットダウン/再起動/サインアウトと実再生アプリ/スピーカー効果は実行していない。
- 発行update.zipは31,692 bytes／SHA256 `cf1f62471fc2533f53b02a82ff2ffca58f565a04d926d6981decba5276bd3459`、DLL SHA256 `e034d99c3eb7eaf9b181cc01cf31ac722d306e83f430b99bec14abb5f805f0f0`。ZIP/manifest/feedは0.2.4、最低本体0.85.9、91件・description欠落0が一致。
- 本体の標準109ファイル842件は、実装時の関連10件と、影響のない832件の成功を入力/依存グラフ/完全ログから再利用する。SDK parsingは今回の42件、固定本体の入力7群・型検査/build/smokeは製品入力とEXEのhash一致で再利用。本体 `.artifacts/commit-validation-media-keys-20261023/reuse.json` が照合の正本。古いAppletのGUI/protocol成功を今回の配布物へ読み替えない。
- 証跡: `.artifacts/commit-validation-system-commands-20261023/validation.json`、review.json、published.json、完全ログと入力hash。制限内の.NET/UNC Git失敗は保持し同じ処理を通常ユーザーで成功させた。不要な全回帰や本体再発行は行わず、再利用/失敗/状態不明資料を保持して削除0件。今回の対象差分・stage tree/commit treeと両送信先mainの一致を同ディレクトリへ記録する。GitHub Release公開・実利用先deployは行わない。

## 2026-10-23: ターミナル・電源・メディア・音量の12コマンド（0.2.3）

- main／clean／origin/main追跡を確認しpull --ff-only。805d5a3から67bb6f6へLICENSEのみfast-forward。本体は34c1f14から更新なし。新規12件を加えて固定91件。Windowsターミナル、シャットダウン、再起動、サインアウト、Windows標準の終了選択画面をprocesses.startへ、メディア4件・音量3件をinput.sendKeysへ委譲する。キー送信の最低本体版を0.85.9へ揃えた。設定・保存ファイル・既存ID・既定割り当て・外部公開・入力フックは追加変更していない。
- 発行前レビューで要件・呼出し先・許可・取消・説明・既存機能への影響を確認し阻害指摘なし。設定/保存ファイルの追加削除や破壊的変更がないため、0.2.2→0.2.3のパッチ更新。生成スクリプトの配列取込をWindows PowerShell 5.1でも正しく73件＋5件として扱う形へ修正し、BOM/CRLFとmanifest/一覧の生成一致を確認した。
- .NET回帰 **42/42**、本体の型検査・入力/プロセス関連JS **10/10** が成功。発行DLLを実ExtensionHostで読み込み、91件の登録と説明、73件の入力委譲と5件のプロセス委譲を模擬APIで確認。設定変更・停止・再起動・EOF終了も成功した。シャットダウン/s /t 0・再起動/r /t 0、サインアウト/l、/fなし、取消・停止後の非実行、API拒否時の非再送を模擬試験で確認した。
- 最終AppDock 0.85.9 EXEとAppletを隔離コピーしたGUIで、91件の表示/説明・外部非公開、旧設定/割当保持、MCP設定反映・権限/入力拒否・dryRun/競合、消灯予約の取消・Applet再起動/停止が成功。追加した終了画面コマンドを実行し、「Windows のシャットダウン」の選択欄・OK・キャンセルを確認。新しく現れた同名ダイアログだけをWM_CLOSEでキャンセルし、閉じたことを確認した。画像は `.artifacts/host-1792760191365/shutdown-dialog.png`。
- 最終固定EXEの入力試験 **7群** とsmoke成功。新しい7つのメディア/音量キーを、キーとWM_APPCOMMANDを処理する専用WinFormsへ実送信。前面変更拒否・許可撤回による未送信処理の取消・既存Node Applet/カスタムコマンド経路・記号配列・連続ジェスチャーの待機抑制維持も確認した。再生アプリ・実スピーカーの反応、実シャットダウン・再起動・サインアウト、ターミナルの実起動は未確認。
- 発行済みupdate.zipは30,275 bytes／SHA256 `6233e3a2d3647dfc43552417d7445376b6b828047ea8ad3b51f0e043007cd851`。feed、ZIP内manifest（0.2.3/minimumHostVersion 0.85.9/91件）、DLLとdepsが一致。DLL SHA256 `a7af7ad863738a0510a65498d1e7b1879aa3fd208bbb4d3d1efa83437114df86`。標準の40.ARTIFACTへ発行し、実利用先deploy・commit・push・Releaseは行っていない。
- 証跡: `.artifacts/system-commands-20261023/validation.json`、完全ログ・入力hash・review.json・published.json。固定GUIは `.artifacts/host-1792760191365/result.json`、共通入力は本体 `.artifacts/native-input-ui-1792760127527/result.json`。制限内の.NET/EXE spawn失敗はログを保持し、同じ処理を通常ユーザー実行して成功。旧GUIの一覧/タブ参照、既定falseの省略、WinFormsのMediaPreviousTrack表記、DPI付き画像取得は試験だけ修正して成功した。発行後の製品変更なし。既存/途中失敗と再利用資料を保持し、状態不明資料は削除せず、今回のテストフォルダー削除0件。

## 2026-10-19: GitHub更新元の新アカウント対応（0.2.2）

- 開始時main／clean、現在のorigin/mainからpull --ff-only成功（Already up to date）。更新元を`yukichi-7sai/Applet.WindowsTools.at365`へ統一。既存updateRepositoryの旧アカウント名を置換。.NET発行ビルドとruntime/manifestの版一致を確認。

- 発行前レビューは指摘なし。設定項目・保存ファイル・公開APIの追加削除なしのためパッチ更新。既存publish.batで自身の40.ARTIFACT配下へ発行成功。ZIP/feed/同梱manifestのID・版・size・SHA256・runtimeの存在が一致。

- 最終AppDock 0.62.4 EXE（SHA256 043d72acd4f4176d0417bbc4166a111cecc5aaa7e8740baf21b8d6647b0e85ab）と最終Applet一式を隔離コピー。全6Appletを停止した状態で、保存済み旧指定の保持、新しい既定値の説明、版表示、「既定に戻す」後の画面／メモリ／settings.jsonの一致、停止状態維持を各Appletで確認。正常終了0。

- 証跡: `.artifacts/github-account-20261019/validation.json`。共通GUI/画像: `../AppDock.at365/.artifacts/applet-github-account-20261019/profile-1792345747763/`。入力hashは前後一致、hostはb27bd046b704e1a27ad31fdc2f481f80ea086b31。変更と無関係な全回帰・実アカウント・実Google受信・実GitHub Release取得・各機能の実Windows効果は未実施。今回と過去の成功/失敗・再利用資料を保持し削除0件。

- 本体のGitHub参照修正は先にb27bd04へコミット済み。今回のApplet変更は未コミット。push・Release・実利用先deployなし。
