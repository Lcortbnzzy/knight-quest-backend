import { createServer } from './server';
import { logger } from '@utils/logging';
import { env } from '@utils/env';

const startServer = async () => {
    try {
        logger.info('🔧 Starting server...');
        
        const server = await createServer();
        const PORT = env.PORT;

        server.listen(PORT, () => {
            logger.info('='.repeat(50));
            logger.info(`🚀 Server running on port ${PORT}`);
            logger.info(`📝 Environment: ${env.NODE_ENV}`);
            logger.info(`🔗 Health check: http://localhost:${PORT}/health`);
            logger.info('='.repeat(50));
        });

        // Handle server errors
        server.on('error', (error: NodeJS.ErrnoException) => {
            if (error.code === 'EADDRINUSE') {
                logger.error(`❌ Port ${PORT} is already in use`);
            } else {
                logger.error(`❌ Server error: ${error.message}`);
            }
            process.exit(1);
        });

        // Graceful shutdown
        const shutdown = async (signal: string) => {
            logger.info(`\n⚠️  ${signal} received. Shutting down gracefully...`);
            
            server.close(() => {
                logger.info('✅ Server closed successfully');
                process.exit(0);
            });

            // Force shutdown after 10 seconds
            setTimeout(() => {
                logger.error('❌ Forcing shutdown after timeout');
                process.exit(1);
            }, 10000);
        };

        // Listen for termination signals
        process.on('SIGTERM', () => shutdown('SIGTERM'));
        process.on('SIGINT', () => shutdown('SIGINT'));
        process.on('uncaughtException', (error: Error) => {
            logger.error(`❌ Uncaught Exception: ${error.message}`);
            logger.error(error.stack || '');
            process.exit(1);
        });
        process.on('unhandledRejection', (reason: unknown) => {
            logger.error(`❌ Unhandled Rejection: ${reason}`);
            process.exit(1);
        });

    } catch (error) {
        const errorMessage = error instanceof Error ? error.message : String(error);
        logger.error(`❌ Failed to start server: ${errorMessage}`);
        process.exit(1);
    }
};

// Start the server
startServer();