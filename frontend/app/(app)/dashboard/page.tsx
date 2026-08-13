"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api, apiError } from "@/lib/api";
import { useAuth } from "@/lib/auth";
import { Spinner, Alert, Badge } from "@/components/ui";
import type { Assignment, StudentAssignment, User, ClassCourse } from "@/lib/types";

export default function DashboardPage() {
  const { user } = useAuth();
  if (!user) return null;
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-medium">Welcome back, {user.fullName.split(" ")[0]} 👋</h1>
        <p className="text-sm text-classroom-gray">
          {user.role === "Student"
            ? "Here is what's due for your class."
            : user.role === "Teacher"
            ? "Here is an overview of your assignments."
            : "System overview and quick stats."}
        </p>
      </div>
      {user.role === "Admin" && <AdminDashboard />}
      {user.role === "Teacher" && <TeacherDashboard />}
      {user.role === "Student" && <StudentDashboard />}
    </div>
  );
}

function StatCard({ label, value, color }: { label: string; value: number | string; color: string }) {
  return (
    <div className="card p-5">
      <div className={`mb-2 h-1 w-10 rounded ${color}`} />
      <div className="text-3xl font-semibold">{value}</div>
      <div className="text-sm text-classroom-gray">{label}</div>
    </div>
  );
}

function AdminDashboard() {
  const [users, setUsers] = useState<User[]>([]);
  const [classes, setClasses] = useState<ClassCourse[]>([]);
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get<User[]>("/users"),
      api.get<ClassCourse[]>("/classes"),
      api.get<Assignment[]>("/assignments"),
    ])
      .then(([u, c, a]) => {
        setUsers(u.data);
        setClasses(c.data);
        setAssignments(a.data);
      })
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  const teachers = users.filter((u) => u.role === "Teacher").length;
  const students = users.filter((u) => u.role === "Student").length;

  return (
    <>
      <Alert message={error} />
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Users" value={users.length} color="bg-classroom-purple" />
        <StatCard label="Teachers" value={teachers} color="bg-classroom-blue" />
        <StatCard label="Students" value={students} color="bg-classroom-green" />
        <StatCard label="Classes / Courses" value={classes.length} color="bg-classroom-teal" />
      </div>
      <div className="card p-5">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="font-medium">Recent assignments</h2>
          <Link href="/assignments" className="text-sm text-classroom-blue hover:underline">
            View all
          </Link>
        </div>
        <div className="divide-y">
          {assignments.slice(0, 5).map((a) => (
            <div key={a.id} className="flex items-center justify-between py-2 text-sm">
              <span className="font-medium">{a.title}</span>
              <span className="text-classroom-gray">
                {a.classCourseName} · {a.subjectName}
              </span>
            </div>
          ))}
          {assignments.length === 0 && <p className="py-4 text-sm text-classroom-gray">No assignments yet.</p>}
        </div>
      </div>
    </>
  );
}

function TeacherDashboard() {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<Assignment[]>("/assignments")
      .then((res) => setAssignments(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  const published = assignments.filter((a) => a.status === "Published").length;
  const drafts = assignments.filter((a) => a.status === "Draft").length;
  const totalSubs = assignments.reduce((sum, a) => sum + a.submissionCount, 0);

  return (
    <>
      <Alert message={error} />
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Assignments" value={assignments.length} color="bg-classroom-blue" />
        <StatCard label="Published" value={published} color="bg-classroom-green" />
        <StatCard label="Drafts" value={drafts} color="bg-classroom-gray" />
        <StatCard label="Submissions" value={totalSubs} color="bg-classroom-orange" />
      </div>
      <div className="flex justify-end">
        <Link href="/assignments/new" className="btn-primary">
          + Create assignment
        </Link>
      </div>
      <div className="card p-5">
        <h2 className="mb-3 font-medium">Your assignments</h2>
        <div className="divide-y">
          {assignments.slice(0, 6).map((a) => (
            <Link key={a.id} href={`/assignments/${a.id}`} className="flex items-center justify-between py-3 text-sm hover:bg-gray-50">
              <div>
                <div className="font-medium">{a.title}</div>
                <div className="text-classroom-gray">{a.classCourseName} · {a.subjectName}</div>
              </div>
              <Badge color="orange">{a.submissionCount} submissions</Badge>
            </Link>
          ))}
          {assignments.length === 0 && <p className="py-4 text-sm text-classroom-gray">No assignments yet.</p>}
        </div>
      </div>
    </>
  );
}

function StudentDashboard() {
  const [assignments, setAssignments] = useState<StudentAssignment[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<StudentAssignment[]>("/assignments")
      .then((res) => setAssignments(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  const pending = assignments.filter((a) => !a.hasSubmitted).length;
  const submitted = assignments.filter((a) => a.hasSubmitted).length;
  const graded = assignments.filter((a) => a.submissionStatus === "Graded").length;

  return (
    <>
      <Alert message={error} />
      <div className="grid grid-cols-3 gap-4">
        <StatCard label="To do" value={pending} color="bg-classroom-red" />
        <StatCard label="Submitted" value={submitted} color="bg-classroom-blue" />
        <StatCard label="Graded" value={graded} color="bg-classroom-green" />
      </div>
      <div className="card p-5">
        <h2 className="mb-3 font-medium">Upcoming &amp; assigned work</h2>
        <div className="divide-y">
          {assignments.slice(0, 6).map((a) => (
            <Link key={a.id} href={`/assignments/${a.id}`} className="flex items-center justify-between py-3 text-sm hover:bg-gray-50">
              <div>
                <div className="font-medium">{a.title}</div>
                <div className="text-classroom-gray">
                  {a.subjectName} · Due {new Date(a.deadline).toLocaleDateString()}
                </div>
              </div>
              {a.hasSubmitted ? (
                <Badge color="green">{a.submissionStatus}</Badge>
              ) : a.isOverdue ? (
                <Badge color="red">Overdue</Badge>
              ) : (
                <Badge color="orange">To do</Badge>
              )}
            </Link>
          ))}
          {assignments.length === 0 && <p className="py-4 text-sm text-classroom-gray">Nothing assigned yet.</p>}
        </div>
      </div>
    </>
  );
}
