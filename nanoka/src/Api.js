// gets the absolute URI for the specified path
export function getEndpoint(path) {
  // allow overriding base API url for custom databases
  let base = localStorage.getItem('api_base');

  base = base || 'https://nanoka-idx.chiya.dev';

  if (path)
    return new URL(path, base);

  return base;
}

// retrieves user credentials saved in HTML5 localStorage
export function getCredentials() {
  const id = localStorage.getItem('auth_id');
  const secret = localStorage.getItem('auth_secret');

  if (id && secret)
    return { id, secret };

  return null;
}

// updates user credentials saved in HTML5 localStorage
export function setCredentials(id, secret) {
  if (id)
    localStorage.setItem('auth_id', id);
  else
    localStorage.removeItem('auth_id');

  if (secret)
    localStorage.setItem('auth_secret', secret);
  else
    localStorage.removeItem('auth_secret');

  window.location.reload();
}

export var accessToken;
export var currentUser;
export var version;

// authenticates to the API and bootstraps the API client
// note that this is different to calling authenticate(id, secret)
export async function startAsync() {
  const cred = getCredentials();

  if (!cred)
    return false;

  const result = await authenticateAsync(cred.id, cred.secret);

  if (result.error) {
    setCredentials();
    return;
  }

  accessToken = result.accessToken;
  currentUser = result.user;
  version = result.version;

  // set timeout to reauthenticate automatically before token expiry
  const expiry = new Date(result.expiry);

  setTimeout(startAsync, expiry.getTime() - Date.now() - 10000);
}

export async function getAsync(path) {
  const headers = {};

  if (accessToken)
    headers['Authorization'] = `Bearer ${accessToken}`;

  const response = await fetch(getEndpoint(path), {
    method: 'GET',
    headers: headers
  });

  const result = await response.json();

  if (!response.ok) {
    result.error = true;
    result.message = response.statusText;
  }

  return result;
}

export async function postAsync(path, data) {
  const headers = {
    'Content-Type': 'application/json'
  };

  if (accessToken)
    headers['Authorization'] = `Bearer ${accessToken}`;

  const response = await fetch(getEndpoint(path), {
    method: 'POST',
    headers: headers,
    body: JSON.stringify(data)
  });

  const result = await response.json();

  if (!response.ok) {
    result.error = true;
    result.message = response.statusText;
  }

  return result;
}

// retrieves the cached value of the current user
export function getCurrentUser() {
  return currentUser;
}

// POST user/auth
export function authenticateAsync(id, secret) {
  return postAsync('users/auth', { id, secret });
}

// POST doujinshi/search
export function searchDoujinshiAsync(query) {
  // limit to 100 by default
  if (!query.limit)
    query.limit = 100;

  return postAsync('doujinshi/search', query);
}

// POST doujinshi
export function createDoujinshiAsync(doujinshi, variant) {
  return postAsync('doujinshi', { doujinshi, variant });
}

export async function uploadFileAsync(id, file, final) {
  const form = new FormData();

  form.append('file', file);

  const headers = {};

  if (accessToken)
    headers['Authorization'] = `Bearer ${accessToken}`;

  let endpoint = getEndpoint(`uploads/${id}`);

  if (final)
    endpoint += '?final=true';

  const response = await fetch(endpoint, {
    method: 'POST',
    headers: headers,
    body: form
  });

  return await response.json();
}

// GET doujinshi/{id}
export function getDoujinshiAsync(id) {
  return getAsync(`doujinshi/${id}`);
}
