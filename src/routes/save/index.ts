import { getSave, updateSave, resetSave } from '@controllers/save'
import { authenticate } from '@middlewares/authenticate'
import { RequestHandler } from 'express'

// ✅ Wrap handlers with authenticate middleware
export const get: RequestHandler[] = [authenticate, getSave]
export const put: RequestHandler[] = [authenticate, updateSave]
export const del: RequestHandler[] = [authenticate, resetSave]