const assert = require('node:assert/strict');
const path = require('node:path');

// The saved process name intentionally cannot match a real application.
module.exports = async function checkProcessWindow({ page, ui, profile, until }) {
  const title = 'Process-window UI fixture';
  const processName = 'appdock-missing-' + require('node:crypto').randomUUID();
  const definitions = async () =>
    (await page.evaluate(() => window.dock.snapshot())).settings.value.customCommands?.items ?? [];
  await ui.getByRole('button', { name: 'カスタムコマンド', exact: true }).click();
  await ui.getByRole('button', { name: 'コマンドを追加', exact: true }).click();
  const dialog = ui.getByTestId('settings-dialog');
  await dialog.getByTestId('custom-command-kind').selectOption('at365.windows-tools.process-window');
  await dialog.getByLabel('名前', { exact: true }).fill(title);
  await dialog.getByRole('checkbox', { name: /公開/ }).check();
  if (await dialog.getByTestId('custom-command-section:action').isVisible())
    await dialog.getByTestId('custom-command-section:action').click();
  assert.equal(await dialog.getByLabel('操作', { exact: true }).inputValue(), 'toggle');
  await dialog.getByTestId('settings-dialog-confirm').click();
  await dialog.getByRole('alert').waitFor();
  await ui.screenshot({ path: path.join(profile, 'process-window-required.png'), fullPage: true });
  assert(await dialog.isVisible(), 'invalid form remains open');
  assert(!(await definitions()).some(item => item.title === title), 'missing process cannot be saved');
  await dialog.getByLabel('対象プロセス', { exact: true }).fill(processName + '.exe');
  await dialog.getByLabel('操作', { exact: true }).selectOption('restore');
  await dialog.getByTestId('settings-dialog-confirm').click();
  await dialog.waitFor({ state: 'hidden' });
  await until(async () => (await definitions()).some(item => item.title === title), 'custom command saved');
  const saved = (await definitions()).find(item => item.title === title);
  assert.deepEqual(saved.config, { processName: processName + '.exe', action: 'restore' });
  assert.deepEqual(saved.owner, { kind: 'applet', id: 'at365.windows-tools' });
  assert.equal(saved.automation, true);
  await ui.getByRole('button', { name: title + 'を編集', exact: true }).click();
  if (await dialog.getByTestId('custom-command-section:action').isVisible())
    await dialog.getByTestId('custom-command-section:action').click();
  await dialog.getByLabel('操作', { exact: true }).selectOption('toggle');
  await ui.screenshot({ path: path.join(profile, 'process-window-form.png'), fullPage: true });
  await dialog.getByTestId('settings-dialog-confirm').click();
  await dialog.waitFor({ state: 'hidden' });
  assert.equal((await definitions()).find(item => item.id === saved.id).config.action, 'toggle');
  await page.evaluate(id => window.dock.executeCommand(id), 'appdock.custom.' + saved.id);
  await ui.screenshot({ path: path.join(profile, 'process-window-saved.png'), fullPage: true });
  return { id: saved.id, processName: processName + '.exe' };
};
