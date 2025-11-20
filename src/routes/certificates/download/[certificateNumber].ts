import { RequestHandler } from 'express'
import { authenticate } from '@middlewares/authenticate'
import { downloadCertificate } from '@controllers/certificate'

// GET /certificates/download/:certificateNumber - Download personalized PDF
// This file should be at: routes/certificates/download/[certificateNumber].ts
export const get: RequestHandler[] = [authenticate, downloadCertificate]