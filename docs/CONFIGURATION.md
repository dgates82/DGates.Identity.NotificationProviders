# Configuration

## Email

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

## SMS

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

## Redirecting to a test inbox/phone

Every provider supports `OverrideRecipient`: when set, all messages are redirected there instead
of their real recipient, with the original recipient appended to the subject (email) or message
body (SMS). Useful for smoke-testing against production-like config without emailing/texting
real users.

## Pointing at a sandbox instead of the real vendor

Twilio, SendGrid, Postmark, and SNS all support a base-URL override (`BaseUrlOverride` for
Twilio/SendGrid/Postmark, `ServiceUrlOverride` for SNS) that redirects API requests away from
the real vendor endpoint — useful for pointing at a sandbox or mock service of your own instead
of a live account. See [LOCAL_DEV.md](LOCAL_DEV.md) for the exact values that point each provider
at its local mock.
