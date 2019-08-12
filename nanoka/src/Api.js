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

export function getClientInfo() {
  return fetch(getEndpoint());
}
