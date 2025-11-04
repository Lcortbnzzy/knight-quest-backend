import middlewares from '@middlewares'
import { Role } from '@prisma/client'
import { getSave, resetSave, updateSave } from '@controllers/save'

export const get = [
    middlewares.permitOnly(Role.Student),
    getSave
]

export const put = [
    middlewares.permitOnly(Role.Student),
    updateSave
]

export const del = [
    middlewares.permitOnly(Role.Student),
    resetSave
]