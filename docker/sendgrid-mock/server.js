const express = require('express');

const app = express();
app.use(express.json());

const messages = [];

function generateMessageId() {
  return `mock-msg-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
}

// Mirrors SendGrid's v3 Mail Send endpoint (https://docs.sendgrid.com/api-reference/mail-send/mail-send).
// Doesn't validate the Authorization header - matches how the Twilio mock accepts any credentials.
app.post('/v3/mail/send', (req, res) => {
  const body = req.body || {};
  const personalization = (body.personalizations || [])[0] || {};
  const to = ((personalization.to || [])[0] || {}).email;
  const subject = personalization.subject;
  const htmlContent = (body.content || []).find((c) => c.type === 'text/html');
  const anyContent = htmlContent || (body.content || [])[0];

  if (!to || !body.from || !body.from.email || !subject) {
    res.status(400).json({
      errors: [{ message: 'to, from, and subject are required fields' }],
    });
    return;
  }

  const messageId = generateMessageId();
  const message = {
    messageId,
    to,
    from: body.from.email,
    subject,
    body: anyContent ? anyContent.value : '',
  };

  messages.unshift(message);
  console.log(`[EMAIL SENT] To: ${message.to}, From: ${message.from}, Subject: ${message.subject}`);

  res.status(202).set('X-Message-Id', messageId).send();
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

const port = 3040;
app.listen(port, () => {
  console.log(`SendGrid mock server listening on port ${port}`);
});
