import Link from 'next/link';
import { ArrowRight, Shield, Zap, Globe } from 'lucide-react';

import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui';

const features = [
  {
    icon: Shield,
    title: 'Secure by Default',
    description:
      'Enterprise-grade security with JWT authentication, role-based access control, and secure session management.',
  },
  {
    icon: Zap,
    title: 'Lightning Fast',
    description:
      'Built with Next.js 14 and React Server Components for optimal performance and user experience.',
  },
  {
    icon: Globe,
    title: 'Internationalization Ready',
    description:
      'Multi-language support out of the box with next-intl for global reach.',
  },
];

export default function HomePage() {
  return (
    <div className="flex min-h-screen flex-col">
      {/* Hero Section */}
      <header className="container mx-auto px-4 py-6">
        <nav className="flex items-center justify-between">
          <Link href="/" className="text-xl font-bold">
            ProjectName
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href="/auth/login"
              className="text-sm text-muted-foreground hover:text-foreground"
            >
              Sign in
            </Link>
            <Button asChild>
              <Link href="/auth/register">Get Started</Link>
            </Button>
          </div>
        </nav>
      </header>

      <main className="flex-1">
        {/* Hero */}
        <section className="container mx-auto px-4 py-24 text-center">
          <h1 className="text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl">
            Build Enterprise Apps
            <span className="block text-primary">With Confidence</span>
          </h1>
          <p className="mx-auto mt-6 max-w-2xl text-lg text-muted-foreground">
            A production-ready frontend template designed to work seamlessly with
            the .NET Clean Architecture backend. Authentication, state management,
            and UI components - all configured and ready to use.
          </p>
          <div className="mt-10 flex items-center justify-center gap-4">
            <Button size="lg" asChild>
              <Link href="/auth/register">
                Get Started <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
            <Button variant="outline" size="lg" asChild>
              <Link href="/dashboard">View Demo</Link>
            </Button>
          </div>
        </section>

        {/* Features */}
        <section className="container mx-auto px-4 py-24">
          <div className="grid gap-8 md:grid-cols-3">
            {features.map((feature) => (
              <Card key={feature.title}>
                <CardHeader>
                  <feature.icon className="h-10 w-10 text-primary" />
                  <CardTitle className="mt-4">{feature.title}</CardTitle>
                </CardHeader>
                <CardContent>
                  <CardDescription>{feature.description}</CardDescription>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>

        {/* Tech Stack */}
        <section className="border-t bg-muted/50 py-24">
          <div className="container mx-auto px-4 text-center">
            <h2 className="text-2xl font-bold">Built With Modern Tech Stack</h2>
            <p className="mt-4 text-muted-foreground">
              Next.js 14 • React 18 • TypeScript • Tailwind CSS • shadcn/ui •
              TanStack Query • Zustand
            </p>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t py-8">
        <div className="container mx-auto px-4 text-center text-sm text-muted-foreground">
          <p>&copy; {new Date().getFullYear()} Your Company. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
