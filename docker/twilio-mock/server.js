const { createTwillioMockServer } = require('twillio-sms-mock');

const server = createTwillioMockServer({ port: 3030 });
server.start();
