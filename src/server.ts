import express from 'express';
import { router } from 'express-file-routing';
import * as path from 'node:path';
import { fileURLToPath } from 'node:url';
import middlewares from '@middlewares';
import cors from 'cors';
import cookieParser from 'cookie-parser';
import * as http from 'node:http';
import morgan from 'morgan';
import { logger } from '@utils/logging'; // <- your logger

export const createServer = async () => {
    const app = express();
    const server = http.createServer(app);

    // ----- Morgan setup -----
    // Forward morgan logs to your logger
    app.use(
        morgan('dev', {
            stream: {
                write: (message: string) => {
                    logger.info(message.trim()); // trim to remove extra newline
                },
            },
        })
    );

    // ----- Existing middlewares -----
    app.use(middlewares.responseMiddleware);
    app.use(middlewares.requestMiddleware);

    app.use(express.json());
    app.use(cookieParser());
    app.use(cors());

    app.use(middlewares.requestLogger);

    // ----- Routes -----
    const __filename = fileURLToPath(import.meta.url);
    const __dirname = path.dirname(__filename);

    app.use('/', await router({ directory: path.join(__dirname, 'routes') }));

    // ----- Error handler -----
    app.use(middlewares.errorHandler);

    return server;
};
