const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");

// Only cancel-display-off and a no-match process-window fixture are executed. Test bindings are disabled.
module.exports = async function checkCommands({
  call,
  client,
  page,
  id,
  catalog,
  fixedCommands,
  profile,
}) {
  for (const suffix of ['rdp-minimize', 'rdp-restore', 'rdp-toggle']) {
    const command = fixedCommands.find(c => c.id === id + '.' + suffix);
    assert(command, 'RDP command declared: ' + suffix);
  }
  for (const expected of fixedCommands) {
    const actual = catalog.find((c) => c.id === expected.id);
    assert(actual?.available, expected.id);
    assert.equal(actual.title, expected.title, expected.id);
    assert.equal(actual.description, expected.description, expected.id);
  }
  assert.equal(
    catalog.filter((c) => fixedCommands.some((e) => e.id === c.id)).length,
    fixedCommands.length,
  );
  const details = await call("appdock_get_applet", { id });
  const { type } = await call('appdock_get_custom_command_type', { typeId: id + '.process-window' });
  assert.equal(type.available, true);
  assert.equal(type.fields.find(f => f.key === 'action').options.length, 3);
  const { items } = await call('appdock_list_custom_commands', { ownerId: id });
  const processDefinition = items.find(item => item.typeId === type.id);
  assert(processDefinition?.available && processDefinition.automation);
  assert(catalog.some(command => command.id === processDefinition.commandId && command.available));
  const outcome = await call('appdock_execute_command', { id: processDefinition.commandId });
  assert.equal(outcome.completion, 'handlerReturned');
  assert.equal(outcome.effectVerified, false);
  for (const expected of fixedCommands)
    assert.equal(
      details.commands.find((c) => c.id === expected.id)?.description,
      expected.description,
    );

  const before = await call("appdock_get_shortcuts");
  for (const expected of fixedCommands) {
    const choice = before.assignableCommands.find((c) => c.id === expected.id);
    assert(choice?.available, expected.id);
    assert.equal(choice.title, expected.title);
    assert.equal(choice.ownerId, id);
  }
  const operations = fixedCommands.map((c, i) => ({
    kind: "add",
    binding: {
      id: "mcp-publication-" + i,
      command: c.id,
      key: "Ctrl+Alt+F24",
      enabled: false,
      when: { scope: "app", appletIds: [] },
    },
  }));
  const edit = (args) =>
    call("appdock_execute_command", { id: "appdock.shortcuts.update", args });
  assert.equal(
    (
      await edit({
        expectedRevision: before.revision,
        operations,
        dryRun: true,
      })
    ).completion,
    "validated",
  );
  assert.deepEqual(
    (await call("appdock_get_shortcuts")).bindings,
    before.bindings,
  );
  assert.equal(
    (await edit({ expectedRevision: before.revision, operations })).completion,
    "settingsSaved",
  );
  const saved = await call("appdock_get_shortcuts");
  for (const operation of operations) {
    const row = saved.bindings.find((b) => b.id === operation.binding.id);
    assert.equal(row?.command, operation.binding.command);
    assert.equal(row.enabled, false);
  }
  assert.equal(
    (
      await edit({
        expectedRevision: saved.revision,
        operations: operations.map((o) => ({
          kind: "remove",
          id: o.binding.id,
        })),
      })
    ).completion,
    "settingsSaved",
  );
  assert.deepEqual(
    (await call("appdock_get_shortcuts")).bindings,
    before.bindings,
  );

  const safeId = id + ".cancel-display-off";
  await page.evaluate(() =>
    window.dock.automation({ kind: "setExecution", allowed: false }),
  );
  const denied = await client.callTool({
    name: "appdock_execute_command",
    arguments: { id: safeId },
  });
  assert.equal(denied.isError, true);
  await page.evaluate(() =>
    window.dock.automation({ kind: "setExecution", allowed: true }),
  );
  const accepted = await call("appdock_execute_command", { id: safeId });
  assert.equal(accepted.completion, "handlerReturned");
  assert.equal(accepted.effectVerified, false);
  const unknown = await client.callTool({
    name: "appdock_execute_command",
    arguments: { id: id + ".send.shortcut-1" },
  });
  assert.equal(unknown.isError, true);
  fs.writeFileSync(
    path.join(profile, "commands-mcp.json"),
    JSON.stringify(
      {
        ok: true,
        fixedCommands: fixedCommands.length,
        checks: [
          "list and Applet details include all descriptions",
          "all fixed commands assignable",
          "all 95 disabled bindings validated, saved, read and removed",
          "existing bindings preserved",
          "execution permission denied and safe cancellation executed",
          "unknown command rejected",
        ],
        executedCommands: [safeId],
        powerCommandsExecuted: false,
      },
      null,
      2,
    ),
  );
};
