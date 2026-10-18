# 検証記録

## 2026-10-19: GitHub更新元変更のコミット確認

- 固定EXE GUIとpublish成功を再利用。標準回帰40/40を今回追加しすべて成功。
- 実装時の全追跡入力（検証文書を除く）・成功ログ・配布ファイル・共通GUI結果とスクリプト・本体treeをSHA256/IDで再照合し一致。製品追加修正・再発行なし。レビューで新たな指摘なし。保存済み設定と実利用先は保持。
- `.artifacts/commit-validation-github-account-20261019/`へ再利用根拠・追加試験のログ・stage/commit treeを記録。今回対象だけをコミットし、その後、本体と合わせて登録済みGitHub/AQUAへの通常pushと先端一致を確認する。過去/今回の再利用資料と未確認状態のテスト資料は保持し削除0件。Release・実利用先deployなし。

## 2026-10-19: GitHub更新元の新アカウント対応（0.2.2）

- 開始時main／clean、現在のorigin/mainからpull --ff-only成功（Already up to date）。更新元を`yukichi-7sai/Applet.WindowsTools.at365`へ統一。既存updateRepositoryの旧アカウント名を置換。.NET発行ビルドとruntime/manifestの版一致を確認。

- 発行前レビューは指摘なし。設定項目・保存ファイル・公開APIの追加削除なしのためパッチ更新。保存済みupdateSourceは変更・移行せず、既定復帰で新しいmanifestの更新元へ戻す。既存publish.batで自身の40.ARTIFACT配下へ発行成功。ZIP/feed/同梱manifestのID・版・size・SHA256・runtimeの存在が一致。

- 最終AppDock 0.62.4 EXE（SHA256 043d72acd4f4176d0417bbc4166a111cecc5aaa7e8740baf21b8d6647b0e85ab）と最終Applet一式を隔離コピー。全6Appletを停止した状態で、保存済み旧指定の保持、新しい既定値の説明、版表示、「既定に戻す」後の画面／メモリ／settings.jsonの一致、停止状態維持を各Appletで確認。正常終了0。

- 証跡: `.artifacts/github-account-20261019/validation.json`。共通GUI/画像: `../AppDock.at365/.artifacts/applet-github-account-20261019/profile-1792345747763/`。入力hashは前後一致、hostはb27bd046b704e1a27ad31fdc2f481f80ea086b31。変更と無関係な全回帰・実アカウント・実Google受信・実GitHub Release取得・各機能の実Windows効果は未実施。今回と過去の成功/失敗・再利用資料を保持し削除0件。

- 本体のGitHub参照修正は先にb27bd04へコミット済み。今回のApplet変更は未コミット。push・Release・実利用先deployなし。

## 2026-10-19: Windows標準コマンド統合のコミット確認（0.2.1）

- 利用者が普段使う追加コマンドの動作成功を確認し、コミットを依頼した。最新0.2.0の旧キー送信撤去・ジェスチャー修正を保持し、標準66件＋専用ロックを加えた79固定登録、設定／旧値／割り当ての非移行を再確認。未解決の製品指摘なし。
- 最終回帰 **40/40**、実ランナーの79登録／説明と66件の模擬入力API委譲、AppDock 0.62.3固定EXE＋最終DLLのGUI成功を、入力／完全ログ／配布物のhashと照合して再利用した。本体の標準回帰は再利用39件＋追加598件、合計 **637/637**。今回の追記は検証記録だけで、0.2.1と発行DLL／ZIPを維持し再発行しない。
- [コミット前照合](.artifacts/commit-validation-windows-shortcuts-20261019/)と[実装時証跡](.artifacts/windows-shortcuts-merge-20261019/validation.json)を参照。stage/tree・commit名義を照合して今回の実装をコミットする。統合前の保全資料、既存の成功・失敗・再利用資料は保持。push・Release・実利用先deployなし。

## 2026-10-19: 最新0.2.0への標準コマンド統合（0.2.1）

- リモートを取得し、mainを`09f90034ad49acc966b1d0fb370acc7fab103c5a`へfast-forwardしてから、今回の未コミット変更を統合した。変更前14ファイルの照合済みコピー・patch・stashを`.artifacts/windows-shortcuts-merge-20261019/`へ保全。以下の0.1.6としての標準コマンド実装記録を、この0.2.1で置き換える。履歴と失敗資料は保持する。
- 0.2.0の旧キー送信一覧・`send.<id>`・Shortcut/SendAsyncの撤去、旧設定と割り当ての非移行・非削除を維持。ジェスチャー対象のプロセス名／旧PID判定も現mainと同一。設定定義・既定値・セクションと既存12コマンドのID／タイトルを照合した。標準66件と専用ロックを加え、固定79件を登録する。自由登録32件を併用する仕様は採用しない。
- 標準コマンドは`context.AppDock.CallAsync`から共通`input.sendKeys`へ直接委譲し、旧プラットフォーム送信層を復活させない。専用ロックは既存LockWorkStationを使い、自動ロック設定を変更しない。説明とmanifestの生成・表示・検索を維持。発行前レビューと修正確認で製品の未解決指摘なし。
- 最終Release回帰 **40/40**、発行DLLの実ランナー接続が終了0。79登録と説明、66件の模擬ホストAPIへの単発委譲、旧設定の無視、設定更新・停止／再起動／EOF終了を確認した。実ランナー試験補強時の変数名衝突は試験側だけを修正し、最終回帰とprotocolを再実行した。本体 **0.62.3固定EXE＋0.2.1 DLL**の隔離GUIも成功（`host-1792342296861`）：説明表示／検索、79件、旧UI／send登録の非復活と旧値／割り当て保持、MCP設定、消灯予約の即時取消、再起動／停止と終了0。画面も目視した。
- 設定・保存仕様を追加削除しないため0.2.0から **0.2.1**へパッチ更新。最低本体0.62.3。通常publish終了0、DLL版0.2.1.0、ZIP **26682 bytes / SHA256 `709cd8f4bdff0c3dbbfb1eb437f1015224f37127b131b2576253cbdc14834e3b`**、DLL **52224 bytes / SHA256 `9aec75283d8c69ebbbbe739385111b323041c5894ced681d0be03325b969326b`**。feed・manifest・ZIP全3ファイルと配布内容、GUIコピーを照合した。本体ソースと発行EXEは維持し、先に成功した実入力6群・本体関連39件等を変更していない範囲で再利用する。
- [統合の検証証跡](.artifacts/windows-shortcuts-merge-20261019/validation.json)、[発行物照合](.artifacts/windows-shortcuts-merge-20261019/payload.json)、[固定EXE GUI](.artifacts/host-1792342296861/result.json)。658入力は最終試験前後で一致し、その後は検証記録のみ追記。標準コマンド全件のWindowsでの実効果や実ロックは一括実行していない。GUI成功2件と既存資料を保持し削除0件。新規commit・push・Release・実利用先deployなし。

