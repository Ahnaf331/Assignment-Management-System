export type Role = "Admin" | "Teacher" | "Student";
export type AssignmentStatus = "Draft" | "Published";
export type SubmissionStatus = "Submitted" | "Late" | "Graded" | "Returned";

export interface UserSummary {
  id: string;
  fullName: string;
  email: string;
  role: Role;
  classCourseId?: string | null;
  classCourseName?: string | null;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  user: UserSummary;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: Role;
  isActive: boolean;
  classCourseId?: string | null;
  classCourseName?: string | null;
  createdAt: string;
}

export interface ClassCourse {
  id: string;
  name: string;
  code: string;
  description?: string | null;
  studentCount: number;
  subjectCount: number;
}

export interface SubjectSummary {
  id: string;
  name: string;
  code: string;
}

export interface ClassCourseDetail {
  id: string;
  name: string;
  code: string;
  description?: string | null;
  subjects: SubjectSummary[];
}

export interface Subject {
  id: string;
  name: string;
  code: string;
  classCourseId: string;
  classCourseName: string;
}

export interface TeacherAssignment {
  id: string;
  teacherId: string;
  teacherName: string;
  subjectId: string;
  subjectName: string;
  classCourseId: string;
  classCourseName: string;
}

export interface Assignment {
  id: string;
  title: string;
  description: string;
  deadline: string;
  maxMarks: number;
  status: AssignmentStatus;
  allowResubmission: boolean;
  allowLateSubmission: boolean;
  classCourseId: string;
  classCourseName: string;
  subjectId: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
  createdAt: string;
  publishedAt?: string | null;
  submissionCount: number;
  isOverdue: boolean;
}

export interface StudentAssignment {
  id: string;
  title: string;
  description: string;
  deadline: string;
  maxMarks: number;
  allowResubmission: boolean;
  allowLateSubmission: boolean;
  classCourseName: string;
  subjectName: string;
  teacherName: string;
  isOverdue: boolean;
  hasSubmitted: boolean;
  submissionStatus?: SubmissionStatus | null;
  marks?: number | null;
}

export interface Submission {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  maxMarks: number;
  studentId: string;
  studentName: string;
  studentEmail: string;
  content: string;
  submittedAt: string;
  updatedAt?: string | null;
  status: SubmissionStatus;
  marks?: number | null;
  feedback?: string | null;
  gradedAt?: string | null;
  gradedByName?: string | null;
}
