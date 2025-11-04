import { env } from '@utils/env'
import { logger } from '@utils/logging'
import type { ErrorRequestHandler, RequestHandler } from 'express'
import { Response } from '@utils/response'

const unexpectedRequest: RequestHandler = (_req, res) => {
    res.respond(Response.notFound())
}

const addErrorToRequestLog: ErrorRequestHandler = (err, _req, res, next) => {
    res.locals.err = err

    if (err instanceof SyntaxError) {
        return res.respond(Response.badRequest({ message: err.message }))
    }

    // ✅ FIX: Only log safe error properties
    const safeError = {
        message: err.message || 'Unknown error',
        name: err.name,
        stack: env.isProduction ? undefined : err.stack
    }

    logger.error(safeError, safeError.message)

    res.respond(Response.error({ 
        error: env.isProduction ? err.message : safeError.message 
    }))

    next(err)
}

export const errorHandler = [ unexpectedRequest, addErrorToRequestLog ]