import { Request, Response } from 'express'
import { prisma } from '@utils/database'
import { v4 as uuidv4 } from 'uuid'
import fs from 'fs/promises'
import path from 'path'
import { z } from 'zod'
import PDFDocument from 'pdfkit'

// Validation Schema
const CreateCertificateSchema = z.object({
    studentId: z.string(),
    gradeLevel: z.string(),
    achievement: z.string(),
    issuedBy: z.string()
})

// GET /certificates/student/:studentId
// Verify student exists and return basic info
export async function getStudentInfo(req: Request, res: Response): Promise<void> {
    let { studentId } = req.params
    
    console.log('🔍 GET /certificates/student/' + studentId)
    
    try {
        // FIX: Ensure # prefix for database lookup
        if (!studentId.startsWith('#')) {
            studentId = '#' + studentId
            console.log('🔧 Added # prefix, searching for:', studentId)
        }
        
        // Find user by username (student ID format: #123456789)
        const student = await prisma.user.findFirst({
            where: {
                username: studentId,
                role: 'Student'
            },
            select: {
                id: true,
                username: true,
                firstName: true,
                lastName: true
            }
        })
        
        if (!student) {
            console.log('❌ Student not found with username:', studentId)
            res.status(404).json({
                success: false,
                code: 404,
                message: 'Student not found',
                errorCode: 'STUDENT_NOT_FOUND'
            })
            return
        }
        
        console.log('✅ Student found:', student.firstName, student.lastName)
        
        res.status(200).json({
            success: true,
            code: 200,
            message: 'Student verified',
            data: {
                studentId: student.id.toString(),
                firstName: student.firstName,
                lastName: student.lastName,
                fullName: `${student.firstName} ${student.lastName}`
            }
        })
    } catch (error) {
        console.error('❌ Error verifying student:', error)
        res.status(500).json({
            success: false,
            code: 500,
            message: 'Failed to verify student',
            errorCode: 'VERIFICATION_ERROR'
        })
    }
}

// POST /certificates
// Create a new certificate
export async function createCertificate(req: Request, res: Response): Promise<void> {
    console.log('📝 POST /certificates')
    
    try {
        // Validate request body
        const validation = CreateCertificateSchema.safeParse(req.body)
        
        if (!validation.success) {
            console.log('❌ Validation failed:', validation.error.issues)
            res.status(400).json({
                success: false,
                code: 400,
                message: 'Invalid request data',
                errors: validation.error.issues
            })
            return
        }
        
        const { studentId, gradeLevel, achievement, issuedBy } = validation.data
        
        console.log('Student ID:', studentId)
        console.log('Grade Level:', gradeLevel)
        
        // Get student info by ID
        const student = await prisma.user.findUnique({
            where: { id: parseInt(studentId) },
            select: {
                id: true,
                firstName: true,
                lastName: true,
                username: true,
                role: true
            }
        })
        
        if (!student) {
            console.log('❌ Student not found with ID:', studentId)
            res.status(404).json({
                success: false,
                code: 404,
                message: 'Student not found',
                errorCode: 'STUDENT_NOT_FOUND'
            })
            return
        }
        
        if (student.role !== 'Student') {
            console.log('❌ User is not a student:', student.role)
            res.status(400).json({
                success: false,
                code: 400,
                message: 'User is not a student',
                errorCode: 'INVALID_ROLE'
            })
            return
        }
        
        console.log('✅ Creating certificate for:', student.firstName, student.lastName)
        
        // Generate unique certificate number
        const certificateNumber = `KQ-${Date.now()}-${uuidv4().substring(0, 8).toUpperCase()}`
        
        // Create certificate record in database
        const certificate = await prisma.certificate.create({
            data: {
                certificateNumber,
                studentId: student.id,
                gradeLevel,
                achievement,
                issuedBy,
                pdfUrl: null
            }
        })
        
        console.log('✅ Certificate record created:', certificateNumber)
        
        res.status(201).json({
            success: true,
            code: 201,
            message: 'Certificate created successfully',
            data: {
                certificateId: certificate.id,
                certificateNumber,
                studentName: `${student.firstName} ${student.lastName}`,
                gradeLevel,
                issuedAt: certificate.createdAt.toISOString()
            }
        })
    } catch (error) {
        console.error('❌ Error creating certificate:', error)
        res.status(500).json({
            success: false,
            code: 500,
            message: 'Failed to create certificate',
            errorCode: 'CREATION_ERROR'
        })
    }
}

