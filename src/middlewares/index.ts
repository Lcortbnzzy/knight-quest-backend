import { authenticate } from './authenticate'
import { errorHandler } from './error'
import { permit, permitExcept, permitOnly } from './permit'
import { requestLogger } from './request-logger'
import { responseMiddleware } from './response'
import {
    requestMiddleware,
    validateRequest,
    validateRequestBody,
    validateRequestParams,
    validateRequestQuery,
} from './request'


export default {
    authenticate,
    errorHandler,
    requestLogger,
    permit,
    permitOnly,
    permitExcept,
    responseMiddleware,
    requestMiddleware,
    validateRequestBody,
    validateRequestParams,
    validateRequestQuery,
    validateRequest,
}