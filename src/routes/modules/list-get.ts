import { authenticate } from '@middlewares/authenticate'
import { Request, Response } from 'express'
import { prisma } from '../../lib/prisma'

export const get = [
  authenticate,
  async (req: Request, res: Response) => {
    const modules = await prisma.module.findMany({
      where: { teacherId: req.user.id },
      include: {
        questions: true,
        assignments: {
          include: {
            student: {
              select: { 
                id: true,
                username: true  // ✅ Get the student ID
              }
            }
          }
        }
      }
    })
    
    const formattedModules = modules.map((m: any) => ({
      Id: m.id,
      Name: m.name,
      Grade: m.grade,
      Subject: m.subject,
      Questions: m.questions,
      // ✅ Return username (student ID like #333333333)
      AssignedStudents: m.assignments.map((a: any) => a.student.username)
    }))
    
    res.json({ success: true, data: formattedModules })
  }
]