import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        // Google Classroom-inspired palette
        classroom: {
          green: "#1e8e3e",
          greenDark: "#188038",
          blue: "#1967d2",
          purple: "#9334e6",
          teal: "#00897b",
          red: "#d93025",
          orange: "#e8710a",
          gray: "#5f6368",
          surface: "#f1f3f4",
        },
      },
      fontFamily: {
        sans: ["Roboto", "Google Sans", "Arial", "sans-serif"],
      },
      boxShadow: {
        card: "0 1px 2px 0 rgba(60,64,67,.3), 0 1px 3px 1px rgba(60,64,67,.15)",
        "card-hover": "0 1px 3px 0 rgba(60,64,67,.3), 0 4px 8px 3px rgba(60,64,67,.15)",
      },
    },
  },
  plugins: [],
};

export default config;
