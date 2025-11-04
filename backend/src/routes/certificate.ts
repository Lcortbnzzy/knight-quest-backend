import { Handler } from 'express'
import { downloadCertificate } from '@controllers/certificate'
import middlewares from '@middlewares'

export const get: Handler[] = [
    middlewares.authenticate,
    downloadCertificate
]
