// ===== student-register.ts =====
import { prisma } from '@utils/database'
import { z } from 'zod'
import { validateRequestBody } from '@middlewares/request'
import jwt from 'jsonwebtoken'

const StudentRegisterSchema = z.object({
    firstName: z.string().min(1),
    lastName: z.string().min(1),
    studentId: z.string().regex(/^#\d{9}$/),
    role: z.literal('student')
})

export const POST = validateRequestBody(StudentRegisterSchema, async (req, res): Promise<void> => {
    const { firstName, lastName, studentId } = req.parsedBody
    
    console.log('🔐 POST /auth/student (REGISTER)')
    console.log('Student ID:', studentId)
    
    try {
        const existing = await prisma.user.findFirst({
            where: { username: studentId }
        })
        
        if (existing) {
            res.status(400).json({
                success: false,
                code: 400,
                message: 'Student ID already registered.'
            })
            return
        }
        
        const user = await prisma.user.create({
            data: {
                username: studentId,
                password: '',
                firstName,
                lastName,
                role: 'Student',
            }
        })
        
        // ❌ NO save creation here - will be created on first login
        
        const token = jwt.sign(
            { id: user.id, username: studentId, role: 'Student' },
            process.env.JWT_SECRET || 'fallback-secret',
            { expiresIn: '30d' }
        )
        
        console.log('✅ Student registered successfully')
        
        res.status(201).json({
            success: true,
            code: 201,
            message: 'Success',
            data: { username: studentId, firstName, lastName, token, role: 'student' }
        })
    } catch (error) {
        console.error('❌ Registration error:', error)
        res.status(500).json({ 
            success: false,
            code: 500,
            message: 'Registration failed' 
        })
    }
})