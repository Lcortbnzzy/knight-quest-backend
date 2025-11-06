import type { Handler } from 'express'
import { prisma } from '@utils/database'

type AuthPinData = {
    pin: string
    token: string
    username: string
    first_name: string
    last_name: string
    expires_at: Date
}

export const POST: Handler = async (req, res) => {
    const { pin } = req.body

    if (!pin || typeof pin !== 'string' || pin.length !== 6) {
        return res.json({ 
            success: false, 
            message: 'Invalid PIN format. Must be 6 digits.' 
        })
    }

    try {
        // Get auth data from database
        const result = await prisma.$queryRaw<AuthPinData[]>`
            SELECT pin, token, username, first_name, last_name, expires_at
            FROM auth_pins 
            WHERE pin = ${pin} 
            AND expires_at > NOW()
            LIMIT 1
        `

        if (!result || result.length === 0) {
            return res.json({ 
                success: false, 
                message: 'Invalid or expired PIN. Please try logging in again.' 
            })
        }

        const authData = result[0]

        // Delete used PIN (one-time use)
        await prisma.$executeRaw`
            DELETE FROM auth_pins 
            WHERE pin = ${pin}
        `

        // Return user data
        return res.json({
            success: true,
            message: 'Login successful!',
            data: {
                token: authData.token,
                username: authData.username,
                firstName: authData.first_name,
                lastName: authData.last_name
            }
        })

    } catch (error) {
        console.error('PIN verification error:', error)
        return res.json({
            success: false,
            message: 'An error occurred during verification. Please try again.'
        })
    }
}