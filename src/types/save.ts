export type Account = {
    token: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string | null;
};

export type PerformanceStats = {
    totalturns: number;
    remaininghealth: number;
    maxhealth: number;
    healthpercentage: number;
    correctanswers: number;
    wronganswers: number;
    accuracy: number;
    totaldamage_dealt: number;  // ✅ WITH UNDERSCORE!
    totaldamage_taken: number;  // ✅ WITH UNDERSCORE!
};

export type LevelCompletion = {
    id: string;
    duration: number;
    starsEarned: number;
    completedAt: string;
    performance?: PerformanceStats;
};

export type Progression = {
    totalStarsEarned: number;
    levelsFinished: LevelCompletion[];
    current_level_id?: string;
};

export type InventoryItem = {
    id: string;
    quantity: number;
    acquiredAt: string;
};

export type PurchaseHistoryItem = {
    id: string;
    quantity: number;
    cost: number;
    purchasedAt: string;
};

export type Shop = {
    stars: number;
    purchaseHistory: PurchaseHistoryItem[];
};

export type Save = {
    account?: Account;
    progression: Progression;
    inventory: InventoryItem[];
    shop: Shop;
};