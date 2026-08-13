"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { api, apiError } from "@/lib/api";
import { useAuth } from "@/lib/auth";
import { Alert, Spinner } from "@/components/ui";
import type { TeacherAssignment } from "@/lib/types";

export default function NewAssignmentPage() {
  const { user } = useAuth();
  const router = useRouter();

  const [teaching, setTeaching] = useState<TeacherAssignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [pairKey, setPairKey] = useState("");
  const [deadline, setDeadline] = useState("");
  const [maxMarks, setMaxMarks] = useState(100);
  const [allowResubmission, setAllowResubmission] = useState(true);
  const [allowLateSubmission, setAllowLateSubmission] = useState(false);
  const [publishImmediately, setPublishImmediately] = useState(true);

  useEffect(() => {
    api
      .get<TeacherAssignment[]>("/teacher-assignments/mine")
      .then((res) => setTeaching(res.data))
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  }, []);

  // Unique class+subject pairs the teacher may author for.
  const pairs = useMemo(
    () =>
      teaching.map((t) => ({
        key: `${t.classCourseId}|${t.subjectId}`,
        label: `${t.classCourseName} — ${t.subjectName}`,
        classCourseId: t.classCourseId,
        subjectId: t.subjectId,
      })),
    [teaching]
  );

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError("");
    const pair = pairs.find((p) => p.key === pairKey);
    if (!pair) {
      setError("Please choose the class and subject.");
      return;
    }
    setSubmitting(true);
    try {
      const res = await api.post("/assignments", {
        title,
        description,
        deadline: new Date(deadline).toISOString(),
        maxMarks: Number(maxMarks),
        classCourseId: pair.classCourseId,
        subjectId: pair.subjectId,
        allowResubmission,
        allowLateSubmission,
        publishImmediately,
      });
      router.push(`/assignments/${res.data.id}`);
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSubmitting(false);
    }
  }

  if (!user) return null;
  if (loading) return <Spinner />;

  return (
    <div className="mx-auto max-w-2xl space-y-5">
      <h1 className="text-2xl font-medium">Create assignment</h1>

      {pairs.length === 0 ? (
        <Alert message="You are not assigned to teach any subject yet. Ask an admin to assign you." />
      ) : (
        <form onSubmit={handleSubmit} className="card space-y-4 p-6">
          <div>
            <label className="label">Title</label>
            <input className="input" value={title} onChange={(e) => setTitle(e.target.value)} required />
          </div>

          <div>
            <label className="label">Description / instructions</label>
            <textarea
              className="input min-h-[120px]"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="label">Class &amp; subject</label>
            <select className="input" value={pairKey} onChange={(e) => setPairKey(e.target.value)} required>
              <option value="">Select…</option>
              {pairs.map((p) => (
                <option key={p.key} value={p.key}>
                  {p.label}
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="label">Deadline</label>
              <input
                type="datetime-local"
                className="input"
                value={deadline}
                onChange={(e) => setDeadline(e.target.value)}
                required
              />
            </div>
            <div>
              <label className="label">Maximum marks</label>
              <input
                type="number"
                min={1}
                max={1000}
                className="input"
                value={maxMarks}
                onChange={(e) => setMaxMarks(Number(e.target.value))}
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={allowResubmission} onChange={(e) => setAllowResubmission(e.target.checked)} />
              Allow students to update their submission before the deadline
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={allowLateSubmission} onChange={(e) => setAllowLateSubmission(e.target.checked)} />
              Allow late submissions (flagged as Late)
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={publishImmediately} onChange={(e) => setPublishImmediately(e.target.checked)} />
              Publish immediately (uncheck to save as draft)
            </label>
          </div>

          <Alert message={error} />

          <div className="flex justify-end gap-2">
            <button type="button" className="btn-secondary" onClick={() => router.back()}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={submitting}>
              {submitting ? "Creating…" : "Create assignment"}
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
