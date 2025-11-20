import { authenticate } from '@middlewares/authenticate'
import { Request, Response } from 'express'
import { prisma } from '../../lib/prisma'

export const post = [
  authenticate,
  async (req: Request, res: Response) => {
    const { moduleId, studentIds, assignToAll } = req.body
    
    let finalStudentIds: number[] = []
    
    if (assignToAll) {
      // ✅ Assign to ALL students linked to this teacher
      const teacherStudents = await prisma.teacherStudent.findMany({
        where: { 
          teacherId: req.user.id
        },
        include: {
          student: {
            select: { id: true }
          }
        }
      })
      
      finalStudentIds = teacherStudents.map(ts => ts.student.id)
      
      if (finalStudentIds.length === 0) {
        return res.status(400).json({ 
          success: false, 
          message: `No students found for your class` 
        })
      }
    } else {
      // ✅ FIXED: Assign ONLY to the specific student IDs provided
      // Convert username format (#123456789) to user IDs
      const students = await prisma.user.findMany({
        where: { 
          username: { in: studentIds },
          role: 'Student'
        },
        select: { id: true, username: true }
      })
      
      finalStudentIds = students.map(s => s.id)
      
      // Check if all provided student IDs were found
      const foundUsernames = students.map(s => s.username)
      const notFoundIds = studentIds.filter((id: string) => !foundUsernames.includes(id))
      
      if (notFoundIds.length > 0) {
        return res.status(400).json({ 
          success: false, 
          message: `Student IDs not found: ${notFoundIds.join(', ')}` 
        })
      }
      
      if (finalStudentIds.length === 0) {
        return res.status(400).json({ 
          success: false, 
          message: 'No valid student IDs provided' 
        })
      }
    }
    
    // Create assignments (skipDuplicates prevents double assignment)
    await prisma.moduleAssignment.createMany({
      data: finalStudentIds.map(sid => ({ 
        moduleId: parseInt(moduleId),
        studentId: sid
      })),
      skipDuplicates: true
    })
    
    res.json({ 
      success: true, 
      message: `Module assigned to ${finalStudentIds.length} student(s)` 
    })
  }
]