const {
  getPublishDirectory,
} = require("../../AppDock.at365/scripts/publish-paths.cjs");
const fs = require("node:fs");
const path = require("node:path");
const assert = require("node:assert/strict");
const { spawn } = require("node:child_process");
const { createHash } = require("node:crypto");
const root = path.resolve(__dirname, "..");
const host = path.resolve(
  process.argv[2] || path.join(root, "../AppDock.at365"),
);
const packaged = process.argv[3] ? path.resolve(process.argv[3]) : null;
const { chromium } = require(path.join(host, "node_modules/playwright"));
const { freePort } = require(
  path.join(host, "scripts/electron-main-helper.cjs"),
);
const { createDefaultSettings } = require(
  path.join(host, "out/main/shared/settings-schema.js"),
);
const { settleSettings } = require(
  path.join(host, "scripts/settings-immediate-helper.cjs"),
);
const { settingsPage } = require(
  path.join(host, "scripts/settings-host-helper.cjs"),
);
const profile = path.join(root, ".artifacts", `host-${Date.now()}`);
const folder = path.join(profile, "extensions/Applet.WindowsTools.at365");
fs.mkdirSync(folder, { recursive: true });
for (const name of [
  "extension.json",
  "Applet.WindowsTools.at365.dll",
  "Applet.WindowsTools.at365.deps.json",
])
  fs.copyFileSync(
    path.join(getPublishDirectory(root), "Applet.WindowsTools.at365", name),
    path.join(folder, name),
  );
const id = "at365.windows-tools";
const catalog = JSON.parse(fs.readFileSync(path.join(root, 'windows-shortcuts.json'), 'utf8'));
const processCatalog = JSON.parse(fs.readFileSync(path.join(root, 'windows-process-commands.json'), 'utf8'));
const manifestCommands = JSON.parse(fs.readFileSync(path.join(root, 'extension.json'), 'utf8')).commands;
const fixedCount = 13 + catalog.length + processCatalog.length;
const { overlayPage, closingAction } = require(path.join(host, 'scripts/shell-overlay-helper.cjs'));
const dynamicId = id + ".send.shortcut-1";
const windowIds = [
  "left",
  "right",
  "full-height",
  "center",
  "minimize",
  "maximize",
  "restore",
  "close",
].map((s) => id + "." + s);
const settings = createDefaultSettings();
settings.host.notifications = false;
const oldKeys = [
  { id: "shortcut-1", title: "Obsolete saved key", keys: "Ctrl+A" },
];
settings.extensions[id] = {
  enabled: true,
  settings: {
    displayOffSeconds: 3600,
    autoLockEnabled: false,
    shortcuts: oldKeys,
  },
};
settings.keybindings = [
  {
    id: "test-dynamic-key",
    command: dynamicId,
    key: "Ctrl+Alt+F19",
    enabled: true,
    when: { scope: "global", appletIds: [] },
  },
];
fs.writeFileSync(path.join(profile, "settings.json"), JSON.stringify(settings));
let child,
  browser,
  page,
  processOutput = "",
  success = false;
const hash = (file) =>
  createHash("sha256").update(fs.readFileSync(file)).digest("hex");
