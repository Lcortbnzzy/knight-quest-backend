import { RequestHandler } from 'express'
import { Response, SuccessParams } from '@utils/response'

export const responseMiddleware: RequestHandler = (req, res, next) => {
    res.respond = <T>(body: Response<T>) => {
        const sanitizedBody = Object.fromEntries(Object.entries(body).filter(([ _, v ]) => v !== undefined));
        res.status(body.code).json(sanitizedBody);
    }

    const ok = <T>(params: SuccessParams<T> = {}) => res.respond(Response.success(params));

    // Custom response methods
    res.success = res.ok = ok
    res.error = (params) => res.respond(Response.error(params))
    res.unauthorized = (params = {}) => res.respond(Response.unauthorized(params));
    res.forbidden = (params = {}) => res.respond(Response.forbidden(params));
    res.notFound = (params = {}) => res.respond(Response.notFound(params));
    res.badRequest = (params) => res.respond(Response.badRequest(params));

    next()
}