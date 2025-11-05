import { RequestHandler } from 'express'
import { validateRequestBody } from '@middlewares/request'
import { LoginSchema, RegisterSchema } from '@schemas/auth'
import { prisma } from '@utils/database'
import * as hashing from '@utils/hashing'
import { Role } from '@prisma/client'
import { StatusCodes } from 'http-status-codes'

export const login: RequestHandler = validateRequestBody(LoginSchema, async (req, res) => {
    const { username, password } = req.parsedBody

    const user = await prisma.user.findUnique({ where: { username } })

    if (!user) {
        return res.unauthorized({ message: 'Invalid username. User not found.' })
    }

    const isPasswordValid = hashing.compare(password, user.password)

    if (!isPasswordValid) {
        return res.unauthorized({ message: 'Invalid password. Please try again.' })
    }

    const token = await prisma.user.generateJwt(user.id)

    if (!token) {
        return res.error({ message: 'Failed to generate authentication token. Please try again later.' })
    }

    return res.success({
        message: 'Login successful.',
        data: {
            username: user.username,
            token,
            firstName: user.firstName,
            lastName: user.lastName
        }
    })
})

export const register: RequestHandler = validateRequestBody(RegisterSchema, async (req, res) => {
    const { teacherId, parentId, ...data } = req.parsedBody

    const existingUser = await prisma.user.findUnique({ where: { username: data.username } })

    if (existingUser) {
        return res.badRequest({ message: 'Username already taken. Please choose another one.' })
    }

    if (teacherId) {
        const teacher = await prisma.user.findUnique({ where: { id: teacherId, role: Role.Teacher } })
        if (!teacher) {
            return res.badRequest({ message: 'Invalid teacherId. Teacher not found.' })
        }
    }

    if (parentId) {
        const parent = await prisma.user.findUnique({ where: { id: parentId, role: Role.Parent } })
        if (!parent) {
            return res.badRequest({ message: 'Invalid parentId. Parent not found.' })
        }
    }

    data.password = await hashing.hash(data.password)

    const user = await prisma.user.create({
        omit: {
            password: true,
            createdAt: true,
            updatedAt: true
        },
        include: {
            save: data.role === Role.Student ? { omit: { userId: true } } : false
        },
        data: {
            ...data,
            save: data.role === Role.Student ? { create: {} } : undefined
        }
    })

    // create relation
    if (teacherId) {
        await prisma.user.update({
            where: { id: user.id },
            data: {
                asStudent: { create: { teacherId } }
            }
        })
    }

    if (parentId) {
        await prisma.user.update({
            where: { id: user.id },
            data: {
                asChild: { create: { parentId } }
            }
        })
    }

    const token = await prisma.user.generateJwt(user.id)

    if (!token) {
        return res.error({ message: 'Failed to generate authentication token. Please try again later.' })
    }

    res.cookie('access_token', token, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        sameSite: 'lax',
        maxAge: 7 * 24 * 60 * 60 * 1000 // 7 days
    })

    return res.success({
        code: StatusCodes.CREATED,
        message: 'Registration successful.',
        data: {
            username: user.username,
            firstName: user.firstName,
            lastName: user.lastName,
            token
        }
    })
})