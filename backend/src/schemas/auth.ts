import { z } from 'zod'
import { Role } from '@prisma/client'

export const LoginSchema = z.object({
    username: z.string().min(3).max(30),
    password: z.string().min(6).max(100)
})

export const RegisterSchema = LoginSchema.extend({
    firstName: z.string().min(1).max(50),
    lastName: z.string().min(1).max(50),
    role: z.enum(Role).default(Role.Student),
    teacherId: z.number().min(1).optional(),
    parentId: z.number().min(1).optional()
})