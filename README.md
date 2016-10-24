# Fake Server/Proxy with fiddlercore

## Getting Started

### Adding Certificate for Fake C&C Server
Add both the CA and the Signed Certificate to your local Certificates.

```
netsh http add sslcert ipport=0.0.0.0:9443 certhash=2efd0547e2203ecad65f484f8cefbebc42455103 appid={a6a2f28b-4b3b-4f5e-a568-2615c6ccc874}
```

### Fake C&

Ports 9080 for HTTP and 9443 for HTTPS
