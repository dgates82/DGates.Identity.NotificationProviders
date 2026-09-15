# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-09-15

### Added
- `SonarAnalyzer.CSharp` as a build-time Roslyn analyzer (`PrivateAssets=all`, never flows
  to consumers).
- SonarQube Cloud static analysis, wired into CI's `build-and-test` job via
  `dotnet-sonarscanner` and gated on the quality gate result, with coverage
  (`dotnet test --collect:"XPlat Code Coverage"` across both unit and integration test
  runs) fed into the scan via `sonar.cs.cobertura.reportsPaths`. Explicit
  `sonar.branch.name` for non-PR triggers, and `SONAR_PROJECT_KEY`/`SONAR_ORG` repo
  variables instead of hardcoded literals.

### Fixed
- `NuGet/login@v1` pinned to a commit SHA (SonarQube Cloud finding) - was the only real
  vulnerability from the first full scan. The other (`EnableSsl should be true` on
  `SmtpEmailSender`) is a false positive - `EnableSsl` is intentionally configurable, off
  for local dev against Mailpit (no TLS), on for real SMTP.
- `SendEmailAsync` parameter names in `SmtpEmailSender`, `SendGridEmailSender`,
  `PostMarkEmailSender`, and `OverrideRecipientEmailSender` now match the `IEmailSender`
  interface declaration (SonarQube S927), and interpolated-string logging calls in the email
  and SMS senders now use structured/parameterized logging (SonarQube S2629). The parameter
  renames are source-level only - they affect direct-named-argument callers of the concrete
  classes, not interface-typed consumers or binary compatibility - so this does not warrant a
  major version bump.

## [1.0.0] - 2026-08-04

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
- Local dev/test mocks for Twilio, SendGrid, and Postmark, pulled from
  [`dgates-mock-servers`](https://github.com/dgates82/dgates-mock-servers) on GHCR, plus
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
