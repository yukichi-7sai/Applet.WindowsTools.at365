const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

// Only cancel-display-off is executed. Every saved test binding is disabled.
module.exports = async function checkCommands({ call, client, page, id, catalog, fixedCommands, profile }) {
  assert.equal(fixedCommands.length, 91);
  for (const expected of fixedCommands) {
    const actual = catalog.find(c => c.id === expected.id);
    assert(actual?.available, expected.id);
    assert.equal(actual.title, expected.title, expected.id);
    assert.equal(actual.description, expected.description, expected.id);
  }
  assert.equal(catalog.filter(c => fixedCommands.some(e => e.id === c.id)).length, 91);
  const details = await call('appdock_get_applet', { id });
  for (const expected of fixedCommands)
    assert.equal(details.commands.find(c => c.id === expected.id)?.description, expected.description);

  await page.evaluate(() => window.dock.automation({ kind: 'setShortcutEditing', allowed: true }));
  const before = await call('appdock_get_shortcuts');
  for (const expected of fixedCommands) {
    const choice = before.assignableCommands.find(c => c.id === expected.id);
    assert(choice?.available, expected.id);
    assert.equal(choice.title, expected.title);
    assert.equal(choice.ownerId, id);
  }
  const operations = fixedCommands.map((c, i) => ({
    kind: 'add',
    binding: {
      id: 'mcp-publication-' + i,
      command: c.id,
      key: 'Ctrl+Alt+F24',
      enabled: false,
      when: { scope: 'app', appletIds: [] },
    },
  }));
  const edit = args => call('appdock_execute_command', { id: 'appdock.shortcuts.update', args });
  assert.equal((await edit({ expectedRevision: before.revision, operations, dryRun: true })).completion, 'validated');
  assert.deepEqual((await call('appdock_get_shortcuts')).bindings, before.bindings);
  assert.equal((await edit({ expectedRevision: before.revision, operations })).completion, 'settingsSaved');
  const saved = await call('appdock_get_shortcuts');
  for (const operation of operations) {
    const row = saved.bindings.find(b => b.id === operation.binding.id);
    assert.equal(row?.command, operation.binding.command);
    assert.equal(row.enabled, false);
  }
  assert.equal((await edit({
    expectedRevision: saved.revision,
    operations: operations.map(o => ({ kind: 'remove', id: o.binding.id })),
  })).completion, 'settingsSaved');
  assert.deepEqual((await call('appdock_get_shortcuts')).bindings, before.bindings);

  const safeId = id + '.cancel-display-off';
  await page.evaluate(() => window.dock.automation({ kind: 'setExecution', allowed: false }));
  const denied = await client.callTool({ name: 'appdock_execute_command', arguments: { id: safeId } });
  assert.equal(denied.isError, true);
  await page.evaluate(() => window.dock.automation({ kind: 'setExecution', allowed: true }));
  const accepted = await call('appdock_execute_command', { id: safeId });
  assert.equal(accepted.completion, 'handlerReturned');
  assert.equal(accepted.effectVerified, false);
  const unknown = await client.callTool({
    name: 'appdock_execute_command', arguments: { id: id + '.send.shortcut-1' },
  });
  assert.equal(unknown.isError, true);
  fs.writeFileSync(path.join(profile, 'commands-mcp.json'), JSON.stringify({
    ok: true, fixedCommands: fixedCommands.length,
    checks: ['list and Applet details include all descriptions', 'all fixed commands assignable',
      'all 91 disabled bindings validated, saved, read and removed', 'existing bindings preserved',
      'execution permission denied and safe cancellation executed', 'unknown command rejected'],
    executedCommands: [safeId],
    powerCommandsExecuted: false,
  }, null, 2));
};
