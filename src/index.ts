import { RequestHandler } from 'express'

export const get: RequestHandler = (req, res) => {
    res.ok({ message: 'Server is running!' })
}

export const head: RequestHandler = (req, res) => {
    res.status(200).end()
}