// GET /certificates/download/:certificateNumber
// Generate and download personalized certificate PDF with student name
export async function downloadCertificate(req: Request, res: Response): Promise<void> {
    const { certificateNumber } = req.params
    
    console.log('📥 GET /certificates/download/' + certificateNumber)
    
    try {
        // Get certificate from database
        const certificate = await prisma.certificate.findUnique({
            where: { certificateNumber },
            include: {
                student: {
                    select: {
                        firstName: true,
                        lastName: true
                    }
                }
            }
        })
        
        if (!certificate) {
            console.log('❌ Certificate not found:', certificateNumber)
            res.status(404).json({
                success: false,
                code: 404,
                message: 'Certificate not found',
                errorCode: 'CERTIFICATE_NOT_FOUND'
            })
            return
        }
        
        const fullName = `${certificate.student.firstName} ${certificate.student.lastName}`.toUpperCase()
        const gradeLevel = certificate.gradeLevel
        
        console.log('✅ Generating PDF for:', fullName, '- Grade', gradeLevel)
        
        // Path to certificate template PNG
        const templatePath = path.join(process.cwd(), 'certificates', `GRADE-${gradeLevel}-CERTIFICATE.png`)
        
        // Check if template exists
        try {
            await fs.access(templatePath)
            console.log('✅ Template found:', templatePath)
        } catch {
            console.error('❌ Template not found:', templatePath)
            res.status(404).json({
                success: false,
                code: 404,
                message: `Certificate template for Grade ${gradeLevel} not found`,
                errorCode: 'TEMPLATE_NOT_FOUND'
            })
            return
        }
        
        // Create PDF document (Letter landscape: 11" x 8.5" = 792 x 612 points)
        const doc = new PDFDocument({
            size: [792, 612],
            margins: { top: 0, bottom: 0, left: 0, right: 0 }
        })
        
        // Set response headers for PDF download
        const fileName = `Certificate_Grade${gradeLevel}_${certificate.student.firstName}_${certificate.student.lastName}_${Date.now()}.pdf`
        res.setHeader('Content-Type', 'application/pdf')
        res.setHeader('Content-Disposition', `attachment; filename="${fileName}"`)
        
        // Pipe PDF directly to response
        doc.pipe(res)
        
        // Add background image (certificate template)
        doc.image(templatePath, 0, 0, {
            width: 792,
            height: 612
        })
        
        // Add student name text
        // IMPORTANT: Adjust Y value (220) based on your template design
        // Try different values: 180, 200, 220, 240, 260 until position is correct
        doc.font('Helvetica-Bold')
           .fontSize(28)
           .fillColor('#000000')
           .text(fullName, 0, 220, {
               width: 792,
               align: 'center'
           })
        
        // Add certificate number at bottom
        doc.font('Helvetica')
           .fontSize(10)
           .fillColor('#666666')
           .text(`Certificate No: ${certificate.certificateNumber}`, 0, 570, {
               width: 792,
               align: 'center'
           })
        
        // Add issue date
        const issueDate = new Date(certificate.createdAt).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        })
        doc.fontSize(10)
           .text(`Issued: ${issueDate}`, 0, 585, {
               width: 792,
               align: 'center'
           })
        
        // Finalize PDF
        doc.end()
        
        console.log('✅ PDF generated and sent:', fileName)
        
    } catch (error) {
        console.error('❌ Error generating certificate PDF:', error)
        
        // Check if headers already sent
        if (!res.headersSent) {
            res.status(500).json({
                success: false,
                code: 500,
                message: 'Failed to generate certificate PDF',
                errorCode: 'PDF_GENERATION_ERROR'
            })
        }
    }
}

