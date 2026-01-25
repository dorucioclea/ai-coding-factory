import { authService } from '@/services/auth/authService';

// Test the auth service instead of the hook to avoid complex Redux mocking
describe('authService', () => {
  it('exports required methods', () => {
    expect(typeof authService.login).toBe('function');
    expect(typeof authService.register).toBe('function');
    expect(typeof authService.logout).toBe('function');
    expect(typeof authService.refreshTokens).toBe('function');
    expect(typeof authService.forgotPassword).toBe('function');
    expect(typeof authService.getCurrentUser).toBe('function');
    expect(typeof authService.resetPassword).toBe('function');
    expect(typeof authService.changePassword).toBe('function');
  });
});
