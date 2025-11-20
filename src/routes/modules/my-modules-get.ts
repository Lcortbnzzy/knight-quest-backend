import { authenticate } from '@middlewares/authenticate'
import { Request, Response } from 'express'
import { prisma } from '../../lib/prisma'

// GET /modules/my-modules-get
// Returns modules assigned to the logged-in student
export const get = [
  authenticate,
  async (req: Request, res: Response) => {
    // ✅ Get modules assigned specifically to THIS student
    const specificAssignments = await prisma.moduleAssignment.findMany({
      where: { 
        studentId: req.user.id  // Current logged-in student
      },
      include: {
        module: {
          include: {
            questions: true,
            teacher: {
              select: { id: true }
            }
          }
        }
      }
    })
    
    // ✅ Get ALL teachers that this student is linked to
    const myTeachers = await prisma.teacherStudent.findMany({
      where: { studentId: req.user.id },
      select: { teacherId: true }
    })
    
    const teacherIds = myTeachers.map(ts => ts.teacherId)
    
    // ✅ Get modules from my teachers that are marked "assign to all"
    // (modules that have assignments for ALL students of that teacher)
    const allStudentModules = await prisma.module.findMany({
      where: {
        teacherId: { in: teacherIds }
      },
      include: {
        questions: true,
        assignments: {
          select: { studentId: true }
        }
      }
    })
    
    // ✅ Filter: "assign to all" means the module has assignments for ALL students of that teacher
    const assignedToAllModules = []
    
    for (const module of allStudentModules) {
      // Get all students of this teacher
      const teacherStudents = await prisma.teacherStudent.findMany({
        where: { teacherId: module.teacherId },
        select: { studentId: true }
      })
      
      const allStudentIds = teacherStudents.map(ts => ts.studentId)
      const assignedStudentIds = module.assignments.map((a: any) => a.studentId)
      
      // If ALL students of this teacher are assigned, it's an "assign to all" module
      const isAssignedToAll = allStudentIds.every(id => assignedStudentIds.includes(id))
      
      if (isAssignedToAll && allStudentIds.length > 0) {
        assignedToAllModules.push(module)
      }
    }
    
    // ✅ Combine specific assignments + "assign to all" modules
    const allModuleIds = new Set<number>()
    const formattedModules = []
    
    // Add specifically assigned modules
    for (const assignment of specificAssignments) {
      if (!allModuleIds.has(assignment.module.id)) {
        allModuleIds.add(assignment.module.id)
        formattedModules.push({
          Id: assignment.module.id,
          Name: assignment.module.name,
          Grade: assignment.module.grade,
          Subject: assignment.module.subject,
          Questions: assignment.module.questions,
          AssignedAt: assignment.createdAt || new Date(),
          AssignmentType: 'specific'
        })
      }
    }
    
    // Add "assign to all" modules
    for (const module of assignedToAllModules) {
      if (!allModuleIds.has(module.id)) {
        allModuleIds.add(module.id)
        formattedModules.push({
          Id: module.id,
          Name: module.name,
          Grade: module.grade,
          Subject: module.subject,
          Questions: module.questions,
          AssignedAt: module.createdAt,
          AssignmentType: 'all'
        })
      }
    }
    
    res.json({ 
      success: true, 
      data: formattedModules 
    })
  }
]