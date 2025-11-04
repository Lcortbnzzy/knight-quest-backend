import { Handler } from 'express'
import { Response } from '@utils/response'
import prettyMilliseconds from 'pretty-ms'

export const get: Handler = async (req, res) => {
    return res.respond(Response.success({
        message: 'API is running',
        data: {
            uptime: prettyMilliseconds(process.uptime() * 1000, { secondsDecimalDigits: 0 }),
            timestamp: new Date().toLocaleString()
        }
    }))
}