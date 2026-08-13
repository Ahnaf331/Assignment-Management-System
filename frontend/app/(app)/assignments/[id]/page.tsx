"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { api, apiError } from "@/lib/api";
import { useAuth } from "@/lib/auth";
import {
  Spinner,
  Alert,
  StatusBadge,
  SubmissionBadge,
  Badge,
  formatDate,
  accentColor,
} from "@/components/ui";
import type { Assignment, Submission } from "@/lib/types";

export default function AssignmentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<Assignment>(`/assignments/${id}`)
      .then((res) => setAssignment(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, [id]);

  if (!user) return null;
  if (loading) return <Spinner />;
  if (error || !assignment) return <Alert message={error || "Not found"} />;

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      {/* Header banner */}
      <div
        className="rounded-lg p-6 text-white shadow-card"
        style={{ backgroundColor: accentColor(assignment.subjectName + assignment.title) }}
      >
        <div className="mb-2 flex flex-wrap items-center gap-2">
          <span className="rounded-full bg-white/20 px-2.5 py-0.5 text-xs">{assignment.subjectName}</span>
          <span className="rounded-full bg-white/20 px-2.5 py-0.5 text-xs">{assignment.classCourseName}</span>
        </div>
        <h1 className="text-2xl font-medium">{assignment.title}</h1>
        <p className="mt-1 text-sm opacity-90">
          {assignment.teacherName} · Due {formatDate(assignment.deadline)} · {assignment.maxMarks} marks
        </p>
      </div>

      <div className="card p-6">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="font-medium">Instructions</h2>
          {user.role !== "Student" && <StatusBadge status={assignment.status} />}
        </div>
        <p className="whitespace-pre-wrap text-sm text-gray-700">{assignment.description}</p>
        <div className="mt-4 flex flex-wrap gap-2 text-xs">
          {assignment.allowResubmission && <Badge color="blue">Updates allowed before deadline</Badge>}
          {assignment.allowLateSubmission && <Badge color="orange">Late submissions allowed</Badge>}
          {assignment.isOverdue && <Badge color="red">Past deadline</Badge>}
        </div>
      </div>

      {user.role === "Student" ? (
        <StudentSubmission assignment={assignment} />
      ) : (
        <TeacherSubmissions assignment={assignment} isOwner={assignment.teacherId === user.id} />
      )}
    </div>
  );
}

/* ------------------------- Student view ------------------------- */

