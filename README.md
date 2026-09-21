# DGates.Identity.NotificationProviders

[![CI](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/ci.yml/badge.svg)](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=dgates_identity-notificationproviders&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=dgates_identity-notificationproviders)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=dgates_identity-notificationproviders&metric=coverage)](https://sonarcloud.io/summary/new_code?id=dgates_identity-notificationproviders)
[![CodeQL](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/codeql.yml/badge.svg)](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/codeql.yml)
[![NuGet](https://img.shields.io/nuget/v/DGates.Identity.NotificationProviders.svg)](https://www.nuget.org/packages/DGates.Identity.NotificationProviders)

ASP.NET Core Identity ships a no-op `IEmailSender` — confirmation links, password resets, and
2FA codes are generated but never actually sent — and no SMS-sender concept at all. This
package provides real senders for both: three for email, two for SMS, each wired up with one
DI call.

Targets **.NET 10 only** — not compatible with .NET Framework (e.g. net48).

## See it running

Register an account on the auth template's [live demo](https://angular-dotnet-auth-template-1019453023791.us-central1.run.app).
The confirmation email and any 2FA codes you request are sent by these providers into public
mock inboxes — [SendGrid mock](https://sendgrid-mock-7qs7btajdq-uc.a.run.app) (email) and
[Twilio mock](https://twilio-mock-1019453023791.us-central1.run.app) (SMS). No real email or
SMS is sent. See [angular-dotnet-auth-template](https://github.com/dgates82/angular-dotnet-auth-template)
for the app and [dgates-mock-servers](https://github.com/dgates82/dgates-mock-servers) for the
mocks themselves.

## What you get

- **Email**: SMTP, SendGrid, Postmark — each implements ASP.NET Core Identity's own
  `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`.
- **SMS**: Twilio, AWS SNS — both implement this package's own `ISmsSender`. There's no stock
  SMS-sender contract in ASP.NET Core Identity, so this package defines one, deliberately shaped
  against two independent backends from the start so it doesn't end up fitting only one vendor.
- One DI call per provider (`AddSmtpEmailSender`, `AddTwilioSmsSender`, etc.) — no hand-rolled
  sender required.
- A separate `IOptions<T>` per provider — no cross-provider coupling, no shared config surface
  you don't need.
- `OverrideRecipient` on every provider, to redirect all messages to a single test inbox or
  phone instead of real recipients.
- A base-URL override on every provider, to point at a sandbox or local mock instead of the
  real vendor.

## Install

```sh
dotnet add package DGates.Identity.NotificationProviders
```

[`DGates.Identity.Jwt2Fa`](https://github.com/dgates82/DGates.Identity.Jwt2Fa) depends on this
package — if you're already using it, you have this too.

## Usage

Each provider is registered with a single extension method, which binds its own config section
and registers the sender for DI:

```csharp
builder.Services.AddSmtpEmailSender(builder.Configuration);
// or: AddSendGridEmailSender / AddPostMarkEmailSender

builder.Services.AddTwilioSmsSender(builder.Configuration);
// or: AddSnsSmsSender
```

Only register one email provider and one SMS provider — the last one registered wins for its
respective interface.

**Works with `MapIdentityApi<TUser>`, with one ordering rule.** ASP.NET Core Identity's own
`AddIdentityApiEndpoints<TUser>()` registers an internal adapter,
`DefaultMessageEmailSender<TUser>`, that satisfies `MapIdentityApi`'s `IEmailSender<TUser>` by
forwarding to whatever non-generic `IEmailSender` is registered — which is exactly what this
package's email senders implement. `AddIdentityApiEndpoints<TUser>()` also does
`TryAddTransient<IEmailSender, NoOpEmailSender>()` as its own fallback, so register a sender
from this package **before** calling `AddIdentityApiEndpoints<TUser>()`, or Identity's no-op
wins instead. `MapIdentityApi` has no SMS concept at all, so `ISmsSender` isn't something it
uses — that's for your own 2FA layer (e.g. `DGates.Identity.Jwt2Fa`) or a hand-rolled endpoint.

If you're bringing your own SMS provider instead, implement `ISmsSender` directly:

```csharp
public interface ISmsSender
{
    Task<string> SendSmsAsync(string number, string message, CancellationToken cancellationToken = default);
}
```

Implementations should throw on delivery failure rather than fail silently, so callers see
consistent behavior regardless of which registered `ISmsSender` they're using.

## Providers at a glance

| Provider | Channel | Registration | Config section | Required keys | Notes |
| --- | --- | --- | --- | --- | --- |
| SMTP | Email | `AddSmtpEmailSender` | `SmtpEmailConfigs` | `Host`, `Port`, `FromAddress` | No API key — any SMTP server, including a local one like Mailpit |
| SendGrid | Email | `AddSendGridEmailSender` | `SendGridEmailConfigs` | `ApiKey`, `FromAddress`, `FromName` | |
| Postmark | Email | `AddPostMarkEmailSender` | `PostMarkEmailConfigs` | `ApiKey`, `FromAddress` | |
| Twilio | SMS | `AddTwilioSmsSender` | `TwilioSmsConfigs` | `AccountSid`, `AuthToken`, `FromNumber` | |
| AWS SNS | SMS | `AddSnsSmsSender` | `SnsSmsConfigs` | `Region` | Uses the default AWS credential chain (IAM role, environment) unless `AccessKey`/`SecretKey` are set — leave them unset in production |

Full JSON per provider, `OverrideRecipient`, and the base-URL overrides are in
[docs/CONFIGURATION.md](https://github.com/dgates82/DGates.Identity.NotificationProviders/blob/main/docs/CONFIGURATION.md).

## Test without vendor accounts

Every provider has a fake backend to develop and test against — no SendGrid, Postmark, Twilio,
or AWS account required:

```sh
docker compose up -d --wait
```

This starts Mailpit (SMTP), and mocks for SendGrid, Twilio, and Postmark, plus LocalStack for
SNS — five services, all healthchecked before `--wait` returns. Point a provider at its mock
with the same base-URL override used against a real sandbox:

```json
{ "TwilioSmsConfigs": { "AccountSid": "ACtest", "AuthToken": "test", "FromNumber": "+15555550100", "BaseUrlOverride": "http://localhost:3030" } }
```

None of the mocks validate credentials, so any non-empty values work. See
[docs/LOCAL_DEV.md](https://github.com/dgates82/DGates.Identity.NotificationProviders/blob/main/docs/LOCAL_DEV.md)
for the full walkthrough, including how to view messages each mock received, and
[dgates-mock-servers](https://github.com/dgates82/dgates-mock-servers) for the mock images
themselves.

## Redirecting to a test inbox or phone

Every provider supports `OverrideRecipient`: when set, all messages are redirected there
instead of their real recipient, with the original recipient appended to the subject (email) or
message body (SMS). Useful for smoke-testing against production-like config without emailing or
texting real users.

## Part of a small ecosystem

| Project | What it is | Reach for it when |
| --- | --- | --- |
| **DGates.Identity.NotificationProviders** (you are here) | Email and SMS senders (SMTP, SendGrid, Postmark, Twilio, SNS) | you need drop-in senders for ASP.NET Core Identity |
| [DGates.Identity.Jwt2Fa](https://github.com/dgates82/DGates.Identity.Jwt2Fa) ([NuGet](https://www.nuget.org/packages/DGates.Identity.Jwt2Fa)) | JWT issuance and multi-channel 2FA for ASP.NET Core Identity (uses this package) | you want real JWTs and TOTP/email/SMS 2FA on your own API |
| [angular-dotnet-auth-template](https://github.com/dgates82/angular-dotnet-auth-template) | Angular 21 + .NET 10 starter with both packages wired in, live demo, Cloud Run pipeline | you want a running app, not just the library |
| [dgates-mock-servers](https://github.com/dgates82/dgates-mock-servers) | Public GHCR images mocking SendGrid, Twilio, and Postmark | you want to develop or test notification flows with no accounts |

angular-dotnet-auth-template → DGates.Identity.Jwt2Fa → DGates.Identity.NotificationProviders → dgates-mock-servers (in dev)

This package doesn't always track the same version as the packages that depend on it — check
each repo's own `PackageReference` for what it actually pins.

More from dgates82: [DGates.AwsSecretsManager](https://github.com/dgates82/DGates.AwsSecretsManager)
and [dotnet-nuget-release-template](https://github.com/dgates82/dotnet-nuget-release-template).

## License

MIT — see [LICENSE](https://github.com/dgates82/DGates.Identity.NotificationProviders/blob/main/LICENSE).
