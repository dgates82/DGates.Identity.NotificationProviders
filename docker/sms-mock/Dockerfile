# Twilio-compatible mock SMS server for local dev/integration tests (see
# docker-compose.yml's smsmock service). No published image exists for
# twillio-sms-mock, so it's containerized here the same way Mailpit is used
# pre-built for email.
FROM node:20-alpine
WORKDIR /app
COPY package.json ./
RUN npm install --omit=dev
COPY server.js ./
EXPOSE 3030
CMD ["node", "server.js"]
