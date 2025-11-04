import { Prisma } from '@prisma/client';
import jwt from '@utils/jwt'
import { prisma } from '@utils/database'

export const user = Prisma.defineExtension({
    model: {
        user: {
            async generateJwt(userId: number) {
                const user = await prisma.user.findUnique({
                    where: { id: userId },
                    omit: {
                        password: true,
                        createdAt: true,
                        updatedAt: true
                    }
                })

                if (!user)
                    return null;

                return jwt.generateToken(user, { expiresIn: '7d' });
            }
        }
    },
    result: {
        user: {
            name: {
                needs: {
                    firstName: true,
                    lastName: true
                },
                compute: ({ firstName, lastName }) => `${ firstName } ${ lastName }`
            }
        }
    }
})