console.log('🎯🎯🎯 SAVE.TS LOADED AT:', new Date().toISOString());
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

    // ✅ LOG EVERYTHING
    console.error('🔥🔥🔥 RAW DB DATA 🔥🔥🔥')  // Using console.error so it ALWAYS shows
    console.error(JSON.stringify(save?.data, null, 2))
    console.error('🔥🔥🔥 END RAW DATA 🔥🔥🔥')

    res.ok({
        message: 'Save data retrieved successfully.',
        data: save?.data
    })
}

// PUT /save - I-update ang save data ng user
export const updateSave = validateRequestBody(SaveSchema, async (req, res) => {
    // ABSOLUTE FIRST THING - NO DEPENDENCIES
    console.log('🔥🔥🔥🔥🔥 PUT /save HIT! 🔥🔥🔥🔥🔥');
    console.log('🔥🔥🔥🔥🔥 PUT /save HIT! 🔥🔥🔥🔥🔥');
    console.log('🔥🔥🔥🔥🔥 PUT /save HIT! 🔥🔥🔥🔥🔥');
    
    const { user } = req
    const saveData = req.parsedBody
    
    console.log('========================================')
    console.log('=== PUT /save CALLED ===')
    console.log('========================================')
    console.log('User ID:', user!.id)
    console.log('Request body keys:', Object.keys(req.body))
    
    // Log the INCOMING data
    console.log('--- INCOMING SAVE DATA ---')
    console.log('Full body:', JSON.stringify(req.body, null, 2))
    console.log('Parsed body:', JSON.stringify(saveData, null, 2))
    
    // Check progression details
    if (saveData.progression) {
        console.log('--- Progression Details ---')
        console.log('progression keys:', Object.keys(saveData.progression))
        console.log('totalStarsEarned:', saveData.progression.totalStarsEarned)
        console.log('total_stars_earned:', (saveData.progression as any).total_stars_earned)
        console.log('Has LevelPerformance?', !!(saveData.progression as any).LevelPerformance)
        console.log('Has levelPerformance?', !!(saveData.progression as any).levelPerformance)
        console.log('Has level_performance?', !!(saveData.progression as any).level_performance)
        
        // Log each possible performance property
        const perf1 = (saveData.progression as any).LevelPerformance
        const perf2 = (saveData.progression as any).levelPerformance
        const perf3 = (saveData.progression as any).level_performance
        
        if (perf1) console.log('LevelPerformance count:', perf1.length)
        if (perf2) console.log('levelPerformance count:', perf2.length)
        if (perf3) console.log('level_performance count:', perf3.length)
        
        // Log the actual performance data
        const perfData = perf1 || perf2 || perf3
        if (perfData && perfData.length > 0) {
            console.log('--- Performance Entries ---')
            perfData.forEach((entry: any, index: number) => {
                console.log(`Entry ${index}:`, JSON.stringify(entry, null, 2))
            })
        } else {
            console.log('⚠️ NO PERFORMANCE DATA FOUND IN ANY FORMAT!')
        }
    } else {
        console.log('⚠️ NO PROGRESSION IN SAVE DATA!')
    }
    
    const save = await prisma.save.findUnique({ where: { userId: user!.id } })

    if (!save) {
        console.log('❌ Save not found for user')
        return res.notFound({ message: 'Save data not found for the user.' })
    }

    console.log('--- BEFORE DATABASE UPDATE ---')
    const beforeSave = await prisma.save.findUnique({
        where: { userId: user!.id }
    })
    console.log('Current DB data:', JSON.stringify(beforeSave?.data, null, 2))

    // I-save sa database
    console.log('--- UPDATING DATABASE ---')
    console.log('Data being saved:', JSON.stringify(saveData, null, 2))
    
    const updatedSave = await prisma.save.update({
        where: { userId: user!.id },
        data: { data: saveData },
        omit: { userId: true }
    })

    console.log('--- AFTER DATABASE UPDATE ---')
    console.log('Updated save data:', JSON.stringify(updatedSave.data, null, 2))
    
    // Verify the save
    const verifyCheck = await prisma.save.findUnique({
        where: { userId: user!.id }
    })
    console.log('--- VERIFICATION CHECK ---')
    console.log('DB now contains:', JSON.stringify(verifyCheck?.data, null, 2))
    
    if (verifyCheck?.data) {
        const vData = verifyCheck.data as any
        console.log('Verification - Has performance?', 
            !!(vData.progression?.level_performance || vData.progression?.levelPerformance || vData.progression?.LevelPerformance))
    }
    
    console.log('✅ Save update completed')
    console.log('========================================')

    res.ok({
        message: 'Save data updated successfully.',
        data: updatedSave.data
    })
})

// DELETE /save - I-reset ang save data to default
export const resetSave: RequestHandler = async (req, res) => {
    const { user } = req

    console.log('========================================')
    console.log('=== DELETE /save CALLED ===')
    console.log('User ID:', user!.id)
    console.log('========================================')

    const save = await prisma.save.findUnique({ where: { userId: user!.id } })

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