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
        (await state(page))?.commands.length === 12 &&
        (await state(page)).state === "running",
      "Initial 12 commands missing",
    );
    assert.equal((await state(page)).state, "running");
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
    });
    await until(
      async () => (await state(page)).panel.facts[0].value.includes("1800秒"),
      "MCP setting did not reach runtime",
    );
    assert.equal(
      (await snapshot(page)).settings.value.extensions[id].settings
        .autoLockEnabled,
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
        (await state(page)).commands.length === 12,
      "Restart failed",
    );
    await checkWindowCommands();
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
      12,
      "Manifest command catalog retained while stopped",
    );
    assert.ok(
      stopped.commands.every((command) => command.available === false),
      "Stopped commands unavailable",
    );
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
      "PASS removed key settings and commands, retained obsolete settings/bindings without migration, 12 fixed commands, rejected stale command, countdown/cancel, restart, deactivate",
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
            "12 fixed commands and panel actions",
            "key UI and send commands removed; obsolete settings and bindings preserved without migration",
            "MCP display countdown / exclusions / permissions / dryRun / conflicts / validation / runtime",
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
