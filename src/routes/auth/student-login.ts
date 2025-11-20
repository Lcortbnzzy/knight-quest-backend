
// ===== student-login.ts =====
import { prisma } from '@utils/database'
import { z } from 'zod'
import { validateRequestBody } from '@middlewares/request'
import jwt from 'jsonwebtoken'

const StudentLoginSchema = z.object({
    studentId: z.string().regex(/^#\d{9}$/)
})

export const POST = validateRequestBody(StudentLoginSchema, async (req, res): Promise<void> => {
    const { studentId } = req.parsedBody
    
    console.log('🔐 POST /auth/student/login')
    console.log('Student ID:', studentId)
    
    try {
        const user = await prisma.user.findFirst({
            where: { 
                username: studentId,
                role: 'Student'
            }
        })
        
        if (!user) {
            console.log('❌ Student not found')
            res.status(404).json({
                success: false,
                code: 404,
                message: 'Student ID not found.'
            })
            return
        }
        
        // ✅ Create save if doesn't exist (upsert)
        await prisma.save.upsert({
            where: { userId: user.id },
            create: {
                userId: user.id,
                data: {
                    account: { 
                        token: '', 
                        username: studentId, 
                        firstName: user.firstName, 
                        lastName: user.lastName, 
                        role: 'student' 
                    },
                    progression: { 
                        totalStarsEarned: 0, 
                        levelsFinished: [], 
                        current_level_id: '' 
                    },
                    inventory: [],
                    shop: { stars: 0, purchaseHistory: [] }
                }
            },
            update: {} // Don't change existing save
        })
        
        const token = jwt.sign(
            { id: user.id, username: studentId, role: 'Student' },
            process.env.JWT_SECRET || 'fallback-secret',
            { expiresIn: '30d' }
        )
        
        console.log('✅ Student logged in successfully')
        
        res.status(200).json({
            success: true,
            code: 200,
            message: 'Login successful',
            data: { 
                username: studentId, 
                firstName: user.firstName, 
                lastName: user.lastName, 
                token, 
                role: 'student' 
            }
        })
    } catch (error) {
        console.error('❌ Login error:', error)
        res.status(500).json({ 
            success: false,
            code: 500,
            message: 'Login failed' 
        })
    }
})