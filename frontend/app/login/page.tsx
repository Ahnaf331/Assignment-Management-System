"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth";
import { Alert } from "@/components/ui";

const DEMO = [
  { role: "Admin", email: "admin@school.edu", password: "Admin@123", color: "bg-classroom-purple" },
  { role: "Teacher", email: "teacher@school.edu", password: "Teacher@123", color: "bg-classroom-blue" },
  { role: "Student", email: "student@school.edu", password: "Student@123", color: "bg-classroom-green" },
];

export default function LoginPage() {
  const { login, user, loading } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!loading && user) router.replace("/dashboard");
  }, [user, loading, router]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      await login(email, password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-green-50 via-white to-blue-50 p-4">
      <div className="mb-6 flex items-center gap-2">
        <span className="text-4xl">🎓</span>
        <span className="text-3xl font-medium text-classroom-gray">
          Class<span className="text-classroom-green">Work</span>
        </span>
      </div>

      <div className="card w-full max-w-md p-8">
        <h1 className="mb-1 text-2xl font-medium">Sign in</h1>
        <p className="mb-6 text-sm text-classroom-gray">
          Assignment &amp; Submission Management System
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="label">Email</label>
            <input
              type="email"
              className="input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@school.edu"
              required
            />
          </div>
          <div>
            <label className="label">Password</label>
            <input
              type="password"
              className="input"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>
          <Alert message={error} />
          <button type="submit" className="btn-primary w-full" disabled={submitting}>
            {submitting ? "Signing in..." : "Sign in"}
          </button>
        </form>
      </div>

      <div className="mt-6 w-full max-w-md">
        <p className="mb-2 text-center text-xs font-medium uppercase tracking-wide text-classroom-gray">
          Demo accounts — click to fill
        </p>
        <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
          {DEMO.map((d) => (
            <button
              key={d.role}
              onClick={() => {
                setEmail(d.email);
                setPassword(d.password);
              }}
              className="card flex flex-col items-center gap-1 p-3 transition-shadow hover:shadow-card-hover"
            >
              <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium text-white ${d.color}`}>
                {d.role}
              </span>
              <span className="text-[11px] text-classroom-gray">{d.email}</span>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
