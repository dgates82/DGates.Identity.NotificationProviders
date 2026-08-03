# Postmark-compatible mock mail API for local dev/integration tests (see
# docker-compose.yml's postmarkmock service). No published image exists for a
# Postmark mock, so it's containerized here the same way the Twilio mock is.
FROM node:20-alpine
WORKDIR /app
COPY package.json ./
RUN npm install --omit=dev
COPY server.js ./
EXPOSE 3050
CMD ["node", "server.js"]
