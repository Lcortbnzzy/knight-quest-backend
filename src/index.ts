import { env } from '@utils/env'
import { logger } from '@utils/logging'
import { createServer } from '@src/server'

console.log('🚀🚀🚀 CONSOLE.LOG TEST - Server starting... 🚀🚀🚀');
logger.info('🚀🚀🚀 LOGGER.INFO TEST - Server starting... 🚀🚀🚀');

const server = await createServer();

server.listen(env.PORT, () => {
    console.log('✅✅✅ CONSOLE.LOG - Server is running! ✅✅✅');
    logger.info(`✅✅✅ LOGGER.INFO - Server is running on http://localhost:${env.PORT} ✅✅✅`);
});