import { RequestHandler } from 'express'
import { Response } from '@utils/response'
import jwt, { Token } from '@utils/jwt'

export const authenticate: RequestHandler = async (req, res, next) => {
    const token: string = req.cookies['access_token'] ?? req.headers['authorization']?.replace('Bearer ', '')

    if (!token) res.respond(Response.unauthorized({ message: 'No valid session found. Please login again.' }))

    const decoded = jwt.verifyToken<Token>(token)

    if (!decoded) {
        return res.respond(Response.unauthorized({ message: 'Invalid token' }))
    }

    req.user = decoded

    next()
}