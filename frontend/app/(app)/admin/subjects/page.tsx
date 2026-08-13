"use client";

import { FormEvent, useEffect, useState } from "react";
import { api, apiError } from "@/lib/api";
import { Spinner, Alert, Badge, Modal } from "@/components/ui";
import type { ClassCourse, Subject } from "@/lib/types";

export default function AdminSubjectsPage() {
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [classes, setClasses] = useState<ClassCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Subject | null>(null);

  const load = () => {
    setLoading(true);
    Promise.all([api.get<Subject[]>("/subjects"), api.get<ClassCourse[]>("/classes")])
      .then(([s, c]) => {
        setSubjects(s.data);
        setClasses(c.data);
      })
      .catch((e) => setError(apiError(e)))
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  async function remove(s: Subject) {
    if (!confirm(`Delete ${s.name}?`)) return;
    try {
      await api.delete(`/subjects/${s.id}`);
      load();
    } catch (e) {
      setError(apiError(e));
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-medium">Subjects</h1>
        <button className="btn-primary" onClick={() => { setEditing(null); setOpen(true); }}>
          + Add subject
        </button>
      </div>
      <Alert message={error} />
      {loading ? (
        <Spinner />
      ) : (
        <div className="card overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="border-b bg-gray-50 text-left text-xs uppercase text-classroom-gray">
              <tr>
                <th className="px-4 py-3">Subject</th>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Class / course</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {subjects.map((s) => (
                <tr key={s.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{s.name}</td>
                  <td className="px-4 py-3"><Badge color="blue">{s.code}</Badge></td>
                  <td className="px-4 py-3 text-classroom-gray">{s.classCourseName}</td>
                  <td className="px-4 py-3 text-right">
                    <button className="mr-3 text-classroom-blue hover:underline" onClick={() => { setEditing(s); setOpen(true); }}>
                      Edit
                    </button>
                    <button className="text-classroom-red hover:underline" onClick={() => remove(s)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {subjects.length === 0 && (
                <tr><td colSpan={4} className="px-4 py-6 text-center text-classroom-gray">No subjects yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
      <SubjectModal open={open} onClose={() => setOpen(false)} classes={classes} editing={editing} onSaved={() => { setOpen(false); load(); }} />
    </div>
  );
}

function SubjectModal({
  open,
  onClose,
  classes,
  editing,
  onSaved,
}: {
  open: boolean;
  onClose: () => void;
  classes: ClassCourse[];
  editing: Subject | null;
  onSaved: () => void;
}) {
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [classId, setClassId] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    setError("");
    setName(editing?.name ?? "");
    setCode(editing?.code ?? "");
    setClassId(editing?.classCourseId ?? "");
  }, [open, editing]);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      if (editing) {
        await api.put(`/subjects/${editing.id}`, { name, code });
      } else {
        await api.post("/subjects", { name, code, classCourseId: classId });
      }
      onSaved();
    } catch (err) {
      setError(apiError(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal open={open} onClose={onClose} title={editing ? "Edit subject" : "Add subject"}>
      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="label">Name</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div>
          <label className="label">Code</label>
          <input className="input" value={code} onChange={(e) => setCode(e.target.value)} required />
        </div>
        {!editing && (
          <div>
            <label className="label">Class / course</label>
            <select className="input" value={classId} onChange={(e) => setClassId(e.target.value)} required>
              <option value="">Select…</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
        )}
        <Alert message={error} />
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={onClose}>Cancel</button>
          <button type="submit" className="btn-primary" disabled={saving}>{saving ? "Saving…" : "Save"}</button>
        </div>
      </form>
    </Modal>
  );
}
