import { RequestHandler } from 'express'
import { Response } from '@utils/response'
import jwt, { Token } from '@utils/jwt'

export const authenticate: RequestHandler = async (req, res, next) => {
    console.log('🔐 === AUTHENTICATE MIDDLEWARE CALLED ===')
    console.log('📋 Cookies:', req.cookies)
    console.log('📋 Headers.authorization:', req.headers.authorization)
    
    // ✅ FIX: Handle string | string[] type from headers
    const authHeaderRaw = req.headers.authorization || req.headers['Authorization']
    const authHeader = Array.isArray(authHeaderRaw) ? authHeaderRaw[0] : authHeaderRaw
    const cookieToken = req.cookies['access_token']
    
    console.log('🍪 Cookie token:', cookieToken ? `${cookieToken.substring(0, 20)}...` : 'None')
    console.log('🔑 Auth header:', authHeader ? authHeader.substring(0, 30) + '...' : 'None')
    
    // ✅ Extract token from cookie OR authorization header
    let token: string | undefined
    
    if (cookieToken) {
        token = cookieToken
        console.log('✅ Using token from cookie')
    } else if (authHeader && authHeader.startsWith('Bearer ')) {
        token = authHeader.substring(7) // Remove "Bearer "
        console.log('✅ Using token from Authorization header')
    }
    
    if (!token) {
        console.log('❌ No token found in cookies or headers')
        return res.respond(Response.unauthorized({ message: 'No valid session found. Please login again.' }))
    }
    
    console.log('🔍 Verifying token:', token.substring(0, 20) + '...')
    
    const decoded = jwt.verifyToken<Token>(token)
    
    if (!decoded) {
        console.log('❌ Token verification failed')
        return res.respond(Response.unauthorized({ message: 'Invalid token' }))
    }
    
    console.log('✅ Token verified successfully')
    console.log('👤 User:', decoded)
    
    req.user = decoded
    
    next()
}