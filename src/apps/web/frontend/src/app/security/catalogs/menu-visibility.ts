export function canSeeCatalogs(): boolean {
  const token = window.localStorage.getItem('access_token');
  if (!token) {
    return false;
  }

  try {
    const payload = JSON.parse(atob(token.split('.')[1] ?? '')) as { roles?: string[] };
    return (payload.roles ?? []).some((role) => role.toLowerCase() === 'globaladmin');
  } catch {
    return false;
  }
}
