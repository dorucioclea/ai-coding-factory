'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  LayoutDashboard,
  Settings,
  Users,
  FileText,
  BarChart3,
  HelpCircle,
  ChevronLeft,
  Link2,
  Calendar,
  Lightbulb,
  UserCircle,
  ClipboardList,
} from 'lucide-react';

import { Button } from '@/components/ui';
import { cn } from '@/lib/utils';
import { useUIStore } from '@/stores';
import { useAuth } from '@/hooks/use-auth';

interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  roles?: string[];
}

const navItems: NavItem[] = [
  {
    title: 'Dashboard',
    href: '/dashboard',
    icon: LayoutDashboard,
  },
  {
    title: 'Content Ideas',
    href: '/dashboard/content',
    icon: Lightbulb,
  },
  {
    title: 'Calendar',
    href: '/dashboard/calendar',
    icon: Calendar,
  },
  {
    title: 'Team',
    href: '/dashboard/team',
    icon: Users,
  },
  {
    title: 'Tasks',
    href: '/dashboard/tasks',
    icon: ClipboardList,
  },
  {
    title: 'Analytics',
    href: '/dashboard/analytics',
    icon: BarChart3,
  },
  {
    title: 'Integrations',
    href: '/dashboard/integrations',
    icon: Link2,
  },
  {
    title: 'Profile',
    href: '/dashboard/profile',
    icon: UserCircle,
  },
  {
    title: 'Documents',
    href: '/dashboard/documents',
    icon: FileText,
  },
  {
    title: 'Settings',
    href: '/settings',
    icon: Settings,
  },
  {
    title: 'Help',
    href: '/help',
    icon: HelpCircle,
  },
];

export function Sidebar() {
  const pathname = usePathname();
  const { sidebar, setSidebarCollapsed } = useUIStore();
  const { hasAnyRole } = useAuth();

  const filteredItems = navItems.filter(
    (item) => !item.roles || hasAnyRole(item.roles)
  );

  return (
    <aside
      className={cn(
        'fixed left-0 top-14 z-40 h-[calc(100vh-3.5rem)] border-r bg-background transition-all duration-300',
        sidebar.isOpen ? 'translate-x-0' : '-translate-x-full',
        sidebar.isCollapsed ? 'w-16' : 'w-64',
        'md:translate-x-0' // Always visible on desktop
      )}
    >
      <div className="flex h-full flex-col">
        {/* Navigation */}
        <nav className="flex-1 space-y-1 p-2">
          {filteredItems.map((item) => {
            const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
            const Icon = item.icon;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                  sidebar.isCollapsed && 'justify-center px-2'
                )}
              >
                <Icon className="h-5 w-5 shrink-0" />
                {!sidebar.isCollapsed && <span>{item.title}</span>}
              </Link>
            );
          })}
        </nav>

        {/* Collapse toggle */}
        <div className="hidden border-t p-2 md:block">
          <Button
            variant="ghost"
            size="sm"
            className={cn(
              'w-full justify-start',
              sidebar.isCollapsed && 'justify-center'
            )}
            onClick={() => setSidebarCollapsed(!sidebar.isCollapsed)}
          >
            <ChevronLeft
              className={cn(
                'h-4 w-4 transition-transform',
                sidebar.isCollapsed && 'rotate-180'
              )}
            />
            {!sidebar.isCollapsed && <span className="ml-2">Collapse</span>}
          </Button>
        </div>
      </div>
    </aside>
  );
}

export default Sidebar;
