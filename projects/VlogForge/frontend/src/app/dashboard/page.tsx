'use client';

import { useAuth } from '@/hooks/use-auth';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Skeleton,
} from '@/components/ui';
import { Users, FileText, Activity, TrendingUp } from 'lucide-react';

const stats = [
  {
    title: 'Total Users',
    value: '2,543',
    change: '+12.5%',
    changeType: 'positive' as const,
    icon: Users,
  },
  {
    title: 'Documents',
    value: '1,234',
    change: '+8.2%',
    changeType: 'positive' as const,
    icon: FileText,
  },
  {
    title: 'Active Sessions',
    value: '423',
    change: '-3.1%',
    changeType: 'negative' as const,
    icon: Activity,
  },
  {
    title: 'Revenue',
    value: '$45,231',
    change: '+20.1%',
    changeType: 'positive' as const,
    icon: TrendingUp,
  },
];

export default function DashboardPage() {
  const { user, isLoading } = useAuth();

  return (
    <div className="space-y-8">
      {/* Welcome section */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        {isLoading ? (
          <Skeleton className="mt-2 h-5 w-48" />
        ) : (
          <p className="text-muted-foreground">
            Welcome back, {user?.firstName ?? 'User'}! Here&apos;s what&apos;s
            happening today.
          </p>
        )}
      </div>

      {/* Stats grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                {stat.title}
              </CardTitle>
              <stat.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p
                className={`text-xs ${
                  stat.changeType === 'positive'
                    ? 'text-success'
                    : 'text-destructive'
                }`}
              >
                {stat.change} from last month
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main content */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
        {/* Overview card */}
        <Card className="col-span-4">
          <CardHeader>
            <CardTitle>Overview</CardTitle>
            <CardDescription>
              Your activity overview for this month
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex h-[350px] items-center justify-center text-muted-foreground">
              Chart placeholder - integrate with Recharts
            </div>
          </CardContent>
        </Card>

        {/* Recent activity card */}
        <Card className="col-span-3">
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
            <CardDescription>Latest actions in your account</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="flex items-center gap-4">
                  <div className="h-9 w-9 rounded-full bg-muted" />
                  <div className="flex-1 space-y-1">
                    <p className="text-sm font-medium">Activity item {i}</p>
                    <p className="text-xs text-muted-foreground">
                      {i} hour{i !== 1 ? 's' : ''} ago
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
