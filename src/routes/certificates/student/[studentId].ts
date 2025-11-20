import { RequestHandler } from 'express'
import { authenticate } from '@middlewares/authenticate'
import { getStudentInfo } from '@controllers/certificate'

export const get: RequestHandler[] = [authenticate, getStudentInfo]