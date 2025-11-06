-- CreateTable
CREATE TABLE "public"."auth_pins" (
    "pin" VARCHAR(6) NOT NULL,
    "token" TEXT NOT NULL,
    "username" VARCHAR(255) NOT NULL,
    "first_name" VARCHAR(255),
    "last_name" VARCHAR(255),
    "expires_at" TIMESTAMP(3) NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "auth_pins_pkey" PRIMARY KEY ("pin")
);

-- CreateIndex
CREATE INDEX "idx_auth_pins_expires_at" ON "public"."auth_pins"("expires_at");
