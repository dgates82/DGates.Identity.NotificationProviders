# DGates.Identity.NotificationProviders

[![CI](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/ci.yml/badge.svg)](https://github.com/dgates82/DGates.Identity.NotificationProviders/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=dgates_identity-notificationproviders&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=dgates_identity-notificationproviders)

Drop-in `IEmailSender`/`ISmsSender` providers for ASP.NET Core Identity. Wire up email or SMS
delivery for password resets, email confirmation, and two-factor codes with one DI call, no
hand-rolled sender required.

Targets **.NET 10 only** — not compatible with .NET Framework (e.g. net48).

- **Email**: SMTP, SendGrid, Postmark — each implements ASP.NET Core Identity's own
  `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`, so they drop straight into Identity
  with no extra plumbing.
- **SMS**: Twilio, AWS SNS — both implement this package's own `ISmsSender`. There's no stock
  SMS-sender contract in ASP.NET Core Identity, so this package defines one, deliberately shaped
  against two independent backends from the start so it doesn't end up fitting only one vendor.

Every provider has its own `IOptions<T>` — no cross-provider coupling, no shared config surface
you don't need.

## Install

```sh
dotnet add package DGates.Identity.NotificationProviders
```

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

### Email

**SMTP** (`SmtpEmailConfigs`):
```json
{
  "SmtpEmailConfigs": {
    "Host": "smtp.example.com",
    "Port": 587,
    "FromAddress": "noreply@example.com",
    "UserName": "smtp-user",
    "Password": "smtp-password",
    "EnableSsl": true
  }
}
```

**SendGrid** (`SendGridEmailConfigs`):
```json
{
  "SendGridEmailConfigs": {
    "ApiKey": "SG.xxxxx",
    "FromAddress": "noreply@example.com",
    "FromName": "Example App"
  }
}
```

**Postmark** (`PostMarkEmailConfigs`):
```json
{
  "PostMarkEmailConfigs": {
    "ApiKey": "xxxxx",
    "FromAddress": "noreply@example.com"
  }
}
```

### SMS

**Twilio** (`TwilioSmsConfigs`):
```json
{
  "TwilioSmsConfigs": {
    "AccountSid": "ACxxxxx",
    "AuthToken": "xxxxx",
    "FromNumber": "+15555550100"
  }
}
```

**AWS SNS** (`SnsSmsConfigs`):
```json
{
  "SnsSmsConfigs": {
    "Region": "us-east-1",
    "SenderId": "ExampleApp",
    "SmsType": "Transactional"
  }
}
```
Uses the default AWS credential chain (IAM role, environment, etc.) unless `AccessKey`/`SecretKey`
are explicitly set — leave them unset in production.

### Redirecting to a test inbox/phone

Every provider supports `OverrideRecipient`: when set, all messages are redirected there instead
of their real recipient, with the original recipient appended to the subject (email) or message
body (SMS). Useful for smoke-testing against production-like config without emailing/texting
real users.

### Pointing at a sandbox instead of the real vendor

Twilio, SendGrid, Postmark, and SNS all support a base-URL override (`BaseUrlOverride` for
Twilio/SendGrid/Postmark, `ServiceUrlOverride` for SNS) that redirects API requests away from
the real vendor endpoint — useful for pointing at a sandbox or mock service of your own instead
of a live account.

## License

MIT — see [LICENSE](https://github.com/dgates82/DGates.Identity.NotificationProviders/blob/main/LICENSE).
