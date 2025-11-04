import { Handler } from 'express'
import { Response } from '@utils/response'

export const get: Handler = async (req, res) => {
    return res.respond(Response.success({ message: 'API is running' }))
}