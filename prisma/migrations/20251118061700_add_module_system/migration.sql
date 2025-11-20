-- CreateTable
CREATE TABLE "modules" (
    "id" SERIAL NOT NULL,
    "name" TEXT NOT NULL,
    "grade" INTEGER NOT NULL,
    "subject" TEXT NOT NULL,
    "teacher_id" INTEGER NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "modules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "module_questions" (
    "id" SERIAL NOT NULL,
    "module_id" INTEGER NOT NULL,
    "question_text" TEXT NOT NULL,
    "answers" JSONB NOT NULL,
    "correct_answer_index" INTEGER NOT NULL,
    "correct_answer" TEXT NOT NULL,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "module_questions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "module_assignments" (
    "id" SERIAL NOT NULL,
    "module_id" INTEGER NOT NULL,
    "student_id" INTEGER NOT NULL,
    "completed" BOOLEAN NOT NULL DEFAULT false,
    "score" INTEGER,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "module_assignments_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE INDEX "modules_teacher_id_idx" ON "modules"("teacher_id");

-- CreateIndex
CREATE INDEX "modules_grade_idx" ON "modules"("grade");

-- CreateIndex
CREATE INDEX "module_questions_module_id_idx" ON "module_questions"("module_id");

-- CreateIndex
CREATE INDEX "module_assignments_student_id_idx" ON "module_assignments"("student_id");

-- CreateIndex
CREATE INDEX "module_assignments_module_id_idx" ON "module_assignments"("module_id");

-- CreateIndex
CREATE UNIQUE INDEX "module_assignments_module_id_student_id_key" ON "module_assignments"("module_id", "student_id");

-- AddForeignKey
ALTER TABLE "modules" ADD CONSTRAINT "modules_teacher_id_fkey" FOREIGN KEY ("teacher_id") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "module_questions" ADD CONSTRAINT "module_questions_module_id_fkey" FOREIGN KEY ("module_id") REFERENCES "modules"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "module_assignments" ADD CONSTRAINT "module_assignments_module_id_fkey" FOREIGN KEY ("module_id") REFERENCES "modules"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "module_assignments" ADD CONSTRAINT "module_assignments_student_id_fkey" FOREIGN KEY ("student_id") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;
