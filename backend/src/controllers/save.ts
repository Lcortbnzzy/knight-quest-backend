import { RequestHandler } from 'express'
import { prisma } from '@utils/database'
import { validateRequestBody } from '@middlewares/request'
import { SaveSchema } from '@schemas/save'

export const getSave: RequestHandler = async (req, res) => {
    const { user } = req

    const save = await prisma.save.findUnique({
        where: { userId: user!.id },
        omit: { userId: true }
    })

    if (!save) {
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    res.ok({
        message: 'Save data retrieved successfully.',
        data: save.data
    })
}

export const updateSave = validateRequestBody(SaveSchema, async (req, res) => {
    const { user } = req
    const saveData = req.parsedBody

    const save = await prisma.save.exists({ userId: user.id })

    if (!save) {
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    const updatedSave = await prisma.save.update({
        where: { userId: user.id },
        data: { data: saveData },
        omit: { userId: true }
    })

    res.ok({
        message: 'Save data updated successfully.',
        data: updatedSave
    })
})

export const resetSave: RequestHandler = async (req, res) => {
    const { user } = req

    const save = await prisma.save.exists({ userId: user.id })

    if (!save) {
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    await prisma.$executeRaw`
        UPDATE "saves"
        SET data = DEFAULT
        WHERE "userId" = ${ user.id }
    `

    res.ok({ message: 'Save data reset to default successfully.' })
}