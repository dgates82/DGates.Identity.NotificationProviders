const crypto = require('crypto');
const express = require('express');

const app = express();
app.use(express.json());
app.use(express.static(__dirname)); // serves index.html at "/"

const messages = [];

// Mirrors Postmark's email send endpoint (https://postmarkapp.com/developer/api/email-api).
// Doesn't validate the X-Postmark-Server-Token header - matches how the Twilio mock accepts
// any credentials. PostmarkResponse.MessageID is typed as a .NET Guid client-side, so the
// mock must return an actual GUID, not an arbitrary string.
app.post('/email', (req, res) => {
  const body = req.body || {};

  if (!body.To || !body.From || !body.Subject) {
    res.status(422).json({
      ErrorCode: 300,
      Message: 'To, From, and Subject are required fields',
    });
    return;
  }

  const messageId = crypto.randomUUID();
  const message = {
    messageId,
    to: body.To,
    from: body.From,
    subject: body.Subject,
    body: body.HtmlBody || body.TextBody || '',
  };

  messages.unshift(message);
  console.log(`[EMAIL SENT] To: ${message.to}, From: ${message.from}, Subject: ${message.subject}`);

  res.status(200).json({
    To: body.To,
    SubmittedAt: new Date().toISOString(),
    MessageID: messageId,
    ErrorCode: 0,
    Message: 'OK',
  });
});

app.get('/api/messages', (_req, res) => {
  res.json(messages);
});

app.delete('/api/messages', (_req, res) => {
  messages.length = 0;
  res.status(204).send();
});

app.get('/health', (_req, res) => {
  res.json({ status: 'ok', messages: messages.length });
});

const port = 3050;
app.listen(port, () => {
  console.log(`Postmark mock server listening on port ${port}`);
});
