import { PrismaClient } from '@prisma/client'
import { generateToken } from './jwt'
import { randomUUID } from 'crypto'

// Extend Prisma Client with custom methods
const prismaClientSingleton = () => {
    return new PrismaClient().$extends({
        model: {
            user: {
                async generateJwt(userId: number) {
                    const user = await prisma.user.findUnique({
                        where: { id: userId },
                        omit: { password: true }
                    })

                    if (!user) return null

                    const sessionId = randomUUID()

                    const token = generateToken(
                        {
                            ...user,
                            sessionId
                        },
                        {
                            expiresIn: '7d' // Token expires in 7 days
                        }
                    )

                    return token
                }
            }
        }
    })
}

declare global {
    var prismaGlobal: undefined | ReturnType<typeof prismaClientSingleton>
}

export const prisma = globalThis.prismaGlobal ?? prismaClientSingleton()

if (process.env.NODE_ENV !== 'production') {
    globalThis.prismaGlobal = prisma
}