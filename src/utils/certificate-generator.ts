import puppeteer from 'puppeteer'
import handlebars from 'handlebars'
import fs from 'fs/promises'
import path from 'path'

interface CertificateData {
    certificateNumber: string
    studentName: string
    gradeLevel: string
    achievement: string
    issuedBy: string
    issuedDate: string
}

// 🎨 Certificate HTML Template (Medieval Knight Quest Theme)
const certificateTemplate = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Certificate of Achievement</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Cinzel:wght@400;700&family=MedievalSharp&display=swap');
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            width: 297mm;
            height: 210mm;
            margin: 0;
            padding: 0;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            font-family: 'Cinzel', serif;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .certificate-container {
            width: 280mm;
            height: 190mm;
            background: linear-gradient(to bottom, #f5e6d3 0%, #ede0ce 100%);
            border: 15px solid #8B4513;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5),
                        inset 0 0 50px rgba(139, 69, 19, 0.1);
            padding: 30px;
            position: relative;
            overflow: hidden;
        }
        
        .certificate-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y="50" font-size="80" opacity="0.03" fill="%238B4513">⚔️</text></svg>') repeat;
            background-size: 100px 100px;
            opacity: 0.3;
            pointer-events: none;
        }
        
        .inner-border {
            border: 3px solid #DAA520;
            border-radius: 15px;
            padding: 40px;
            height: 100%;
            background: rgba(255, 255, 255, 0.4);
            position: relative;
        }
        
        .corner-decoration {
            position: absolute;
            width: 60px;
            height: 60px;
            font-size: 40px;
        }
        
        .corner-top-left { top: 10px; left: 10px; }
        .corner-top-right { top: 10px; right: 10px; }
        .corner-bottom-left { bottom: 10px; left: 10px; }
        .corner-bottom-right { bottom: 10px; right: 10px; }
        
        .shield-emblem {
            text-align: center;
            font-size: 80px;
            margin: 10px 0;
            filter: drop-shadow(0 4px 6px rgba(0, 0, 0, 0.3));
        }
        
        .certificate-title {
            text-align: center;
            font-family: 'MedievalSharp', cursive;
            font-size: 48px;
            color: #8B4513;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
            margin: 15px 0;
            letter-spacing: 3px;
        }
        
        .subtitle {
            text-align: center;
            font-size: 20px;
            color: #555;
            margin-bottom: 30px;
            font-style: italic;
        }
        
        .student-name {
            text-align: center;
            font-size: 42px;
            font-weight: 700;
            color: #1a1a2e;
            margin: 25px 0;
            padding: 15px;
            border-bottom: 3px solid #DAA520;
            text-transform: uppercase;
            letter-spacing: 2px;
            text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.1);
        }
        
        .achievement-text {
            text-align: center;
            font-size: 18px;
            color: #333;
            line-height: 1.8;
            margin: 25px 0;
            padding: 0 40px;
        }
        
        .grade-badge {
            text-align: center;
            font-size: 24px;
            font-weight: 700;
            color: #8B4513;
            margin: 20px 0;
            padding: 10px;
            background: linear-gradient(135deg, #FFD700 0%, #FFA500 100%);
            border-radius: 30px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
            display: inline-block;
            width: fit-content;
            margin-left: 50%;
            transform: translateX(-50%);
            padding: 12px 40px;
        }
        
        .footer {
            display: flex;
            justify-content: space-between;
            margin-top: 40px;
            padding: 0 40px;
        }
        
        .signature-block {
            text-align: center;
            flex: 1;
        }
        
        .signature-line {
            border-top: 2px solid #333;
            margin-bottom: 8px;
            padding-top: 5px;
        }
        
        .signature-label {
            font-size: 14px;
            color: #666;
            font-style: italic;
        }
        
        .certificate-number {
            text-align: center;
            font-size: 12px;
            color: #999;
            margin-top: 20px;
            font-family: monospace;
        }
        
        .seal {
            position: absolute;
            bottom: 40px;
            right: 80px;
            width: 100px;
            height: 100px;
            border-radius: 50%;
            background: radial-gradient(circle, #FFD700 0%, #DAA520 100%);
            border: 5px solid #8B4513;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 50px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            transform: rotate(-15deg);
        }
    </style>
</head>
<body>
    <div class="certificate-container">
        <div class="inner-border">
            <div class="corner-decoration corner-top-left">⚔️</div>
            <div class="corner-decoration corner-top-right">🛡️</div>
            <div class="corner-decoration corner-bottom-left">👑</div>
            <div class="corner-decoration corner-bottom-right">⚔️</div>
            
            <div class="shield-emblem">🏰</div>
            
            <h1 class="certificate-title">CERTIFICATE OF ACHIEVEMENT</h1>
            <p class="subtitle">Knight Quest Academy</p>
            
            <div class="achievement-text">
                This hereby certifies that
            </div>
            
            <div class="student-name">{{studentName}}</div>
            
            <div class="achievement-text">
                Has successfully completed the noble quest of
            </div>
            
            <div class="grade-badge">⭐ GRADE {{gradeLevel}} ⭐</div>
            
            <div class="achievement-text">
                {{achievement}}
            </div>
            
            <div class="footer">
                <div class="signature-block">
                    <div class="signature-line">{{issuedBy}}</div>
                    <div class="signature-label">Issued By</div>
                </div>
                
                <div class="signature-block">
                    <div class="signature-line">{{issuedDate}}</div>
                    <div class="signature-label">Date Issued</div>
                </div>
            </div>
            
            <div class="certificate-number">Certificate No: {{certificateNumber}}</div>
            
            <div class="seal">🎖️</div>
        </div>
    </div>
</body>
</html>
`

export async function generateCertificatePDF(data: CertificateData): Promise<string> {
    console.log('🎨 Generating certificate PDF for:', data.studentName)
    
    try {
        // Compile template
        const template = handlebars.compile(certificateTemplate)
        const html = template(data)
        
        // Ensure certificates directory exists
        const certificatesDir = path.join(process.cwd(), 'certificates')
        await fs.mkdir(certificatesDir, { recursive: true })
        
        // Generate PDF filename
        const fileName = `${data.certificateNumber}.pdf`
        const filePath = path.join(certificatesDir, fileName)
        
        // Launch puppeteer
        const browser = await puppeteer.launch({
            headless: true,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        })
        
        const page = await browser.newPage()
        
        // Set content and wait for it to load
        await page.setContent(html, {
            waitUntil: 'networkidle0'
        })
        
        // Generate PDF
        await page.pdf({
            path: filePath,
            format: 'A4',
            landscape: true,
            printBackground: true,
            preferCSSPageSize: true
        })
        
        await browser.close()
        
        console.log('✅ Certificate PDF generated:', filePath)
        
        // Return relative path for database storage
        return `certificates/${fileName}`
    } catch (error) {
        console.error('❌ Error generating certificate PDF:', error)
        throw new Error('Failed to generate certificate PDF')
    }
}