import 'express'
import { ErrorParams, Response as ResponseObject, SuccessParams } from '@utils/response'
import { Token } from '@utils/jwt'
import { z, ZodSafeParseResult, ZodType } from 'zod'

declare global {
    namespace Express {
        interface Response {
            respond: <T>(body: ResponseObject<T>) => void;
            success: <T>(params?: SuccessParams<T>) => void;
            ok: <T>(params?: SuccessParams<T>) => void;
            error: (params: ErrorParams) => void;
            notFound: (params?: ErrorParams) => void;
            badRequest: (params: ErrorParams) => void;
            unauthorized: (params?: ErrorParams) => void;
            forbidden: (params?: ErrorParams) => void;
        }

        interface Request {
            user: Token;
            parseBody: <T extends ZodType>(schema: T) => ZodSafeParseResult<z.output<T>>;
            parsedBody?: unknown;
            parsedParams?: unknown;
            parsedQuery?: unknown;
        }
    }
}
