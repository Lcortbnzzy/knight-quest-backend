import { RequestHandler } from 'express'
import { prisma } from '@utils/database'
import { validateRequestBody } from '@middlewares/request'
import { SaveSchema } from '@schemas/save'

// GET /save - Get user's save data
export const getSave: RequestHandler = async (req, res) => {
    const { user } = req
    
    if (!user || !user.id) {
        return res.unauthorized({ message: 'Authentication required' })
    }
    
    const save = await prisma.save.findUnique({
        where: { userId: user.id },
        omit: { userId: true }
    })

    console.log('📍 GET /save - User:', user.id)
    console.log('📦 Save data:', JSON.stringify(save?.data, null, 2))  // ← ADD THIS

    res.ok({
        message: 'Save data retrieved successfully.',
        data: save?.data
    })
}

// PUT /save - Update user's save data
export const updateSave = validateRequestBody(SaveSchema, async (req, res) => {
    console.log('🔥 PUT /save called')
    
    const { user } = req
    const saveData = req.parsedBody
    
    // Check if user is authenticated
    if (!user || !user.id) {
        return res.unauthorized({ message: 'Authentication required' })
    }
    
    console.log('========================================')
    console.log('=== PUT /save CALLED ===')
    console.log('User ID:', user.id)
    console.log('Request body keys:', Object.keys(req.body))
    
    // Log the INCOMING data
    console.log('--- INCOMING SAVE DATA ---')
    console.log('Parsed body keys:', Object.keys(saveData))
    
    // Check progression details
    if (saveData.progression) {
        console.log('--- Progression Details ---')
        console.log('Progression keys:', Object.keys(saveData.progression))
        console.log('Total stars earned:', saveData.progression.totalStarsEarned)
        
        // Check for performance data in different formats
        const perf1 = (saveData.progression as any).LevelPerformance
        const perf2 = (saveData.progression as any).levelPerformance
        const perf3 = (saveData.progression as any).level_performance
        
        if (perf1) console.log('✅ LevelPerformance count:', perf1.length)
        if (perf2) console.log('✅ levelPerformance count:', perf2.length)
        if (perf3) console.log('✅ level_performance count:', perf3.length)
        
        // Log the actual performance data
        const perfData = perf1 || perf2 || perf3
        if (perfData && perfData.length > 0) {
            console.log(`📊 Found ${perfData.length} performance entries`)
        } else {
            console.log('⚠️ NO PERFORMANCE DATA FOUND')
        }
    } else {
        console.log('⚠️ NO PROGRESSION IN SAVE DATA')
    }
    
    const save = await prisma.save.findUnique({ where: { userId: user.id } })

    if (!save) {
        console.log('❌ Save not found for user')
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    console.log('--- UPDATING DATABASE ---')
    
    const updatedSave = await prisma.save.update({
        where: { userId: user.id },
        data: { data: saveData },
        omit: { userId: true }
    })

    console.log('✅ Save updated successfully')
    console.log('========================================')

    res.ok({
        message: 'Save data updated successfully.',
        data: updatedSave.data
    })
})

// DELETE /save - Reset user's save data to default
export const resetSave: RequestHandler = async (req, res) => {
    const { user } = req
    
    // Check if user is authenticated
    if (!user || !user.id) {
        return res.unauthorized({ message: 'Authentication required' })
    }

    console.log('========================================')
    console.log('=== DELETE /save CALLED ===')
    console.log('User ID:', user.id)
    console.log('========================================')

    const save = await prisma.save.findUnique({ where: { userId: user.id } })

    if (!save) {
        console.log('❌ Save not found for user')
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    await prisma.$executeRaw`
        UPDATE "saves"
        SET data = DEFAULT
        WHERE "userId" = ${user.id}
    `

    console.log('✅ Save reset completed')
    res.ok({ message: 'Save data reset to default successfully.' })
}