"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, apiError } from "@/lib/api";
import { Spinner, Alert, Badge, Modal } from "@/components/ui";
import type { ClassCourse, Subject, TeacherAssignment, User } from "@/lib/types";

export default function AdminTeacherAssignmentsPage() {
  const [items, setItems] = useState<TeacherAssignment[]>([]);
  const [teachers, setTeachers] = useState<User[]>([]);
  const [classes, setClasses] = useState<ClassCourse[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [open, setOpen] = useState(false);

  const load = () => {
    setLoading(true);
    Promise.all([
      api.get<TeacherAssignment[]>("/teacher-assignments"),
      api.get<User[]>("/users?role=Teacher"),
      api.get<ClassCourse[]>("/classes"),
      api.get<Subject[]>("/subjects"),
    ])
      .then(([t, u, c, s]) => {
        setItems(t.data);
        setTeachers(u.data);
        setClasses(c.data);
        setSubjects(s.data);
      })
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  async function remove(t: TeacherAssignment) {
    if (!confirm(`Remove ${t.teacherName} from ${t.subjectName} (${t.classCourseName})?`)) return;
    try {
      await api.delete(`/teacher-assignments/${t.id}`);
      load();
    } catch (e) {
      setError(apiError(e));
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-medium">Teacher Assignments</h1>
        <button className="btn-primary" onClick={() => setOpen(true)}>
          + Assign teacher
        </button>
      </div>
      <p className="text-sm text-classroom-gray">
        Define which teacher is allowed to create assignments for each subject &amp; class.
      </p>
      <Alert message={error} />
      {loading ? (
        <Spinner />
      ) : (
        <div className="card overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="border-b bg-gray-50 text-left text-xs uppercase text-classroom-gray">
              <tr>
                <th className="px-4 py-3">Teacher</th>
                <th className="px-4 py-3">Subject</th>
                <th className="px-4 py-3">Class / course</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((t) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{t.teacherName}</td>
                  <td className="px-4 py-3"><Badge color="blue">{t.subjectName}</Badge></td>
                  <td className="px-4 py-3 text-classroom-gray">{t.classCourseName}</td>
                  <td className="px-4 py-3 text-right">
                    <button className="text-classroom-red hover:underline" onClick={() => remove(t)}>
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr><td colSpan={4} className="px-4 py-6 text-center text-classroom-gray">No assignments yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
      <AssignModal
        open={open}
        onClose={() => setOpen(false)}
        teachers={teachers}
        subjects={subjects}
        classes={classes}
        onSaved={() => { setOpen(false); load(); }}
      />
    </div>
  );
}

function AssignModal({
  open,
  onClose,
  teachers,
  subjects,
  classes,
  onSaved,
}: {
  open: boolean;
  onClose: () => void;
  teachers: User[];
  subjects: Subject[];
  classes: ClassCourse[];
  onSaved: () => void;
}) {
  const [teacherId, setTeacherId] = useState("");
  const [classId, setClassId] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError("");
    setTeacherId("");
    setClassId("");
    setSubjectId("");
  }, [open]);

  const classSubjects = subjects.filter((s) => s.classCourseId === classId);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      await api.post("/teacher-assignments", {
        teacherId,
        subjectId,
        classCourseId: classId,
      });
      onSaved();
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal open={open} onClose={onClose} title="Assign teacher">
      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="label">Teacher</label>
          <select className="input" value={teacherId} onChange={(e) => setTeacherId(e.target.value)} required>
            <option value="">Select…</option>
            {teachers.map((t) => (
              <option key={t.id} value={t.id}>{t.fullName}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">Class / course</label>
          <select
            className="input"
            value={classId}
            onChange={(e) => {
              setClassId(e.target.value);
              setSubjectId("");
            }}
            required
          >
            <option value="">Select…</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="label">Subject</label>
          <select className="input" value={subjectId} onChange={(e) => setSubjectId(e.target.value)} required disabled={!classId}>
            <option value="">{classId ? "Select…" : "Choose a class first"}</option>
            {classSubjects.map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
        </div>
        <Alert message={error} />
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>{saving ? "Saving…" : "Assign"}</button>
        </div>
      </form>
    </Modal>
  );
}
