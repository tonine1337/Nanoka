// used to keep track of the number of total requests sent
var requestNum = 1;

function getEndpoint(path) {
  // *.localhost.chiya.dev resolves to 127.0.0.1 (loopback address).
  // this allows making requests to the client via HTTPS using a self-signed preinstalled certificate.
  // subdomains are used to circumvent browser request rate limiting.
  const base = `https://${requestNum++}.localhost.chiya.dev:7230`;

  if (path)
    return new URL(path, base);

  return base;
}

function get(path, events) {
  const promise = fetch(getEndpoint(path), {
    method: 'GET'
  });

  configureEvents(promise, events);
}

function post(path, data, events) {
  const promise = fetch(getEndpoint(path), {
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
        .then(r => {
          if (!r.ok)
            throw Error(r.text());

          return r.json();
        })
        .then(r => events.success(r));

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

export function uploadDoujinshiAsync(request, events) {
  var form = new FormData();

  form.append('doujinshi', new Blob([JSON.stringify(request.doujinshi)], { type: 'application/json' }));
  form.append('variant', new Blob([JSON.stringify(request.variant)], { type: 'application/json' }));
  form.append('file', request.file);

  const promise = fetch(getEndpoint('doujinshi'), {
    method: 'POST',
    body: form
  });

  configureEvents(promise, events);
}
