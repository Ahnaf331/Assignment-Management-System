"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api, apiError } from "@/lib/api";
import { useAuth } from "@/lib/auth";
import { Spinner, Alert, EmptyState, StatusBadge, Badge, accentColor } from "@/components/ui";
import type { Assignment, StudentAssignment } from "@/lib/types";

export default function AssignmentsPage() {
  const { user } = useAuth();
  const [items, setItems] = useState<(Assignment | StudentAssignment)[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");

  useEffect(() => {
    api
      .get<(Assignment | StudentAssignment)[]>("/assignments")
      .then((res) => setItems(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  if (!user) return null;

  const isStudent = user.role === "Student";
  const filtered = items.filter((a) =>
    a.title.toLowerCase().includes(search.toLowerCase()) ||
    a.subjectName.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-medium">Assignments</h1>
        {user.role === "Teacher" && (
          <Link href="/assignments/new" className="btn-primary">
            + Create assignment
          </Link>
        )}
      </div>

      <input
        className="input max-w-sm"
        placeholder="Search assignments..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <Alert message={error} />

      {loading ? (
        <Spinner />
      ) : filtered.length === 0 ? (
        <EmptyState
          title="No assignments found"
          subtitle={
            user.role === "Teacher"
              ? "Create your first assignment to get started."
              : "You have no assignments in your class yet."
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {filtered.map((a) => (
            <Link
              key={a.id}
              href={`/assignments/${a.id}`}
              className="card overflow-hidden transition-shadow hover:shadow-card-hover"
            >
              <div
                className="flex h-24 items-end p-4 text-white"
                style={{ backgroundColor: accentColor(a.subjectName + a.title) }}
              >
                <h3 className="line-clamp-2 text-lg font-medium">{a.title}</h3>
              </div>
              <div className="space-y-3 p-4">
                <div className="flex items-center justify-between text-sm text-classroom-gray">
                  <span>{a.subjectName}</span>
                  {!isStudent && <StatusBadge status={(a as Assignment).status} />}
                </div>
                <p className="line-clamp-2 text-sm text-gray-600">{a.description}</p>
                <div className="flex items-center justify-between border-t pt-3 text-xs">
                  <span className={a.isOverdue ? "font-medium text-classroom-red" : "text-classroom-gray"}>
                    Due {new Date(a.deadline).toLocaleDateString()}
                  </span>
                  {isStudent ? (
                    (a as StudentAssignment).hasSubmitted ? (
                      <Badge color="green">{(a as StudentAssignment).submissionStatus}</Badge>
                    ) : a.isOverdue ? (
                      <Badge color="red">Overdue</Badge>
                    ) : (
                      <Badge color="orange">To do</Badge>
                    )
                  ) : (
                    <Badge color="blue">{(a as Assignment).submissionCount} submitted</Badge>
                  )}
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
