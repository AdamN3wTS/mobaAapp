import "./globals.css";
import { Suspense } from "react";
import Navbar from "@/components/Navbar";


export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`h-full antialiased`}
    >
      <body className="min-h-full flex flex-col bg-neutral-950">
        <Suspense fallback={<div className="h-14 border-b border-neutral-800" />}>
          <Navbar />
        </Suspense>
        {children}
      </body>
    </html>
  );
}