// GET /certificates?studentId=123
// Get all certificates for a student
export async function listStudentCertificates(req: Request, res: Response): Promise<void> {
    const { studentId } = req.query
    
    if (!studentId) {
        res.status(400).json({
            success: false,
            code: 400,
            message: 'studentId query parameter is required',
            errorCode: 'MISSING_PARAMETER'
        })
        return
    }
    
    try {
        const certificates = await prisma.certificate.findMany({
            where: {
                studentId: parseInt(studentId as string)
            },
            orderBy: {
                createdAt: 'desc'
            },
            select: {
                id: true,
                certificateNumber: true,
                gradeLevel: true,
                achievement: true,
                issuedBy: true,
                createdAt: true,
                pdfUrl: true
            }
        })
        
        console.log(`✅ Found ${certificates.length} certificates for student ${studentId}`)
        
        res.status(200).json({
            success: true,
            code: 200,
            data: certificates
        })
    } catch (error) {
        console.error('❌ Error fetching certificates:', error)
        res.status(500).json({
            success: false,
            code: 500,
            message: 'Failed to fetch certificates',
            errorCode: 'FETCH_ERROR'
        })
    }
}

// POST /certificates/convert-to-pdf
// Convert screenshot image to PDF
export async function convertImageToPDF(req: Request, res: Response): Promise<void> {
    console.log('🔄 POST /certificates/convert-to-pdf')
    
    try {
        const { image, studentName, gradeLevel, certificateNumber, width, height } = req.body
        
        if (!image) {
            res.status(400).json({
                success: false,
                code: 400,
                message: 'Image data is required'
            })
            return
        }
        
        console.log('📥 Received image data, converting to PDF...')
        console.log(`Image dimensions: ${width || 1920}x${height || 1080}`)
        
        // Decode base64 image
        const imageBuffer = Buffer.from(image, 'base64')
        
        // PDF dimensions (Letter landscape: 792 x 612 points)
        const pdfWidth = 792
        const pdfHeight = 612
        
        // Create PDF document
        const doc = new PDFDocument({
            size: [pdfWidth, pdfHeight],
            margins: { top: 0, bottom: 0, left: 0, right: 0 }
        })
        
        // Collect PDF chunks
        const chunks: Buffer[] = []
        doc.on('data', (chunk) => chunks.push(chunk))
        
        // Create promise to wait for PDF completion
        const pdfPromise = new Promise<Buffer>((resolve, reject) => {
            doc.on('end', () => resolve(Buffer.concat(chunks)))
            doc.on('error', reject)
        })
        
        // Add the screenshot image to PDF (scaled to fit)
        doc.image(imageBuffer, 0, 0, {
            width: pdfWidth,
            height: pdfHeight,
            fit: [pdfWidth, pdfHeight],
            align: 'center',
            valign: 'center'
        })
        
        // Add metadata
        doc.info.Title = `Certificate - ${studentName} - Grade ${gradeLevel}`
        doc.info.Author = 'KnightQuest'
        doc.info.Subject = `Grade ${gradeLevel} Certificate`
        if (certificateNumber) {
            doc.info.Keywords = `certificate, grade${gradeLevel}, ${certificateNumber}`
        }
        
        // Finalize PDF
        doc.end()
        
        // Wait for PDF to be generated
        const pdfBuffer = await pdfPromise
        
        console.log(`✅ PDF generated: ${pdfBuffer.length} bytes`)
        
        // Set response headers
        const safeName = studentName.replace(/\s/g, '_')
        const fileName = `Certificate_Grade${gradeLevel}_${safeName}_${Date.now()}.pdf`
        res.setHeader('Content-Type', 'application/pdf')
        res.setHeader('Content-Disposition', `attachment; filename="${fileName}"`)
        res.setHeader('Content-Length', pdfBuffer.length)
        
        // Send PDF
        res.send(pdfBuffer)
        
    } catch (error) {
        console.error('❌ Error converting image to PDF:', error)
        
        if (!res.headersSent) {
            res.status(500).json({
                success: false,
                code: 500,
                message: 'Failed to convert image to PDF',
                errorCode: 'CONVERSION_ERROR'
            })
        }
    }
}