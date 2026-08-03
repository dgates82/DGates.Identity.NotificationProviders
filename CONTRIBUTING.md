# Contributing

Thanks for your interest in improving this package. A few guidelines to keep things consistent.

## Before you start

- For anything beyond a small fix, open an issue first to discuss the change.
- `ISmsSender` is a contract defined by this package, not an official ASP.NET Core Identity
  interface — be precise about that distinction anywhere it's documented or discussed.
- `ISmsSender` is deliberately designed against two independent backends (Twilio, AWS SNS) so
  the contract doesn't end up shaped around one vendor's quirks. If you're adding a third SMS
  backend and find you need to change the interface to fit it, treat that as a signal something
  in the existing shape was under-generalized, not just "one more special case."

## Development setup

- `dotnet restore`
- `docker compose up -d --wait` — starts Mailpit, the Twilio/SendGrid/Postmark mocks, and
  LocalStack (SNS), all healthchecked before integration tests run against them
- `dotnet test` — runs everything; use `--filter "Category!=Integration"` for unit tests only
- See [docs/LOCAL_DEV.md](docs/LOCAL_DEV.md) for the full local development walkthrough,
  including how to point each provider at its mock for manual testing.

## Making changes

- Branch from `main`, open a PR — direct pushes to `main` are blocked.
- Commit format: `type: lowercase description` (e.g. `feat:`, `fix:`, `docs:`, `chore:`).
- Every provider owns its own `IOptions<T>` — don't introduce config shared across providers.
- Cross-cutting behavior that applies to every provider in a domain (email or SMS) — like the
  `OverrideRecipient` redirect — belongs in the shared decorator
  (`OverrideRecipientEmailSender`/`OverrideRecipientSmsSender`), not copy-pasted into each
  concrete sender. If you're adding a new provider, it should only need to implement the sender
  interface itself; the decorator wraps it automatically via its `AddXxxSender()` registration.
- SNS's test assertions differ from Twilio's on purpose: Twilio's mock is an HTTP redirect you
  can inspect directly, but SNS SMS has no real delivery to observe even against LocalStack, so
  those tests assert `PublishAsync` succeeded and inspect LocalStack's
  `/_aws/sns/sms-messages` introspection endpoint instead. Don't force both providers into one
  assertion pattern.
- If your change affects `docker-compose.yml`, confirm the full integration suite still passes
  against real containers, not just that it builds. The Twilio/SendGrid/Postmark mocks are
  pulled from GHCR (`ghcr.io/dgates82/*-mock`, published by
  [`dgates-mock-servers`](https://github.com/dgates82/dgates-mock-servers)) — if a mock needs a
  behavior change, that happens upstream in that repo, not here.
- Update `CHANGELOG.md` under `[Unreleased]` for any user-facing change.

## Pull requests

- 1 approval required before merge (GitHub Ruleset on `main`).
- CI must pass — build, unit tests, and container-backed integration tests.
- Merge via merge commit, not squash — keeps full commit history intact.

## Releasing

This package uses real `vX.Y.Z` tags (e.g. `v1.0.0`), which `release.yml` watches for directly
— there's no separate template-versioning scheme to worry about.

1. Push the tag explicitly: `git tag vX.Y.Z <sha> && git push origin vX.Y.Z`.
2. Let `release.yml` fire, publishing to NuGet.org via Trusted Publishing (OIDC) — no API key
   is stored in the repo.
3. Confirm the workflow run is green.
4. Create the GitHub Release by selecting the tag that already exists — never type a new tag
   name into the release form.

## What not to contribute

- API keys, secrets, or anything that would require moving off Trusted Publishing.
- Anything that shapes `IEmailSender`/`ISmsSender` around one specific vendor's behavior instead
  of the general contract both interfaces are meant to express.

## Questions

Open an issue if you're not sure whether something fits.
