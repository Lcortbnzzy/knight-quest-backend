import { authenticate } from '@middlewares/authenticate'
import { Request, Response } from 'express'
import { prisma } from '../../lib/prisma'

export const post = [
  authenticate,
  async (req: Request, res: Response) => {
    const { name, grade, subject, questions } = req.body
    
    const module = await prisma.module.create({
      data: {
        name,
        grade,
        subject,
        questions: {
          create: questions
        },
        teacherId: req.user.id
      }
    })
    
    res.json({ success: true, data: { moduleId: module.id } })
  }
]