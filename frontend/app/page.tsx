"use client";

import Link from "next/link";
import { motion, Variants } from "framer-motion";
import { useAuth } from "@/lib/auth";

const container: Variants = {
  hidden: { opacity: 0 },
  show: {
    opacity: 1,
    transition: { staggerChildren: 0.12, delayChildren: 0.1 },
  },
};

const item: Variants = {
  hidden: { opacity: 0, y: 24 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5, ease: "easeOut" } },
};

const ROLES = [
  {
    title: "Admin",
    color: "#9334e6",
    points: ["Manage users & roles", "Create classes and subjects", "Assign teachers to subjects"],
  },
  {
    title: "Teacher",
    color: "#1967d2",
    points: ["Create & publish assignments", "Review student submissions", "Grade and give feedback"],
  },
  {
    title: "Student",
    color: "#1e8e3e",
    points: ["See work for your class", "Submit & update answers", "Track marks and feedback"],
  },
];

const FEATURES = [
  { color: "#1967d2", title: "Secure by design", text: "JWT authentication with role-based authorization enforced on the API." },
  { color: "#1e8e3e", title: "Assignments & drafts", text: "Publish to a class/subject or keep as a draft until you're ready." },
  { color: "#e8710a", title: "Submissions workflow", text: "Deadlines, late rules, resubmissions, grading and feedback." },
  { color: "#9334e6", title: "Fast & responsive", text: "A clean, Google Classroom-style UI that works on any screen." },
];

