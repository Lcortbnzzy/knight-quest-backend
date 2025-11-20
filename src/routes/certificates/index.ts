import { RequestHandler } from 'express'
import { authenticate } from '@middlewares/authenticate'
import { 
    createCertificate,
    listStudentCertificates 
} from '@controllers/certificate'

// POST /certificates - Create new certificate
export const post: RequestHandler[] = [authenticate, createCertificate]

// GET /certificates?studentId=123 - List certificates for a student
export const get: RequestHandler[] = [authenticate, listStudentCertificates]