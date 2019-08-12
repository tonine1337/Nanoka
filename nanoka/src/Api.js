// used to keep track of the number of total requests sent
var requestNum = 1;

function getEndpoint(path) {
  // *.localhost.chiya.dev resolves to 127.0.0.1 (loopback address).
  // this allows making requests to the client via HTTPS using a self-signed preinstalled certificate.
  // subdomains are used to circumvent browser request rate limiting.
  let base = `https://${requestNum++}.localhost.chiya.dev:7230`;

  if (path)
    return new URL(path, base);

  return base;
}

function get(path, events) {
  let promise = fetch(getEndpoint(path), {
    method: 'GET'
  });

  configureEvents(promise, events);
}

function post(path, data, events) {
  let promise = fetch(getEndpoint(path), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });

  configureEvents(promise, events);
}

function configureEvents(promise, events) {
  if (events) {
    if (typeof events.success === 'function')
      promise = promise
        .then(r => r.json())
        .then(r => events.success(r.body));

    if (typeof events.error === 'function')
      promise = promise
        .catch(e => events.error(e));

    if (typeof events.finish === 'function')
      promise = promise
        .finally(() => events.finish());
  }
}

export function getClientInfo(events) {
  get(null, events);
}

export function searchDoujinshiAsync(query, events) {
  // limit to 100 by default
  if (!query.limit)
    query.limit = 100;

  post('doujinshi/search', query, events);
}