export default function LandingPage() {
  const { user, loading } = useAuth();
  const primaryHref = user ? "/dashboard" : "/login";
  const primaryLabel = user ? "Go to dashboard" : "Get started";

  return (
    <div className="relative min-h-screen overflow-hidden bg-gradient-to-br from-green-50 via-white to-blue-50">
      {/* Animated decorative blobs */}
      <motion.div
        aria-hidden
        className="pointer-events-none absolute -left-24 -top-24 h-72 w-72 rounded-full bg-classroom-green/20 blur-3xl"
        animate={{ y: [0, 30, 0], x: [0, 20, 0] }}
        transition={{ duration: 12, repeat: Infinity, ease: "easeInOut" }}
      />
      <motion.div
        aria-hidden
        className="pointer-events-none absolute -right-24 top-40 h-80 w-80 rounded-full bg-classroom-blue/20 blur-3xl"
        animate={{ y: [0, -40, 0], x: [0, -20, 0] }}
        transition={{ duration: 14, repeat: Infinity, ease: "easeInOut" }}
      />
      <motion.div
        aria-hidden
        className="pointer-events-none absolute bottom-0 left-1/3 h-72 w-72 rounded-full bg-classroom-purple/10 blur-3xl"
        animate={{ y: [0, -25, 0] }}
        transition={{ duration: 10, repeat: Infinity, ease: "easeInOut" }}
      />

      {/* Nav */}
      <motion.nav
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="relative z-10 mx-auto flex max-w-6xl items-center justify-between px-6 py-5"
      >
        <div className="flex items-center gap-2">
          <span className="text-3xl">🎓</span>
          <span className="text-2xl font-medium text-classroom-gray">
            Class<span className="text-classroom-green">Work</span>
          </span>
        </div>
        <Link href={primaryHref} className="btn-primary !px-5">
          {loading ? "…" : user ? "Dashboard" : "Sign in"}
        </Link>
      </motion.nav>

      {/* Hero */}
      <section className="relative z-10 mx-auto grid max-w-6xl items-center gap-12 px-6 pb-16 pt-10 lg:grid-cols-2 lg:pt-20">
        <motion.div variants={container} initial="hidden" animate="show">
          <motion.span
            variants={item}
            className="inline-block rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-classroom-greenDark"
          >
            Assignment &amp; Submission Management System
          </motion.span>

          <motion.h1
            variants={item}
            className="mt-4 text-4xl font-bold leading-tight text-gray-900 sm:text-5xl"
          >
            Assignments, submissions &amp;{" "}
            <span className="text-classroom-green">feedback</span> — all in one place.
          </motion.h1>

          <motion.p variants={item} className="mt-4 max-w-lg text-lg text-classroom-gray">
            A role-based classroom for schools and colleges. Teachers create and grade work,
            students submit and track progress, and admins keep everything organized.
          </motion.p>

          <motion.div variants={item} className="mt-8 flex flex-wrap gap-3">
            <Link href={primaryHref} className="btn-primary !px-6 !py-3 text-base">
              {primaryLabel}
              <span aria-hidden>→</span>
            </Link>
            <Link href="/login" className="btn-secondary !px-6 !py-3 text-base">
              Try a demo account
            </Link>
          </motion.div>

          <motion.p variants={item} className="mt-4 text-xs text-classroom-gray">
            Demo logins for Admin, Teacher and Student are available on the sign-in page.
          </motion.p>
        </motion.div>

        {/* Floating mock cards */}
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6, delay: 0.2 }}
          className="relative mx-auto h-80 w-full max-w-md"
        >
          <motion.div
            className="absolute left-0 top-4 w-64 rounded-xl bg-white p-4 shadow-card"
            animate={{ y: [0, -12, 0] }}
            transition={{ duration: 6, repeat: Infinity, ease: "easeInOut" }}
          >
            <div className="mb-3 h-16 rounded-lg bg-classroom-green" />
            <div className="text-sm font-medium">Algebra Problem Set 1</div>
            <div className="text-xs text-classroom-gray">Mathematics · Due in 7 days</div>
            <div className="mt-2 inline-block rounded-full bg-green-100 px-2 py-0.5 text-[10px] font-medium text-green-800">
              Published
            </div>
          </motion.div>

          <motion.div
            className="absolute right-0 top-24 w-60 rounded-xl bg-white p-4 shadow-card-hover"
            animate={{ y: [0, 14, 0] }}
            transition={{ duration: 7, repeat: Infinity, ease: "easeInOut", delay: 0.5 }}
          >
            <div className="mb-3 h-16 rounded-lg bg-classroom-blue" />
            <div className="text-sm font-medium">Hello World in C#</div>
            <div className="text-xs text-classroom-gray">Programming · 20 marks</div>
            <div className="mt-2 inline-block rounded-full bg-blue-100 px-2 py-0.5 text-[10px] font-medium text-blue-800">
              3 submitted
            </div>
          </motion.div>

          <motion.div
            className="absolute bottom-0 left-10 w-56 rounded-xl bg-white p-4 shadow-card"
            animate={{ y: [0, -10, 0] }}
            transition={{ duration: 5.5, repeat: Infinity, ease: "easeInOut", delay: 1 }}
          >
            <div className="flex items-center justify-between">
              <div className="text-sm font-medium">Your grade</div>
              <div className="text-lg font-bold text-classroom-greenDark">92/100</div>
            </div>
            <div className="mt-1 text-xs text-classroom-gray">“Excellent work! 🎉”</div>
          </motion.div>
        </motion.div>
      </section>

      {/* Roles */}
      <section className="relative z-10 mx-auto max-w-6xl px-6 py-12">
        <motion.h2
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5 }}
          className="mb-8 text-center text-2xl font-medium text-gray-900"
        >
          Built for every role
        </motion.h2>

        <motion.div
          variants={container}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true }}
          className="grid gap-6 md:grid-cols-3"
        >
          {ROLES.map((role) => (
            <motion.div
              key={role.title}
              variants={item}
              whileHover={{ y: -6, transition: { duration: 0.2 } }}
              className="card overflow-hidden"
            >
              <div className="h-1.5" style={{ backgroundColor: role.color }} />
              <div className="p-6">
                <div className="mb-4 flex items-center gap-3">
                  <div
                    className="flex h-11 w-11 items-center justify-center rounded-full text-lg font-medium text-white"
                    style={{ backgroundColor: role.color }}
                  >
                    {role.title.charAt(0)}
                  </div>
                  <h3 className="text-lg font-medium">{role.title}</h3>
                </div>
                <ul className="space-y-2 text-sm text-classroom-gray">
                  {role.points.map((p) => (
                    <li key={p} className="flex items-start gap-2">
                      <span style={{ color: role.color }}>✓</span>
                      {p}
                    </li>
                  ))}
                </ul>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </section>

      {/* Features */}
      <section className="relative z-10 mx-auto max-w-6xl px-6 py-12">
        <motion.div
          variants={container}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true }}
          className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4"
        >
          {FEATURES.map((f) => (
            <motion.div key={f.title} variants={item} className="card p-6">
              <div className="mb-3 h-1 w-8 rounded-full" style={{ backgroundColor: f.color }} />
              <h3 className="mb-1 font-medium">{f.title}</h3>
              <p className="text-sm text-classroom-gray">{f.text}</p>
            </motion.div>
          ))}
        </motion.div>
      </section>

      {/* CTA */}
      <section className="relative z-10 mx-auto max-w-6xl px-6 pb-20">
        <motion.div
          initial={{ opacity: 0, scale: 0.97 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5 }}
          className="rounded-2xl bg-gradient-to-r from-classroom-greenDark to-classroom-green px-8 py-12 text-center text-white shadow-card"
        >
          <h2 className="text-2xl font-medium sm:text-3xl">Ready to get started?</h2>
          <p className="mx-auto mt-2 max-w-xl text-sm text-white/90">
            Sign in with a demo account and explore the admin, teacher and student experiences.
          </p>
          <Link
            href={primaryHref}
            className="mt-6 inline-flex items-center gap-2 rounded-md bg-white px-6 py-3 text-base font-medium text-classroom-greenDark transition-transform hover:scale-105"
          >
            {primaryLabel} <span aria-hidden>→</span>
          </Link>
        </motion.div>
      </section>

      {/* Footer */}
      <footer className="relative z-10 border-t bg-white/60 py-6 text-center text-xs text-classroom-gray">
        ClassWork — Assignment &amp; Submission Management System
      </footer>
    </div>
  );
}
