import { Handler } from 'express'
import { generateCertificate } from '@utils/certificate'

export const downloadCertificate: Handler = async (req, res, next) => {
    const pdfBuffer = await generateCertificate({
        username: req.user.username,
        firstName: req.user.firstName,
        lastName: req.user.lastName
    })

    const filename = `certificate-${ req.user.username }.pdf`

    res.setHeader('Content-Type', 'application/pdf')
    res.setHeader('Content-Disposition', `attachment; filename="${ filename }"`)
    res.setHeader('Content-Length', pdfBuffer.length)

    res.send(pdfBuffer)
}

