-- CreateTable
CREATE TABLE "certificates" (
    "id" TEXT NOT NULL,
    "certificate_number" TEXT NOT NULL,
    "student_id" INTEGER NOT NULL,
    "grade_level" TEXT NOT NULL,
    "achievement" TEXT NOT NULL,
    "issued_by" TEXT NOT NULL,
    "pdf_url" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "certificates_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "certificates_certificate_number_key" ON "certificates"("certificate_number");

-- CreateIndex
CREATE INDEX "certificates_student_id_idx" ON "certificates"("student_id");

-- CreateIndex
CREATE INDEX "certificates_certificate_number_idx" ON "certificates"("certificate_number");

-- AddForeignKey
ALTER TABLE "certificates" ADD CONSTRAINT "certificates_student_id_fkey" FOREIGN KEY ("student_id") REFERENCES "users"("id") ON DELETE CASCADE ON UPDATE CASCADE;
