import { Role } from '@prisma/client'
import { RequestHandler } from 'express'
import { Response } from '@utils/response'
import { authenticate } from '@middlewares/authenticate'

type PermitConfig = {
    include?: Role[]
    exclude?: Role[]
}

export const permit = (config: PermitConfig): RequestHandler[] => {
    const { include = [], exclude = [] } = config

    const permissionMiddleware: RequestHandler = (req, res, next) => {
        const role = req.user?.role

        if (!role) {
            return res.respond(Response.unauthorized())
        }

        if (include.length > 0 && !include.includes(role)) {
            return res.respond(Response.forbidden())
        }

        if (exclude.length > 0 && exclude.includes(role)) {
            return res.respond(Response.forbidden())
        }

        next()
    }

    return [
        authenticate,
        permissionMiddleware
    ]
}

export const permitOnly = (...roles: Role[]): RequestHandler[] => permit({ include: roles })

export const permitExcept = (...roles: Role[]): RequestHandler[] => permit({ exclude: roles })