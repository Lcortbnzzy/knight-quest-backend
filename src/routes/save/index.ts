import { getSave, updateSave, resetSave } from '@controllers/save'
import middlewares from '@middlewares'

// ✅ IMPORTANT: Add authentication middleware
export const middleware = [middlewares.authenticate]

export const get = getSave
export const put = updateSave
export const del = resetSave