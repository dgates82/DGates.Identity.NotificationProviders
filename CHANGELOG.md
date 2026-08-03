# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `IEmailSender` implementations: `SmtpEmailSender`, `SendGridEmailSender`, `PostMarkEmailSender`
  — each with its own `IOptions<T>`, no cross-provider coupling
- `ISmsSender` (package-defined, not a stock ASP.NET Core Identity contract) and its
  implementations: `TwilioSmsSender`, `SnsSmsSender`
- `OverrideRecipient` redirect behavior for every provider, implemented once per domain via
  `OverrideRecipientEmailSender`/`OverrideRecipientSmsSender` decorators rather than duplicated
  per concrete sender
- `BaseUrlOverride` (Twilio, SendGrid, Postmark) / `ServiceUrlOverride` (SNS) for redirecting
  API requests to a sandbox or mock instead of the real vendor
- Hand-rolled local dev/test mocks for Twilio, SendGrid, and Postmark (`docker/*-mock`), plus
  Mailpit and LocalStack (SNS) via `docker-compose.yml` — usable from local dev, not just tests
- Integration tests for every provider against real mock backends, plus DI registration tests
  for all five `AddXxxSender()` extension methods
- `CancellationToken` support on `ISmsSender.SendSmsAsync`

### Changed
- `ISmsSender.SendSmsAsync` returns `Task<string>` (the provider's message ID - Twilio's `Sid`,
  SNS's `MessageId`) instead of a bare `Task`
- `TwilioSmsSender` passes a scoped Twilio client per call instead of mutating the SDK's global
  static client, removing a race condition under concurrent calls
- `SmtpEmailOptions.Port` is `int`, not a runtime-parsed `string`

### Fixed
- Secrets (SMTP password, SendGrid/Postmark API keys, Twilio auth token, SNS secret key)
  excluded from debug logging via `[JsonIgnore]`
- `PostMarkEmailSender`/`SendGridEmailSender` now throw on send failure instead of swallowing it
  silently
- `PostMarkEmailSender`/`SendGridEmailSender` were injecting the wrong `ILogger<T>` (a
  copy-paste artifact), miscategorizing their log output

<!--
## [X.Y.Z] - YYYY-MM-DD

### Added
-

### Changed
-

### Fixed
-
-->