## 2026-10-19: 共通入力の実機試験成功

- 利用者が通常の対話PowerShellからAppDock 0.62.3の固定EXEに対して実入力GUI試験を実行し、全6群が成功。保存された[実入力の結果](../AppDock.at365/.artifacts/native-input-ui-1792341550956/result.json)の`ok: true`と報告内容の一致を確認した。以下の過去の記録にある実入力GUIの確認待ちは解消した。
- キー送信と現在の配列での記号キー実到達、Applet/MCPの独立した入力許可と取消、対象変更拒否、修飾キー押下中の取消、連続ジェスチャー入力を確認。詳細は[本体の検証記録](../AppDock.at365/VERIFICATION.md#2026-10-19-対話端末での実入力試験成功0623)を参照。全66件のWindows機能の実効果や実ロックは一括実行していない。
- 試験profile内のEXEと発行EXEのhash一致、WindowsTools DLLのhash不変を確認。0.1.6／最低本体0.62.3と発行物は維持し、製品変更・再発行なし。過去の失敗資料も保持し、[検証証跡](.artifacts/windows-shortcuts-20261019/validation.json)を成功結果で更新した。

## 2026-10-19: コンソール復帰後の実入力確認

- コンソール復帰後の再試験でも実入力は未確認。実行プロセスとアクティブコンソールは同じSession 1、入力デスクトップもDefault、最前面はExplorerへ戻っていた。可視の専用試験画面からのAttachThreadInputはWin32エラー5、SetForegroundWindowもfalseで、RDPだけを原因とは扱わない。
- 製品と配布物は変更せず、本体側の試験画面へ前面化診断を追加。再試験・診断ログと失敗profileを保持した。[本体の追記](../AppDock.at365/VERIFICATION.md#2026-10-19-コンソール復帰後の実入力再試験)を参照。通常のPowerShellから同じ入力GUI試験を再確認する。0.1.6/0.62.3の版・既存の成功結果・発行物hashは維持し、再発行しない。

## 2026-10-19: Windows標準ショートカットと専用ロック（0.1.6）

- [設計書](docs/windows-shortcuts-design.md)に沿って、指定2節から重複と除外を整理した66件と「Windowsをロックする」を静的登録。既存12件・自由登録32件・設定のキー/既定値/保存先は保持。環境依存の項目は説明付きで収録し、Win+Ctrl+Shift+B・Win+L・Win+カンマは除外。既定のキー割り当てと外部実行の公開は追加していない。
- 発行前に本体/SDK/Runtimeを含む差分をレビュー。記号解決を送信直前の対象配列へ追従させ、不正な仮想キー0を拒否。説明省略時の従来RPC形式・manifest説明の保持を確認し、文書の無関係なTabs上限変更を修正した。未解決の製品コード指摘なし。発行後の変更は入力GUI試験のportable起動方法と記録のみで、製品の再発行は不要。
- 標準回帰 **41/41**、本体の関連回帰 **39/39**、本体型検査/buildが成功。キー66件の模擬送信、除外・重複、JIS/US記号変換、79固定＋32自由登録、変更・取消・停止、専用ロックの単発要求と自動ロック設定の非変更を確認。PCを実際にはロックしていない。
- 発行DLLの実ランナー試験は79固定＋32自由登録、動的置換、パネル、トレイなし、停止・再起動・EOF終了が成功。本体 **0.62.3の固定EXE**とDLLを隔離配置したGUIも成功。全静的ID/タイトル/説明、111件同時登録、説明の表示・説明によるパレット検索、自由登録の追加/名称変更/削除、既存ホットキー、MCP設定の権限・schema・dryRun・revision・非公開項目・実プロセス反映、予約取消・再起動・停止、終了0を確認。静的コマンドによるOS操作は一括実行していない。
- 実入力GUIは確認待ち。最初の既存Playwright起動はportable EXEへの接続でタイムアウトし、成功済みのCDP方式へテスト側を修正。その後は専用入力画面への前面移動で失敗した。GetForegroundWindowの所有プロセスを読み取り、**LockApp**が最前面であることを確認。画面ロックを保ち、保護を回避せず停止した。失敗ログと2つのprofileを保持。実機のキー入力・記号入力・権限取消の通し試験、および各Windows機能の実効果は成功扱いしない。解除後は本体の`dev.bat exec node scripts/native-input-ui-test.cjs A:\40.ARTIFACT\AppDock.at365\AppDock.at365.exe`で再確認できる。
- SDK/Runtimeの互換性として、未変更native WatchをRelease buildし、隔離RPCで起動・画像描画・設定反映・停止/EOF終了0を確認。本体固定EXEの標準smokeも成功。Watchのソース・版・40.ARTIFACT配布物は変更していない。
- 保存仕様と既存契約に破壊的変更がないため **0.1.5 → 0.1.6**、本体 **0.62.2 → 0.62.3**のパッチ更新。最低本体版0.62.3。両publish.bat終了0、manifest/DLL/feed/ZIP版・サイズ・SHA256を照合し、ZIP内3ファイルは配布内容と一致。DLL SHA256 `afefacc50d7f92bc4ff4dfde97f834af8d7773a0106b8c20776aeb45c416bcb4`、ZIP `8c228cbd0e0597cc0e5f9e8eaed1537bf3add7cd11255404c54f60e105d51a02`、本体EXE `ac641e1d243a9c54f3c0e910923d31359ba241a4a7c44f713f1de52b4d7a9939`。固定EXEのGUI/smokeコピーも同一hash。lockfile/toolchain不変。
- [検証証跡](.artifacts/windows-shortcuts-20261019/validation.json)、[固定EXE GUI](.artifacts/host-1792340281997/result.json)、[本体証跡](../AppDock.at365/.artifacts/windows-shortcuts-20261019/)にログ・入力hashを保存。検証時659入力を照合し、発行後の差分は入力試験スクリプトだけ。今回のhost GUI成功は1件、入力失敗2件は保持。smokeは直近成功3件と、セットアップ証跡から参照される旧成功1件・状態未確認1件を保持し、削除対象なし。全回帰・未変更全Appletの再発行、commit/push/Release/実利用先deployは行っていない。

## 2026-10-18: 旧キー送信撤去のコミット／プッシュ確認（0.2.0）

- 本体のカスタムコマンド拡張と合わせた依頼により、旧キー送信撤去と関連文書を対象にコミット／プッシュする。実装完了時の24入力に製品の追加変更なし。C#実装／試験と共通SDKの不変を照合し、回帰**35/35**、最終DLLのprotocol／native smokeを再利用した。回帰後のcsproj変更はAssemblyVersionを0.2.0へ更新したものだけ。
- 既存GUIは本体0.62.0の成功だったため、最終**AppDock 0.62.1**の固定EXEと同じ発行DLLでGUIだけ追加実行して成功（`host-1792322404244`、`1792322403941-7c04b152`）。12固定コマンド、旧UI／send登録の撤去、旧設定／割当の非移行、MCP公開設定、消灯予約の即時取消、再起動／停止と終了0を確認。本体側のhelper／生成schema／EXEも照合し、旧GUIの成功を別EXEへ流用していない。
- 0.2.0のZIP／DLL／feedは実装完了時のhashを保持し、再発行なし。実消灯・PCロック・実利用アプリ操作は行わず、利用中の設定を変更していない。実装時レビューと今回の差分確認に未解決指摘なし。
- 証跡は`.artifacts/commit-validation-custom-commands-20261018/validation.json`と本体側同名フォルダー。stage／作業内容／commit treeは`stage.json`・`commit.json`、GitHub／AQUA両mainの通常push後の先端は`push.json`へ記録する。今回の成功profileと既存の再利用資料を保持し、整理結果は`cleanup.json`へ記録。Release・実利用先deployは行わない。

## 2026-10-18: 旧キー送信機能の撤去（0.2.0）

- AppDockの本体カスタムコマンドにキー送信・プロセス起動・共通説明文を追加し、その必要検証が成功した後に旧キー送信機能を撤去。Shortcut／SendAsync／`send.<id>`生成／キー一覧設定・件数UI・専用試験・未使用appdock-api capabilityを削除した。固定12 ID、ウィンドウ操作、消灯予約／取消、自動ロックの設定・実装は維持する。旧設定の移行・清掃・割当の付替えは行わない。
- 発行前レビューと修正確認で未解決指摘なし。設定項目削除のため0.1.6→**0.2.0**。Release回帰**35/35**（`1792314158058-82b1f510`）、発行DLLのprotocol（`1792315435479-9c8b58df`）・通常前面native smoke（`1792315445913-7e2b2912`）が成功。左右・全高・中央・最大化／復元・最小化・取消可能な終了、開始HWND固定・名前／旧PID・不正対象の除外、前面とカーソル保持、停止／EOF終了を専用Windowで確認した。
- 最終AppDock **0.62.0固定EXE**と発行DLLを隔離配置したGUI試験が成功（`host-1792315476059`、`1792315475777-fb5b82cb`）。旧キー設定UI／send登録がなく、旧settings／キー割当が保持されて新定義へ変換されないこと、古いIDの実行拒否、12固定コマンド／パネル、MCP公開設定の許可・dryRun・競合・範囲外拒否、消灯予約の即時取消、再起動／停止と終了0を確認。最終画面を目視した。試験helperの参照名修正後に全項目を通した。
- 通常publish終了0。`A:\40.ARTIFACT\Applet.WindowsTools.at365\update.zip`は**16383 bytes / SHA256 `e0bc845a8cedef6ca6b17b13e7a2f74da148b1aa09f24a12feaeaac1651d72e9`**。DLLは**32256 bytes / SHA256 `1070becc6645ca4445762bcef722e2d10d8e2b06e68a10176622635292358a7f`**、AssemblyVersion/FileVersion **0.2.0.0**。manifest/feed・ZIP全3ファイルと配布フォルダーの一致、固定EXE/DLLコピー、deploy関連ファイル保持を確認した。
- 証跡は`.artifacts/custom-command-types-20261018/validation.json`と各runのログ・入力hash前後・`assembly.json`、本体側同名フォルダーのartifact/package/ZIP照合・review・cleanup。試験プロセス残存0。今回の成功profileと過去の再利用・失敗・利用状況不明を保持し、フォルダー削除0。実消灯／PCロック／実利用アプリ操作は行わず、利用中設定・認証を変更していない。本体の入力・プロセス起動の結果は[本体検証記録](../AppDock.at365/VERIFICATION.md)を参照。commit/push/Release/実利用deployは未実施。

## 2026-10-18: ジェスチャー修正のコミット確認（0.1.6）

- コミット依頼に合わせ、今回の6ファイルの差分と対象固定・旧PID互換・プロセス消失時の扱いを再確認し、未解決指摘なし。製品の追加変更はなく0.1.6と発行済みZIP/DLLを維持した。実装時の標準回帰**38/38**はソース・SDK・依存・完全ログ・生成物のSHA256を照合して再利用した。
- 本体のレイアウト追加修正を含む最終0.59.0 EXE（SHA256 `1f8fdf6b299ba6c15903bf3beeff6b318fe20d529e1f93d05c453c1dbebb4ee8`）で、発行DLLのprotocol/通常前面/ジェスチャー対象smokeと、実OSジェスチャー経由の**右移動・左移動・全高3件**を再実行して成功。ZIPは19648 bytes / SHA256 `2a4d0870f9ceb26b7a01ca5d2e6ced25627284b617d669a19aeffae73559ac3c`を保持し、manifest・feed・DLL版・deploy関連ファイルの一致も再確認した。
- 証跡は[本体側の検証まとめ](../AppDock.at365/.artifacts/commit-validation-gestures-20261018/validation.json)と、当repoの`.artifacts/commit-validation-gestures-20261018/validation.json`。stage/commit treeの一致、設定済みGitHub/AQUA両mainへの通常push後の先端を記録する。実利用アプリ・UAC・混在DPI・実利用先deployの確認は含まず、Releaseは行わない。

## 2026-10-18: ジェスチャー対象のプロセス名を受け付ける修正（0.1.6）

- AppDock.InputHostのinvocation.processは拡張子なしのプロセス名だったが、WindowsToolsは数値PIDだけを受け付けていた。開始HWNDの所有プロセス名を大文字小文字を区別せず照合するよう修正。従来の数値PID照合も維持し、不正・不一致・消滅した対象では別の前面Windowへフォールバックしない。通常ショートカットの対象取得・移動計算・DPI・取消は維持した。共通SDK/APIや最低ホスト版0.36.8は変更していない。
- 発行前に対象情報の契約、既存の通常実行、対象消失、設定/APIへの影響をレビューし未解決指摘なし。通常の不具合修正として0.1.6へパッチ更新。Release build/回帰 **38/38**、発行DLLのprotocol smoke、通常前面と名前形式のジェスチャー文脈を使うnative smokeが成功。全8ウィンドウ操作、元の対象固定、名前の大文字小文字、旧PID、不一致名/PID、破棄されたHWND、前面/カーソルの保持を確認した。消灯とPCロックは模擬APIのみ。
- 最終AppDock 0.59.0 EXEとこの発行DLLを隔離配置し、専用の使い捨てWinForms Windowへ実OSジェスチャーを送る**3件**が成功。右移動は`Left 100 → 2260`、左移動は`100 → 0`、全高は`Top 150/Bottom 550 → 0/2088`で幅600を保持した。試験Windowの重なりと操作後の安定待ちをfixtureで調整した。実利用アプリのWindowは操作していない。
- 発行終了0。`A:\40.ARTIFACT\Applet.WindowsTools.at365\update.zip`は**19648 bytes / SHA256 `2a4d0870f9ceb26b7a01ca5d2e6ced25627284b617d669a19aeffae73559ac3c`**。DLLのSHA256は`244bf8a683c3804bec760db5acb3d36fd17b5c810c7e5d28c1b2ea0c5b23c415`、AssemblyVersion 0.1.6.0。manifest/feedの版・最低ホスト版・ZIP全3ファイル・発行先deploy関連ファイル保持を照合。commit/push/Release/実利用deployなし。
- 証跡: [.artifacts/gesture-target-20261018/host-gesture-result.json](.artifacts/gesture-target-20261018/host-gesture-result.json)、同フォルダーの隔離fixtureとスクリプトhash。本体側の[検証まとめ](../AppDock.at365/.artifacts/gesture-behavior-20261018/validation.json)とwindows-regression/windows-protocol/windows-native/windows-gesture-native/published-window-completeのログ・入力hash前後を参照。未変更ソースの回帰結果は最終入力と照合して再利用。本体側の古い成功出力整理以外に当repoの出力は削除していない。手操作、他のアプリ固有制約、UAC、混在DPIは未確認。

## 2026-10-14: WindowMover統合のコミット前照合

- 実装時の入力37件、成功ログ6件、発行済みDLL/deps.json/manifestと更新ZIPのSHA256を現状と照合し、すべて一致。Releaseビルド、標準回帰38/38、発行済みDLLの接続・実ウィンドウ操作は元の成功結果を再利用した。
- GUI試験だけは、前回の入力記録に含まれていなかった生成済みsettings-schema.js、pnpmのlock/依存定義、固定Node実行ファイル等を補い、同じAppDock 0.36.8単一EXEとWindowsTools 0.1.5の発行物で再実行。入力46件が前後一致し、12コマンド/パネル、動的設定、MCP設定、消灯予約取消、再起動/停止、ホスト終了コード0を確認。試験コマンドも終了0。
- 最終差分を要件・既存コマンド/設定・対象固定/取消・寿命・検証範囲の観点で再レビューし、未解決指摘なし。製品の追加修正はなく、0.1.5と既存publishを維持。本体/SDK/他Appletの製品変更はないため、無関係な回帰は実行していない。WindowMover側は移行案内の文書差分だけをコミット対象とする。
- [コミット前の照合・stage/tree/commit証跡](.artifacts/commit-validation-window-integration-20261014/validation.json)と[今回のGUI結果](.artifacts/host-1791967143898/result.json)を保存。実装時の証跡も再利用元として保護する。GUI方式別の成功profileは単一EXE3件・win-unpacked3件・ソース1件で、今回の整理対象なし。失敗/不明/再利用資料を保持し、自試験プロセスの正常終了を確認。


## 2026-10-14: WindowMoverの8機能を統合（0.1.5）

- 左右移動・全高・中央・最小化・最大化・復元・通常終了を`at365.windows-tools.*`へ統合。元の移動計算・DWM枠補正・作業領域・対象除外・通常終了のキャンセルを維持。固定12コマンドと動的キー送信を同じ更新経路へ登録し、manifestにも固定コマンドを宣言。既存の設定キー・保存形式・入力APIは変更していない。旧AppletのDLL/ソース参照はなく、本体/SDK/APIの変更も不要。
- 発行前レビューで要件・既存動作・設定/保存/API・対象固定/取消・検証範囲を確認。旧GUI試験の4コマンド前提、保存ボタン、単一ページへの直接操作を現行の12コマンド・即時保存・Settings Shellへ修正。単一EXEはコピーのSHA256を照合してCDP接続する。停止時はmanifest候補を保持し全件available=falseになる契約と、隔離EXEの終了コード0を確認する期待値へ修正。最終差分を再確認し、未解決指摘なし。
- Releaseビルドは警告/エラー0、終了0。標準回帰は移植した23件、取消・不正ジェスチャー等3件、既存11件、登録維持/停止1件の**38/38**が成功。発行済みDLLの実ランナー試験で12コマンド、動的追加/削除、パネル、トレイ0件、停止、再起動、EOF終了を確認。
- 発行済みDLLの通常最前面経路で試験専用WinFormsウィンドウを実操作し、左右・全高・中央・最大化/復元・最小化・取消可能な終了/許可した終了、カーソル/前面保持を確認。ジェスチャーの開始HWND固定と、不正プロセスで別の前面へフォールバックしないことも確認。初期の前面取得タイムアウトと制限環境のビルド失敗はログを保持し、通常ユーザー権限の最終試験はすべて終了0。
- AppDock **0.36.8の固定単一EXE**とWindowsToolsの発行済みDLLを隔離配置したGUI試験が終了0。固定12コマンド/パネル、キー送信一覧の追加/名称変更/削除、ホットキー登録/解除、古いID拒否、MCP設定schema/非公開項目/許可/dryRun/revision/値検証と実プロセス反映、消灯予約の即時取消、再起動/停止、EXE正常終了を確認。実消灯・PCロックは行っていない。本体全回帰・未変更兄弟Applet回帰は実行していない。
- 既存のコマンド/設定/保存仕様を破壊しない機能追加なのでパッチ更新**0.1.4 → 0.1.5**。publish.batは終了0、個別ZIPと更新feed、manifest、DLL版を照合。ZIP全3ファイルがpublish内容と一致し、ZIP SHA256は`634fbb9f472881af2c24766b68f44c16f011250c4f380f2e02693b09677cdfc3`、DLLは`e8e0683a4fb8199aced4188be51b38bf1639b89a2714e7a3305e9082eeb42561`。本体EXEのhashは`eda1b3d4bb025f1f9ad94def40102ab1b372637b99e708895246414ae90d9010`で変更なし。ソリューションビルドはSDK Debug参照、publishはSDK Release参照でDLLのhashが異なるが、発行済みDLLそのものの接続/実操作/GUI試験を成功確認している。
- [検証証跡](.artifacts/window-integration-20261014/validation.json)、[発行物照合](.artifacts/window-integration-20261014/payload.json)、[最終GUI結果](.artifacts/host-1791966053510/result.json)を保存。製品/SDK/Runtime/ホストEXE等の入力37件を照合し、発行後の変更はGUI試験スクリプトだけ（最終版でGUIを再実行）。方式ごとの確認済み成功profileはwin-unpacked3件、ソース1件、単一EXE2件で、直近3件を超える削除対象なし。失敗・不明・既存再利用資料は保持し、試験の自プロセスは正常終了。commit/push/Release/実利用先deployは行っていない。


## 2026-10-14: 設定テンプレートの仕上げとコミット前検証

- 標準回帰 **11/11** が終了0。設定宣言と版以外の実行処理を変更していないこと、manifestの既存設定/コマンドを保持していることを再レビューし、未解決指摘なし。
- 最終AppDock 0.36.8の固定EXEと当Appletの発行物で、共通フォーム・全項目・即時保存・明暗テーマ・狭い幅を確認。本体標準回帰410件と、設定/検索/Shell/画面遷移/WebAppletの必要GUIが成功。実OS操作/実サービスを模擬試験の成功に含めない。
- 前回のpublishの版を維持し、個別ZIPと発行ファイルの全hash一致を再確認。本体だけが余白・グループ背景の修正により再発行されている。
- [今回の標準回帰・検証証跡](.artifacts/commit-validation-settings-template-20261014/validation.json)と[本体のGUI証跡](../AppDock.at365/.artifacts/commit-validation-applet-settings-20261014/validation.json)を保存。検証treeを照合後、VERIFICATIONのみを追記してコミットする。固定配布物と再利用資料を保持。push・Release・実利用先deployなし。


## 2026-10-14: 共通設定テンプレートへの統一（0.1.4）

- extension.jsonのsettingSectionsで用途別のカードを宣言し、最低本体版を0.36.8へ統一。既存設定のキー/型/既定値、操作・コマンドは旧HEADとJSON比較で同一。描画と保存は本体の共通テンプレートへ委譲し、実行処理や保存領域は変更していない。
- 発行前レビューで設定/操作の参照・重複・配置漏れ、設定互換性、版と最低本体版、共通ガイドへの参照を確認。未解決指摘なし。設定項目・保存ファイルの増減や破壊的変更がないためパッチ更新。
- publish.bat終了0。manifest/feed/配布フォルダーの版は0.1.4、minimumHostVersionは0.36.8。update.zipは14294 bytes、SHA256 e7ee22cde08baee52dd41a74d0f04360261b372a98aee57097aa68e082e678b3。ZIP全ファイルと配布フォルダーのhash一致を確認。[発行ログと照合](.artifacts/settings-template-20261014/)を保存。
- 固定AppDock 0.36.8と今回の発行物を隔離配置し、設定を停止状態で表示・保存できること、全項目と共通起動欄、明暗テーマ/1280・760幅を確認。本体GUI全4群・検索7群・画面遷移が成功。[共通GUI結果](../AppDock.at365/.artifacts/applet-settings-1791960532161/result.json)と[本体検証記録](../AppDock.at365/VERIFICATION.md)を参照。本体の標準回帰は410/410、fail/skip 0。
- 今回は設定宣言と版のみを変更したため、Applet固有のOS実操作・実サービス・全回帰は対象外。実壁紙・ロック・消灯・認証は操作していない。未変更Appletの発行、commit/push/Release/実利用先deployなし。GUI資料は本体側の隔離profileへ保持。

## 2026-10-11: 本体共通入力APIへ移行（0.1.3）

- WindowsPlatformのキー送信をinput.sendKeysへ委譲し、InputKeyChordをSDKへ共有。無操作判定・消灯/取消・フォルダー表示・動的ID/キー更新を維持。最低本体0.26.37、appdock-api capabilityと全Applet共通sendInput許可が必要。
- 通常回帰11/11とhost GUI（host-1791700566681）成功。最終発行DLLを本体最終win-unpackedへ入れ、専用WinFormsへのCtrl+A・許可OFF拒否・許可取消による未送信キャンセルを確認。旧NativeSend単独試験を本体共通入力の統合試験へ置換。
- publish/update.zip: 14122 bytes、SHA256 31e97f01ed98f1ecd815fb35bf0bdede3eb92962d646d896ddd392d3f5fea4f1。feed一致、DLL/manifest/update.zip/update.jsonをローカル発行。公開・実利用配置・コミットなし。
- 証跡は[本体validation](../AppDock.at365/.artifacts/native-api-20261011/validation.json)、最終入力native-input-ui-1791701515610。実OSロック/消灯は模擬APIのみで、今回実行していない。テスト残存なし、最新3回/過去参照/失敗不明資料を保持し整理対象なし。

## 2026-10-11: 公開設定拡充のコミット前確認

- stageしたtree `8b9132ec8adfba9bb0842531b3d152333d910a9d`の標準回帰と発行ホスト/実AppletのGUI・MCPを再実行して成功。壁紙はRPC/発行native EXEも終了0。検証前後の製品・テスト・発行物hash不変を確認し、再発行なし。
- 証跡は.artifacts/commit-validation-public-settings-20261011。完全ログ/終了コード/hash、対象tree/commit、兄弟ホストとツールの入力を記録。検証後の変更は本記録の追加のみで、最終treeを保存する。実サービス/実消灯・ロック/実デスクトップ壁紙は未操作。ローカルコミットまで。

## 2026-10-11: 0.1.2 単純な設定の外部公開

- displayOffSecondsだけを公開。自動ロックの有効状態/時間、送信キー一覧、実行コマンドは非公開のまま。待ち時間変更は次の予約へ反映し、既存予約は変更しない。runtime実装の変更なし。
- Release標準回帰11/11、publish終了0。発行DLL＋発行ホストでMCP schema/値/更新、範囲外/非公開項目を含む一括変更の拒否、書込許可・dryRun・revisionを確認。1800秒が実runtimeパネルへ反映し、自動ロック無効を保持。既存動的コマンドの追加/名称変更/削除・登録解除・停止/再起動も成功（host-1791667830091）。30分後の消灯予約は直後に取り消し、実消灯・ロック・実アプリへのキー送信は行わない。
- 必要本体版0.26.30。自身のpublish/update.zipは17113 bytes、SHA256 3cb4e21e2b26307e91aa03c3dec17719034aa16f90513754e6cb7508ea783573。manifest/feed/ZIP全3ファイルの内容一致を確認。未変更本体/Gmail等の再発行、依存更新、commit/push/Release/deployなし。
- 証跡は.artifacts/public-settings-20261011。GUI試験を現行のApplet詳細タブ/保存操作へ更新し、試験用ポート固定・起動完了待ち・意図した拒否ログ・画像の縦横比の期待値を修正。途中失敗ログは保持し、最終試験は終了0。認証・実メール・実OS更新は対象外。古い成功profileは方式別で3件以下、その他の失敗/不明/再利用資料は保持し削除対象なし。

## 2026-10-10: 開発生成物を`.artifacts`へ改名

- ユーザー指定でartifacts→.artifactsを改名。移動直後に既存502項目の相対パス/size/mtime/ディレクトリ・リンク属性が一致し、検証終了時も元の全項目のsize/mtime/属性が不変。配布物5ファイルのSHA256も検証前後で一致。保存済みログ/JSONは内部パスを含めて保持し、過去記録の当repoのartifacts/は.artifacts/へ読み替える。
- テスト/開発用の参照とGit除外/開発手順を更新。6repo合計の変更CJS18件の構文、Gmail start-dev.ps1の構文/UTF-8 BOM、各repoのgit diff --checkが成功。旧artifactsの再生成なし、新.artifactsのGit除外を確認。
- 既存.NET回帰11/11、隔離確認で開始/4コマンド登録/停止成功（.artifacts/rename-startstop-1791569717700/result.json）。実消灯/ロックは行っていない。
- ログは.artifacts/rename-20261010-regression.log。Gmail/Wallpaper/Watchの元の統合試験ログは.artifacts/rename-20261010-integration.log。残りの開始/停止確認の再現スクリプト/ログはA:/XX.TEMP/applets-artifacts-rename-startstop-20261010.cjsと同.log。確認スクリプトのsnapshot非同期取得/待機の途中失敗は修正し、最終は4件すべて終了0。棚卸し/最終照合はA:/XX.TEMP/applets-artifacts-rename-20261010-{before,after,final}.json。
- 製品実装は変更せず、manifest版0.1.1と既存publishを保持。再発行/commit/push/Release/実利用deployなし。同期・バックアップ設定はユキちゃんが担当。今回の成功した新規profileは各方式で直近3回以下、古い証跡は使用終了/再利用要否を一括確定していないため保持し削除0。

## 2026-10-09: 更新配布物の自動生成

- `codex/update-packages`で発行スクリプトだけを更新。Applet本体の版は0.1.1を維持し、`publish.bat`終了コード0。共通パッカーはAppDock 0.23.0のソースから発行。
- `publish/update.json`のID・版をmanifestと照合し、ZIPのサイズ17100bytesとSHA256 `a6370dad3b1af099e3e3bd3bf8f87d9390f6d9f74c66be854c01db72b12ecee8`を照合。ZIP内3ファイルすべてを通常発行フォルダーとバイト単位で比較し一致。収録: `Applet.WindowsTools.at365.deps.json`, `Applet.WindowsTools.at365.dll`, `extension.json`。
- 旧SDK/旧DLLの生成物が残るWatch・WindowMover・WindowsToolsでは、既存deployと一致する配布内容へ整理する処理を追加。任意のユーザーファイルの再帰削除は行わない。
- 共通検証結果はAppDockの`artifacts/applet-update-packages.json`、発行ログは`artifacts/Applet.WindowsTools.at365-update-publish.log`。実利用先deploy・外部公開・pushは未実施。実GitHub/HTTP(S)/UNC配布先の確認はユーザーが後で行う。Applet固有機能・実アカウント操作の再試験は今回の発行変更の対象外。

## 2026-10-08: 実利用先へのdeploy

- 配置後の実利用について、ユーザーが正常動作を確認したと報告（2026-10-08）。

- ユーザーの明示指示により、AppDockと全6Appletの`deploy.bat`を引数なしで実行し、7件すべて終了コード0。配置先は`A:\00.ESSENTIAL\00.MainTools\AppDock.at365`。5つの.NET Appletは現ソース/SDKで`publish.bat`を先に実行し、Gmailはdeploy内で再発行した。
- AppDock0.16.2、Gmail0.5.1、WallpaperSlideshow0.3.0、Watch0.1.1（native）、WebBrowserTools0.2.4、WindowMover0.2.1、WindowsTools0.1.1を配置。Watchの古いDLL版manifestを配置せず、現ソースのnative版へ更新。
- 配置対象21ファイルのSHA256はすべて発行元と一致。現ソースと配置manifestの版/runtime/entry、minimumHostVersionも照合。settings.json・avatar.png・Gmail accounts.jsonの3ファイルは配置前後のハッシュ不変。
- 配置前後とも関連プロセスなし。実利用アプリは起動していないため、次回起動で反映する。旧ファイル退避は行わず、設定・認証領域を配置スクリプトで変更していない。結果は`../AppDock.at365/artifacts/deploy-2026-10-08-result.json`（本体では`artifacts/deploy-2026-10-08-result.json`）。

## 2026-10-08: 依存パッケージ確認・現SDKへの回帰テスト追従

- 外部NuGet PackageReferenceなし。slnxの`dotnet list package --outdated`も更新なし。参照するAppDockのnpm更新詳細は[本体検証記録](../AppDock.at365/VERIFICATION.md)を参照。
- 現SDKのIUiService.GetImageDirectoryAsyncをテスト用FakeContextに追加（未使用のためNotSupportedException）。修正前のCS0535を解消し、Release build警告0/エラー0、新しいビルドから回帰11/11成功。製品コード・設定・版は変更なし。
- 実ディスプレイOFF・ロック・実アプリへのキー送信、publish/deployは今回実施していない。

2026-10-06 / Windows / .NET SDK 10.0.401

- WindowsTools Releaseビルド: 警告0件・エラー0件。発行成功。
- 回帰テスト: 11/11成功。キー構文、設定の不正値・重複ID、動的追加／変更／削除、消灯の期限・取消・停止、無操作時間・入力後の再監視・32bit時刻周回、自動ロックの有効化／無効化を確認。
- SendInput実動作: テスト専用TextBoxでCtrl+Aによる全選択、フォーカスとカーソル位置の維持を確認。テストのDPIをPerMonitorV2に固定して座標の仮想化を避けた。
- AppDock worktree: 型チェックとビルド成功、既存テストを含む33/33成功。
- Electron実UI + 発行DLL: 空のキー一覧で4コマンド登録、設定一覧からの追加・名前変更・削除、グローバルホットキー登録・解除、削除したコマンドの拒否を確認。
- 3600秒後の消灯を予約し、直後に取り消してパネル更新を確認。Applet再起動・無効化後のコマンド解除、ホスト終了を確認。テストプロファイルのみ使用。
- AppDock v0.4.0のポータブルEXE生成成功。展開済み配布EXE + 発行DLLでも同じUIテストに成功。
- ポータブルEXE自体は専用プロファイルの組み込みスモークテストでWindowsToolsの起動・パネル表示・終了コード0を確認。初回は設定ファイル置換時のEPERMで失敗し、同じプロファイルでの再実行が成功。ポータブルラッパーへのPlaywright直接接続はタイムアウトしたため、UIテストは展開済み配布EXEで実施した。

## 未検証の範囲

- 実PCをロックする操作、実ディスプレイの消灯は実行していない。期限計算と停止・取消は模擬APIで検証。
- 実エクスプローラーの起動、保護されたプロセスの実行ファイル取得、RDP、長時間常駐、スリープ復帰、管理者権限アプリへのキー送信は未検証。
- 実利用先への配置、設定変更、AppDock mainへのマージ、コミットは未実施。

## 2026-10-11: 共通入力APIのコミット前検証

今回の変更を実装時の記録と照合して固定し、本体276件・型検査、WindowsTools11件、WebBrowserTools2群、両Appletの配布版GUI、固定EXEの新旧SDK/API5群・MCP15群、共通入力4群、物理ジェスチャー17項目、設定UI7群、文書リンク177件を確認しました。製品/発行物の追加変更はありません。実入力試験では想定外Sキー/送信先変化と開始直後の3項目不一致があり、同一内容で再確認後成功。入力外乱の可能性がありますが個別原因は未特定、失敗ログを保持します。実ブラウザーの効果/実OSロック・消灯は対象外。

検証tree: `0c2fb2066e9a6981c023dda3ccf4fd4dce6e0f46`。完全な証跡は本体`.artifacts/commit-validation-native-api-20261011/`（このrepoにも写しを保存）。試験プロセス残存0、既存の最新3回・参照済み検証資料・失敗/不明資料を保持して削除なし。ローカルコミットのみでpushは行いません。

## 2026-10-16 発行先の40.ARTIFACTへの移行

- 発行先: `A:\40.ARTIFACT\Applet.WindowsTools.at365`。publish.batとビルド・更新梱包・関連検証の参照を変更。配置用正本はdeployment/へ移動し、成功後に発行先へ追加。deployは発行済み成果物を使い、既存deploy.bat・関連スクリプト・deploy.local.txtを保持する。配置先設定はハッシュ一致を確認して旧repo直下から移動。旧publishの過去成果物は保持。
- レビュー: 発行パス・削除境界・配置の独立動作・設定保持・梱包/Release参照を確認。GmailのWeb出力ガード、配置時の再ビルド依存、試験fixtureの分離、文書参照を修正して再確認。未解決指摘なし。発行基盤のみの変更のため製品版は0.1.5を維持。
- 検証: publish.batを2回実行し両方終了0（`.artifacts/publish-relocation-20261016/publish.log`、`.artifacts/publish-relocation-20261016/republish.log`）。7repo合計24配置用ファイルのハッシュ保持。全7repoの独立配置fixture28ケースとWebApplet専用6ケース、WebApplet単体2件、梱包/Release fixture11件、本体型検査・最終EXE隔離起動が成功。全feedの版・サイズ・SHA256、Applet ZIPの全収録ファイルと発行物を照合。
- 発行物SHA256: `025bb44c97923ab3c3f28492f11f06a54beca34fbcb8b425e7076b37359e4a97`。共通証跡: [検証一覧](../AppDock.at365/.artifacts/publish-relocation-20261016/validation.json)、[出力照合](../AppDock.at365/.artifacts/publish-relocation-20261016/artifact-validation.json)、[再発行保持](../AppDock.at365/.artifacts/publish-relocation-20261016/republish-result.json)。
- 制限環境のpnpm STATUS_STACK_OVERFLOWとEXE spawn失敗、fixtureのrename EPERMは通常ユーザー権限で同一処理を再実行して成功。依存版・lockfile・検証方針は維持。実利用先deploy・commit・push・外部公開は未実行。製品機能の変更がないため無関係な全回帰・実アカウント操作は実行していない。

## 2026-10-16 発行先移行のコミット前確認

- 実装時の入力スナップショット全7repoとログSHA256、最終発行EXE・各ZIP/feed・配置用24ファイルを照合し、型検査・ビルド・発行2回・配置・隔離起動・梱包Release fixture11件・WebApplet単体2件/配置6ケース・WebBrowserTools単体2件を再利用。製品と発行物の変更はないため再発行・版更新は不要。
- 不足分として本体標準回帰の残り487件、Gmail22件、Watch8件、WindowsTools38件、WallpaperSlideshow標準回帰を確認。本体486件は初回で成功、実行ディレクトリに依存するnode-invocationの1件は正しいrepoディレクトリから単独再実行して成功。既存11件と合わせ本体498件・skipなし。初回コマンドの終了1は保持し、全体成功へ読み替えず試験単位で照合。
- 通常ユーザー実行でfixtureビルドとネットワーク依存の.NET検証・Git fetchが成功。全7repoの入力と発行物を維持。失敗時の作業ディレクトリに生成された今回の試験資料は共通証跡内のinitial-cwd-artifactsへ移動して保持。
- 共通証跡: [コミット検証](../AppDock.at365/.artifacts/commit-validation-publish-relocation-20261016/validation.json)。stage treeとcommit tree、プッシュ先mainの一致を同フォルダーへ記録する。実利用先deployとGitHub Releaseは対象外。
