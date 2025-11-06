import middlewares from '@middlewares'
import { Role } from '@prisma/client'
import { getSave, resetSave, updateSave } from '@controllers/save'  // ✅ UNCOMMENT
import { Handler } from 'express'

console.log('📁📁📁 SAVE ROUTE FILE LOADED! 📁📁📁')

// Debug middleware
const debugMiddleware: Handler = (req, res, next) => {
    console.log('🚨🚨🚨 /save route HIT! 🚨🚨🚨')
    console.log('Method:', req.method)
    console.log('Path:', req.path)
    console.log('User exists:', !!req.user)
    next()
}

export const get = [
    debugMiddleware,
    middlewares.authenticate,
    getSave  // ✅ UNCOMMENT
]

export const put = [
    debugMiddleware,
    middlewares.authenticate,
    updateSave  // ✅ UNCOMMENT
]

export const del = [
    debugMiddleware,
    middlewares.authenticate,
    resetSave  // ✅ UNCOMMENT
]