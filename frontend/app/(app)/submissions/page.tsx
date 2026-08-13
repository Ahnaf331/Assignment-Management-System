"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api, apiError } from "@/lib/api";
import { Spinner, Alert, EmptyState, SubmissionBadge, formatDate } from "@/components/ui";
import type { Submission } from "@/lib/types";

export default function MyWorkPage() {
  const [subs, setSubs] = useState<Submission[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<Submission[]>("/submissions/mine")
      .then((res) => setSubs(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-medium">My Work</h1>
      <Alert message={error} />
      {loading ? (
        <Spinner />
      ) : subs.length === 0 ? (
        <EmptyState title="No submissions yet" subtitle="Your submitted assignments will appear here." />
      ) : (
        <div className="card divide-y">
          {subs.map((s) => (
            <Link
              key={s.id}
              href={`/assignments/${s.assignmentId}`}
              className="flex items-center justify-between p-4 hover:bg-gray-50"
            >
              <div>
                <div className="font-medium">{s.assignmentTitle}</div>
                <div className="text-xs text-classroom-gray">Submitted {formatDate(s.submittedAt)}</div>
              </div>
              <div className="flex items-center gap-3">
                {s.status === "Graded" && (
                  <span className="text-sm font-medium">
                    {s.marks}/{s.maxMarks}
                  </span>
                )}
                <SubmissionBadge status={s.status} />
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
