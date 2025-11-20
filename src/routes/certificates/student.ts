import { RequestHandler } from 'express'
import { authenticate } from '@middlewares/authenticate'
import { getStudentInfo } from '@controllers/certificate'

// GET /certificates/student/:studentId - Verify student for certificate creation
export const get: RequestHandler[] = [authenticate, getStudentInfo]