export type Account = {
    token: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string;
};

export type LevelCompletion = {
    id: string;
    duration: number;
    starsEarned: number;
    completedAt: string;
};

export type Progression = {
    totalStarsEarned: number;
    levelsFinished: LevelCompletion[];
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