function StudentSubmission({ assignment }: { assignment: Assignment }) {
  const [submission, setSubmission] = useState<Submission | null>(null);
  const [content, setContent] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    api
      .get<Submission>(`/assignments/${assignment.id}/submissions/mine`)
      .then((res) => {
        // 204 => no content/body
        if (res.status === 200 && res.data) {
          setSubmission(res.data);
          setContent(res.data.content);
        }
      })
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, [assignment.id]);

  async function save() {
    setError("");
    setMessage("");
    setSaving(true);
    try {
      const res = submission
        ? await api.put<Submission>(`/assignments/${assignment.id}/submissions`, { content })
        : await api.post<Submission>(`/assignments/${assignment.id}/submissions`, { content });
      setSubmission(res.data);
      setContent(res.data.content);
      setMessage(submission ? "Submission updated." : "Submitted successfully.");
    } catch (e) {
      setError(apiError(e));
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <Spinner />;

  const isGraded = submission?.status === "Graded";
  const locked = isGraded || (submission ? !assignment.allowResubmission || assignment.isOverdue : assignment.isOverdue && !assignment.allowLateSubmission);

  return (
    <div className="card p-6">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-medium">Your work</h2>
        {submission && <SubmissionBadge status={submission.status} />}
      </div>

      {isGraded && (
        <div className="mb-4 rounded-md border border-green-200 bg-green-50 p-4">
          <div className="text-2xl font-semibold text-classroom-greenDark">
            {submission?.marks} / {assignment.maxMarks}
          </div>
          {submission?.feedback && (
            <p className="mt-1 text-sm text-gray-700">
              <span className="font-medium">Feedback:</span> {submission.feedback}
            </p>
          )}
          {submission?.gradedByName && (
            <p className="mt-1 text-xs text-classroom-gray">Graded by {submission.gradedByName}</p>
          )}
        </div>
      )}

      <textarea
        className="input min-h-[160px]"
        placeholder="Type your answer here…"
        value={content}
        disabled={locked}
        onChange={(e) => setContent(e.target.value)}
      />

      <Alert message={error} />
      {message && (
        <div className="mt-2 rounded-md border border-green-200 bg-green-50 px-4 py-2 text-sm text-green-700">
          {message}
        </div>
      )}

      <div className="mt-4 flex items-center justify-between">
        <p className="text-xs text-classroom-gray">
          {submission
            ? `Submitted ${formatDate(submission.submittedAt)}`
            : "Not submitted yet"}
        </p>
        {!locked && (
          <button className="btn-primary" onClick={save} disabled={saving || !content.trim()}>
            {saving ? "Saving…" : submission ? "Update submission" : "Submit"}
          </button>
        )}
        {locked && !isGraded && (
          <span className="text-xs text-classroom-red">Editing is closed for this assignment.</span>
        )}
      </div>
    </div>
  );
}

/* ------------------------- Teacher / Admin view ------------------------- */

function TeacherSubmissions({ assignment, isOwner }: { assignment: Assignment; isOwner: boolean }) {
  const router = useRouter();
  const { user } = useAuth();
  const [subs, setSubs] = useState<Submission[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  const load = () => {
    api
      .get<Submission[]>(`/assignments/${assignment.id}/submissions`)
      .then((res) => setSubs(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  };
  useEffect(load, [assignment.id]);

  async function togglePublish() {
    try {
      const action = assignment.status === "Published" ? "unpublish" : "publish";
      await api.post(`/assignments/${assignment.id}/${action}`);
      router.refresh();
      window.location.reload();
    } catch (e) {
      setError(apiError(e));
    }
  }

  async function remove() {
    if (!confirm("Delete this assignment and all its submissions?")) return;
    try {
      await api.delete(`/assignments/${assignment.id}`);
      router.push("/assignments");
    } catch (e) {
      setError(apiError(e));
    }
  }

  return (
    <div className="space-y-4">
      {(isOwner || user?.role === "Admin") && (
        <div className="card flex flex-wrap items-center justify-between gap-3 p-4">
          <span className="text-sm text-classroom-gray">Manage this assignment</span>
          <div className="flex gap-2">
            {isOwner && (
              <button className="btn-secondary" onClick={togglePublish}>
                {assignment.status === "Published" ? "Revert to draft" : "Publish"}
              </button>
            )}
            <button className="btn-danger" onClick={remove}>
              Delete
            </button>
          </div>
        </div>
      )}

      <div className="card p-6">
        <h2 className="mb-4 font-medium">
          Submissions <span className="text-classroom-gray">({subs.length})</span>
        </h2>
        <Alert message={error} />
        {loading ? (
          <Spinner />
        ) : subs.length === 0 ? (
          <p className="py-4 text-sm text-classroom-gray">No submissions yet.</p>
        ) : (
          <div className="space-y-3">
            {subs.map((s) => (
              <SubmissionRow key={s.id} submission={s} maxMarks={assignment.maxMarks} canGrade={isOwner || user?.role === "Admin"} onGraded={load} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function SubmissionRow({
  submission,
  maxMarks,
  canGrade,
  onGraded,
}: {
  submission: Submission;
  maxMarks: number;
  canGrade: boolean;
  onGraded: () => void;
}) {
  const [open, setOpen] = useState(false);
  const [marks, setMarks] = useState<number>(submission.marks ?? 0);
  const [feedback, setFeedback] = useState(submission.feedback ?? "");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  async function grade() {
    setError("");
    setSaving(true);
    try {
      await api.post(`/submissions/${submission.id}/grade`, { marks: Number(marks), feedback });
      setOpen(false);
      onGraded();
    } catch (e) {
      setError(apiError(e));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="rounded-md border border-gray-200">
      <button
        className="flex w-full items-center justify-between px-4 py-3 text-left hover:bg-gray-50"
        onClick={() => setOpen((o) => !o)}
      >
        <div className="flex items-center gap-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-classroom-blue text-xs font-medium text-white">
            {submission.studentName.charAt(0)}
          </div>
          <div>
            <div className="text-sm font-medium">{submission.studentName}</div>
            <div className="text-xs text-classroom-gray">{formatDate(submission.submittedAt)}</div>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {submission.status === "Graded" && (
            <span className="text-sm font-medium">{submission.marks}/{maxMarks}</span>
          )}
          <SubmissionBadge status={submission.status} />
        </div>
      </button>

      {open && (
        <div className="space-y-3 border-t px-4 py-3">
          <p className="whitespace-pre-wrap rounded bg-gray-50 p-3 text-sm text-gray-700">
            {submission.content}
          </p>

          {canGrade && (
            <div className="space-y-2">
              <div className="flex flex-wrap items-end gap-3">
                <div>
                  <label className="label">Marks (out of {maxMarks})</label>
                  <input
                    type="number"
                    min={0}
                    max={maxMarks}
                    className="input w-28"
                    value={marks}
                    onChange={(e) => setMarks(Number(e.target.value))}
                  />
                </div>
                <div className="flex-1">
                  <label className="label">Feedback</label>
                  <input className="input" value={feedback} onChange={(e) => setFeedback(e.target.value)} />
                </div>
              </div>
              <Alert message={error} />
              <div className="flex justify-end">
                <button className="btn-primary" onClick={grade} disabled={saving}>
                  {saving ? "Saving…" : submission.status === "Graded" ? "Update grade" : "Save grade"}
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
