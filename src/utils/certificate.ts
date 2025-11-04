import { PDFDocument, rgb, StandardFonts } from 'pdf-lib'
import * as fs from 'node:fs/promises'
import * as path from 'node:path'
import { fileURLToPath } from 'node:url'

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export interface CertificateData {
    username: string
    firstName: string
    lastName: string
}

export const generateCertificate = async (userData: CertificateData): Promise<Buffer> => {
    const templatePath = path.join(__dirname, '../../assets/templates/certificate-template.pdf')
    const existingPdfBytes = await fs.readFile(templatePath)

    const pdfDoc = await PDFDocument.load(existingPdfBytes)

    const font = await pdfDoc.embedFont(StandardFonts.HelveticaBold)

    const pages = pdfDoc.getPages()
    const firstPage = pages[0]
    const { width, height } = firstPage.getSize()

    const fullName = `${ userData.firstName } ${ userData.lastName }`

    const fontSize = 36
    const textWidth = font.widthOfTextAtSize(fullName, fontSize)

    firstPage.drawText(fullName, {
        x: (width - textWidth) / 2,
        y: (height / 2) - 50,
        size: fontSize,
        font: font,
        color: rgb(0, 0, 0)
    })

    const usernameFontSize = 20
    const usernameText = `@${ userData.username }`
    const usernameWidth = font.widthOfTextAtSize(usernameText, usernameFontSize)

    firstPage.drawText(usernameText, {
        x: (width - usernameWidth) / 2,
        y: (height / 2) - 100,
        size: usernameFontSize,
        font: font,
        color: rgb(0.3, 0.3, 0.3)
    })

    const pdfBytes = await pdfDoc.save()

    return Buffer.from(pdfBytes)
}

