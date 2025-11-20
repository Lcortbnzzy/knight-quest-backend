
// Create this file at: src/routes/certificates/convert-to-pdf.ts

import { RequestHandler } from 'express'
import { authenticate } from '@middlewares/authenticate'
import { convertImageToPDF } from '@controllers/certificate'

// POST /certificates/convert-to-pdf - Convert screenshot to PDF
export const post: RequestHandler[] = [authenticate, convertImageToPDF]