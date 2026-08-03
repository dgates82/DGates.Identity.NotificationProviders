# DGates.Identity.NotificationProviders — Local Development

Every provider in this package can be exercised locally against a fake backend instead of a
real vendor account — the same containers this repo's own integration tests use. This covers
running them, pointing a locally-running app at them, and building/testing a local NuGet
package.

## Starting the mocks

```sh
docker compose up -d --wait
```

This starts five services, all healthchecked before `--wait` returns:

| Service        | Port | Backs                          |
|----------------|------|---------------------------------|
| `mailpit`      | 1025 (SMTP), 8025 (web UI/API) | `SmtpEmailSender` |
| `twiliomock`   | 3030 | `TwilioSmsSender` (Twilio-compatible REST API) |
| `sendgridmock` | 3040 | `SendGridEmailSender` |
| `postmarkmock` | 3050 | `PostMarkEmailSender` |
| `localstack`   | 4566 | `SnsSmsSender` (AWS SNS) |

Tear down with `docker compose down -v`.

## Pointing a provider at its mock

**SMTP** needs no override — it's a plain protocol, so just point `Host`/`Port` at Mailpit
directly:
```json
{ "SmtpEmailConfigs": { "Host": "localhost", "Port": 1025, "FromAddress": "test@example.com" } }
```
Open `http://localhost:8025` to see anything sent.

**Twilio, SendGrid, Postmark** each support a `BaseUrlOverride` that redirects API requests away
from the real vendor:
```json
{ "TwilioSmsConfigs": { "AccountSid": "ACtest", "AuthToken": "test", "FromNumber": "+15555550100", "BaseUrlOverride": "http://localhost:3030" } }
```
The mocks don't validate credentials, so any non-empty values work. Inspect what was "sent" via
each mock's `GET /api/messages` (`twiliomock`, `sendgridmock`, `postmarkmock` respectively).

**AWS SNS** uses `ServiceUrlOverride` plus static test credentials (LocalStack doesn't validate
them either):
```json
{ "SnsSmsConfigs": { "Region": "us-east-1", "AccessKey": "test", "SecretKey": "test", "ServiceUrlOverride": "http://localhost:4566" } }
```
SNS SMS has no real delivery to observe even against LocalStack — inspect what was published via
LocalStack's own introspection endpoint: `GET http://localhost:4566/_aws/sns/sms-messages`.

## Running tests

```sh
# Unit tests only (no Docker required)
dotnet test --filter "Category!=Integration"

# Integration tests (requires the mocks running, see above)
dotnet test --filter "Category=Integration"

# Everything
dotnet test
```

## Building a local NuGet package

Use this to test `dotnet pack`/versioning locally before relying on `release.yml`, or to
sanity-check the packed output before tagging a real release.

```sh
dotnet pack --configuration Release /p:Version=0.1.0 --output ./nupkg
```

To reference the local package from another project, add a local NuGet source:

```sh
dotnet nuget add source /path/to/this/repo/nupkg --name LocalNotificationProvidersTest
```

Then reference it normally in the consuming project's `.csproj`:

```xml
<PackageReference Include="DGates.Identity.NotificationProviders" Version="0.1.0" />
```
