import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createRequire } from 'node:module';
import { resolve, join } from 'node:path';
import vm from 'node:vm';

const root = resolve(process.argv[2]);
const frontendRequire = createRequire(join(resolve(process.argv[3] ?? root), 'package.json'));
const parser = frontendRequire('@babel/parser');
const babel = frontendRequire('@babel/core');
const quietConsole = { error() {} };
const authSource = await readFile(join(root, 'src/context/AuthContext.jsx'), 'utf8');
const authTree = parser.parse(authSource, { sourceType: 'module', plugins: ['jsx'] });
let logoutNode;
function visit(node) {
  if (!node || typeof node !== 'object') return;
  if (node.type === 'VariableDeclarator' && node.id?.name === 'logout') logoutNode = node.init;
  for (const value of Object.values(node)) {
    if (Array.isArray(value)) value.forEach(visit);
    else if (value && typeof value === 'object') visit(value);
  }
}
visit(authTree);
assert.ok(logoutNode, 'Find actual AuthContext logout function');
const createLogout = new Function('AsyncStorage', 'setUser', 'console',
  `return (${authSource.slice(logoutNode.start, logoutNode.end)});`);
const storage = new Map([['@user', 'saved-user'], ['userToken', 'saved-token'], ['theme', 'dark']]);
let currentUser = { id: 1 };
const logout = createLogout({
  async multiRemove(keys) {
    assert.deepEqual(keys, ['@user', 'userToken']);
    keys.forEach(key => storage.delete(key));
  },
}, user => { currentUser = user; }, quietConsole);
assert.equal((await logout()).success, true);
assert.equal(currentUser, null);
assert.equal(storage.has('@user'), false);
assert.equal(storage.has('userToken'), false);
assert.equal(storage.get('theme'), 'dark', 'Keep unrelated application settings');
currentUser = { id: 2 };
const failingLogout = createLogout({ async multiRemove() { throw new Error('Storage unavailable'); } },
  user => { currentUser = user; }, quietConsole);
assert.equal((await failingLogout()).success, false);
assert.equal(currentUser.id, 2, 'Storage failure must allow retry without pretending logout succeeded');

const navigatorSource = await readFile(join(root, 'src/navigation/AppNavigator.jsx'), 'utf8');
const transformed = babel.transformSync(navigatorSource, {
  configFile: false, babelrc: false,
  plugins: [frontendRequire.resolve('@babel/plugin-transform-react-jsx'),
    frontendRequire.resolve('@babel/plugin-transform-modules-commonjs')],
}).code;
let authState = { isAuthenticated: false, isInitializing: false };
const module = { exports: {} };
vm.runInNewContext(transformed, {
  module, exports: module.exports,
  React: { createElement: (type, props, ...children) => ({ type, props, children }) },
  require(name) {
    if (name === '@react-navigation/native-stack') return {
      createNativeStackNavigator: () => ({ Navigator: 'Navigator', Screen: 'Screen' }),
    };
    if (name === 'react-native') return { ActivityIndicator: 'ActivityIndicator', View: 'View' };
    if (name === '../context/AuthContext') return { useAuth: () => authState };
    return { __esModule: true, default: name };
  },
});
const navigator = module.exports.default;
assert.equal(navigator().children[0].props.name, 'Login');
authState = { isAuthenticated: true, isInitializing: false };
assert.equal(navigator().children[0].props.name, 'MainTabs');
authState = { isAuthenticated: false, isInitializing: false };
assert.equal(navigator().children[0].props.name, 'Login', 'Logout removes authenticated routes');
authState = { isAuthenticated: false, isInitializing: true };
assert.equal(navigator().children[0].type, 'ActivityIndicator', 'Wait for stored session on startup');
console.log('Passed logout storage cleanup, failure/retry, login/logout routing and startup checks.');
