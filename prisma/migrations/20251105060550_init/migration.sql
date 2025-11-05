-- CreateEnum
CREATE TYPE "Role" AS ENUM ('Student', 'Teacher', 'Parent', 'Admin');

-- CreateTable
CREATE TABLE "users" (
    "id" SERIAL NOT NULL,
    "username" TEXT NOT NULL,
    "password" TEXT NOT NULL,
    "firstName" TEXT NOT NULL,
    "lastName" TEXT NOT NULL,
    "role" "Role" NOT NULL DEFAULT 'Student',
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "users_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "teacher_students" (
    "id" SERIAL NOT NULL,
    "teacherId" INTEGER NOT NULL,
    "studentId" INTEGER NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "teacher_students_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "parent_child" (
    "id" SERIAL NOT NULL,
    "parentId" INTEGER NOT NULL,
    "childId" INTEGER NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "parent_child_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "saves" (
    "userId" INTEGER NOT NULL,
    "data" JSONB NOT NULL DEFAULT '{"account":null,"progression":{"totalStarsEarned":0,"levelsFinished":[],"current_level_id":""},"inventory":[],"shop":{"stars":0,"purchaseHistory":[]}}',

    CONSTRAINT "saves_pkey" PRIMARY KEY ("userId")
);

-- CreateIndex
CREATE UNIQUE INDEX "users_username_key" ON "users"("username");

-- CreateIndex
CREATE UNIQUE INDEX "teacher_students_teacherId_studentId_key" ON "teacher_students"("teacherId", "studentId");

-- CreateIndex
CREATE UNIQUE INDEX "parent_child_parentId_childId_key" ON "parent_child"("parentId", "childId");

-- AddForeignKey
ALTER TABLE "teacher_students" ADD CONSTRAINT "teacher_students_teacherId_fkey" FOREIGN KEY ("teacherId") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "teacher_students" ADD CONSTRAINT "teacher_students_studentId_fkey" FOREIGN KEY ("studentId") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "parent_child" ADD CONSTRAINT "parent_child_parentId_fkey" FOREIGN KEY ("parentId") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "parent_child" ADD CONSTRAINT "parent_child_childId_fkey" FOREIGN KEY ("childId") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "saves" ADD CONSTRAINT "saves_userId_fkey" FOREIGN KEY ("userId") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;