async function snapshot(page) {
  return page.evaluate(() => window.dock.snapshot());
}
async function until(check, message) {
  const end = Date.now() + 15000;
  while (Date.now() < end) {
    if (await check()) return;
    await new Promise((r) => setTimeout(r, 80));
  }
  throw new Error(message);
}
async function state(page) {
  return (await snapshot(page)).extensions.find((e) => e.id === id);
}
(async () => {
  try {
    const port = await freePort(),
      env = { ...process.env };
    delete env.ELECTRON_RUN_AS_NODE;
    const executable = packaged
      ? path.join(profile, "AppDock.at365.exe")
      : require(path.join(host, "node_modules/electron"));
    if (packaged) {
      fs.copyFileSync(packaged, executable);
      assert.equal(hash(executable), hash(packaged), "Fixed host EXE copy");
    }
    child = spawn(
      executable,
      [
        ...(packaged ? [] : [host]),
        `--test-profile=${profile}`,
        `--test-local-state=${path.join(profile, "local")}`,
        `--remote-debugging-port=${port}`,
      ],
      { cwd: host, env, windowsHide: true, stdio: "pipe" },
    );
    child.stdout.on("data", (data) => (processOutput += data));
    child.stderr.on("data", (data) => (processOutput += data));
    await until(async () => {
      browser = await chromium
        .connectOverCDP(`http://127.0.0.1:${port}`, { timeout: 1000 })
        .catch(() => null);
      return browser;
    }, "Host CDP connection failed");
    await until(() => {
      page = browser
        .contexts()
        .flatMap((c) => c.pages())
        .find((p) => p.url().startsWith("appdock://host/"));
      return page;
    }, "Host page missing");
    await until(
      async () =>
        (await state(page))?.commands.length === fixedCount &&
        (await state(page)).state === "running",
      "Initial fixed commands missing",
    );
    assert.equal((await state(page)).state, "running");
    const checkStatic = async () => {
      const current = await state(page);
      for (const entry of manifestCommands) {
        const command = current.commands.find(c => c.id === entry.id);
        assert.ok(entry.description?.trim(), entry.id);
        assert.equal(command?.description, entry.description, entry.id);
        assert.equal(command.automation, true, entry.id);
      }
      for (const entry of catalog) {
        const id = 'at365.windows-tools.windows.' + entry.id;
        const command = current.commands.find(c => c.id === id);
        assert.equal(command?.title, entry.title, id);
        assert.equal(command.description, `送信キー: ${entry.keys}。${entry.description}`, id);
        assert.equal(command.automation, true);
      }
      assert.ok(current.commands.some(c => c.id === 'at365.windows-tools.lock-windows'));
      for (const entry of processCatalog) {
        const command = current.commands.find(c => c.id === 'at365.windows-tools.' + entry.id);
        assert.equal(command?.title, entry.title);
        assert.equal(command.description, entry.description);
        assert.equal(command.automation, true);
      }
    };
    await checkStatic();
    await page.evaluate(() => window.dock.setAppletApiPermissions({}));
    await page.evaluate(() => window.dock.setAppletApiPermissions({ read: true, execute: true, runProcesses: true }));
    const terminalProbe = spawn('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', path.join(root, 'scripts/check-shutdown-dialog.ps1'), '-OutputDirectory', profile, '-Kind', 'terminal'], { windowsHide: true, stdio: 'pipe' });
    let terminalOutput = '';
    terminalProbe.stdout.on('data', data => { terminalOutput += data; });
    terminalProbe.stderr.on('data', data => { terminalOutput += data; });
    await until(() => fs.existsSync(path.join(profile, 'terminal-probe.ready')), 'Terminal probe ready');
    await page.evaluate(() => window.dock.executeCommand('at365.windows-tools.launch-terminal'));
    await until(() => terminalProbe.exitCode !== null, 'Terminal probe completed');
    assert.equal(terminalProbe.exitCode, 0, terminalOutput);
    assert.equal(JSON.parse(fs.readFileSync(path.join(profile, 'terminal-window.json'), 'utf8').replace(/^\uFEFF/, '')).cancelled, true);
    const probe = spawn('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', path.join(root, 'scripts/check-shutdown-dialog.ps1'), '-OutputDirectory', profile], { windowsHide: true, stdio: 'pipe' });
    let probeOutput = '';
    probe.stdout.on('data', data => { probeOutput += data; });
    probe.stderr.on('data', data => { probeOutput += data; });
    await until(() => fs.existsSync(path.join(profile, 'dialog-probe.ready')), 'Shutdown dialog probe ready');
    await page.evaluate(() => window.dock.executeCommand('at365.windows-tools.show-shutdown-dialog'));
    await until(() => probe.exitCode !== null, 'Shutdown dialog probe completed');
    assert.equal(probe.exitCode, 0, probeOutput);
    assert.equal(JSON.parse(fs.readFileSync(path.join(profile, 'shutdown-dialog.json'), 'utf8').replace(/^\uFEFF/, '')).cancelled, true);
    await page.evaluate(() => window.dock.setAppletApiPermissions({}));
    await page.evaluate(() => window.dock.executeCommand('appdock.settings.open'));
    await page.locator('.settings-categories').getByRole('button', { name: 'ショートカットキー', exact: true }).click();
    const folderRow = page.locator('[data-shortcut-command="at365.windows-tools.open-folder"]');
    await folderRow.getByText(manifestCommands.find(c => c.id === 'at365.windows-tools.open-folder').description, { exact: true }).waitFor();
    await folderRow.scrollIntoViewIfNeeded();
    await page.screenshot({ path: path.join(profile, 'added-description.png') });
    const recallRow = page.locator('[data-shortcut-command="at365.windows-tools.windows.recall"]');
    await recallRow.getByText(/最小システム要件/).waitFor();
    await recallRow.scrollIntoViewIfNeeded();
    await page.screenshot({ path: path.join(profile, 'static-description.png') });
    await page.evaluate(() => window.dock.executeCommand('appdock.commands.search'));
    const overlay = await overlayPage(page);
    await overlay.getByRole('combobox').fill('最小システム要件');
    await overlay.locator('[data-command-id="at365.windows-tools.windows.recall"]').getByText(/最小システム要件/).waitFor();
    await overlay.screenshot({ path: path.join(profile, 'description-search.png') });
    await closingAction(overlay.keyboard.press('Escape'));
    const checkWindowCommands = async () => {
      const current = await state(page);
      for (const command of windowIds) {
        assert.ok(
          current.commands.some((c) => c.id === command),
          "Missing window command: " + command,
        );
        assert.ok(
          current.panel.actions.some((a) => a.command === command),
          "Missing panel action: " + command,
        );
      }
    };
    await checkWindowCommands();
    await page.evaluate(() =>
      window.dock.executeCommand("appdock.settings.open"),
    );
    const ui = await settingsPage(page);
    await ui
      .getByRole("button", { name: "WindowsTools.at365", exact: false })
      .first()
      .click();
    assert.equal(
      await ui
        .getByRole("button", { name: "コマンドを追加", exact: true })
        .count(),
      0,
    );
    assert.equal(
      await ui.getByText("ショートカットキーの送信", { exact: true }).count(),
      0,
    );
    assert.equal(
      await ui.getByText("キー送信コマンド", { exact: true }).count(),
      0,
    );
    assert(
      !(await state(page)).commands.some((c) => c.id.startsWith(id + ".send.")),
    );
    assert.equal(
      (await snapshot(page)).settings.value.keybindings[0].command,
      dynamicId,
    );
    assert.equal(
      (await snapshot(page)).settings.value.customCommands,
      undefined,
    );
    assert.deepEqual(
      (await snapshot(page)).settings.value.extensions[id].settings.shortcuts,
      oldKeys,
    );
    await ui.screenshot({
      path: path.join(profile, "settings-without-key-sending.png"),
      fullPage: true,
    });
    await checkWindowCommands();
    await until(
      async () =>
        !(await snapshot(page)).globalHotKeys.some(
          (h) => h.commandId === dynamicId && h.registered,
        ),
      "Obsolete hotkey must remain unregistered",
    );
    await assert.rejects(
      page.evaluate(
        (command) => window.dock.executeCommand(command),
        dynamicId,
      ),
      /利用できません/,
    );
    await require("./settings-mcp-check.cjs")({
      host,
      profile,
      page,
      id,
      keys: ["displayOffSeconds"],
      changes: { displayOffSeconds: 1800 },
      invalid: [
        { displayOffSeconds: 0 },
        { displayOffSeconds: 3601 },
        { displayOffSeconds: 1200, autoLockEnabled: true },
        { autoLockMinutes: 1 },
        { shortcuts: [] },
      ],
      hidden: ["autoLockEnabled", "autoLockMinutes", "shortcuts"],
      fixedCommands: manifestCommands,
    });
    await until(
      async () => (await state(page)).panel.facts[0].value.includes("1800秒"),
      "MCP setting did not reach runtime",
    );
    assert.equal(
      (await snapshot(page)).settings.value.extensions[id].settings
        .autoLockEnabled ?? false,
      false,
    );
    assert.deepEqual(
      (await snapshot(page)).settings.value.extensions[id].settings.shortcuts,
      oldKeys,
    );
    // The half-hour reservation is canceled immediately; the display is never actually switched off.
    await page.evaluate(
      (command) => window.dock.executeCommand(command),
      id + ".display-off",
    );
    assert.match((await state(page)).panel.facts[0].value, /予約中/);
    await page.evaluate(
      (command) => window.dock.executeCommand(command),
      id + ".cancel-display-off",
    );
    assert.match((await state(page)).panel.facts[0].value, /待機中/);
    await page.evaluate(
      (extension) => window.dock.restartExtension(extension),
      id,
    );
    await until(
      async () =>
        (await state(page)).state === "running" &&
        (await state(page)).commands.length === fixedCount,
      "Restart failed",
    );
    await checkWindowCommands();
    await checkStatic();
    await page.evaluate(
      (extension) => window.dock.toggleExtension(extension, false),
      id,
    );
    await until(
      async () => (await state(page)).state === "stopped",
      "Deactivate failed",
    );
    const stopped = await state(page);
    assert.equal(
      stopped.commands.length,
      fixedCount,
      "Manifest command catalog retained while stopped",
    );
    assert.ok(
      stopped.commands.every((command) => command.available === false),
      "Stopped commands unavailable",
    );
    assert.ok(stopped.commands.every((command) => command.automation === true));
    const errors = (await snapshot(page)).logs.filter(
      (entry) => entry.level === "error",
    );
    assert.equal(
      errors.filter(
        (e) =>
          e.source === "host" &&
          e.message ===
            "dock:executeCommand: このコマンドは現在利用できません。",
      ).length,
      1,
      "stale command rejection logged once",
    );
    assert.deepEqual(
      errors.filter(
        (e) =>
          !(
            e.source === "host" &&
            e.message ===
              "dock:executeCommand: このコマンドは現在利用できません。"
          ),
      ),
      [],
    );
    success = true;
    console.log(
      "PASS removed key settings and commands, retained obsolete settings/bindings without migration, 91 fixed commands with static descriptions, rejected stale command, countdown/cancel, restart, deactivate",
    );
    console.log(profile);
  } finally {
    if (page)
      await page
        .evaluate(() => window.dock.executeCommand("appdock.quit"))
        .catch(() => {});
    if (child) {
      try {
        await until(() => child.exitCode !== null, "Host shutdown failed");
        assert.equal(child.exitCode, 0, "Isolated host exit code");
      } catch (error) {
        child.kill();
        success = false;
        process.exitCode = 1;
        console.error(error);
      }
    }
    if (browser) await browser.close().catch(() => {});
    fs.writeFileSync(path.join(profile, "process.log"), processOutput);
    fs.writeFileSync(
      path.join(profile, "result.json"),
      JSON.stringify(
        {
          ok: success,
          packaged,
          hostExitCode: child?.exitCode ?? null,
          hostHash: packaged ? hash(packaged) : null,
          appletHash: hash(path.join(folder, "Applet.WindowsTools.at365.dll")),
          checks: [
            "91 fixed commands with static descriptions and panel actions",
            "all 91 fixed commands remain public after dynamic replacement, restart and stop",
            "native Windows shutdown dialog has a choice, OK and Cancel and closes by cancellation without shutdown",
            "Windows Terminal opens through the exact launch command; only the new terminal window is closed",
            "key UI and send commands removed; obsolete settings and bindings preserved without migration",
            "MCP display countdown / exclusions / permissions / dryRun / conflicts / validation / runtime",
            "MCP command descriptions / all 91 shortcut assignments dryRun and persistence / safe execution and execution denial",
            "countdown cancel / restart / stop",
          ],
        },
        null,
        2,
      ),
    );
  }
})().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
