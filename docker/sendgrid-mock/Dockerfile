# SendGrid-compatible mock mail API for local dev/integration tests (see
# docker-compose.yml's sendgridmock service). No published image exists for a
# SendGrid mock, so it's containerized here the same way the Twilio mock is.
FROM node:20-alpine
WORKDIR /app
COPY package.json ./
RUN npm install --omit=dev
COPY server.js index.html ./
EXPOSE 3040
CMD ["node", "server.js"]
